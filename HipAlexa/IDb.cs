using System.Threading.Tasks;

namespace HipAlexa
{
    public interface IDb
    {
        /// <summary>
        /// May return null.
        /// </summary>
        /// <param name="forTopic"></param>
        /// <returns></returns>
        Task<IFact> RandomFactAsync(string forTopic);

        Task<IFact> RandomFactAsync();

        Task<IQuiz> RandomQuizAsync(string forTopic);

        /// <summary>
        /// May return null.
        /// </summary>
        /// <param name="forTopic"></param>
        /// <returns></returns>
        Task<IQuiz> RandomQuizAsync();

        Task<IQuiz> QuizByIdAsync(int id);
    }
}