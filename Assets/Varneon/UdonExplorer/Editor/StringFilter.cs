
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StringFilterMode = Varneon.UdonExplorer.FilterOptions.StringFilterMode;
using UdonBehaviourInfo = Varneon.UdonExplorer.UdonListView.UdonBehaviourInfo;

namespace Varneon.UdonExplorer
{
    internal static class StringFilter
    {
        internal static IEnumerable<UdonBehaviourInfo> FilterUdonBehaviours(IEnumerable<UdonBehaviourInfo> udonBehaviours, FilterOptions.FilterType type, StringFilterMode filterMode, string reference)
        {
            return udonBehaviours.Where(c => CompareFilter(filterMode, (string)c.GetFilterComparisonReferenceVariable(type), reference));
        }

        private static bool CompareFilter(StringFilterMode filterMode, string input, string reference)
        {
            switch (filterMode)
            {
                case StringFilterMode.Equals:
                    return input.Equals(reference);
                case StringFilterMode.Contains:
                    return input.Contains(reference);
                case StringFilterMode.StartsWith:
                    return input.StartsWith(reference);
                case StringFilterMode.EndsWith:
                    return input.EndsWith(reference);
                case StringFilterMode.DoesNotContain:
                    return !input.Contains(reference);
                case StringFilterMode.Regex:
                    return Regex.IsMatch(input, Regex.Unescape(reference));
                default:
                    throw new NotImplementedException($"[StringFilter]: StringComparison {filterMode} has not been implemented!");
            }
        }
    }
}
