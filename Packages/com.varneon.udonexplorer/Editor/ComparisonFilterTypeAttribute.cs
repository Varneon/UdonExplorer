
using System;

namespace Varneon.UdonExplorer
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ComparisonFilterTypeAttribute : Attribute
    {
        internal FilterOptions.ComparisonType Type { get; private set; }

        internal ComparisonFilterTypeAttribute(FilterOptions.ComparisonType type)
        {
            Type = type;
        }
    }
}
