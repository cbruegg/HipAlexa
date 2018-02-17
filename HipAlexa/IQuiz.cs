namespace HipAlexa
{
    public interface IQuiz
    {
        int Id { get; }
        IStage[] Stages { get; }
    }
}