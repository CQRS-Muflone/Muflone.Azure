using System;

namespace Muflone.Azure.Exceptions;

public class MufloneExceptionArgs : EventArgs
{
    public readonly string Message;
    public readonly string StackTrace;
    public readonly string Source;

    public MufloneExceptionArgs(Exception ex)
    {
        Message = ex.InnerException is not null
            ? ex.InnerException.Message
            : ex.Message;
        StackTrace = ex.StackTrace;
        Source = ex.Source;
    }
}