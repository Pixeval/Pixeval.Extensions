// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common;

[GeneratedComClass]
[Guid("2B88079D-DC90-4AC2-805F-7D276BC584F4")]
public partial class TaskCompletionSourceWrapper(TaskCompletionSource source) : ITaskCompletionSource
{
    public TaskCompletionSource Source { get; } = source;

    public Task Task => Source.Task;

    public void SetCompleted() => Source.SetResult();

    public void SetException(string message) => Source.SetException(new Exception(message));
}
