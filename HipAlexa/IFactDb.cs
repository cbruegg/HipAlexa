using System.Threading.Tasks;

namespace HipAlexa
{
    public interface IFactDb
    {
        /// <summary>
        /// May return null.
        /// </summary>
        /// <param name="forTopic"></param>
        /// <returns></returns>
        Task<IFact> RandomFact(string forTopic);

        Task<IFact> RandomFact();
    }
}