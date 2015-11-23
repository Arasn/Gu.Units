﻿namespace Gu.Units
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal static class Parser
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyList<Symbol>> SymbolCache = new ConcurrentDictionary<Type, IReadOnlyList<Symbol>>();

        internal static TQuantity Parse<TUnit, TQuantity>(string s,
            Func<double, TUnit, TQuantity> creator,
            NumberStyles style,
            IFormatProvider provider)
            where TUnit : IUnit
            where TQuantity : IQuantity
        {
            if (provider == null)
            {
                provider = NumberFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            }
            try
            {
                int end;
                var d = DoubleParser.Parse(s, 0, style, provider, out end);
                var us = s.Substring(end, s.Length - end);
                var unit = ParseUnit<TUnit>(us);
                return creator(d, unit);
            }
            catch (Exception e)
            {
                throw new FormatException("Could not parse the unit value from: " + s, e);
            }
        }

        internal static bool TryParse<TUnit, TQuantity>(string s,
            Func<double, TUnit, TQuantity> creator,
            NumberStyles style,
            IFormatProvider provider,
            out TQuantity value)
        {
            if (provider == null)
            {
                provider = NumberFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            }
            int end;
            double d;
            if (!DoubleParser.TryParse(s, 0, style, provider, out d, out end))
            {
                value = default(TQuantity);
                return false;
            }
            var us = s.Substring(end, s.Length - end);
            TUnit unit;
            if (!TryParseUnit(us, out unit))
            {
                value = default(TQuantity);
                return false;
            }
            value = creator(d, unit);
            return true;
        }

        internal static TUnit ParseUnit<TUnit>(string s)
            where TUnit : IUnit
        {
            var type = typeof(TUnit);
            var symbols = SymbolCache.GetOrAdd(type, CreateSymbolsForType);
            var matches = symbols.Where(x => x.IsMatch(s)).ToArray();
            if (matches.Length == 0)
            {
                var message = string.Format("Could not parse: '{0}' to {1}", s, typeof(TUnit).Name);
                throw new FormatException(message);
            }

            if (matches.Length > 1)
            {
                var patterns = string.Join(
                    Environment.NewLine,
                    matches.Select(x => string.Format("Unit: {0} with pattern: {1}", x.Unit.Symbol, x.Tokens)));
                var message = string.Format(
                    @"Could not parse: '{0}' to {1}{2}The following matches:{2}{3}",
                    s,
                    typeof(TUnit).Name,
                    Environment.NewLine,
                    patterns);
                throw new FormatException(message);
            }
            return (TUnit)matches[0].Unit;
        }

        internal static bool TryParseUnit<TUnit>(string text, out TUnit value)
        {
            var type = typeof(TUnit);
            var symbols = SymbolCache.GetOrAdd(type, CreateSymbolsForType);
            var matches = symbols.Where(x => x.IsMatch(text)).ToArray();
            if (matches.Length != 1)
            {
                value = default(TUnit);
                return false;
            }
            value = (TUnit)matches[0].Unit;
            return true;
        }

        internal static IReadOnlyList<SymbolAndPower> TokenizeUnit(string s)
        {
            int pos = 0;
            var sign = Sign.Positive;
            var tokens = new List<SymbolAndPower>();
            while (SymbolAndPower.CanRead(s, ref pos))
            {
                var sap = SymbolAndPower.Read(s, ref pos, ref sign);
                if (tokens.Any(t => t.Symbol == sap.Symbol))
                {
                    var message =
                        string.Format(
                            "Cannot contain the same symbol more than once {0} contains {1} more than one time", s,
                            sap.Symbol);
                    throw new FormatException(message);
                }
                tokens.Add(sap);
            }
            return tokens;
        }

        internal static bool TryTokenizeUnit(string s, out IReadOnlyList<SymbolAndPower> tokens)
        {
            int pos = 0;
            var sign = Sign.Positive;
            var temp = new List<SymbolAndPower>();
            while (SymbolAndPower.CanRead(s, ref pos))
            {
                SymbolAndPower sap;
                if (!SymbolAndPower.TryRead(s, ref pos, ref sign, out sap))
                {
                    tokens = null;
                    return false;
                }
                if (temp.Any(t => t.Symbol == sap.Symbol))
                {
                    tokens = null;
                    return false;
                }
                temp.Add(sap);
            }
            tokens = temp;
            return true;
        }

        private static IReadOnlyList<Symbol> CreateSymbolsForType(Type type)
        {
            var symbols = type.GetUnitsForType()
                              .Select(x => new Symbol(x))
                              .ToArray();
            return symbols;
        }

        private static IEnumerable<IUnit> GetUnitsForType(this Type t)
        {
            var units = t.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static)
                         .Where(f => typeof(IUnit).IsAssignableFrom(f.FieldType))
                         .Select(f => (IUnit)f.GetValue(null))
                         .Distinct()
                         .ToArray();
            return units;
        }

        internal class Symbol
        {
            public Symbol(IUnit unit)
            {
                Unit = unit;
                Tokens = new HashSet<SymbolAndPower>(TokenizeUnit(unit.Symbol));
            }

            public HashSet<SymbolAndPower> Tokens { get; private set; }

            public IUnit Unit { get; private set; }

            public bool IsMatch(string s)
            {
                var saps = TokenizeUnit(s);
                return Tokens.SetEquals(saps);
            }

            public override string ToString()
            {
                return String.Format("Unit: {0}, Pattern: {1}", this.Unit, string.Join(", ", this.Tokens));
            }
        }
    }
}