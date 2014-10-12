﻿namespace Gu.Units.Generator
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;

    public class ViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<MetaDataViewModel> _unitData = new ObservableCollection<MetaDataViewModel>();
        private readonly ObservableCollection<Prefix> _prefixes = new ObservableCollection<Prefix>();

        private string _nameSpace;

        public ViewModel()
        {
            var settings = Settings.Instance;
            if (settings != null)
            {
                foreach (var unit in settings.ValueTypes)
                {
                    this.UnitData.Add(new MetaDataViewModel(unit, unit.SiUnit) { IsSiUnit = true });
                    foreach (var u in unit.Units)
                    {
                        this.UnitData.Add(new MetaDataViewModel(unit, u));
                    }
                    var emptyUnit = new MetaDataViewModel(unit, UnitMetaData.Empty);
                    emptyUnit.PropertyChanged += this.EmptyOnPropertyChanged;
                    this.UnitData.Add(emptyUnit);
                }
                foreach (var prefix in settings.Prefixes)
                {
                    Prefixes.Add(prefix);
                }
            }
            var empty = new MetaDataViewModel(ValueMetaData.Empty, UnitMetaData.Empty);
            empty.PropertyChanged += EmptyOnPropertyChanged;
            this.UnitData.Add(empty);
            NameSpace = Settings.ProjectName;
        }

        private void EmptyOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var i = UnitData.IndexOf((MetaDataViewModel)sender);
            var empty = new MetaDataViewModel(ValueMetaData.Empty, UnitMetaData.Empty);
            empty.PropertyChanged += EmptyOnPropertyChanged;
            UnitData.Insert(i + 1, empty);
            ((INotifyPropertyChanged)sender).PropertyChanged -= this.EmptyOnPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MetaDataViewModel> UnitData
        {
            get
            {
                return this._unitData;
            }
        }

        public ObservableCollection<Prefix> Prefixes
        {
            get { return _prefixes; }
        }

        public string NameSpace
        {
            get
            {
                return this._nameSpace;
            }
            set
            {
                if (value == this._nameSpace)
                {
                    return;
                }
                this._nameSpace = value;
                this.OnPropertyChanged();
            }
        }

        public static void Save(IEnumerable<MetaDataViewModel> data, string nameSpace, IEnumerable<Prefix> prefixes)
        {
            var settings = new Settings();
            var nonEmpty = data.Where(x => x != null && x.UnitMetaData != null && !x.UnitMetaData.IsEmpty && !string.IsNullOrEmpty(x.ValueTypeName))
                                   .ToArray();
            var valueNames = nonEmpty.Select(x => x.ValueTypeName)
                                     .Distinct()
                                     .ToArray();
            foreach (var valueName in valueNames)
            {
                var values = nonEmpty.Where(x => x.ValueTypeName == valueName)
                                             .ToArray();
                var units = values.Where(x => !x.IsSiUnit).Select(x => x.UnitMetaData).ToArray();
                foreach (var unit in units)
                {
                    unit.ValueType = new TypeMetaData(valueName);
                    unit.Namespace = nameSpace;
                }
                var siUnit = values.Single(x => x.IsSiUnit).UnitMetaData;
                siUnit.ValueType = new TypeMetaData(valueName);
                siUnit.Namespace = nameSpace;
                var unitValueMetaData = new ValueMetaData(siUnit, nameSpace, valueName, units);
                settings.ValueTypes.Add(unitValueMetaData);
            }
            settings.Prefixes.AddRange(prefixes);
            Settings.Save(settings, Settings.FullFileName);
        }

        public void Save()
        {
            ViewModel.Save(UnitData, NameSpace, Prefixes);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
