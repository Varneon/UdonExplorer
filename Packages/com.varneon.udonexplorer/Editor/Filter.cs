
using System;
using System.Collections.Generic;
using System.Reflection;
using VRC.SDKBase;
using EnumerableStringFilterMode = Varneon.UdonExplorer.FilterOptions.EnumerableStringFilterMode;
using EqualityFilterMode = Varneon.UdonExplorer.FilterOptions.EqualityFilterMode;
using FilterType = Varneon.UdonExplorer.FilterOptions.FilterType;
using StringFilterMode = Varneon.UdonExplorer.FilterOptions.StringFilterMode;
using UdonBehaviourInfo = Varneon.UdonExplorer.UdonListView.UdonBehaviourInfo;
using ValueFilterMode = Varneon.UdonExplorer.FilterOptions.ValueFilterMode;

namespace Varneon.UdonExplorer
{
    internal class Filter
    {
        internal string StringReference;
        internal Networking.SyncType SyncModeReference;
        internal ValueType ValueReference, ValueReferenceMin, ValueReferenceMax;

        internal FilterType Type;

        internal EqualityFilterMode EqualityFilterMode;
        internal StringFilterMode StringFilterMode;
        internal ValueFilterMode ValueFilterMode;
        internal EnumerableStringFilterMode SymbolFilterMode;

        public string PreviewText { get; set; }

        private FilterOptions.ComparisonType comparisonType;

        public Filter(FilterType type, Enum filterMode, string stringReference = null, Networking.SyncType syncModeReference = Networking.SyncType.Unknown, ValueType valueReference = null, ValueType valueReferenceMin = null, ValueType valueReferenceMax = null)
        {
            Type = type;

            comparisonType = typeof(FilterType).GetField(Type.ToString()).GetCustomAttribute<ComparisonFilterTypeAttribute>(false).Type;

            switch (comparisonType)
            {
                case FilterOptions.ComparisonType.String:
                    StringFilterMode = (StringFilterMode)filterMode;
                    StringReference = stringReference;
                    PreviewText = $"{Type}, {StringFilterMode}: {stringReference}";
                    break;
                case FilterOptions.ComparisonType.Integer:
                    ValueFilterMode = (ValueFilterMode)filterMode;
                    ValueReference = valueReference;
                    ValueReferenceMin = valueReferenceMin;
                    ValueReferenceMax = valueReferenceMax;
                    PreviewText = $"{Type}, {ValueFilterMode}: {FilterOptions.GetValueFilterPreview(ValueFilterMode, valueReference, valueReferenceMin, valueReferenceMax)}";
                    break;
                case FilterOptions.ComparisonType.Float:
                    ValueFilterMode = (ValueFilterMode)filterMode;
                    ValueReference = valueReference;
                    ValueReferenceMin = valueReferenceMin;
                    ValueReferenceMax = valueReferenceMax;
                    PreviewText = $"{Type}, {ValueFilterMode}: {FilterOptions.GetValueFilterPreview(ValueFilterMode, valueReference, valueReferenceMin, valueReferenceMax)}";
                    break;
                case FilterOptions.ComparisonType.SyncType:
                    EqualityFilterMode = (EqualityFilterMode)filterMode;
                    SyncModeReference = syncModeReference;
                    PreviewText = $"{Type}, {EqualityFilterMode}: {syncModeReference}";
                    break;
                case FilterOptions.ComparisonType.EnumerableString:
                    SymbolFilterMode = (EnumerableStringFilterMode)filterMode;
                    StringReference = stringReference;
                    PreviewText = $"{Type}, {SymbolFilterMode}: {stringReference}";
                    break;
            }
        }

        public IEnumerable<UdonBehaviourInfo> FilterUdonBehaviours(IEnumerable<UdonBehaviourInfo> udonBehaviours)
        {
            switch (comparisonType)
            {
                case FilterOptions.ComparisonType.String:
                    return StringFilter.FilterUdonBehaviours(udonBehaviours, Type, StringFilterMode, StringReference);
                case FilterOptions.ComparisonType.Integer:
                    return ValueTypeFilter.FilterUdonBehaviours(udonBehaviours, Type, ValueFilterMode, ValueReference, ValueReferenceMin, ValueReferenceMax);
                case FilterOptions.ComparisonType.Float:
                    return ValueTypeFilter.FilterUdonBehaviours(udonBehaviours, Type, ValueFilterMode, ValueReference, ValueReferenceMin, ValueReferenceMax);
                case FilterOptions.ComparisonType.SyncType:
                    return EqualityFilter.FilterUdonBehaviours(udonBehaviours, Type, EqualityFilterMode, SyncModeReference);
                case FilterOptions.ComparisonType.EnumerableString:
                    return EnumerableStringFilter.FilterUdonBehaviours(udonBehaviours, Type, SymbolFilterMode, StringReference);
                default:
                    throw new NotImplementedException($"[Filter]: ComparisonType {comparisonType} has not been implemented!");
            }
        }
    }
}
