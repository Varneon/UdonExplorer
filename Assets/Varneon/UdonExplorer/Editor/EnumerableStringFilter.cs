
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnumerableStringFilterMode = Varneon.UdonExplorer.FilterOptions.EnumerableStringFilterMode;
using UdonBehaviourInfo = Varneon.UdonExplorer.UdonListView.UdonBehaviourInfo;

namespace Varneon.UdonExplorer
{
    internal static class EnumerableStringFilter
    {
        internal static IEnumerable<UdonBehaviourInfo> FilterUdonBehaviours(IEnumerable<UdonBehaviourInfo> udonBehaviours, FilterOptions.FilterType type, EnumerableStringFilterMode comparisonMode, string reference)
        {
            return udonBehaviours.Where(c => CompareFilter(comparisonMode, (IEnumerable<string>)c.GetFilterComparisonReferenceVariable(type), reference));
        }

        private static bool CompareFilter(EnumerableStringFilterMode filterMode, IEnumerable<string> input, string reference)
        {
            switch (filterMode)
            {
                case EnumerableStringFilterMode.Equals:
                    return input.Any(c => c.Equals(reference));
                case EnumerableStringFilterMode.Contains:
                    return input.Any(c => c.Contains(reference));
                case EnumerableStringFilterMode.StartsWith:
                    return input.Any(c => c.StartsWith(reference));
                case EnumerableStringFilterMode.EndsWith:
                    return input.Any(c => c.EndsWith(reference));
                case EnumerableStringFilterMode.Regex:
                    return input.Any(c => Regex.IsMatch(c, Regex.Unescape(reference)));
                default:
                    throw new NotImplementedException($"[SymbolFilter]: EnumerableStringComparison {filterMode} has not been implemented!");
            }
        }
    }
}
