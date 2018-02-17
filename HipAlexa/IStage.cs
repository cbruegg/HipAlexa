using System;
using System.Text;

namespace HipAlexa
{
    public interface IStage
    {
        string Question { get; }
        string[] Answers { get; }
        string CorrectAnswer { get; }
    }

    // ReSharper disable once InconsistentNaming
    public static class IStageExtensions
    {
        private static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static string PosedQuestionSsml(this IStage stage, bool wrapInSpeakTags = true)
        {
            var sb = new StringBuilder();
            if (wrapInSpeakTags) sb.Append("<speak>");
            sb.Append("<p>");
            sb.Append(stage.Question);
            sb.Append("</p>");
            for (var i = 0; i < stage.Answers.Length; i++)
            {
                sb.Append(Alphabet[i]);
                sb.Append("<break time=\"800ms\"/>");
                sb.AppendLine(stage.Answers[i]);
                sb.Append("<break time=\"800ms\"/>");
            }

            if (wrapInSpeakTags) sb.Append("</speak>");

            return sb.ToString();
        }

        private static string RemoveSuffix(this string str, string suffix) =>
            str.EndsWith(suffix) ? str.Substring(0, str.Length - suffix.Length) : str;

        public enum AnswerResult
        {
            Wrong,
            Correct,
            UnknownAnswer
        }

        public static string Spoken(this AnswerResult result)
        {
            switch (result)
            {
                case AnswerResult.Wrong:
                    return "<p>Das war leider falsch</p>";
                case AnswerResult.Correct:
                    return "<p>Das war richtig!</p>";
                case AnswerResult.UnknownAnswer:
                    return "<p>Entschuldigung, das habe ich leider nicht verstanden</p>";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static AnswerResult IsAnswerCorrect(this IStage stage, string answer)
        {
            var trimmed = answer.Trim().RemoveSuffix(".");
            if (trimmed.Length != 1)
                return AnswerResult.UnknownAnswer;

            for (var i = 0; i < Math.Min(Alphabet.Length, stage.Answers.Length); i++)
            {
                if (char.ToUpperInvariant(trimmed[0]) == Alphabet[i])
                {
                    return stage.Answers[i] == stage.CorrectAnswer
                        ? AnswerResult.Correct
                        : AnswerResult.Wrong;
                }
            }

            return AnswerResult.UnknownAnswer;
        }
    }
}