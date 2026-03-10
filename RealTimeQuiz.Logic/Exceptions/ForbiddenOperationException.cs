namespace RealTimeQuiz.Logic.Exceptions;

public class ForbiddenOperationException : Exception
{
    public ForbiddenOperationException(string message)
        : base(message)
    {
        
    }
}