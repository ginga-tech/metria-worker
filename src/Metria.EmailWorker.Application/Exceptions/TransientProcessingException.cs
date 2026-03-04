namespace Metria.EmailWorker.Application.Exceptions;

public sealed class TransientProcessingException : Exception
{
    public TransientProcessingException(string message) : base(message)
    {
    }

    public TransientProcessingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
