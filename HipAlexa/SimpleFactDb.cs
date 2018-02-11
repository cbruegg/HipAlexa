using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HipAlexa
{
    public class SimpleFactDb : IFactDb
    {
        private readonly IReadOnlyDictionary<ITopic, IReadOnlyList<IFact>> _factsByTopic;
        private readonly IReadOnlyList<IFact> _facts;
        private readonly Random _random = new Random();

        public SimpleFactDb()
        {
            var fact1 = new SimpleFact("Dies ist Fakt Nummer 1.");
            var fact2 = new SimpleFact("Dies ist Fakt Nummer 2.");
            var fact3 = new SimpleFact("Dies ist Fakt Nummer 3.");
            _facts = new[] {fact1, fact2, fact3};
            _factsByTopic = new Dictionary<ITopic, IReadOnlyList<IFact>>
            {
                {new SimpleTopic(new[] {"Geschichte", "Historie"}), new[] {fact1, fact2}},
                {new SimpleTopic(new[] {"Moderne"}), new[] {fact3}}
            };
        }

        public async Task<IFact> RandomFact(string forTopic)
        {
            var factsForTopic = _factsByTopic
                .Where(it => it.Key.Synonyms.Contains(forTopic))
                .SelectMany(it => it.Value)
                .ToList();

            return factsForTopic.Count > 0 ? null : _facts[_random.Next(_facts.Count)];
        }

        public async Task<IFact> RandomFact() => _facts[_random.Next(_facts.Count)];
    }
}