using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HipAlexa
{
    public class SimpleDb : IDb
    {
        private readonly IReadOnlyDictionary<ITopic, IReadOnlyList<IFact>> _factsByTopic;
        private readonly IReadOnlyList<IFact> _facts;

        private readonly IReadOnlyDictionary<ITopic, IReadOnlyList<IQuiz>> _quizzesByTopic;
        private readonly IReadOnlyList<IQuiz> _quizzes;

        private readonly Random _random = new Random();

        public SimpleDb()
        {
            var fact1 = new SimpleFact("Dies ist Fakt Nummer 1.");
            var fact2 = new SimpleFact("Dies ist Fakt Nummer 2.");
            var fact3 = new SimpleFact("Dies ist Fakt Nummer 3.");
            var historyTopic = new SimpleTopic(new[] {"Geschichte", "Historie"});
            var modernTopic = new SimpleTopic(new[] {"Moderne"});
            _facts = new[] {fact1, fact2, fact3};
            _factsByTopic = new Dictionary<ITopic, IReadOnlyList<IFact>>
            {
                {historyTopic, new[] {fact1, fact2}},
                {modernTopic, new[] {fact3}}
            };

            var quiz1Stages = new IStage[]
            {
                new SimpleStage("Wie heisst Paderborn?",
                    new[] {"Bielefeld", "Paderborn", "Berlin", "Hamburg"},
                    "Paderborn"),
                new SimpleStage("Hat Paderborn einen Dom?", new[] {"Ja", "Nein", "Vielleicht"}, "Ja"),
                new SimpleStage("Ja oder Nein?", new[] {"Ja", "Nein"}, "Ja")
            };
            var quiz2Stages = new IStage[]
            {
                new SimpleStage("Schere, Stein, Papier?", new[] {"Schere", "Stein", "Papier"}, "Stein"),
                new SimpleStage("Kopf oder Zahl?", new[] {"Kopf", "Zahl"}, "Zahl"),
                new SimpleStage("Waehle eine Antwort.", new[] {"Die Nuss liegt in der Mitte."},
                    "Die Nuss liegt in der Mitte.")
            };

            var quiz1 = new SimpleQuiz(quiz1Stages);
            var quiz2 = new SimpleQuiz(quiz2Stages);
            _quizzes = new[] {quiz1, quiz2};
            _quizzesByTopic = new Dictionary<ITopic, IReadOnlyList<IQuiz>>
            {
                {historyTopic, new[] {quiz1}},
                {modernTopic, new[] {quiz2}}
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

        public async Task<IQuiz> RandomQuiz(string forTopic)
        {
            var quizzesForTopic = _quizzesByTopic
                .Where(it => it.Key.Synonyms.Contains(forTopic))
                .SelectMany(it => it.Value)
                .ToList();

            return quizzesForTopic.Count > 0 ? null : _quizzes[_random.Next(_facts.Count)];
        }

        public async Task<IQuiz> RandomQuiz() => _quizzes[_random.Next(_quizzes.Count)];
    }
}