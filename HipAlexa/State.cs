using System;
using System.Collections.Generic;
using Alexa.NET.Request;

namespace HipAlexa
{
    public partial class HipHandler
    {
        public class State
        {
            public int QuizId { get; }
            public int CorrectAnswers { get; }
            public int QuestionsPosed { get; }

            public State(int quizId, int correctAnswers = 0, int questionsPosed = 0)
            {
                QuizId = quizId;
                CorrectAnswers = correctAnswers;
                QuestionsPosed = questionsPosed;
            }

            public State Next(bool wasAnswerCorrect)
            {
                return new State(QuizId, CorrectAnswers + (wasAnswerCorrect ? 1 : 0), QuestionsPosed + 1);
            }

            public void WriteTo(Session session)
            {
                var sessionAttributes = session.Attributes ?? new Dictionary<string, object>();
                session.Attributes = sessionAttributes;
                sessionAttributes["QuestionsPosed"] = QuestionsPosed;
                sessionAttributes["QuizId"] = QuizId;
                sessionAttributes["CorrectAnswers"] = CorrectAnswers;
            }

            public static State From(Session session)
            {
                return new State(
                    Convert.ToInt32(session.Attributes["QuizId"]),
                    Convert.ToInt32(session.Attributes["CorrectAnswers"]),
                    Convert.ToInt32(session.Attributes["QuestionsPosed"])
                );
            }

            public static bool ContainedIn(Session session)
            {
                return session.Attributes != null && session.Attributes.ContainsKey("QuizId") &&
                       session.Attributes.ContainsKey("CorrectAnswers") &&
                       session.Attributes.ContainsKey("QuestionsPosed");
            }
        }
    }
}