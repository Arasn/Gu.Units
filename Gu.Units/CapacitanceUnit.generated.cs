﻿namespace Gu.Units
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// A type for the unit <see cref="Gu.Units.CapacitanceUnit"/>.
	/// Contains conversion logic.
    /// </summary>
    [Serializable, TypeConverter(typeof(CapacitanceUnitTypeConverter)), DebuggerDisplay("1{symbol} == {ToSiUnit(1)}{Farads.symbol}")]
    public struct CapacitanceUnit : IUnit, IUnit<Capacitance>, IEquatable<CapacitanceUnit>
    {
        /// <summary>
        /// The Farads unit
        /// Contains conversion logic to from and formatting.
        /// </summary>
        public static readonly CapacitanceUnit Farads = new CapacitanceUnit(1.0, "F");

        /// <summary>
        /// The Farads unit
        /// Contains conversion logic to from and formatting.
        /// </summary>
		public static readonly CapacitanceUnit F = Farads;

        private readonly double conversionFactor;
        private readonly string symbol;

        public CapacitanceUnit(double conversionFactor, string symbol)
        {
            this.conversionFactor = conversionFactor;
            this.symbol = symbol;
        }

        /// <summary>
        /// The symbol for the <see cref="Gu.Units.CapacitanceUnit"/>.
        /// </summary>
        public string Symbol
        {
            get
            {
                return this.symbol;
            }
        }

        public static Capacitance operator *(double left, CapacitanceUnit right)
        {
            return Capacitance.From(left, right);
        }

        public static bool operator ==(CapacitanceUnit left, CapacitanceUnit right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CapacitanceUnit left, CapacitanceUnit right)
        {
            return !left.Equals(right);
        }

        public static CapacitanceUnit Parse(string text)
        {
            return Parser.ParseUnit<CapacitanceUnit>(text);
        }

        public static bool TryParse(string text, out CapacitanceUnit value)
        {
            return Parser.TryParseUnit<CapacitanceUnit>(text, out value);
        }

        /// <summary>
        /// Converts <see <paramref name="value"/> to Farads.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The converted value</returns>
        public double ToSiUnit(double value)
        {
            return this.conversionFactor * value;
        }

        /// <summary>
        /// Converts a value from Farads.
        /// </summary>
        /// <param name="value">The value in Farads</param>
        /// <returns>The converted value</returns>
        public double FromSiUnit(double value)
        {
            return value / this.conversionFactor;
        }

        /// <summary>
        /// Creates a quantity with this unit
        /// </summary>
        /// <param name="value"></param>
        /// <returns>new TTQuantity(value, this)</returns>
        public Capacitance CreateQuantity(double value)
        {
            return new Capacitance(value, this);
        }

        /// <summary>
        /// Gets the scalar value
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public double GetScalarValue(Capacitance quantity)
        {
            return FromSiUnit(quantity.farads);
        }

        public override string ToString()
        {
            return this.symbol;
        }

        public bool Equals(CapacitanceUnit other)
        {
            return this.symbol == other.symbol;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is CapacitanceUnit && Equals((CapacitanceUnit)obj);
        }

        public override int GetHashCode()
        {
            if (this.symbol == null)
            {
                return 0; // Needed due to default ctor
            }

            return this.symbol.GetHashCode();
        }
    }
}