﻿namespace Gu.Units.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    using Gu.Units.Generator.WpfStuff;

    [TypeConverter(typeof(UnitPartsConverter))]
    public class UnitParts : ObservableCollection<UnitAndPower>
    {
        public UnitParts(IEnumerable<UnitAndPower> parts)
            : base(parts)
        {
        }

        public UnitParts()
        {
            this.CollectionChanged += (sender, args) =>
                {
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Expression"));
                    if (args.NewItems != null)
                    {
                        foreach (var newItem in args.NewItems.OfType<INotifyPropertyChanged>())
                        {
                            newItem.PropertyChanged += this.OnPartPropertyChanged;
                        }
                    }
                    if (args.OldItems != null)
                    {
                        foreach (var oldItem in args.OldItems.OfType<INotifyPropertyChanged>())
                        {
                            oldItem.PropertyChanged -= this.OnPartPropertyChanged;
                        }
                    }
                };
        }

        public IEnumerable<UnitAndPower> Flattened
        {
            get
            {
                var all = new List<UnitAndPower>();
                foreach (var up in this)
                {
                    GetAll(up, 0, all);
                }
                var distinct = all.Select(x => x.Unit).Distinct().ToArray();
                foreach (SiUnit unit in distinct)
                {
                    var sum = all.Where(x => x.UnitName == unit.ClassName).Sum(x => x.Power);
                    if (sum != 0)
                    {
                        yield return new UnitAndPower(unit, sum);
                    }
                }
            }
        }

        [XmlIgnore]
        public string Expression
        {
            get
            {
                if (!this.Any())
                {
                    return "ERROR No Units";
                }
                var sb = new StringBuilder();
                UnitAndPower previous = null;
                foreach (var unitAndPower in this)
                {
                    if (previous != null)
                    {
                        if (previous.Power > 0 && unitAndPower.Power < 0)
                        {
                            sb.Append(" / ");
                        }
                        else
                        {
                            sb.Append("⋅");
                        }
                    }
                    else
                    {
                        if (unitAndPower.Power < 0)
                        {
                            sb.Append("1 / ");
                        }
                    }
                    sb.Append(unitAndPower.Unit == null ? unitAndPower.UnitName : unitAndPower.Unit.Symbol);
                    if (Math.Abs(unitAndPower.Power) > 1)
                    {
                        sb.Append("^")
                          .Append(Math.Abs(unitAndPower.Power));
                    }
                    previous = unitAndPower;
                }
                return sb.ToString();
            }
        }

        [XmlIgnore]
        public string UnitName
        {
            get
            {
                var builder = new StringBuilder();
                int sign = 1;
                foreach (var up in this)
                {
                    if (sign == 1 && up.Power < 0)
                    {
                        builder.Append("Per");
                        sign = -1;
                    }
                    var p = Math.Abs(up.Power);
                    switch (p)
                    {
                        case 1:
                            break;
                        case 2:
                            builder.Append("Square");
                            break;
                        case 3:
                            builder.Append("Cubic");
                            break;
                        default:
                            throw new NotImplementedException("message");
                    }
                    if (up.Power > 0)
                    {
                        builder.Append(up.UnitName);
                    }
                    else
                    {
                        builder.Append(up.UnitName.TrimEnd('s'));
                    }
                }
                return builder.ToString();
            }
        }

        public void Replace(UnitAndPower old, UnitAndPower @new)
        {
            var indexOf = base.IndexOf(old);
            base.RemoveAt(indexOf);
            base.Insert(indexOf, @new);
        }

        public override string ToString()
        {
            return this.Expression;
        }

        private void GetAll(UnitAndPower up, int power, List<UnitAndPower> list)
        {
            if (list.Count > 100)
            {
                Debugger.Break();
            }
            if (up.Unit is SiUnit)
            {
                list.Add(new UnitAndPower(up.Unit, up.Power + power));
                return;
            }
            var derivedUnit = (DerivedUnit)up.Unit;
            foreach (var unitPart in derivedUnit.Parts)
            {
                GetAll(unitPart, up.Power - 1, list);
            }
        }

        private void OnPartPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("Expression"));
        }
    }
}