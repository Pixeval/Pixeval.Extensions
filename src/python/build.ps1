param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [ValidateSet("Auto", "Native", "CMake")]
    [string] $BuildSystem = "Auto",

    [string] $RuntimeIdentifier = "",

    [string] $Python = "python",

    [string] $CMakePath = "",

    [string] $NinjaPath = "",

    [string] $VsDevCmd = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat",

    [string] $InstallDirectory = "",

    [string] $PublishConfig = "",

    [switch] $PackSdk,

    [switch] $Publish,

    [switch] $Install
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root = (Resolve-Path -LiteralPath $Root).Path
$SourceRoot = (Resolve-Path -LiteralPath (Join-Path $Root "..")).Path
$IsWindowsPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
$IsLinuxPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)
$IsMacOSPlatform = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
$Architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()

switch ($Architecture) {
    "x64" { $RidArch = "x64"; break }
    "x86" { $RidArch = "x86"; break }
    "arm64" { $RidArch = "arm64"; break }
    "arm" { $RidArch = "arm"; break }
    default { $RidArch = $Architecture; break }
}

if ([string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
    if ($IsWindowsPlatform) {
        $RuntimeIdentifier = "win-$RidArch"
    } elseif ($IsLinuxPlatform) {
        $RuntimeIdentifier = "linux-$RidArch"
    } elseif ($IsMacOSPlatform) {
        $RuntimeIdentifier = "osx-$RidArch"
    } else {
        $RuntimeIdentifier = "native-$RidArch"
    }
}

if ($BuildSystem -eq "Auto") {
    $BuildSystem = if ($IsWindowsPlatform) { "Native" } else { "CMake" }
}

if ($BuildSystem -eq "Native" -and -not $IsWindowsPlatform) {
    throw "Native build uses MSVC and is Windows-only. Use -BuildSystem CMake on this platform."
}

$BuildFlavor = if ($BuildSystem -eq "CMake") { "$Configuration-cmake" } else { $Configuration }
$BuildDir = Join-Path (Join-Path (Join-Path $Root "build") $RuntimeIdentifier) $BuildFlavor

if ($RuntimeIdentifier -like "win-*") {
    $OutputName = "Pixeval.Extensions.Python.Demo.dll"
} elseif ($RuntimeIdentifier -like "osx-*") {
    $OutputName = "libpixeval_extensions_python_demo.dylib"
} else {
    $OutputName = "libpixeval_extensions_python_demo.so"
}

$OutputDll = Join-Path $BuildDir $OutputName
$GeneratorDirectory = Join-Path $SourceRoot "Pixeval.Extensions.Generator"
$GeneratorProject = Join-Path $GeneratorDirectory "Pixeval.Extensions.Generator.csproj"
$GeneratorAssembly = Join-Path (Join-Path (Join-Path (Join-Path $GeneratorDirectory "bin") $Configuration) "net8.0") "Pixeval.Extensions.Generator.dll"
$IdlDir = Join-Path $SourceRoot "Pixeval.Extensions.IDL"
$IdlFiles = @(
    Join-Path $IdlDir "metadata.json"
    Join-Path $IdlDir "commands.pidl"
    Join-Path $IdlDir "core.pidl"
    Join-Path $IdlDir "format-providers.pidl"
    Join-Path $IdlDir "settings.pidl"
)

function Resolve-Executable {
    param(
        [string] $NameOrPath,
        [string] $CommandName,
        [string[]] $Fallbacks = @()
    )

    if (-not [string]::IsNullOrWhiteSpace($NameOrPath)) {
        if (Test-Path -LiteralPath $NameOrPath) {
            return (Resolve-Path -LiteralPath $NameOrPath).Path
        }

        $command = Get-Command $NameOrPath -ErrorAction SilentlyContinue
        if ($command) {
            return $command.Source
        }

        throw "$CommandName was not found: $NameOrPath"
    }

    foreach ($fallback in $Fallbacks) {
        if (Test-Path -LiteralPath $fallback) {
            return (Resolve-Path -LiteralPath $fallback).Path
        }
    }

    $pathCommand = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($pathCommand) {
        return $pathCommand.Source
    }

    throw "$CommandName was not found. Pass an explicit path to the script."
}

function Resolve-OptionalExecutable {
    param(
        [string] $NameOrPath,
        [string] $CommandName,
        [string[]] $Fallbacks = @()
    )

    if (-not [string]::IsNullOrWhiteSpace($NameOrPath)) {
        return Resolve-Executable $NameOrPath $CommandName $Fallbacks
    }

    foreach ($fallback in $Fallbacks) {
        if (Test-Path -LiteralPath $fallback) {
            return (Resolve-Path -LiteralPath $fallback).Path
        }
    }

    $pathCommand = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($pathCommand) {
        return $pathCommand.Source
    }

    return ""
}

function Resolve-VsDevCmd {
    if (Test-Path -LiteralPath $VsDevCmd) {
        return (Resolve-Path -LiteralPath $VsDevCmd).Path
    }

    $candidates = @()
    $vsWhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path -LiteralPath $vsWhere) {
        $installationPath = & $vsWhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
        if ($installationPath) {
            $candidates += (Join-Path $installationPath "Common7\Tools\VsDevCmd.bat")
        }
    }

    foreach ($year in @("18", "2022")) {
        foreach ($edition in @("Community", "Professional", "Enterprise", "BuildTools")) {
            $candidates += "C:\Program Files\Microsoft Visual Studio\$year\$edition\Common7\Tools\VsDevCmd.bat"
        }
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Visual Studio developer command prompt was not found. Pass -VsDevCmd explicitly."
}

function ConvertTo-CmdArgument {
    param([string] $Argument)

    if ($Argument -match '[\s&()^|<>]') {
        return "`"$Argument`""
    }

    return $Argument
}

function Invoke-VsCommand {
    param(
        [string] $VsDevCmdPath,
        [string] $Executable,
        [string[]] $Arguments
    )

    $vsArch = if ($RidArch -eq "arm64") { "arm64" } elseif ($RidArch -eq "x86") { "x86" } else { "x64" }
    $commandLine = "`"$VsDevCmdPath`" -arch=$vsArch -host_arch=$vsArch && `"$Executable`" " + (($Arguments | ForEach-Object { ConvertTo-CmdArgument $_ }) -join " ")
    & cmd.exe /c $commandLine
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

function Remove-BuildDirectoryIfExists {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $resolved = (Resolve-Path -LiteralPath $Path).Path
    if (-not $resolved.StartsWith($Root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a directory outside the Python extension workspace: $resolved"
    }

    Remove-Item -LiteralPath $resolved -Recurse -Force
}

function Invoke-Generator {
    $packageDirectory = Join-Path $Root "pixeval_extensions"
    New-Item -ItemType Directory -Force $packageDirectory | Out-Null

    & dotnet build $GeneratorProject -c $Configuration -p:Platform=AnyCPU -v:minimal
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & dotnet $GeneratorAssembly python-symbols (Join-Path $packageDirectory "symbols.py")
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & dotnet $GeneratorAssembly python-common $IdlFiles (Join-Path $packageDirectory "abi.py")
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & dotnet $GeneratorAssembly python-sdk $IdlFiles (Join-Path $packageDirectory "sdk.py")
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

function Get-PythonInfo {
    param([string] $PythonExecutable)

    $pythonInfoJson = & $PythonExecutable -c @"
import json
import pathlib
import sys
import sysconfig

prefix = pathlib.Path(sys.prefix)
base_prefix = pathlib.Path(getattr(sys, 'base_prefix', sys.prefix))
info = {
    'prefix': str(prefix),
    'base_prefix': str(base_prefix),
    'include': sysconfig.get_config_var('INCLUDEPY') or sysconfig.get_path('include'),
    'libdir': sysconfig.get_config_var('LIBDIR') or sysconfig.get_config_var('LIBPL'),
    'libs': str(base_prefix / 'libs'),
    'ldlibrary': sysconfig.get_config_var('LDLIBRARY') or '',
    'version': f'{sys.version_info.major}{sys.version_info.minor}',
    'stdlib': sysconfig.get_path('stdlib') or '',
    'platstdlib': sysconfig.get_path('platstdlib') or '',
    'dlls': str(base_prefix / 'DLLs'),
}
print(json.dumps(info))
"@

    return $pythonInfoJson | ConvertFrom-Json
}

function Build-Native {
    param([object] $PythonInfo)

    $vsDevCmdPath = Resolve-VsDevCmd
    $pythonLibraryDirectory = $PythonInfo.libdir
    if ([string]::IsNullOrWhiteSpace($pythonLibraryDirectory)) {
        $pythonLibraryDirectory = $PythonInfo.libs
    }

    if ([string]::IsNullOrWhiteSpace($PythonInfo.include) -or -not (Test-Path -LiteralPath $PythonInfo.include)) {
        throw "Python include directory was not found. Install Python development headers or pass -Python to a full CPython install."
    }

    if ([string]::IsNullOrWhiteSpace($pythonLibraryDirectory) -or -not (Test-Path -LiteralPath $pythonLibraryDirectory)) {
        throw "Python library directory was not found. Install Python development libraries or pass -Python to a full CPython install."
    }

    New-Item -ItemType Directory -Force $BuildDir | Out-Null

    $sourceFile = Join-Path (Join-Path $Root "runtime") "pixeval_python_bootstrap.c"
    $compilerFlags = @(
        "/nologo",
        "/LD",
        "/W4",
        "/DWIN32_LEAN_AND_MEAN",
        "/I`"$($PythonInfo.include)`"",
        "`"$sourceFile`"",
        "/Fe:`"$OutputDll`"",
        "/link",
        "/LIBPATH:`"$pythonLibraryDirectory`"",
        "python$($PythonInfo.version).lib"
    )

    if ($Configuration -eq "Release") {
        $compilerFlags = @("/O2") + $compilerFlags
    } else {
        $compilerFlags = @("/Zi") + $compilerFlags
    }

    $compileCommand = "`"$vsDevCmdPath`" -arch=$RidArch -host_arch=$RidArch && cl " + ($compilerFlags -join " ")
    & cmd.exe /c $compileCommand
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

function Build-CMake {
    param([string] $PythonExecutable)

    $visualStudioRoots = @()
    if ($IsWindowsPlatform) {
        foreach ($year in @("18", "2022")) {
            foreach ($edition in @("Community", "Professional", "Enterprise", "BuildTools")) {
                $visualStudioRoots += "C:\Program Files\Microsoft Visual Studio\$year\$edition"
            }
        }
    }

    $cmakeFallbacks = $visualStudioRoots | ForEach-Object {
        Join-Path $_ "Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
    }
    $ninjaFallbacks = $visualStudioRoots | ForEach-Object {
        Join-Path $_ "Common7\IDE\CommonExtensions\Microsoft\CMake\Ninja\ninja.exe"
    }

    $cmakeExecutable = Resolve-Executable $CMakePath "cmake" $cmakeFallbacks
    $ninjaExecutable = Resolve-OptionalExecutable $NinjaPath "ninja" $ninjaFallbacks

    $configureArguments = @(
        "-S", $Root,
        "-B", $BuildDir,
        "-DCMAKE_BUILD_TYPE=$Configuration",
        "-DPython3_EXECUTABLE=$PythonExecutable"
    )

    if (-not [string]::IsNullOrWhiteSpace($ninjaExecutable)) {
        $configureArguments += @("-G", "Ninja", "-DCMAKE_MAKE_PROGRAM=$ninjaExecutable")
    }

    if ($IsWindowsPlatform) {
        $vsDevCmdPath = Resolve-VsDevCmd
        Invoke-VsCommand $vsDevCmdPath $cmakeExecutable $configureArguments
        Invoke-VsCommand $vsDevCmdPath $cmakeExecutable @("--build", $BuildDir, "--config", $Configuration)
    } else {
        & $cmakeExecutable @configureArguments
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }

        & $cmakeExecutable --build $BuildDir --config $Configuration
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }

    $builtLibrary = Get-ChildItem -LiteralPath $BuildDir -Recurse -File |
        Where-Object { $_.Name -eq $OutputName } |
        Select-Object -First 1

    if (-not $builtLibrary) {
        throw "CMake build completed but $OutputName was not found under $BuildDir"
    }

    if ($builtLibrary.FullName -ne $OutputDll) {
        Copy-Item -LiteralPath $builtLibrary.FullName -Destination $OutputDll -Force
    }
}

function Copy-PythonRuntime {
    param(
        [object] $PythonInfo,
        [string] $RuntimeDestination
    )

    if ($IsWindowsPlatform) {
        $prefixes = @($PythonInfo.prefix, $PythonInfo.base_prefix) |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Select-Object -Unique

        foreach ($dllName in @("python3.dll", "python$($PythonInfo.version).dll")) {
            $dllSource = $prefixes |
                ForEach-Object { Join-Path $_ $dllName } |
                Where-Object { Test-Path -LiteralPath $_ } |
                Select-Object -First 1

            if (-not $dllSource) {
                throw "Python runtime DLL was not found: $dllName"
            }

            Copy-Item -LiteralPath $dllSource -Destination $RuntimeDestination -Force
        }

        foreach ($runtimeName in @("vcruntime140.dll", "vcruntime140_1.dll")) {
            $runtimeSource = $prefixes |
                ForEach-Object { Join-Path $_ $runtimeName } |
                Where-Object { Test-Path -LiteralPath $_ } |
                Select-Object -First 1

            if ($runtimeSource) {
                Copy-Item -LiteralPath $runtimeSource -Destination $RuntimeDestination -Force
            }
        }

        foreach ($runtimeDirectory in @("Lib", "DLLs")) {
            $source = $prefixes |
                ForEach-Object { Join-Path $_ $runtimeDirectory } |
                Where-Object { Test-Path -LiteralPath $_ } |
                Select-Object -First 1

            if (-not $source) {
                throw "Python runtime directory was not found: $runtimeDirectory"
            }

            $runtimeDestinationPath = Join-Path $RuntimeDestination $runtimeDirectory
            Remove-BuildDirectoryIfExists $runtimeDestinationPath
            New-Item -ItemType Directory -Force $runtimeDestinationPath | Out-Null
            Copy-Item -Path (Join-Path $source "*") -Destination $runtimeDestinationPath -Recurse -Force
        }

        Set-Content -LiteralPath (Join-Path $RuntimeDestination "pixeval_python_home.txt") -Value "." -NoNewline
    } else {
        Set-Content -LiteralPath (Join-Path $RuntimeDestination "pixeval_python_home.txt") -Value $PythonInfo.prefix -NoNewline
    }
}

function Copy-Payload {
    param(
        [object] $PythonInfo,
        [string] $Destination
    )

    Copy-PythonRuntime $PythonInfo $Destination

    $packageDestination = Join-Path $Destination "pixeval_extensions"
    Remove-BuildDirectoryIfExists $packageDestination
    Copy-Item -LiteralPath (Join-Path $Root "pixeval_extensions") -Destination $packageDestination -Recurse -Force

    Copy-Item -LiteralPath (Join-Path (Join-Path $Root "demo") "pixeval_extension_host.py") -Destination $Destination -Force

    $logoPath = "D:\logo.png"
    if (Test-Path -LiteralPath $logoPath) {
        Copy-Item -LiteralPath $logoPath -Destination (Join-Path $Destination "logo.png") -Force
    }
}

function Get-DefaultInstallDirectory {
    $configPath = $PublishConfig
    if ([string]::IsNullOrWhiteSpace($configPath)) {
        $configPath = Join-Path $Root "publish.json"
    }

    if (Test-Path -LiteralPath $configPath) {
        $config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
        $publishDir = [string] $config.publishDir
        if (-not [string]::IsNullOrWhiteSpace($publishDir)) {
            if ([System.IO.Path]::IsPathRooted($publishDir)) {
                return [System.IO.Path]::GetFullPath($publishDir)
            }

            return [System.IO.Path]::GetFullPath((Join-Path $Root $publishDir))
        }
    }

    return [System.IO.Path]::GetFullPath((Join-Path $SourceRoot "..\publish\Pixeval.Extensions.Python.Demo"))
}

function Ensure-PythonBuildModule {
    param([string] $PythonExecutable)

    & $PythonExecutable -c "import build" *> $null
    if ($LASTEXITCODE -eq 0) {
        return
    }

    & $PythonExecutable -m pip install --user build
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

function Pack-Sdk {
    param([string] $PythonExecutable)

    Ensure-PythonBuildModule $PythonExecutable

    $distDirectory = Join-Path $Root "dist"
    $packageBuildDirectory = Join-Path $Root "build"
    Remove-BuildDirectoryIfExists (Join-Path $packageBuildDirectory "lib")

    if (Test-Path -LiteralPath $packageBuildDirectory) {
        Get-ChildItem -LiteralPath $packageBuildDirectory -Directory |
            Where-Object { $_.Name -like "bdist.*" -or $_.Name -like "temp.*" -or $_.Name -like "scripts-*" } |
            ForEach-Object { Remove-BuildDirectoryIfExists $_.FullName }
    }

    Remove-BuildDirectoryIfExists (Join-Path $Root "pixeval_extensions.egg-info")
    Remove-BuildDirectoryIfExists $distDirectory
    New-Item -ItemType Directory -Force $distDirectory | Out-Null

    & $PythonExecutable -m build --wheel --outdir $distDirectory $Root
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $wheel = Get-ChildItem -LiteralPath $distDirectory -Filter "*.whl" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $wheel) {
        throw "Python SDK wheel was not found under $distDirectory"
    }

    Write-Host "Packed Python SDK wheel: $($wheel.FullName)"
}

$pythonExecutable = Resolve-Executable $Python "python" @()
$pythonInfo = Get-PythonInfo $pythonExecutable

Invoke-Generator

if ($BuildSystem -eq "Native") {
    Build-Native $pythonInfo
} else {
    Build-CMake $pythonExecutable
}

Copy-Payload $pythonInfo $BuildDir

if ($PackSdk) {
    Pack-Sdk $pythonExecutable
}

if ($Install -or $Publish) {
    if ([string]::IsNullOrWhiteSpace($InstallDirectory)) {
        $InstallDirectory = Get-DefaultInstallDirectory
    }

    New-Item -ItemType Directory -Force $InstallDirectory | Out-Null
    Copy-Item -Path (Join-Path $BuildDir "*") -Destination $InstallDirectory -Recurse -Force

    if ($IsWindowsPlatform) {
        Set-Content -LiteralPath (Join-Path $InstallDirectory "pixeval_python_home.txt") -Value "." -NoNewline
    }

    $verb = if ($Publish -and -not $Install) { "Published" } else { "Installed" }
    Write-Host "$verb $OutputName to $InstallDirectory"
} else {
    Write-Host "Built $OutputName via $BuildSystem at $BuildDir"
}
