#region Copyright

// GPL v3 License
// 
// Pixeval.Extensions/Pixeval.Extensions.Common
// Copyright (c) 2024 Pixeval.Extensions.Common/ExtensionsMetadataBase.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.SDK.Internal;

namespace Pixeval.Extensions.SDK;

/// <inheritdoc cref="IExtensionsHost" />
[GeneratedComClass]
public abstract partial class ExtensionsHostBase : IExtensionsHost
{
    /// <summary>
    /// The directory of Pixeval where the temporary files are located.
    /// </summary>
    public static string TempDirectory { get; protected set; } = "";

    /// <summary>
    /// The directory of Pixeval where the extensions are located.
    /// </summary>
    public static string ExtensionDirectory { get; protected set; } = "";

    /// <inheritdoc cref="IExtensionsHost.GetExtensionName" />
    public abstract string ExtensionName { get; }

    /// <inheritdoc cref="IExtensionsHost.GetAuthorName" />
    public abstract string AuthorName { get; }

    /// <inheritdoc cref="IExtensionsHost.GetExtensionLink" />
    public abstract string ExtensionLink { get; }

    /// <inheritdoc cref="IExtensionsHost.GetHelpLink" />
    public abstract string HelpLink { get; }

    /// <inheritdoc cref="IExtensionsHost.GetDescription" />
    public abstract string Description { get; }

    /// <inheritdoc cref="IExtensionsHost.GetVersion" />
    public abstract string Version { get; }

    /// <inheritdoc cref="IExtensionsHost.GetExtensions" />
    public abstract IExtension[] Extensions { get; }

    /// <inheritdoc cref="IExtensionsHost.GetIcon" />
    public abstract byte[]? Icon { get; }

    public ILogger Logger { get; private set; } = null!;

    /// <inheritdoc />
    void IExtensionsHost.Initialize(string cultureName, string tempDirectory, string extensionDirectory, ILogger logger)
    {
        TempDirectory = tempDirectory;
        ExtensionDirectory = extensionDirectory;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new(cultureName);
        Logger = logger;
        Initialize();
    }

    /// <inheritdoc cref="IExtensionsHost.Initialize"/>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// In addition to implementing abstract members, you should also write these <see langword="static"/> members:
    /// <code>
    /// <see langword="public static"/> ExtensionsHost Current { <see langword="get"/>; } = <see langword="new"/>();
    /// 
    /// [<see cref="UnmanagedCallersOnlyAttribute"/>(EntryPoint = <see langword="nameof"/>(DllGetExtensionsHost))]<br/>
    /// <see langword="private static unsafe int"/> DllGetExtensionsHost(<see langword="void"/>** ppv) =&gt; DllGetExtensionsHost(ppv, Current);
    /// </code>
    /// </summary>
    /// <param name="ppv">Pointer of the pointer to the CCW of <paramref name="current"/></param>
    /// <param name="current">Current instance of the derived class of <see cref="ExtensionsHostBase"/></param>
    /// <returns></returns>
    public static unsafe int DllGetExtensionsHost(void** ppv, ExtensionsHostBase current)
    {
        if (_CcwCache is null)
        {
            var comWrappers = new StrategyBasedComWrappers();
            _CcwCache = (void*)comWrappers.GetOrCreateComInterfaceForObject(current, CreateComInterfaceFlags.None);
        }

        *ppv = _CcwCache;
        return 0;
    }

    private static unsafe void* _CcwCache;

    /// <inheritdoc />
    string IExtensionsHost.GetExtensionName() => ExtensionName;

    /// <inheritdoc />
    string IExtensionsHost.GetAuthorName() => AuthorName;

    /// <inheritdoc />
    string IExtensionsHost.GetExtensionLink() => ExtensionLink;

    /// <inheritdoc />
    string IExtensionsHost.GetHelpLink() => HelpLink;

    /// <inheritdoc />
    string IExtensionsHost.GetDescription() => Description;

    /// <inheritdoc />
    string IExtensionsHost.GetSdkVersion() => IExtensionsHost.SdkVersion.ToString();

    /// <inheritdoc />
    string IExtensionsHost.GetVersion() => Version;

    /// <inheritdoc />
    IExtension[] IExtensionsHost.GetExtensions(out int count) => Extensions.GetArray(out count);

    /// <inheritdoc />
    byte[]? IExtensionsHost.GetIcon(out int count) => Icon.GetArray(out count);
}

public static class LoggerHelper
{
    extension(ILogger logger)
    {
        /// <inheritdoc cref="ILogger.Log" />
        public void Log(LogLevel logLevel, string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(logLevel, message, exception?.ToIException(), memberName, filePath, lineNumber);

        /// <inheritdoc cref="Log" />
        public void LogTrace(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Trace, message, exception, memberName, filePath, lineNumber);

        /// <inheritdoc cref="Log" />
        public void LogDebug(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Debug, message, exception, memberName, filePath, lineNumber);

        /// <inheritdoc cref="Log" />
        public void LogInformation(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Information, message, exception, memberName, filePath, lineNumber);

        /// <inheritdoc cref="Log" />
        public void LogWarning(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Warning, message, exception, memberName, filePath, lineNumber);

        /// <inheritdoc cref="Log" />
        public void LogError(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Error, message, exception, memberName, filePath, lineNumber);

        /// <inheritdoc cref="Log" />
        public void LogCritical(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) =>
            logger.Log(LogLevel.Critical, message, exception, memberName, filePath, lineNumber);
    }
}
