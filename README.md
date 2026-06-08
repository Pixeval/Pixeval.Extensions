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

### C# 扩展案例

- [Pixeval.Extensions.Sample](./src/Pixeval.Extensions.Sample)：本项目自带的简单 C# 扩展示例
- [Pixeval.Extensions.ImageTransformers](https://github.com/Pixeval/Pixeval.Extensions.ImageTransformers)：Pixeval AI提升画质插件
- [Pixeval.Extensions.Formats](https://github.com/Pixeval/Pixeval.Extensions.Formats)：Pixeval 下载格式扩展插件
- [Pixeval.Extensions.Translators](https://github.com/Pixeval/Pixeval.Extensions.Translators)：Pixeval 翻译器插件
- [Pixeval.Extensions.Downloaders](https://github.com/Pixeval/Pixeval.Extensions.Downloaders)：Pixeval 下载器插件
