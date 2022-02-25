
using System;

namespace Varneon.UdonExplorer
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ValueComparisonFieldAttribute : Attribute
    {
        internal FilterOptions.ValueFilterFields Fields { get; private set; }

        internal ValueComparisonFieldAttribute(FilterOptions.ValueFilterFields fields)
        {
            Fields = fields;
        }
    }
}
