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

理论上来说本扩展系统可以加载任何不需要运行时的扩展文件（尤其是C++），但我目前还没有为其他语言写工具库，所以实际用除C#之外的语言开发可能会遇到很多困难。

## 扩展案例

- [Pixeval.Extensions.Sample](https://github.com/Pixeval/Pixeval.Extensions/tree/master/src/Pixeval.Extensions.Sample)：本项目自带的简单扩展示例
- [Pixeval.Extensions.Upscaler](https://github.com/Pixeval/Pixeval.Extensions.Upscaler)：Pixeval AI提升画质插件
