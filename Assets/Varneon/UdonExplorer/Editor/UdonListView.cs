
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

        private static UdonBehaviour[] sceneUdonBehaviours;
        private static int udonBehaviourCount;
        private int selectedId = -1;

        private static readonly GUIContent
            IconPrefab = EditorGUIUtility.IconContent("d_Prefab Icon"),
            IconFont = EditorGUIUtility.IconContent("d_Font Icon"),
            IconSorting = EditorGUIUtility.IconContent("d_CustomSorting"),
            IconSync = EditorGUIUtility.IconContent("d_NetworkAnimator Icon"),
            IconScriptableObject = EditorGUIUtility.IconContent("d_ScriptableObject Icon"),
            IconScript = EditorGUIUtility.IconContent("d_cs Script Icon"),
            IconSize = EditorGUIUtility.IconContent("d_SaveAs@2x");

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
            rootItem.children = GetData();
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

                Explorer.Focus();

                SortingChanged(multiColumnHeader);
            }
        }

        internal void ReloadData()
        {
            data = LoadUdonBehaviourInfo();
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

        internal List<TreeViewItem> GetData()
        {
            return data.Cast<TreeViewItem>().ToList();
        }

        internal List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
            => GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();

        private IEnumerable<UdonBehaviourInfo> GetSortedData0(int columnIndex, bool isAscending)
        {
            switch (columnIndex)
            {
                case 0:
                    return isAscending
                    ? data.OrderBy(item => item.GameObjectActive)
                    : data.OrderByDescending(item => item.GameObjectActive);
                case 1:
                    return isAscending
                    ? data.OrderBy(item => item.ComponentActive)
                    : data.OrderByDescending(item => item.ComponentActive);
                case 2:
                    return isAscending
                    ? data.OrderBy(item => item.BehaviourName)
                    : data.OrderByDescending(item => item.BehaviourName);
                case 3:
                    return isAscending
                    ? data.OrderBy(item => item.SyncType.ToString())
                    : data.OrderByDescending(item => item.SyncType.ToString());
                case 4:
                    return isAscending
                    ? data.OrderBy(item => item.UpdateOrder)
                    : data.OrderByDescending(item => item.UpdateOrder);
                case 5:
                    return isAscending
                    ? data.OrderBy(item => item.UdonProgramSourceName)
                    : data.OrderByDescending(item => item.UdonProgramSourceName);
                case 6:
                    return isAscending
                    ? data.OrderBy(item => item.SerializedUdonProgramSourceName)
                    : data.OrderByDescending(item => item.SerializedUdonProgramSourceName);
                case 7:
                    return isAscending
                    ? data.OrderBy(item => item.SerializedUdonProgramSourceSize)
                    : data.OrderByDescending(item => item.SerializedUdonProgramSourceSize);
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
            }
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

        private List<UdonBehaviourInfo> data = LoadUdonBehaviourInfo();

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
            UdonBehaviourInfo selectedItem = (UdonBehaviourInfo)GetData()[id];

            UdonBehaviour udonBehaviour = selectedItem.Behaviour;

            if (!udonBehaviour) { ReloadData(); return; }

            if (id == selectedId)
            {
                Selection.activeObject = udonBehaviour;
            }
            else
            {
                selectedId = id;
                selectedItem.LoadProgramInfo();
                Explorer.SelectedItem = selectedItem;
                EditorGUIUtility.PingObject(udonBehaviour);
            }
        }

        private void OnContextClickedItem(int id)
        {
            GenericMenu menu = new GenericMenu();

            UdonBehaviourInfo item = (UdonBehaviourInfo)GetData()[id];

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
            internal string PublicVariablesList;
            internal AbstractUdonProgramSource UdonProgramSource;
            internal string UdonProgramSourceType;
            internal string UdonProgramSourceName;
            internal AbstractSerializedUdonProgramAsset SerializedUdonProgramSource;
            internal string SerializedUdonProgramSourceName;
            internal long SerializedUdonProgramSourceSize;
            internal string SerializedUdonProgramSourceSizeText;
            internal IUdonProgram UdonProgram;
            internal string UdonProgramExportedSymbolList;
            internal string UdonProgramSymbolList;
            internal string UdonProgramExportedEntryPointSymbolList;
            internal string UdonProgramEntryPointSymbolList;
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

            internal void LoadProgramInfo()
            {
                if (!UdonProgramSource) { return; }

                IUdonVariableTable publicVariables = Behaviour.publicVariables;

                PublicVariablesList = string.Join("\n", publicVariables.VariableSymbols.Select(c => $"<{(publicVariables.TryGetVariableType(c, out Type type) ? type.Name : "Unknown")}> {c} = {(publicVariables.TryGetVariableValue(c, out object value) ? value : "NULL")}"));

                IUdonSymbolTable udonProgramSymbolTable = UdonProgram.SymbolTable;

                UdonProgramExportedSymbolList = string.Join("\n", udonProgramSymbolTable.GetExportedSymbols().Select(c => $"<{udonProgramSymbolTable.GetSymbolType(c).Name}> {c}"));

                UdonProgramSymbolList = string.Join("\n", udonProgramSymbolTable.GetSymbols().Select(c => $"<{udonProgramSymbolTable.GetSymbolType(c).Name}> {c}"));

                IUdonSymbolTable udonProgramEntryPointTable = UdonProgram.EntryPoints;

                UdonProgramEntryPointSymbolList = string.Join("\n", udonProgramEntryPointTable.GetSymbols());

                UdonProgramExportedEntryPointSymbolList = string.Join("\n", udonProgramEntryPointTable.GetExportedSymbols());
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
        }

        internal static UdonBehaviour[] GetUdonBehavioursFromScene()
        {
            return Resources.FindObjectsOfTypeAll<UdonBehaviour>().Where(c => !PrefabUtility.IsPartOfPrefabAsset(c)).ToArray();
        }

        internal static List<UdonBehaviourInfo> LoadUdonBehaviourInfo()
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
    }
}
