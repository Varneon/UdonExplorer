
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram.UI.GraphView;

namespace Varneon.UdonExplorer
{
    internal class UdonListView : TreeView
    {
        internal UdonExplorerWindow Explorer;

        internal string StatisticsSummary = string.Empty;

        internal List<Filter> Filters = new List<Filter>();

        internal GUIContent[] FilterPreviewContent = new GUIContent[0];

        internal string
            PublicVariablesList,
            UdonProgramSyncMetadata,
            UdonProgramExportedSymbolList,
            UdonProgramSymbolList,
            UdonProgramExportedEntryPointSymbolList,
            UdonProgramEntryPointSymbolList;

        private int
            udonBehaviourCount,
            includedInBuildCount,
            activeGameObjectCount,
            enabledComponentCount,
            manualSyncedCount,
            continuousSyncedCount;

        private IEnumerable<UdonBehaviourInfo> filteredData;

        private static UdonBehaviour[] sceneUdonBehaviours = new UdonBehaviour[0];

        private const string StatisticsSummaryTemplate = "<color=#54bef8>[ {1} / {0} ]</color> Included In Build  | <color=#54bef8>[ {2} / {0} ]</color> Active | <color=#54bef8>[ {3} / {0} ]</color> Enabled | <color=#54bef8>[ {4} ]</color> Manual | <color=#54bef8>[ {5} ]</color> Continuous";

        private int selectedId = -1;

        private static readonly GUIContent
            IconPrefab = EditorGUIUtility.IconContent("d_Prefab Icon"),
            IconFont = EditorGUIUtility.IconContent("d_Font Icon"),
            IconSorting = EditorGUIUtility.IconContent("d_CustomSorting"),
            IconSync = EditorGUIUtility.IconContent("d_NetworkAnimator Icon"),
            IconScriptableObject = EditorGUIUtility.IconContent("d_ScriptableObject Icon"),
            IconScript = EditorGUIUtility.IconContent("d_cs Script Icon"),
            IconSize = EditorGUIUtility.IconContent("d_SaveAs@2x"),
            IconClose = EditorGUIUtility.IconContent("d_winbtn_mac_close_a");

        private static readonly GUIContent
            ContentOpenUdonSharpSourceScript = new GUIContent("Open U# Source C# Script"),
            ContentSelectUdonSharpSourceScript = new GUIContent("Select U# Source C# Script"),
            ContentOpenUdonGraph = new GUIContent("Open Udon Graph"),
            ContentSelectProgramSource = new GUIContent("Select Program Source"),
            ContentSelectSerializedProgramAsset = new GUIContent("Select Serialized Program Asset"),
            ContentNoActionsAvailable = new GUIContent("No actions available");

