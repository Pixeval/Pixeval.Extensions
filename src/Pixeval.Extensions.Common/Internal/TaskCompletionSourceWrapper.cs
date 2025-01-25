// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Internal;

[GeneratedComClass]
internal partial class TaskCompletionSourceWrapper(TaskCompletionSource source) : ITaskCompletionSource
{
    public TaskCompletionSource Source { get; } = source;

    public Task Task => Source.Task;

    public void SetCompleted() => Source.SetResult();

    public void SetException(string message) => Source.SetException(new Exception(message));
}
