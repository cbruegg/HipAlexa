using System.Threading.Tasks;
using HipAlexa.Model;

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

        /// <summary>
        /// May return null.
        /// </summary>
        /// <param name="forTopic"></param>
        /// <returns></returns>
        Task<IQuiz> RandomQuizAsync(string forTopic);

        Task<IQuiz> RandomQuizAsync();

        Task<IQuiz> QuizByIdAsync(int id);
    }
}