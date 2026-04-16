namespace RealTimeQuiz.Logic.Contracts.Auth.Requests;

public class RefreshRequest
{
    public string RefreshToken { get; set; }  = string.Empty;
}