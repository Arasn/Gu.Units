﻿namespace Gu.Units
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// A type for the unit <see cref="Gu.Units.AnglePerUnitless"/>.
	/// Contains logic for conversion and formatting.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(AnglePerUnitlessUnitTypeConverter))]
    public struct AnglePerUnitlessUnit : IUnit, IUnit<AnglePerUnitless>, IEquatable<AnglePerUnitlessUnit>
    {
        /// <summary>
        /// The AnglePerUnitlessUnit unit
        /// Contains logic for conversion and formatting.
        /// </summary>
        public static readonly AnglePerUnitlessUnit RadiansPerUnitless = new AnglePerUnitlessUnit(radiansPerUnitless => radiansPerUnitless, radiansPerUnitless => radiansPerUnitless, "rad/ul");

        /// <summary>
        /// The DegreesPerPercent unit
        /// Contains conversion logic to from and formatting.
        /// </summary>
        public static readonly AnglePerUnitlessUnit DegreesPerPercent = new AnglePerUnitlessUnit(degreesPerPercent => 1.74532925199433 * degreesPerPercent, radiansPerUnitless => radiansPerUnitless / 1.74532925199433, "°/%");

        /// <summary>
        /// The RadiansPerPercent unit
        /// Contains conversion logic to from and formatting.
        /// </summary>
        public static readonly AnglePerUnitlessUnit RadiansPerPercent = new AnglePerUnitlessUnit(radiansPerPercent => 100 * radiansPerPercent, radiansPerUnitless => radiansPerUnitless / 100, "rad/%");

        private readonly Func<double, double> toRadiansPerUnitless;
        private readonly Func<double, double> fromRadiansPerUnitless;
        internal readonly string symbol;

        public AnglePerUnitlessUnit(Func<double, double> toRadiansPerUnitless, Func<double, double> fromRadiansPerUnitless, string symbol)
        {
            this.toRadiansPerUnitless = toRadiansPerUnitless;
            this.fromRadiansPerUnitless = fromRadiansPerUnitless;
            this.symbol = symbol;
        }

        /// <summary>
        /// The symbol for the <see cref="Gu.Units.AnglePerUnitlessUnit"/>.
        /// </summary>
        public string Symbol => this.symbol;

        /// <summary>
        /// The default unit for <see cref="Gu.Units.AnglePerUnitlessUnit"/>
        /// </summary>
        public AnglePerUnitlessUnit SiUnit => RadiansPerUnitless;

        /// <summary>
        /// The default <see cref="Gu.Units.IUnit"/> for <see cref="Gu.Units.AnglePerUnitlessUnit"/>
        /// </summary>
        IUnit IUnit.SiUnit => RadiansPerUnitless;

        public static AnglePerUnitless operator *(double left, AnglePerUnitlessUnit right)
        {
            return AnglePerUnitless.From(left, right);
        }

        public static bool operator ==(AnglePerUnitlessUnit left, AnglePerUnitlessUnit right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnglePerUnitlessUnit left, AnglePerUnitlessUnit right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Constructs a <see cref="AnglePerUnitlessUnit"/> from a string.
        /// Leading and trailing whitespace characters are allowed.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>An instance of <see cref="AnglePerUnitlessUnit"/></returns>
        public static AnglePerUnitlessUnit Parse(string text)
        {
            return UnitParser<AnglePerUnitlessUnit>.Parse(text);
        }

        public static bool TryParse(string text, out AnglePerUnitlessUnit value)
        {
            return UnitParser<AnglePerUnitlessUnit>.TryParse(text, out value);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to RadiansPerUnitless.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The converted value</returns>
        public double ToSiUnit(double value)
        {
            return this.toRadiansPerUnitless(value);
        }

        /// <summary>
        /// Converts a value from radiansPerUnitless.
        /// </summary>
        /// <param name="radiansPerUnitless">The value in RadiansPerUnitless</param>
        /// <returns>The converted value</returns>
        public double FromSiUnit(double radiansPerUnitless)
        {
            return this.fromRadiansPerUnitless(radiansPerUnitless);
        }

        /// <summary>
        /// Creates a quantity with this unit
        /// </summary>
        /// <param name="value">The scalar value"</param>
        /// <returns>new AnglePerUnitless(<paramref name="value"/>, this)</returns>
        public AnglePerUnitless CreateQuantity(double value)
        {
            return new AnglePerUnitless(value, this);
        }

        /// <summary>
        /// Gets the scalar value of <paramref name="quantity"/> in RadiansPerUnitless
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public double GetScalarValue(AnglePerUnitless quantity)
        {
            return FromSiUnit(quantity.radiansPerUnitless);
        }

        public override string ToString()
        {
            return this.symbol;
        }

        public string ToString(string format)
        {
            AnglePerUnitlessUnit unit;
            var paddedFormat = UnitFormatCache<AnglePerUnitlessUnit>.GetOrCreate(format, out unit);
            if (unit != this)
            {
                return format;
            }

            using (var builder = StringBuilderPool.Borrow())
            {
                builder.Append(paddedFormat.PrePadding);
                builder.Append(paddedFormat.Format);
                builder.Append(paddedFormat.PostPadding);
                return builder.ToString();
            }
        }

        public string ToString(SymbolFormat format)
        {
            var paddedFormat = UnitFormatCache<AnglePerUnitlessUnit>.GetOrCreate(this, format);
            using (var builder = StringBuilderPool.Borrow())
            {
                builder.Append(paddedFormat.PrePadding);
                builder.Append(paddedFormat.Format);
                builder.Append(paddedFormat.PostPadding);
                return builder.ToString();
            }
        }

        public bool Equals(AnglePerUnitlessUnit other)
        {
            return this.symbol == other.symbol;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is AnglePerUnitlessUnit && Equals((AnglePerUnitlessUnit)obj);
        }

        /// <summary>
        /// Returns the hashcode for this <see cref="LengthUnit"/>
        /// </summary>
        /// <returns></returns>
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