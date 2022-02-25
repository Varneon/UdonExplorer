
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Varneon.UdonExplorer
{
    internal class UdonExplorerWindow : EditorWindow
    {
        internal UdonListView.UdonBehaviourInfo SelectedItem;

        private static UdonListView listView;
        private Vector2 scrollPos;
        private float currentScrollViewWidth;
        private bool resize = false;
        private Rect cursorChangeRect;
        private Vector2 lastWindowSize;
        private bool useAutoRefresh = true;
        private bool hasLatestInfo = true;
        private bool
            showFilters,
            showSyncMetadata,
            showPublicVariables,
            showExportedSymbols,
            showSymbols,
            showExportedEntryPoints,
            showEntryPoints;
        private Vector2 filterListScrollPos = new Vector2();
        private const string UseAutoRefreshPreferenceKey = "Varneon/UdonExplorer/UseAutoRefresh";
        private static readonly GUIContent
            UseAutoRefreshToggleContent = new GUIContent("Refresh On Focus", "Should the explorer automatically refresh when the window gains focus?"),
            RefreshButtonContent = new GUIContent("Refresh", "Manually refresh the explorer");
        private static GUIStyle RichTextStyle = new GUIStyle();

        [MenuItem("Varneon/Udon Explorer")]
        public static void Init()
        {
            EditorWindow window = GetWindow<UdonExplorerWindow>("Udon Explorer", true);
            window.minSize = new Vector2(1024, 512);
            window.titleContent.image = Resources.Load<Texture>("Icon_UdonExplorer");
            window.Show();
        }

        private void OnEnable()
        {
            RichTextStyle = new GUIStyle() { richText = true, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f) : Color.black } };

            currentScrollViewWidth = position.width - 300;

            cursorChangeRect = new Rect(currentScrollViewWidth, 0, 3, position.height + 48);

            if (EditorPrefs.HasKey(UseAutoRefreshPreferenceKey))
            {
                useAutoRefresh = EditorPrefs.GetBool(UseAutoRefreshPreferenceKey);
            }

            listView = new UdonListView(new TreeViewState(), UdonListView.Header())
            {
                Explorer = this
            };

            FilterEditor.ListView = listView;

            listView.Refresh();
        }

        private void OnFocus()
        {
            if (hasLatestInfo || !useAutoRefresh) { return; }

            listView.CheckForChangesInScene();

            hasLatestInfo = true;
        }

        private void OnLostFocus()
        {
            hasLatestInfo = false;
        }

        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(listView.StatisticsSummary, RichTextStyle);

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        if(showFilters = EditorGUILayout.Foldout(showFilters, $"Filters ({listView.Filters.Count})", true))
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                FilterEditor.DrawFilterEditor();

                                if (GUILayout.Button("Add Filter", GUILayout.ExpandWidth(false)) && FilterEditor.IsValidFilter())
                                {
                                    listView.AddFilter(FilterEditor.GetFilter());
                                }
                            }

                            using (var scope = new GUILayout.ScrollViewScope(filterListScrollPos, GUILayout.ExpandHeight(false)))
                            {
                                filterListScrollPos = scope.scrollPosition;

                                using (new GUILayout.HorizontalScope())
                                {
                                    for(int i = 0; i < listView.FilterPreviewContent.Length; i++)
                                    {
                                        if (GUILayout.Button(listView.FilterPreviewContent[i], GUILayout.ExpandWidth(false)))
                                        {
                                            listView.RemoveFilterAt(i);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Rect controlRect = EditorGUILayout.GetControlRect(
                        GUILayout.ExpandHeight(true),
                        GUILayout.ExpandWidth(true));

                    listView?.OnGUI(controlRect);
                }

                DetectResizedWindow();

                AdjustSplitView();

                using (new GUILayout.VerticalScope(GUILayout.Width(position.width - currentScrollViewWidth - 2)))
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        using (var scope = new EditorGUI.ChangeCheckScope())
                        {
                            useAutoRefresh = GUILayout.Toggle(useAutoRefresh, UseAutoRefreshToggleContent);

                            if (scope.changed)
                            {
                                EditorPrefs.SetBool(UseAutoRefreshPreferenceKey, useAutoRefresh);
                            }
                        }

                        if (GUILayout.Button(RefreshButtonContent, GUILayout.Width(60)))
                        {
                            listView.CheckForChangesInScene();

                            hasLatestInfo = true;
                        }
                    }

                    scrollPos = GUILayout.BeginScrollView(scrollPos);

                    GUILayout.Label("Udon Behaviour:");
                    EditorGUILayout.TextArea(SelectedItem?.BehaviourName);

                    GUILayout.Space(20);

                    GUILayout.Label("Program Source Type:");
                    EditorGUILayout.TextArea(SelectedItem?.UdonProgramSourceType);

                    GUILayout.Space(20);

                    if (showSyncMetadata = EditorGUILayout.Foldout(showSyncMetadata, "Sync Metadata"))
                    {
                        GUILayout.TextArea(listView?.UdonProgramSyncMetadata);
                    }

                    GUILayout.Space(20);

                    if (showPublicVariables = EditorGUILayout.Foldout(showPublicVariables, "Public Variables"))
                    {
                        GUILayout.TextArea(listView?.PublicVariablesList);
                    }

                    GUILayout.Space(20);

                    if (showExportedSymbols = EditorGUILayout.Foldout(showExportedSymbols, "Exported Symbols"))
                    {
                        GUILayout.TextArea(listView?.UdonProgramExportedSymbolList);
                    }

                    GUILayout.Space(20);

                    if (showSymbols = EditorGUILayout.Foldout(showSymbols, "Symbols"))
                    {
                        GUILayout.TextArea(listView?.UdonProgramSymbolList);
                    }

                    GUILayout.Space(20);

                    if (showExportedEntryPoints = EditorGUILayout.Foldout(showExportedEntryPoints, "Exported Entry Points"))
                    {
                        GUILayout.TextArea(listView?.UdonProgramExportedEntryPointSymbolList);
                    }

                    GUILayout.Space(20);

                    if (showEntryPoints = EditorGUILayout.Foldout(showEntryPoints, "Entry Points"))
                    {
                        GUILayout.TextArea(listView?.UdonProgramEntryPointSymbolList);
                    }

                    GUILayout.EndScrollView();
                }
            }
        }

        private void AdjustSplitView()
        {
            GUI.Box(cursorChangeRect, GUIContent.none);
            EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeHorizontal);

            if (resize)
            {
                ResizeSplitView(true);

                if (Event.current.type == EventType.MouseUp)
                {
                    resize = false;
                }
            }
            else if (Event.current.isMouse && Event.current.type == EventType.MouseDown && cursorChangeRect.Contains(Event.current.mousePosition))
            {
                resize = true;
            }
        }

        private void ResizeSplitView(bool fromMouse = false)
        {
            if (fromMouse)
            {
                currentScrollViewWidth = Event.current.mousePosition.x;
            }

            currentScrollViewWidth = Mathf.Clamp(currentScrollViewWidth, position.width - 400, position.width - 200);

            cursorChangeRect.Set(currentScrollViewWidth, cursorChangeRect.y, cursorChangeRect.width, position.height + 48);

            Repaint();
        }

        private void DetectResizedWindow()
        {
            if (lastWindowSize != position.size)
            {
                ResizeSplitView();

                lastWindowSize = position.size;
            }
        }
    }
}
