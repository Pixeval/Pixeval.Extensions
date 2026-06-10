# Pixeval.Extensions

本项目是Pixeval扩展功能的工具项目，被主项目（下称[Pixeval](https://github.com/Pixeval/Pixeval/)）高强度依赖，所以本项目会和Pixeval同步迭代（使用相同的版本号）。

本项目通过NuGet进行分发，在NuGet上有两个包：

- [Pixeval.Extensions.Common](https://www.nuget.org/packages/Pixeval.Extensions.Common/)：这个包声明了Pixeval与扩展进行通信使用的接口，和一些简单的工具类。这个包会被Pixeval和下面的SDK包引用。
- [Pixeval.Extensions.SDK](https://www.nuget.org/packages/Pixeval.Extensions.SDK/)：这个包对上一个包的接口进行了封装，降低了编写扩展的难度和工作量。扩展的开发者应该引用这个包。

## 关于向后兼容

本项目正处于早期开发阶段，不能保证向后兼容，但遇到相关问题欢迎与[我](https://github.com/Poker-sang)商讨( •̀ ω •́ )✧。

C#/.NET的extension类型对本项目帮助很大，在extension功能发布后势必要对本项目进行全方位的优化，但也会尽量保持向前兼容。

## 关于扩展系统使用的技术

为了方便以后将Pixeval改写为AOT项目，Pixeval的扩展系统要求加载的扩展也是AOT的，所以扩展系统选择了COM + P/Invoke的技术实现。

.NET在8.0版本才实现了[完全支持AOT的COM技术](https://learn.microsoft.com/zh-cn/dotnet/standard/native-interop/com-wrappers)，所以本项目最低支持的.NET SDK是.NET 8。

由于AOT和COM系统的局限性，项目中几乎不能使用反射技术，通信时也不能使用复杂的类型（只有接口和方法，这也是为什么需要SDK项目），但好在除此之外的其他技术并没有多少限制。

为了保持各语言扩展 API 的一致性，本项目使用 .pidl（Pixeval interface definition language）文件定义接口，并统一由 [生成器](./src/Pixeval.Extensions.Generator) 生成对应语言的接口代码和 SDK。开发者在实现扩展时只需要关注接口定义和 SDK 的使用，不需要关心底层的 COM 和 P/Invoke 细节。

## 开发扩展

理论上来说本扩展系统可以加载任何语言的扩展，目前已经有了 [C++](./src/cpp) 和 [Python](./src/python) 的示例库，其他语言理论上也可以，但需要额外的适配工作。

### 其他语言开发扩展

非 C# 扩展需要使用与 Pixeval ABI 对齐的 native 入口库和对应语言 SDK。开发者可以在 GitHub 的 Actions 页面中找到最新一次成功构建，下载目标平台 `<rid>` 对应的 artifact，然后在自己的项目中引用 SDK，并参考本仓库 demo 的写法实现 host 和 extension 对象。

#### 使用 C++ 开发扩展

C++ SDK 会在每次提交时由 [C++ SDK Build](./.github/workflows/cpp_build.yml) 工作流自动打包。开发者可以在 GitHub Actions 对应的构建记录中下载 `pixeval-cpp-sdk-<rid>` 产物，例如 `pixeval-cpp-sdk-win-x64`、`pixeval-cpp-sdk-linux-x64` 或 `pixeval-cpp-sdk-osx-arm64`。

下载后解压 zip，目录中会包含 `include` 和 `share/PixevalExtensionsCpp`。CMake 项目可以这样引用：

```cmake
cmake_minimum_required(VERSION 3.24)
project(MyPixevalExtension LANGUAGES CXX)

find_package(PixevalExtensionsCpp CONFIG REQUIRED)

add_library(MyPixevalExtension SHARED src/extension.cpp)
target_link_libraries(MyPixevalExtension PRIVATE Pixeval.Extensions.Cpp::SDK)
target_compile_features(MyPixevalExtension PRIVATE cxx_std_20)
```

配置项目时把解压目录传给 `CMAKE_PREFIX_PATH`：

```powershell
cmake -S . -B build -DCMAKE_PREFIX_PATH=<PixevalExtensionsCpp SDK 解压目录>
cmake --build build --config Release
```

扩展实现可以参考 [C++ Demo](./src/cpp/Pixeval.Extensions.Cpp.Demo/src/demo.cpp)：包含 `<pixeval/extensions.hpp>`，继承 `HostBase`，在宿主对象中添加设置项或扩展对象，最后使用 `PIXEV_EXTENSION_HOST(g_host)` 导出扩展入口。

本仓库中的 C++ demo 可以用以下命令构建并发布：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File src/cpp/build.ps1 -Configuration Release -Publish -SkipPackage
```

发布目录由 [publish.json](./src/cpp/publish.json) 中的 `publishDir` 指定，默认是仓库根目录下的 `publish/Pixeval.Extensions.Cpp.Demo/`；临时覆盖可传入 `-PublishDirectory <目录>`。该目录可以整体复制到 Pixeval 的扩展目录中使用。

#### 使用 Python 开发扩展

Python SDK 和入口库会在每次提交时由 [Python SDK Build](./.github/workflows/py_build.yml) 工作流自动打包。开发者需要下载 `pixeval-python-sdk-<rid>` 和 `pixeval-python-bootstrap-<rid>`；前者包含 `pixeval-extensions` wheel，后者包含对应平台的 native 入口库，例如 `.dll`、`.so` 或 `.dylib`。

开发时可以先安装 SDK wheel：

```powershell
python -m pip install pixeval_extensions-0.1.0-py3-none-any.whl
```

扩展实现可以参考 [Python Demo](./src/python/demo/pixeval_extension_host.py)：继承 `ExtensionsHostBase` 和对应的 extension/settings 基类，提供 `dll_get_extensions_host()` 返回 host 指针。发布扩展时，把下载到的 bootstrap 库、`pixeval_extension_host.py`、`pixeval_extensions` 包和需要的资源放在同一目录；Windows 下的 demo 还会携带 Python runtime、`Lib`、`DLLs` 和 `pixeval_python_home.txt`。

本仓库中的 Python demo 可以用以下命令构建并发布：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File src/python/build.ps1 -Configuration Release -Publish
```

发布目录由 [publish.json](./src/python/publish.json) 中的 `publishDir` 指定，默认是仓库根目录下的 `publish/Pixeval.Extensions.Python.Demo/`；临时覆盖可传入 `-InstallDirectory <目录>`。该目录可以整体复制到 Pixeval 的扩展目录中使用。

### C# 扩展案例

- [Pixeval.Extensions.Sample](./src/Pixeval.Extensions.Sample)：本项目自带的简单 C# 扩展示例
- [Pixeval.Extensions.ImageTransformers](https://github.com/Pixeval/Pixeval.Extensions.ImageTransformers)：Pixeval AI提升画质插件
- [Pixeval.Extensions.Formats](https://github.com/Pixeval/Pixeval.Extensions.Formats)：Pixeval 下载格式扩展插件
- [Pixeval.Extensions.Translators](https://github.com/Pixeval/Pixeval.Extensions.Translators)：Pixeval 翻译器插件
- [Pixeval.Extensions.Downloaders](https://github.com/Pixeval/Pixeval.Extensions.Downloaders)：Pixeval 下载器插件
