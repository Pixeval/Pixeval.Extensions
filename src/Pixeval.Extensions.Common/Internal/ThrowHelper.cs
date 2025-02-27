// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Pixeval.Extensions.Common.Internal;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void HttpRequest(string? message) => throw new HttpRequestException(message);

    [DoesNotReturn]
    public static void Format(string? message) => throw new FormatException(message);
}
