﻿namespace Gu.Units.Generator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using JetBrains.Annotations;

    [Serializable]
    public class PartConversion : IFactorConversion, INotifyPropertyChanged
    {
        private Unit unit;

        private string name;

        public PartConversion(string name, string symbol, double factor)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.Factor = factor;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == this.name)
                    return;
                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ToSi));
                this.OnPropertyChanged(nameof(this.FromSi));
                this.OnPropertyChanged(nameof(this.ParameterName));
            }
        }

        public string ParameterName => this.Name.ToParameterName();

        public string Symbol { get; }

        public double Factor { get; }

        public string ToSi => this.GetToSi();

        public string FromSi => this.GetFromSi();

        public string SymbolConversion => this.GetSymbolConversion();

        public Unit Unit => this.unit ?? (this.unit = this.GetUnit());

        public bool CanRoundtrip => this.CanRoundtrip();

        public static PartConversion Create(Unit unit, PowerPart c1)
        {
            var name = c1.FirstName;
            var symbol = c1.Symbol;
            var factor = c1.Factor;
            return new PartConversion(name, symbol, factor) { unit = unit }; // hacking unit like this for simpler serialization
        }

        public static PartConversion Create(Unit unit, PowerPart c1, PowerPart c2)
        {
            string name;
            if (c1.Power > 0 &&
                c2.Power > 0)
            {
                name = $"{c1.FirstName}{c2.LastName}";
            }
            else if (c1.Power > 0 && c2.Power < 0)
            {
                name = $"{c1.FirstName}Per{c2.LastName}";
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            var symbolAndPowers = c1.AsSymbolAndPowers().Concat(c2.AsSymbolAndPowers());
            var symbol = symbolAndPowers.AsSymbol();
            var factor = c1.Factor * c2.Factor;
            // hacking unit like this for simpler serialization
            return new PartConversion(name, symbol, factor) { unit = unit };
        }

        public static PowerPart CreatePart(int power, IFactorConversion conversion)
        {
            return new PowerPart(power, conversion);
        }

        public static PowerPart CreatePart(int power, Unit unit)
        {
            return new PowerPart(power, new IdentityConversion(unit));
        }

        public class PowerPart
        {
            public PowerPart(int power, IFactorConversion conversion)
            {
                this.Power = power;
                this.Conversion = conversion;
            }

            public int Power { get; }

            public IFactorConversion Conversion { get; }

            public string FirstName
            {
                get
                {
                    var isLength = IsLength(this.Conversion);
                    return CreateName(this.Power, this.Conversion.Name, isLength);
                }
            }

            public object LastName
            {
                get
                {
                    var trimmedS = this.Conversion.Name.TrimEnd('s');
                    var isLength = IsLength(this.Conversion);
                    return CreateName(this.Power, trimmedS, isLength);
                }
            }

            private static string CreateName(int power, string name, bool isLength)
            {
                name = name.TrimStart('@');
                power = Math.Abs(power);
                if (isLength)
                {
                    switch (power)
                    {
                        case 1:
                            return name;
                        case 2:
                            return $"Square{name}";
                        case 3:
                            return $"Cubic{name}";
                        default:
                            throw new ArgumentOutOfRangeException(nameof(power));
                    }
                }
                switch (power)
                {
                    case 1:
                        return name;
                    case 2:
                        return $"{name}Squared";
                    case 3:
                        return $"{name}Cubed";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(power));
                }
            }

            public string Symbol
            {
                get
                {
                    IReadOnlyList<SymbolAndPower> symbolAndPowers;
                    if (SymbolAndPowerReader.TryRead(this.Conversion.Symbol, out symbolAndPowers))
                    {
                        return symbolAndPowers.Select(x => new SymbolAndPower(x.Symbol, x.Power *this.Power)).AsSymbol();
                    }

                    return "Error";
                }
            }

            public double Factor => Math.Pow(this.Conversion.Factor, this.Power);

            internal IReadOnlyList<SymbolAndPower> AsSymbolAndPowers()
            {
                IReadOnlyList<SymbolAndPower> symbolAndPowers;
                if (SymbolAndPowerReader.TryRead(this.Conversion.Symbol, out symbolAndPowers))
                {
                    return symbolAndPowers.Select(x => new SymbolAndPower(x.Symbol, x.Power *this.Power)).ToList();
                }

                throw new InvalidOperationException();
            }

            private static bool IsLength(IConversion conversion)
            {
                return conversion.Unit.QuantityName == "Length";
            }
        }

        public class IdentityConversion : IFactorConversion
        {

            public IdentityConversion(Unit unit)
            {
                this.Unit = unit;
            }

            public string Name => this.Unit.Name;

            public string ParameterName => this.Name.ToParameterName();

            public string Symbol => this.Unit.Symbol;

            public double Factor => 1;

            public string ToSi => this.GetToSi();

            public string FromSi => this.GetFromSi();

            public Unit Unit { get; }

            public string SymbolConversion => this.GetSymbolConversion();

            public bool CanRoundtrip => true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}