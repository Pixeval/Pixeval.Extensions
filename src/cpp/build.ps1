param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [AllowEmptyString()]
    [ValidateSet("x64", "x86", "arm64")]
    [string] $Architecture = "",

    [AllowEmptyString()]
    [ValidateSet("Ninja", "Unix Makefiles", "Ninja Multi-Config", "Visual Studio 17 2022", "Visual Studio 18 2026", "Xcode")]
    [string] $Generator = "",

    [string] $BuildDir = "",
    [string] $CMake = "",
    [string] $Ninja = "",

    [string] $PublishDirectory = "",
    [string] $PublishConfig = "",

    [switch] $Publish,
    [switch] $Clean,
    [switch] $NoVsDevShell,
    [switch] $SkipPackage
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Root = (Resolve-Path -LiteralPath $Root).Path
$RepositoryRoot = (Resolve-Path -LiteralPath (Join-Path $Root "..\..")).Path

function Get-PlatformName {
    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        return "win"
    }

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
        return "osx"
    }

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
        return "linux"
    }

    return "unknown"
}

function Get-DefaultArchitecture {
    switch ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture) {
        "X64" { return "x64" }
        "X86" { return "x86" }
        "Arm64" { return "arm64" }
        default { return "x64" }
    }
}

function Test-CommandExists([string] $Name) {
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Find-VisualStudioPath {
    $programFilesX86 = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)")
    if ([string]::IsNullOrWhiteSpace($programFilesX86)) {
        $programFilesX86 = ""
    }

    $programFiles = [Environment]::GetEnvironmentVariable("ProgramFiles")
    if ([string]::IsNullOrWhiteSpace($programFiles)) {
        $programFiles = ""
    }

    $vsWhereCandidates = @(
        (Join-Path $programFilesX86 "Microsoft Visual Studio\Installer\vswhere.exe"),
        (Join-Path $programFiles "Microsoft Visual Studio\Installer\vswhere.exe")
    )

    foreach ($candidate in $vsWhereCandidates) {
        if (-not (Test-Path $candidate)) {
            continue
        }

        $installPath = & $candidate -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($installPath)) {
            return $installPath.Trim()
        }
    }

    $roots = @(
        [Environment]::GetEnvironmentVariable("ProgramFiles"),
        [Environment]::GetEnvironmentVariable("ProgramFiles(x86)")
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($rootPath in $roots) {
        $tools = Get-ChildItem -Path (Join-Path $rootPath "Microsoft Visual Studio") -Recurse -Filter "VsDevCmd.bat" -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending |
            Select-Object -First 1

        if ($null -ne $tools) {
            return Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $tools.FullName))
        }
    }

    return ""
}

