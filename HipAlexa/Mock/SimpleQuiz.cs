using HipAlexa.Model;

namespace HipAlexa.Mock
{
    public class SimpleQuiz : IQuiz
    {
        public IStage[] Stages { get; }
        public int Id { get; }

        public SimpleQuiz(IStage[] stages, int id)
        {
            Stages = stages;
            Id = id;
        }

        protected bool Equals(SimpleQuiz other)
        {
            return Equals(Stages, other.Stages) && Id == other.Id;
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
                return ((Stages != null ? Stages.GetHashCode() : 0) * 397) ^ Id;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Stages)}: {Stages}, {nameof(Id)}: {Id}";
        }
    }
}