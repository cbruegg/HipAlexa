using System;
using System.Linq;
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

        public static bool IsAnswerCorrect(this IStage stage, string answer)
        {
            var trimmed = answer.Trim();
            if (trimmed.Length != 1)
                return stage.CorrectAnswer.Trim().Equals(trimmed, StringComparison.InvariantCultureIgnoreCase);

            for (var i = 0; i < Alphabet.Length; i++)
            {
                var letter = Alphabet[i];
                if (char.ToUpperInvariant(trimmed[0]) == letter)
                {
                    return i < stage.Answers.Length && stage.Answers[i] == stage.CorrectAnswer;
                }
            }

            return false;
        }
    }
}