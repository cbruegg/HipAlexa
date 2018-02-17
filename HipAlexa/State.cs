using System;
using System.Collections.Generic;
using Alexa.NET.Request;
using HipAlexa.Model;

namespace HipAlexa
{
    public class State
    {
        public int QuizId { get; }
        public int CorrectAnswers { get; }
        public int QuestionsAsked { get; }

        public State(int quizId, int correctAnswers = 0, int questionsAsked = 0)
        {
            QuizId = quizId;
            CorrectAnswers = correctAnswers;
            QuestionsAsked = questionsAsked;
        }

        public State Next(IStageExtensions.AnswerResult wasAnswerCorrect)
        {
            switch (wasAnswerCorrect)
            {
                case IStageExtensions.AnswerResult.Wrong:
                    return new State(QuizId, CorrectAnswers, QuestionsAsked + 1);
                case IStageExtensions.AnswerResult.Correct:
                    return new State(QuizId, CorrectAnswers + 1, QuestionsAsked + 1);
                case IStageExtensions.AnswerResult.UnknownAnswer:
                    return this;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wasAnswerCorrect), wasAnswerCorrect, null);
            }
        }

        public void WriteTo(Session session)
        {
            var sessionAttributes = session.Attributes ?? new Dictionary<string, object>();
            session.Attributes = sessionAttributes;
            sessionAttributes["QuestionsAsked"] = QuestionsAsked;
            sessionAttributes["QuizId"] = QuizId;
            sessionAttributes["CorrectAnswers"] = CorrectAnswers;
        }

        public static State From(Session session)
        {
            return new State(
                Convert.ToInt32(session.Attributes["QuizId"]),
                Convert.ToInt32(session.Attributes["CorrectAnswers"]),
                Convert.ToInt32(session.Attributes["QuestionsAsked"])
            );
        }

        public static bool ContainedIn(Session session)
        {
            return session.Attributes != null && session.Attributes.ContainsKey("QuizId") &&
                   session.Attributes.ContainsKey("CorrectAnswers") &&
                   session.Attributes.ContainsKey("QuestionsAsked");
        }
    }
}