namespace EstoqueService.Application.Exceptions;

public class SimulatedFailureException : Exception
{
    public SimulatedFailureException(string message) : base(message)
    {
    }
}
