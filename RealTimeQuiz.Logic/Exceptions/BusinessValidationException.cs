namespace RealTimeQuiz.Logic.Exceptions;

public class BusinessValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public BusinessValidationException(string message)
        : this(NormalizeMessage(message), true)
    {
    }
    
    public BusinessValidationException(IEnumerable<string>? errors)
        : base("One or more business validation errors occurred.")
    {
        var normalizedErrors = (errors ?? Enumerable.Empty<string>())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Errors = normalizedErrors.Length > 0
            ? normalizedErrors
            : new[] { "One or more business validation errors occurred." };
    }
    
    private BusinessValidationException(string normalizedMessage, bool _)
        : base(normalizedMessage)
    {
        Errors = new[] { normalizedMessage };
    }
    
    private static string NormalizeMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? "A business validation error occurred."
            : message.Trim();
    }
    
}