        private static readonly GUIStyle
            DefaultTextStyle = new GUIStyle() { clipping = TextClipping.Clip, alignment = TextAnchor.MiddleLeft, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f) : Color.black } },
            WhiteTextStyle = new GUIStyle() { clipping = TextClipping.Clip, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } },
            DisabledTextStyle = new GUIStyle() { clipping = TextClipping.Clip, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.grey } };

        internal UdonListView(TreeViewState state, MultiColumnHeader header) : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += SortingChanged;
            header.ResizeToFit();
            data = LoadUdonBehaviourInfo();
            filteredData = GetFilteredData0();
            RefreshAllStatistics();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem { depth = -1 };
            root.children = new List<TreeViewItem>();
            return root;
        }

        internal void Refresh()
        {
            rootItem.children = GetFilteredData();
            BuildRows(rootItem);
            Repaint();
        }

        internal void CheckForChangesInScene()
        {
            if(!Enumerable.SequenceEqual(sceneUdonBehaviours, GetUdonBehavioursFromScene())) { ReloadData(); }
            else
            {
                for(int i = 0; i < udonBehaviourCount; i++)
                {
                    EditorUtility.DisplayProgressBar("Updating UdonBehaviour Information", $"[{i}/{udonBehaviourCount}] {sceneUdonBehaviours[i].name}", (float)i / (float)udonBehaviourCount);
                    data[i].UpdateInfo();
                }

                EditorUtility.ClearProgressBar();

                RefreshAllStatistics();

                Explorer.Focus();

                SortingChanged(multiColumnHeader);
            }
        }

        internal void ReloadData()
        {
            data = LoadUdonBehaviourInfo();
            filteredData = GetFilteredData0();
            RefreshAllStatistics();
            SortingChanged(multiColumnHeader);
        }

        private void SortingChanged(MultiColumnHeader header)
        {
            int index = multiColumnHeader.sortedColumnIndex;
            
            if(index == -1) {
                Refresh();
                return;
            }
            
            bool ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

            rootItem.children = GetSortedData(index, ascending);
            BuildRows(rootItem);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void ContextClickedItem(int id)
        {
            OnContextClickedItem(id);
            base.ContextClickedItem(id);
        }

        protected override void SingleClickedItem(int id)
        {
            OnSingleClickedItem(id);
            base.SingleClickedItem(id);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            TreeViewItem item = args.item;

            for (int visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                Rect rect = args.GetCellRect(visibleColumnIndex);
                int columnIndex = args.GetColumn(visibleColumnIndex);

                Draw(rect, columnIndex, (UdonBehaviourInfo)item, args.selected);
            }
        }

        internal void AddFilter(Filter filter)
        {
            Filters.Add(filter);
            UpdateFilters();
        }

        internal void RemoveFilterAt(int index)
        {
            Filters.RemoveAt(index);
            UpdateFilters();
        }

        private void UpdateFilters()
        {
            FilterPreviewContent = Filters.Select(c => new GUIContent(c.PreviewText, IconClose.image, "Remove")).ToArray();
            FilterList();
        }

        internal void FilterList()
        {
            rootItem.children = GetFilteredData();
            BuildRows(rootItem);
        }

        internal List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
            => GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();

        internal List<TreeViewItem> GetFilteredData()
            => GetFilteredData0().Cast<TreeViewItem>().ToList();

        private IEnumerable<UdonBehaviourInfo> GetSortedData0(int columnIndex, bool isAscending)
        {
            switch (columnIndex)
            {
                case 0:
                    return isAscending
                    ? filteredData.OrderBy(item => item.GameObjectActive)
                    : filteredData.OrderByDescending(item => item.GameObjectActive);
                case 1:
                    return isAscending
                    ? filteredData.OrderBy(item => item.ComponentActive)
                    : filteredData.OrderByDescending(item => item.ComponentActive);
                case 2:
                    return isAscending
                    ? filteredData.OrderBy(item => item.BehaviourName)
                    : filteredData.OrderByDescending(item => item.BehaviourName);
                case 3:
                    return isAscending
                    ? filteredData.OrderBy(item => item.SyncType.ToString())
                    : filteredData.OrderByDescending(item => item.SyncType.ToString());
                case 4:
                    return isAscending
                    ? filteredData.OrderBy(item => item.UpdateOrder)
                    : filteredData.OrderByDescending(item => item.UpdateOrder);
                case 5:
                    return isAscending
                    ? filteredData.OrderBy(item => item.UdonProgramSourceName)
                    : filteredData.OrderByDescending(item => item.UdonProgramSourceName);
                case 6:
                    return isAscending
                    ? filteredData.OrderBy(item => item.SerializedUdonProgramSourceName)
                    : filteredData.OrderByDescending(item => item.SerializedUdonProgramSourceName);
                case 7:
                    return isAscending
                    ? filteredData.OrderBy(item => item.SerializedUdonProgramSourceSize)
                    : filteredData.OrderByDescending(item => item.SerializedUdonProgramSourceSize);
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }
        }

        private IEnumerable<UdonBehaviourInfo> GetFilteredData0()
        {
            IEnumerable<UdonBehaviourInfo> info = data;

            foreach(Filter filter in Filters)
            {
                info = filter.FilterUdonBehaviours(info);
            }

            filteredData = info;

            return info;
        }

        internal static MultiColumnHeader Header()
        {
            return new MultiColumnHeader(new MultiColumnHeaderState(new[]{
            new MultiColumnHeaderState.Column{headerContent = IconPrefab, contextMenuText = "GameObject Active", width = 20, minWidth = 20, maxWidth = 20, autoResize = false},
            new MultiColumnHeaderState.Column{headerContent = IconScript, contextMenuText = "UdonBehaviour Active", width = 20, minWidth = 20, maxWidth = 20, autoResize = false},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Name"){ image = IconFont.image }, contextMenuText = "Name", width = 100, minWidth = 100, maxWidth = 400},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Sync Type"){ image = IconSync.image }, contextMenuText = "Sync Type", width = 90, minWidth = 90, maxWidth = 90},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Update Order"){ image = IconSorting.image }, contextMenuText = "Update Order", width = 110, minWidth = 110, maxWidth = 110},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Program Source"){ image = IconScriptableObject.image}, contextMenuText = "Program Source", width = 120, minWidth = 120, maxWidth = 400},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Serialized Udon Program Source"){ image = IconScriptableObject.image }, contextMenuText = "Serialized Udon Program Source", width = 250, minWidth = 230, maxWidth = 250},
            new MultiColumnHeaderState.Column{headerContent = new GUIContent("Size"){ image = IconSize.image }, contextMenuText = "Size", width = 60, minWidth = 60, maxWidth = 60, autoResize = false}}));
        }

        private List<UdonBehaviourInfo> data = new List<UdonBehaviourInfo>();

        private void Draw(Rect rect, int columnIndex, UdonBehaviourInfo data, bool selected)
        {
            GUIStyle labelStyle = data.GameObjectActiveInHierarchy ? selected ? WhiteTextStyle : DefaultTextStyle : DisabledTextStyle;

            switch (columnIndex)
            {
                case 0:
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        data.GameObjectActive = EditorGUI.Toggle(new Rect(rect.x + 2, rect.y, rect.width, rect.height), data.GameObjectActive);

                        if (scope.changed)
                        {
                            Undo.RecordObject(data.Behaviour.gameObject, "Set GameObject Active");
                            data.Behaviour.gameObject.SetActive(data.GameObjectActive);
                            data.GameObjectActiveInHierarchy = data.Behaviour.gameObject.activeInHierarchy;
                            RefreshActiveGameObjectCount();
                        }
                    }
                    break;
                case 1:
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        data.ComponentActive = EditorGUI.Toggle(rect, data.ComponentActive);

                        if (scope.changed)
                        {
                            Undo.RecordObject(data.Behaviour, "Set UdonBehaviour Enabled");
                            data.Behaviour.enabled = data.ComponentActive;
                            RefreshEnabledComponentCount();
                        }
                    }
                    break;
                case 2:
                    EditorGUI.LabelField(rect, data.BehaviourName, labelStyle);
                    break;
                case 3:
                    EditorGUI.LabelField(rect, data.SyncType.ToString(), labelStyle);
                    break;
                case 4:
                    EditorGUI.LabelField(rect, data.UpdateOrder.ToString(), labelStyle);
                    break;
                case 5:
                    EditorGUI.LabelField(rect, data.UdonProgramSourceName, labelStyle);
                    break;
                case 6:
                    EditorGUI.LabelField(rect, data.SerializedUdonProgramSourceName, labelStyle);
                    break;
                case 7:
                    EditorGUI.LabelField(rect, data.SerializedUdonProgramSourceSizeText, labelStyle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }
        }

        private void OnSingleClickedItem(int id)
        {
            UdonBehaviourInfo selectedItem = data[id];

            UdonBehaviour udonBehaviour = selectedItem.Behaviour;

            if (!udonBehaviour) { ReloadData(); return; }

            if (id == selectedId)
            {
                Selection.activeObject = udonBehaviour;
            }
            else
            {
                selectedId = id;
                LoadProgramInfo(selectedItem);
                Explorer.SelectedItem = selectedItem;
                EditorGUIUtility.PingObject(udonBehaviour);
            }
        }

        private void OnContextClickedItem(int id)
        {
            GenericMenu menu = new GenericMenu();

            UdonBehaviourInfo item = data[id];

            if (!item.UdonProgramSource) { menu.AddDisabledItem(ContentNoActionsAvailable); menu.ShowAsContext(); return; }

            switch (item.UdonProgramSourceType)
            {
                case "UdonSharpProgramAsset":
                    MonoScript sourceCsScript = (MonoScript)new SerializedObject(item.UdonProgramSource).FindProperty("sourceCsScript").objectReferenceValue;
                    menu.AddItem(ContentOpenUdonSharpSourceScript, false, OpenAsset, sourceCsScript);
                    menu.AddItem(ContentSelectUdonSharpSourceScript, false, SelectAsset, sourceCsScript);
                    break;
                case "UdonGraphProgramAsset":
                    menu.AddItem(ContentOpenUdonGraph, false, OpenUdonGraph, item.UdonProgramSource);
                    break;
            }

            menu.AddItem(ContentSelectProgramSource, false, SelectAsset, item.UdonProgramSource);
            menu.AddItem(ContentSelectSerializedProgramAsset, false, SelectAsset, item.SerializedUdonProgramSource);

            menu.ShowAsContext();
        }

        private void SelectAsset(object userData)
        {
            Selection.activeObject = (UnityEngine.Object)userData;
        }

        private void OpenAsset(object userData)
        {
            AssetDatabase.OpenAsset((UnityEngine.Object)userData);
        }

        private void OpenUdonGraph(object userData)
        {
            UdonGraphWindow window = EditorWindow.GetWindow<UdonGraphWindow>("Udon Graph", true, typeof(SceneView));
            window.InitializeGraph((UdonGraphProgramAsset)userData);
        }

        internal class UdonBehaviourInfo : TreeViewItem
        {
            internal int ID;
            internal UdonBehaviour Behaviour;
            internal bool GameObjectActiveInHierarchy;
            internal bool GameObjectActive;
            internal bool ComponentActive;
            internal string BehaviourName;
            internal VRC.SDKBase.Networking.SyncType SyncType;
            internal AbstractUdonProgramSource UdonProgramSource;
            internal string UdonProgramSourceType;
            internal string UdonProgramSourceName;
            internal AbstractSerializedUdonProgramAsset SerializedUdonProgramSource;
            internal string SerializedUdonProgramSourceName;
            internal long SerializedUdonProgramSourceSize;
            internal string SerializedUdonProgramSourceSizeText;
            internal IUdonProgram UdonProgram;
            internal int UpdateOrder;

            internal UdonBehaviourInfo(int id, UdonBehaviour behaviour) : base(id)
            {
                ID = id;
                Behaviour = behaviour;
                UpdateInfo();
            }

            internal void UpdateInfo()
            {
                GameObjectActiveInHierarchy = Behaviour.gameObject.activeInHierarchy;
                GameObjectActive = Behaviour.gameObject.activeSelf;
                ComponentActive = Behaviour.enabled;
                BehaviourName = Behaviour.name;
                SyncType = Behaviour.SyncMethod;

                if (!(UdonProgramSource = Behaviour.programSource)) { return; }

                UdonProgramSourceType = UdonProgramSource.GetType().Name;
                UdonProgramSourceName = UdonProgramSource.name;
                SerializedUdonProgramSource = UdonProgramSource.SerializedProgramAsset;
                SerializedUdonProgramSourceName = SerializedUdonProgramSource.name;
                SerializedUdonProgramSourceSize = new FileInfo(Path.GetFullPath(AssetDatabase.GetAssetPath(SerializedUdonProgramSource))).Length;
                SerializedUdonProgramSourceSizeText = ParseFileSize(SerializedUdonProgramSourceSize);
                UdonProgram = SerializedUdonProgramSource.RetrieveProgram();
                UpdateOrder = UdonProgram.UpdateOrder;
            }

            private static string ParseFileSize(long fileLength)
            {
                string[] sizes = { "bytes", "KB", "MB", "GB" };
                int i = 0;

                while (fileLength > 1024 && i < sizes.Length)
                {
                    fileLength /= 1024;

                    i++;
                }
                return ($"{fileLength} {sizes[i]}");
            }

            internal object GetFilterComparisonReferenceVariable(FilterOptions.FilterType type)
            {
                switch (type)
                {
                    case FilterOptions.FilterType.Name:
                        return BehaviourName;
                    case FilterOptions.FilterType.ProgramSource:
                        return UdonProgramSourceName;
                    case FilterOptions.FilterType.SyncMode:
                        return SyncType;
                    case FilterOptions.FilterType.UpdateOrder:
                        return UpdateOrder;
                    case FilterOptions.FilterType.SyncMetadata:
                        return UdonProgram.SyncMetadataTable.GetAllSyncMetadata().Select(c => c.Name);
                    case FilterOptions.FilterType.EntryPoint:
                        return UdonProgram.EntryPoints.GetSymbols();
                    case FilterOptions.FilterType.Symbol:
                        return UdonProgram.SymbolTable.GetSymbols();
                    case FilterOptions.FilterType.Proximity:
                        return Behaviour.proximity;
                    case FilterOptions.FilterType.AttachedComponent:
                        return Behaviour.GetComponents<Component>().Select(c => c.GetType().FullName);
                    case FilterOptions.FilterType.InteractText:
                        return Behaviour.interactText;
                    default:
                        throw new NotImplementedException($"Comparison reference variable for FilterType {type} has not been implemented!");
                }
            }
        }

        private void LoadProgramInfo(UdonBehaviourInfo item)
        {
            if (!item.UdonProgramSource) { return; }

            IUdonVariableTable publicVariables = item.Behaviour.publicVariables;

            PublicVariablesList = string.Join("\n", publicVariables.VariableSymbols.Select(c => $"<{(publicVariables.TryGetVariableType(c, out Type type) ? type.Name : "Unknown")}> {c} = {(publicVariables.TryGetVariableValue(c, out object value) ? value : "NULL")}"));

            IUdonSymbolTable udonProgramSymbolTable = item.UdonProgram.SymbolTable;

            UdonProgramSyncMetadata = string.Join("\n", item.UdonProgram.SyncMetadataTable.GetAllSyncMetadata().Select(c => $"[{string.Join(", ", c.Properties.Select(d => $"{d.InterpolationAlgorithm}"))}] <{udonProgramSymbolTable.GetSymbolType(c.Name).Name}> {c.Name}"));

            UdonProgramExportedSymbolList = string.Join("\n", udonProgramSymbolTable.GetExportedSymbols().Select(c => $"<{udonProgramSymbolTable.GetSymbolType(c).Name}> {c}"));

            UdonProgramSymbolList = string.Join("\n", udonProgramSymbolTable.GetSymbols().Select(c => $"<{udonProgramSymbolTable.GetSymbolType(c).Name}> {c}"));

            IUdonSymbolTable udonProgramEntryPointTable = item.UdonProgram.EntryPoints;

            HashSet<string> publicEntryPoints = new HashSet<string>(udonProgramEntryPointTable.GetExportedSymbols());

            UdonProgramExportedEntryPointSymbolList = string.Join("\n", publicEntryPoints);

            UdonProgramEntryPointSymbolList = string.Join("\n", udonProgramEntryPointTable.GetSymbols().Where(c => !publicEntryPoints.Contains(c)));
        }

        internal UdonBehaviour[] GetUdonBehavioursFromScene()
        {
            return Resources.FindObjectsOfTypeAll<UdonBehaviour>().Where(c => !PrefabUtility.IsPartOfPrefabAsset(c)).ToArray();
        }

        internal List<UdonBehaviourInfo> LoadUdonBehaviourInfo()
        {
            sceneUdonBehaviours = GetUdonBehavioursFromScene();

            List<UdonBehaviourInfo> content = new List<UdonBehaviourInfo>();

            udonBehaviourCount = sceneUdonBehaviours.Length;

            for (int i = 0; i < udonBehaviourCount; i++)
            {
                EditorUtility.DisplayProgressBar("Loading UdonBehaviour Information", $"[{i}/{udonBehaviourCount}] {sceneUdonBehaviours[i].name}", (float)i / (float)udonBehaviourCount);

                content.Add(new UdonBehaviourInfo(i, sceneUdonBehaviours[i]));
            }

            EditorUtility.ClearProgressBar();

            return content;
        }

        private void RefreshAllStatistics()
        {
            RefreshActiveGameObjectCount(false);
            RefreshEnabledComponentCount(false);
            includedInBuildCount = data.Where(c => c.Behaviour.transform.GetComponentsInParent<Transform>(true).Where(d => d.tag.Equals("EditorOnly")).Count() == 0).Count();
            manualSyncedCount = data.Where(c => c.SyncType == VRC.SDKBase.Networking.SyncType.Manual).Count();
            continuousSyncedCount = data.Where(c => c.SyncType == VRC.SDKBase.Networking.SyncType.Continuous).Count();
            RefreshStatisticsSummary();
        }

        private void RefreshActiveGameObjectCount(bool refreshSummary = true)
        {
            activeGameObjectCount = data.Where(c => c.GameObjectActive).Count();
            if (refreshSummary) { RefreshStatisticsSummary(); }
        }

        private void RefreshEnabledComponentCount(bool refreshSummary = true)
        {
            enabledComponentCount = data.Where(c => c.ComponentActive).Count();
            if (refreshSummary) { RefreshStatisticsSummary(); }
        }

        private void RefreshStatisticsSummary()
        {
            StatisticsSummary = string.Format(
                StatisticsSummaryTemplate,
                udonBehaviourCount,
                includedInBuildCount,
                activeGameObjectCount,
                enabledComponentCount,
                manualSyncedCount,
                continuousSyncedCount
                );
        }
    }
}
