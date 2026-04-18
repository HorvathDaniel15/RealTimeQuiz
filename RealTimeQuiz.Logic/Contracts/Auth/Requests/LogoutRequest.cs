namespace RealTimeQuiz.Logic.Contracts.Auth.Requests;

public class LogoutRequest
{
    public string? RefreshToken { get; set; }
    public bool LogoutAllDevices { get; set; }
}