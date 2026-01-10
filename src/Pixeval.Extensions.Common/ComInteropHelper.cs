// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Extensions.Common.Internal;

namespace Pixeval.Extensions.Common;

public static class ComInteropHelper
{
    extension(Stream stream)
    {
        public IStream ToIStream() => new StreamToIStreamAdaptor(stream);
    }

    extension(IStream iStream)
    {
        public Stream ToStream() => new IStreamToStreamAdaptor(iStream);
    }

    extension(TaskCompletionSource taskCompletionSource)
    {
        public ITaskCompletionSource ToITaskCompletionSource() => new TaskCompletionSourceAdaptor(taskCompletionSource);
    }

    extension(Exception exception)
    {
        public IException ToIException() => new ExceptionToIExceptionAdaptor(exception);
    }

    extension(IException iException)
    {
        public Exception ToException() => new IExceptionToExceptionAdaptor(iException);
    }
}
