namespace Metria.EmailWorker.Application.Exceptions;

public sealed class PermanentProcessingException : Exception
{
    public PermanentProcessingException(string message) : base(message)
    {
    }

    public PermanentProcessingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
