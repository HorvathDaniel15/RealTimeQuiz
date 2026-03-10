namespace RealTimeQuiz.Logic.Exceptions;

public class BusinessValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; set; }

    public BusinessValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }
    
    public BusinessValidationException(IEnumerable<string> errors)
        : base("One or more business validation errors occurred.")
    {
        Errors = errors
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .Distinct()
            .ToList();
    }
    
}