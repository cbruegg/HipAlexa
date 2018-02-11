namespace HipAlexa
{
    public interface IStage
    {
        string Question { get; }
        string[] Answers { get; }
        string CorrectAnswer { get; }
    }
}