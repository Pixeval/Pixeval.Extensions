// Copyright (c) Pixeval.Extensions.Common.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Pixeval.Extensions.Common.Internal;

[GeneratedComClass]
internal partial class TaskCompletionSourceAdaptor(TaskCompletionSource source) : ITaskCompletionSource
{
    public TaskCompletionSource Source { get; } = source;

    public Task Task => Source.Task;

    public void SetCompleted() => Source.SetResult();

    public void SetException(IException message) => Source.SetException(message.ToException());
}
