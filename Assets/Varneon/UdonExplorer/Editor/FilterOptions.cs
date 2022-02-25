
using System;

namespace Varneon.UdonExplorer
{
    internal static class FilterOptions
    {
        internal enum FilterType
        {
            [ComparisonFilterType(ComparisonType.String)]
            Name,

            [ComparisonFilterType(ComparisonType.String)]
            ProgramSource,

            [ComparisonFilterType(ComparisonType.SyncType)]
            SyncMode,

            [ComparisonFilterType(ComparisonType.Integer)]
            UpdateOrder,

            [ComparisonFilterType(ComparisonType.EnumerableString)]
            SyncMetadata,

            [ComparisonFilterType(ComparisonType.EnumerableString)]
            EntryPoint,

            [ComparisonFilterType(ComparisonType.EnumerableString)]
            Symbol,

            [ComparisonFilterType(ComparisonType.Float)]
            Proximity,

            [ComparisonFilterType(ComparisonType.EnumerableString)]
            AttachedComponent,

            [ComparisonFilterType(ComparisonType.String)]
            InteractText
        }

        internal enum ComparisonType
        {
            EnumerableString,
            String,
            SyncType,
            Integer,
            Float
        }

        internal enum ValueFilterMode
        {
            [ValueComparisonField(ValueFilterFields.Absolute)]
            Equals,

            [ValueComparisonField(ValueFilterFields.Min)]
            Greater,

            [ValueComparisonField(ValueFilterFields.Max)]
            Less,

            [ValueComparisonField(ValueFilterFields.Min)]
            GreaterOrEqual,

            [ValueComparisonField(ValueFilterFields.Max)]
            LessOrEqual,

            [ValueComparisonField(ValueFilterFields.Min | ValueFilterFields.Max)]
            WithinRange,

            [ValueComparisonField(ValueFilterFields.Min | ValueFilterFields.Max)]
            OutsideRange
        }

        [Flags]
        internal enum ValueFilterFields
        {
            Absolute = 1 << 1,
            Min = 1 << 2,
            Max = 1 << 3
        }

        internal enum StringFilterMode
        {
            Equals,
            Contains,
            StartsWith,
            EndsWith,
            DoesNotContain,
            Regex
        }

        internal enum EnumerableStringFilterMode
        {
            Equals,
            Contains,
            StartsWith,
            EndsWith,
            Regex
        }

        internal enum EqualityFilterMode
        {
            Equals,
            DoesNotEqual
        }

        internal static string GetValueFilterPreview(ValueFilterMode filterMode, ValueType reference = null, ValueType referenceMin = null, ValueType referenceMax = null)
        {
            switch (filterMode)
            {
                case ValueFilterMode.Equals:
                    return reference.ToString();
                case ValueFilterMode.Greater:
                    return $"> {referenceMin}";
                case ValueFilterMode.Less:
                    return $"< {referenceMax}";
                case ValueFilterMode.GreaterOrEqual:
                    return $">= {referenceMin}";
                case ValueFilterMode.LessOrEqual:
                    return $"<= {referenceMax}";
                case ValueFilterMode.WithinRange:
                    return $"{referenceMin} < {referenceMax}";
                case ValueFilterMode.OutsideRange:
                    return $"< {referenceMin} OR {referenceMax} <";
                default:
                    throw new NotImplementedException($"ValueFilterMode {filterMode} has not been implemented!");
            }
        }
    }
}
