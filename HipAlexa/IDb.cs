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
        Task<IFact> RandomFact(string forTopic);

        Task<IFact> RandomFact();

        Task<IQuiz> RandomQuiz(string forTopic);

        /// <summary>
        /// May return null.
        /// </summary>
        /// <param name="forTopic"></param>
        /// <returns></returns>
        Task<IQuiz> RandomQuiz();
    }
}