using System;
using System.Diagnostics;

namespace RaspberryPi
{
    /// <summary>
    ///     Representation of an ISO3166-1 Country
    /// </summary>
    [DebuggerDisplay("Country: {this.Name}")]
    public class Country : IEquatable<Country>
    {
        internal Country()
        { }

        public Country(string name, string alpha2, string alpha3, int numericCode)
        {
            this.Name = name;
            this.Alpha2 = alpha2;
            this.Alpha3 = alpha3;
            this.NumericCode = numericCode;
        }

        public string Name { get; internal set; }

        public string Alpha2 { get; internal set; }

        public string Alpha3 { get; internal set; }

        public int NumericCode { get; internal set; }

        public bool Equals(Country other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(this.Alpha2, other.Alpha2) && string.Equals(this.Alpha3, other.Alpha3) && this.NumericCode == other.NumericCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Country)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Alpha2 != null ? this.Alpha2.GetHashCode() : 0;
                hashCode = hashCode * 397 ^ (this.Alpha3 != null ? this.Alpha3.GetHashCode() : 0);
                hashCode = hashCode * 397 ^ this.NumericCode;
                return hashCode;
            }
        }

        public static bool operator ==(Country left, Country right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Country left, Country right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
