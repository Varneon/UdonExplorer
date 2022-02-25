
using System.Collections.Generic;
using System.Linq;
using EqualityFilterMode = Varneon.UdonExplorer.FilterOptions.EqualityFilterMode;
using UdonBehaviourInfo = Varneon.UdonExplorer.UdonListView.UdonBehaviourInfo;

namespace Varneon.UdonExplorer
{
    internal static class EqualityFilter
    {
        internal static IEnumerable<UdonBehaviourInfo> FilterUdonBehaviours(IEnumerable<UdonBehaviourInfo> udonBehaviours, FilterOptions.FilterType type, EqualityFilterMode filterMode, object reference)
        {
            return udonBehaviours.Where(c => filterMode == EqualityFilterMode.Equals ? c.GetFilterComparisonReferenceVariable(type).Equals(reference) : !c.GetFilterComparisonReferenceVariable(type).Equals(reference));
        }
    }
}
