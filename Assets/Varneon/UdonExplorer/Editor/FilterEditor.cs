
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using ComparisonType = Varneon.UdonExplorer.FilterOptions.ComparisonType;
using EnumerableStringFilterMode = Varneon.UdonExplorer.FilterOptions.EnumerableStringFilterMode;
using EqualityFilterMode = Varneon.UdonExplorer.FilterOptions.EqualityFilterMode;
using FilterType = Varneon.UdonExplorer.FilterOptions.FilterType;
using StringFilterMode = Varneon.UdonExplorer.FilterOptions.StringFilterMode;
using ValueFilterFields = Varneon.UdonExplorer.FilterOptions.ValueFilterFields;
using ValueFilterMode = Varneon.UdonExplorer.FilterOptions.ValueFilterMode;

namespace Varneon.UdonExplorer
{
    internal static class FilterEditor
    {
        internal static FilterType Type = FilterType.Name;
        internal static ComparisonType ComparisonType = ComparisonType.String;
        internal static StringFilterMode StringFilterMode;
        internal static ValueFilterMode ValueFilterMode;
        internal static EqualityFilterMode EqualityFilterMode;
        internal static EnumerableStringFilterMode EnumerableStringFilterMode;
        internal static string StringReference;
        internal static int IntegerReference, IntegerReferenceMin, IntegerReferenceMax;
        internal static float FloatReference, FloatReferenceMin, FloatReferenceMax;
        internal static Networking.SyncType SyncModeReference;
        internal static ValueFilterFields ValueFilterFields = ValueFilterFields.Absolute;
        internal static UdonListView ListView;
        internal static GUILayoutOption NoWidthExpand = GUILayout.ExpandWidth(false);

        internal static void DrawFilterEditor()
        {
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                Type = (FilterType)EditorGUILayout.EnumPopup(Type, NoWidthExpand);

                if (scope.changed)
                {
                    ComparisonType = typeof(FilterType).GetField(Type.ToString()).GetCustomAttribute<ComparisonFilterTypeAttribute>(false).Type;
                }
            }

            switch (ComparisonType)
            {
                case ComparisonType.String:
                    StringFilterMode = (StringFilterMode)EditorGUILayout.EnumPopup(StringFilterMode, NoWidthExpand);
                    DrawStringReferenceField();
                    break;
                case ComparisonType.EnumerableString:
                    EnumerableStringFilterMode = (EnumerableStringFilterMode)EditorGUILayout.EnumPopup(EnumerableStringFilterMode, NoWidthExpand);
                    DrawStringReferenceField();
                    break;
                case ComparisonType.SyncType:
                    EqualityFilterMode = (EqualityFilterMode)EditorGUILayout.EnumPopup(EqualityFilterMode, NoWidthExpand);
                    SyncModeReference = (Networking.SyncType)EditorGUILayout.EnumPopup(SyncModeReference, NoWidthExpand);
                    break;
                case ComparisonType.Integer:
                    DrawValueFilterModeField();
                    if (ValueFilterFields.HasFlag(ValueFilterFields.Absolute))
                    {
                        IntegerReference = EditorGUILayout.IntField(IntegerReference);
                    }
                    else
                    {
                        if (ValueFilterFields.HasFlag(ValueFilterFields.Min))
                        {
                            IntegerReferenceMin = EditorGUILayout.IntField(IntegerReferenceMin);
                        }

                        if (ValueFilterFields.HasFlag(ValueFilterFields.Max))
                        {
                            IntegerReferenceMax = EditorGUILayout.IntField(IntegerReferenceMax);
                        }
                    }
                    break;
                case ComparisonType.Float:
                    DrawValueFilterModeField();
                    if (ValueFilterFields.HasFlag(ValueFilterFields.Absolute))
                    {
                        FloatReference = EditorGUILayout.FloatField(FloatReference);
                    }
                    else
                    {
                        if (ValueFilterFields.HasFlag(ValueFilterFields.Min))
                        {
                            FloatReferenceMin = EditorGUILayout.FloatField(FloatReferenceMin);
                        }

                        if (ValueFilterFields.HasFlag(ValueFilterFields.Max))
                        {
                            FloatReferenceMax = EditorGUILayout.FloatField(FloatReferenceMax);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException($"[FilterEditor]: ComparisonType {ComparisonType} has not been implemented!");
            }
        }

        private static void DrawValueFilterModeField()
        {
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                ValueFilterMode = (ValueFilterMode)EditorGUILayout.EnumPopup(ValueFilterMode, NoWidthExpand);

                if (scope.changed)
                {
                    ValueFilterFields = typeof(ValueFilterMode).GetField(ValueFilterMode.ToString()).GetCustomAttribute<ValueComparisonFieldAttribute>(false).Fields;
                }
            }
        }

        private static void DrawStringReferenceField()
        {
            if (Event.current.Equals(Event.KeyboardEvent("return")) && GUI.GetNameOfFocusedControl().Equals("StringReference") && ListView != null)
            {
                Event.current.Use();
                ListView.AddFilter(GetFilter());
                GUI.FocusControl(null);
                StringReference = string.Empty;
            }

            GUI.SetNextControlName("StringReference");

            StringReference = GUILayout.TextField(StringReference);
        }

        internal static bool IsValidFilter()
        {
            switch (ComparisonType)
            {
                case ComparisonType.String:
                    return !string.IsNullOrEmpty(StringReference);
                case ComparisonType.EnumerableString:
                    return !string.IsNullOrEmpty(StringReference);
                case ComparisonType.SyncType:
                    return true;
                case ComparisonType.Integer:
                    return true;
                case ComparisonType.Float:
                    return true;
                default:
                    throw new NotImplementedException($"[FilterEditor]: ComparisonType {ComparisonType} has not been implemented!");
            }
        }

        internal static Filter GetFilter()
        {
            switch (ComparisonType)
            {
                case ComparisonType.String:
                    return new Filter(Type, StringFilterMode, stringReference: StringReference);
                case ComparisonType.EnumerableString:
                    return new Filter(Type, EnumerableStringFilterMode, stringReference: StringReference);
                case ComparisonType.SyncType:
                    return new Filter(Type, EqualityFilterMode, syncModeReference: SyncModeReference);
                case ComparisonType.Integer:
                    return new Filter(Type, ValueFilterMode, valueReference: IntegerReference, valueReferenceMin: IntegerReferenceMin, valueReferenceMax: IntegerReferenceMax);
                case ComparisonType.Float:
                    return new Filter(Type, ValueFilterMode, valueReference: FloatReference, valueReferenceMin: FloatReferenceMin, valueReferenceMax: FloatReferenceMax);
                default:
                    throw new NotImplementedException($"[FilterEditor]: ComparisonType {ComparisonType} has not been implemented!");
            }
        }
    }
}
