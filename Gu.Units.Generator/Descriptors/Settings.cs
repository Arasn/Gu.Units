﻿namespace Gu.Units.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;

    using Gu.Units.Generator.WpfStuff;

    public class Settings
    {
        private readonly List<DerivedUnit> _derivedUnits = new List<DerivedUnit>();
        private readonly List<SiUnit> _siUnits = new List<SiUnit>();
        private readonly List<Prefix> _prefixes = new List<Prefix>();
        private readonly List<Quantity> _quantities = new List<Quantity>();
        public static Settings Instance
        {
            get
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    Settings settings;
                    using (var reader = new StringReader(Properties.Resources.GeneratorSettings))
                    {
                        settings = (Settings)serializer.Deserialize(reader);
                    }
                    settings.Initialize();
                    return settings;
                }
                catch (Exception e)
                {
                    return new Settings();
                }
            }
        }

        public static string NameSpace
        {
            get
            {
                return "Gu.Units";
            }
        }

        public static string FullFileName
        {
            get
            {
                var directoryInfo = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
                var directory = directoryInfo.Parent.Parent.Parent.FullName; // Perhaps not the most elegant code ever written
                return System.IO.Path.Combine(directory, "GeneratorSettings.xml");
            }
        }

        public static string ProjectName
        {
            get
            {
                return "Gu.Units";
            }
        }

        public static string FolderName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The extension for the generated files, set to txt if it does not build so you can´inspect the reult
        /// cs when everything works
        /// </summary>
        public static string Extension
        {
            get
            {
                return "cs";
            }
        }

        public List<DerivedUnit> DerivedUnits
        {
            get
            {
                return _derivedUnits;
            }
        }

        public List<SiUnit> SiUnits
        {
            get { return _siUnits; }
        }

        public List<Prefix> Prefixes
        {
            get { return _prefixes; }
        }

        public List<Quantity> Quantities
        {
            get
            {
                return _quantities;
            }
        }

        public static Settings FromFile(string fullFileName)
        {
            var serializer = new XmlSerializer(typeof(Settings));
            try
            {
                Settings settings;
                using (var reader = new StringReader(fullFileName))
                {
                    settings = (Settings)serializer.Deserialize(reader);
                }
                settings.Initialize();
                return settings;
            }
            catch (FileNotFoundException)
            {
                var settings = new Settings();
                using (var stream = File.Create(fullFileName))
                {
                    serializer.Serialize(stream, settings);
                }
                return settings;
            }
        }
        private void Initialize()
        {
            foreach (var unit in SiUnits)
            {
                unit.Namespace = NameSpace;
                var quantity = new Quantity(unit.Namespace, unit.QuantityName, unit);
                unit.Quantity = quantity;
                _quantities.Add(quantity);
            }
            foreach (var unit in DerivedUnits)
            {
                unit.Namespace = NameSpace;
                var quantity = new Quantity(unit.Namespace, unit.QuantityName, unit);
                _quantities.Add(quantity);
                unit.Quantity = quantity;
                foreach (var unitPart in unit.Parts)
                {
                    if (unitPart.Unit == null)
                    {
                        unitPart.Unit = UnitBase.AllUnitsStatic.Single(x => x.ClassName == unitPart.UnitName);
                    }
                }
            }
            foreach (var quantity in Quantities.Where(x => x.Unit is DerivedUnit))
            {
                var derivedUnit = quantity.Unit as DerivedUnit;
                var unitParts = derivedUnit.Parts;
                foreach (var up in unitParts)
                {
                    if (up.Power > 0)
                    {
                        //var left = up.Unit.Quantity;
                        //left.OperatorOverloads.Add(new OperatorOverload(left, quantity));
                        //quantity.OperatorOverloads.Add(new OperatorOverload(quantity, left));
                    }
                }
            }
        }

        public static void Save(Settings settings, string fullFileName)
        {
            var serializer = new XmlSerializer(typeof(Settings));
            var toSave = new Settings();
            toSave.DerivedUnits.AddRange(settings.DerivedUnits.Where(x => x != null && !x.IsEmpty));
            toSave.SiUnits.AddRange(settings.SiUnits.Where(x => x != null && !x.IsEmpty));
            toSave.Prefixes.AddRange(settings.Prefixes.Where(x => x != null).OrderBy(x => x.Factor));
            using (var stream = File.Create(fullFileName))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    serializer.Serialize(writer, toSave);
                }
            }
        }
    }
}
