namespace HipAlexa
{
    public class SimpleQuiz : IQuiz
    {
        public IStage[] Stages { get; }

        public SimpleQuiz(IStage[] stages)
        {
            Stages = stages;
        }

        protected bool Equals(SimpleQuiz other)
        {
            return Equals(Stages, other.Stages);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleQuiz) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Stages != null ? Stages.GetHashCode() : 0) * 397);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Stages)}: {Stages}";
        }
    }
}