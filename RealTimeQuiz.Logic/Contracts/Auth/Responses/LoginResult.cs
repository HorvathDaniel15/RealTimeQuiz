namespace RealTimeQuiz.Logic.Contracts.Auth.Responses;

public class LoginResult
{
    public string AccessToken { get; set; }  = string.Empty;
    public string RefreshToken { get; set; }  = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
}