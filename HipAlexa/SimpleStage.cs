namespace HipAlexa
{
    public class SimpleStage : IStage
    {
        public string Question { get; }
        public string[] Answers { get; }
        public string CorrectAnswer { get; }

        public SimpleStage(string question, string[] answers, string correctAnswer)
        {
            Question = question;
            Answers = answers;
            CorrectAnswer = correctAnswer;
        }

        protected bool Equals(SimpleStage other)
        {
            return string.Equals(Question, other.Question) && Equals(Answers, other.Answers) &&
                   Equals(CorrectAnswer, other.CorrectAnswer);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleStage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Question != null ? Question.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Answers != null ? Answers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CorrectAnswer != null ? CorrectAnswer.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(Question)}: {Question}, {nameof(Answers)}: {Answers}, {nameof(CorrectAnswer)}: {CorrectAnswer}";
        }
    }
}