function Import-VisualStudioEnvironment([string] $VisualStudioPath, [string] $TargetArchitecture) {
    $vsDevCmd = Join-Path $VisualStudioPath "Common7\Tools\VsDevCmd.bat"
    if (-not (Test-Path $vsDevCmd)) {
        throw "Visual Studio developer command script was not found: $vsDevCmd"
    }

    $hostArchitecture = Get-DefaultArchitecture
    $command = "`"$vsDevCmd`" -arch=$TargetArchitecture -host_arch=$hostArchitecture -no_logo && set"
    $environment = & cmd.exe /s /c $command
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    foreach ($line in $environment) {
        $separator = $line.IndexOf("=")
        if ($separator -le 0) {
            continue
        }

        $name = $line.Substring(0, $separator)
        $value = $line.Substring($separator + 1)
        Set-Item -Path "Env:$name" -Value $value
    }
}

function Resolve-Tool([string] $RequestedPath, [string] $CommandName, [string[]] $FallbackPaths) {
    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        if (-not (Test-Path $RequestedPath)) {
            throw "$CommandName was not found: $RequestedPath"
        }

        return (Resolve-Path $RequestedPath).Path
    }

    $command = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    foreach ($path in $FallbackPaths) {
        if (-not [string]::IsNullOrWhiteSpace($path) -and (Test-Path $path)) {
            return $path
        }
    }

    throw "$CommandName was not found. Install it or pass -$CommandName <path>."
}

function Test-MultiConfigGenerator([string] $Name) {
    return $Name -match "Visual Studio|Xcode|Ninja Multi-Config"
}

function Resolve-PublishDirectory {
    if (-not [string]::IsNullOrWhiteSpace($PublishDirectory)) {
        if ([System.IO.Path]::IsPathRooted($PublishDirectory)) {
            return [System.IO.Path]::GetFullPath($PublishDirectory)
        }

        return [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot $PublishDirectory))
    }

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

    return [System.IO.Path]::GetFullPath((Join-Path $RepositoryRoot "publish\Pixeval.Extensions.Cpp.Demo"))
}

if ([string]::IsNullOrWhiteSpace($Architecture)) {
    $Architecture = Get-DefaultArchitecture
}

$Platform = Get-PlatformName
$Rid = "$Platform-$Architecture"

if ([string]::IsNullOrWhiteSpace($BuildDir)) {
    $BuildDir = Join-Path $Root (Join-Path "build" $Rid)
}

$VisualStudioPath = ""
if ($Platform -eq "win" -and -not $NoVsDevShell) {
    $VisualStudioPath = Find-VisualStudioPath
    if (-not [string]::IsNullOrWhiteSpace($VisualStudioPath)) {
        Import-VisualStudioEnvironment $VisualStudioPath $Architecture
    }
}

$cmakeFallbacks = @()
$ninjaFallbacks = @()
if (-not [string]::IsNullOrWhiteSpace($VisualStudioPath)) {
    $cmakeFallbacks += Join-Path $VisualStudioPath "Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
    $ninjaFallbacks += Join-Path $VisualStudioPath "Common7\IDE\CommonExtensions\Microsoft\CMake\Ninja\ninja.exe"
}

$CMakePath = Resolve-Tool $CMake "cmake" $cmakeFallbacks

$NinjaPath = ""
if (-not [string]::IsNullOrWhiteSpace($Ninja)) {
    $NinjaPath = Resolve-Tool $Ninja "ninja" @()
}
elseif ([string]::IsNullOrWhiteSpace($Generator) -or $Generator -match "Ninja") {
    if (Test-CommandExists "ninja") {
        $NinjaPath = (Get-Command "ninja").Source
    }
    else {
        foreach ($path in $ninjaFallbacks) {
            if (Test-Path $path) {
                $NinjaPath = $path
                break
            }
        }
    }
}

if ([string]::IsNullOrWhiteSpace($Generator)) {
    if (-not [string]::IsNullOrWhiteSpace($NinjaPath)) {
        $Generator = "Ninja"
    }
    elseif ($Platform -eq "win") {
        $Generator = "Visual Studio 18 2026"
    }
    elseif ($Platform -eq "osx") {
        $Generator = "Unix Makefiles"
    }
    else {
        $Generator = "Unix Makefiles"
    }
}

$BuildDir = [System.IO.Path]::GetFullPath($BuildDir)

if ($Clean -and (Test-Path $BuildDir)) {
    $rootFullPath = [System.IO.Path]::GetFullPath($Root)
    if ($BuildDir -eq $rootFullPath -or $BuildDir.Length -lt 8) {
        throw "Refusing to clean unsafe build directory: $BuildDir"
    }

    Write-Host "Cleaning $BuildDir"
    Remove-Item -LiteralPath $BuildDir -Recurse -Force
}

$configureArgs = @("-S", $Root, "-B", $BuildDir, "-G", $Generator, "-DPIXEV_ARCH_DIR=$Architecture")

if ($Generator -eq "Ninja" -and -not [string]::IsNullOrWhiteSpace($NinjaPath)) {
    $configureArgs += "-DCMAKE_MAKE_PROGRAM=$NinjaPath"
}

if ($Generator -match "Visual Studio") {
    $configureArgs += @("-A", $Architecture)
}

if (-not (Test-MultiConfigGenerator $Generator)) {
    $configureArgs += "-DCMAKE_BUILD_TYPE=$Configuration"
}

Write-Host "Configuring $Rid $Configuration with $Generator"
& $CMakePath @configureArgs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$buildArgs = @("--build", $BuildDir, "--config", $Configuration)
Write-Host "Building $Rid $Configuration"
& $CMakePath @buildArgs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$outputDir = Join-Path $Root (Join-Path "Pixeval.Extensions.Cpp.Demo" (Join-Path "bin" (Join-Path $Architecture $Configuration)))
$extension = switch ($Platform) {
    "win" { ".dll" }
    "osx" { ".dylib" }
    default { ".so" }
}

$library = Get-ChildItem -Path $outputDir -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*Pixeval.Extensions.CppDemo*$extension" } |
    Select-Object -First 1

if ($null -eq $library) {
    throw "Build succeeded but native library was not found in: $outputDir"
}

Write-Host "Built $($library.FullName)"

if ($Publish) {
    $publishPath = Resolve-PublishDirectory
    New-Item -ItemType Directory -Force -Path $publishPath | Out-Null
    Copy-Item -LiteralPath $library.FullName -Destination (Join-Path $publishPath $library.Name) -Force
    Write-Host "Published $($library.Name) to $publishPath"
}

if (-not $SkipPackage) {
    $packageArgs = @("--build", $BuildDir, "--target", "package", "--config", $Configuration)
    Write-Host "Packaging SDK zip"
    & $CMakePath @packageArgs
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $package = Get-ChildItem -Path $BuildDir -File -Filter "*.zip" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $package) {
        throw "Package target completed but no zip file was found in: $BuildDir"
    }

    Write-Host "Packaged $($package.FullName)"
}
