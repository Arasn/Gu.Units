﻿namespace Gu.Units.Generator
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    [Serializable]
    public class PrefixConversion : IFactorConversion, INotifyPropertyChanged
    {
        private string name;
        private string symbol;
        [EditorBrowsable(EditorBrowsableState.Never)]
        private Unit unit;

        public PrefixConversion(string name, string symbol, string prefixName)
        {
            this.name = name;
            this.symbol = symbol;
            this.PrefixName = prefixName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return this.name; }
            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ToSi));
                this.OnPropertyChanged(nameof(this.FromSi));
                this.OnPropertyChanged(nameof(this.ParameterName));
            }
        }

        public string ParameterName => this.Name.ToParameterName();

        public string Symbol
        {
            get { return this.symbol; }
            set
            {
                if (value == this.symbol)
                {
                    return;
                }

                this.symbol = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SymbolConversion));
            }
        }

        public string PrefixName { get; }

        public Prefix Prefix => Settings.Instance.Prefixes.Single(x => x.Name == this.PrefixName);

        public double Factor
        {
            get
            {
                if (string.Equals($"{this.Prefix.Name}{this.Unit.Name}", this.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return Math.Pow(10, this.Prefix.Power);
                }

                var factorConversion = this.Unit.FactorConversions.SingleOrDefault(x => string.Equals($"{this.Prefix.Name}{x.Name}", this.Name, StringComparison.OrdinalIgnoreCase));
                if (factorConversion != null)
                {
                    return factorConversion.Factor * Math.Pow(10, this.Prefix.Power);
                }

                throw new ArgumentOutOfRangeException($"Could not calculate factor for {this.Name}");
            }
        }

        public string ToSi => this.GetToSi();

        public string FromSi => this.GetFromSi();

        public string SymbolConversion => this.GetSymbolConversion();

        public Unit Unit => this.unit ?? (this.unit = this.GetUnit());

        public bool CanRoundtrip => this.CanRoundtrip();

        public static PrefixConversion Create(Unit unit, Prefix prefix)
        {
            var prefixConversion = Create((INameAndSymbol)unit, prefix);
            prefixConversion.unit = unit;
            return prefixConversion;
        }

        public static PrefixConversion Create(FactorConversion factorConversion, Prefix prefix)
        {
            var prefixConversion = Create((INameAndSymbol)factorConversion, prefix);
            prefixConversion.unit = factorConversion.GetUnit();
            return prefixConversion;
        }

        private static PrefixConversion Create(INameAndSymbol nas, Prefix prefix)
        {
            return new PrefixConversion(prefix.Name + nas.ParameterName.TrimStart('@'), prefix.Symbol + nas.Symbol, prefix.Name);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
