// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Extensions.Common;

public delegate int GetExtensionsHost(out nint ppv);

public static class ExtensionsHostStatics
{
    public static Version CurrentSdkVersion => typeof(IExtensionsHost).Assembly.GetName().Version ?? new();
}
