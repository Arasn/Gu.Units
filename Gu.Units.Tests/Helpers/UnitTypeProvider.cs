namespace Gu.Units.Tests.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class UnitTypeProvider : IEnumerable<Type>
    {
        public static readonly List<Type> UnitTypes = typeof(Length).Assembly.GetTypes()
                                                                    .Where(x => x.IsValueType && typeof(IUnit).IsAssignableFrom(x))
                                                                    .ToList();

        public IEnumerator<Type> GetEnumerator()
        {
            return UnitTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}