namespace HipAlexa.Model
{
    public interface IQuiz
    {
        int Id { get; }
        IStage[] Stages { get; }
    }
}