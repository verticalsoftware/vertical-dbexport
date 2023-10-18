namespace Vertical.DbExport.Exceptions;

public class CriticalStopException : Exception
{
    public CriticalStopException(string? message = null, Exception? innerException = null)
        : base(message ?? "Export operation cancelled.", innerException)
    {
    }
}