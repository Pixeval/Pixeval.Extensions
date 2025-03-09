using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Extensions.Common;

/// <summary>
/// 
/// </summary>
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("9EFDC07A-8261-4CE9-BD48-5CFF1A803528")]
public partial interface IProgressNotifier
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress">100-based</param>
    void ProgressChanged(double progress);

    void Completed();

    void Aborted(string exceptionMessage, string? stackTrace);
}
