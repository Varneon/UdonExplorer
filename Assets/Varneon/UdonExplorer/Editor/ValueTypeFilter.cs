
using System;
using System.Collections.Generic;
using System.Linq;
using UdonBehaviourInfo = Varneon.UdonExplorer.UdonListView.UdonBehaviourInfo;
using ValueFilterMode = Varneon.UdonExplorer.FilterOptions.ValueFilterMode;

namespace Varneon.UdonExplorer
{
    internal static class ValueTypeFilter
    {
        internal static IEnumerable<UdonBehaviourInfo> FilterUdonBehaviours(IEnumerable<UdonBehaviourInfo> udonBehaviours, FilterOptions.FilterType type, ValueFilterMode comparisonMode, ValueType reference, ValueType referenceMin, ValueType referenceMax)
        {
            return udonBehaviours.Where(c => CompareFilter(comparisonMode, (ValueType)c.GetFilterComparisonReferenceVariable(type), reference, referenceMin, referenceMax));
        }

        private static bool CompareFilter(ValueFilterMode comparisonMode, ValueType input, ValueType reference, ValueType referenceMin, ValueType referenceMax)
        {
            switch (comparisonMode)
            {
                case ValueFilterMode.Equals:
                    return input.Equals(reference);
                case ValueFilterMode.Greater:
                    return Comparer<ValueType>.Default.Compare(input, referenceMin) > 0;
                case ValueFilterMode.Less:
                    return Comparer<ValueType>.Default.Compare(input, referenceMax) < 0;
                case ValueFilterMode.GreaterOrEqual:
                    return Comparer<ValueType>.Default.Compare(input, referenceMin) >= 0;
                case ValueFilterMode.LessOrEqual:
                    return Comparer<ValueType>.Default.Compare(input, referenceMax) <= 0;
                case ValueFilterMode.WithinRange:
                    return Comparer<ValueType>.Default.Compare(input, referenceMin) >= 0 && Comparer<ValueType>.Default.Compare(input, referenceMax) <= 0;
                case ValueFilterMode.OutsideRange:
                    return Comparer<ValueType>.Default.Compare(input, referenceMin) < 0 && Comparer<ValueType>.Default.Compare(input, referenceMax) > 0;
                default:
                    throw new NotImplementedException($"[ValueTypeFilter]: NumberComparison {comparisonMode} has not been implemented!");
            }
        }
    }
}
