namespace HipAlexa
{
    public class SimpleTopic : ITopic
    {
        public string[] Synonyms { get; }

        public SimpleTopic(string[] synonyms)
        {
            Synonyms = synonyms;
        }

        public override string ToString()
        {
            return $"{nameof(Synonyms)}: {Synonyms}";
        }

        protected bool Equals(SimpleTopic other)
        {
            return Equals(Synonyms, other.Synonyms);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleTopic) obj);
        }

        public override int GetHashCode()
        {
            return (Synonyms != null ? Synonyms.GetHashCode() : 0);
        }
    }
}