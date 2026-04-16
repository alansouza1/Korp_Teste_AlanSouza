namespace FaturamentoService.Application.Exceptions;

public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message)
    {
    }
}
