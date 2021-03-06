﻿namespace Gu.Units.Generator
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    /// <summary>
    /// http://physics.nist.gov/cuu/Units/prefixes.html
    /// </summary>
    [DebuggerDisplay("Prefix{Name} ({Symbol}) 1E{Power}")]
    [Serializable]
    public class Prefix : INotifyPropertyChanged
    {
        private string name;
        private string symbol;
        private int power;

        private Prefix()
        {
        }

        public Prefix(string name, string symbol, int power)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.Power = power;
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
            }
        }

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
            }
        }

        public int Power
        {
            get { return this.power; }
            set
            {
                if (value == this.power)
                {
                    return;
                }

                this.power = value;
                this.OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
