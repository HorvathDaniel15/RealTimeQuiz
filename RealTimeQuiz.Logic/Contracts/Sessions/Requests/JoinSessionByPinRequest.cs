namespace RealTimeQuiz.Logic.Contracts.Sessions.Requests;

public class JoinSessionByPinRequest
{
    public string JoinPin { get; set; } =  string.Empty;
    public string DisplayName { get; set; }  = string.Empty;
}