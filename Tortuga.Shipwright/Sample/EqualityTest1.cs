using Tortuga.Shipwright;

namespace Sample
{
    internal partial class EqualityTest1
    {

        public int Age { get; set; }

        [EqualityKey]
        public int? YearsEmployed { get; set; }

        [EqualityKey]
        public DateTime BirthDay { get; set; }

        [EqualityKey]
        public string? FirstName { get; set; }

        [EqualityKey]
        public string? LastName { get; set; }

    }

    internal partial class EqualityTestExpected : IEquatable<EqualityTestExpected>
    {

        public int Age { get; set; }

        public int? YearsEmployed { get; set; }

        public DateTime BirthDay { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }



        public override bool Equals(object? obj)
        {
            var other = obj as EqualityTestExpected;
            if (other == null)
                return false;
            return Equals(other);
        }

        public bool Equals(EqualityTestExpected? other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other)) return true;

            if (!BirthDay.Equals(other.BirthDay)) return false;

            if (YearsEmployed == null)
            {
                if (other.YearsEmployed != null)
                    return false;
            }
            else if (!YearsEmployed.Equals(other.YearsEmployed))
                return false;


            if (FirstName == null)
            {
                if (other.FirstName != null)
                    return false;
            }
            else if (!FirstName.Equals(other.FirstName))
                return false;

            if (LastName == null)
            {
                if (other.LastName != null)
                    return false;
            }
            else if (!LastName.Equals(other.LastName))
                return false;


            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BirthDay, YearsEmployed, FirstName, LastName);
        }

    }

}
