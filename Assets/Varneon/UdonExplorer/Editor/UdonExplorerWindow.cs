﻿
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
            showSyncMetadata,
            showPublicVariables,
            showExportedSymbols,
            showSymbols,
            showExportedEntryPoints,
            showEntryPoints;
        private const string UseAutoRefreshPreferenceKey = "Varneon/UdonExplorer/UseAutoRefresh";

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
                Rect controlRect = EditorGUILayout.GetControlRect(
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true));

                listView?.OnGUI(controlRect);

                DetectResizedWindow();

                AdjustSplitView();

                using (new GUILayout.VerticalScope(GUILayout.Width(position.width - currentScrollViewWidth - 2)))
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        using (var scope = new EditorGUI.ChangeCheckScope())
                        {
                            useAutoRefresh = GUILayout.Toggle(useAutoRefresh, "Refresh On Focus");

                            if (scope.changed)
                            {
                                EditorPrefs.SetBool(UseAutoRefreshPreferenceKey, useAutoRefresh);
                            }
                        }

                        if (GUILayout.Button("Refresh", GUILayout.Width(60)))
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
                        GUILayout.TextArea(SelectedItem?.UdonProgramSyncMetadata);
                    }

                    GUILayout.Space(20);

                    if (showPublicVariables = EditorGUILayout.Foldout(showPublicVariables, "Public Variables"))
                    {
                        GUILayout.TextArea(SelectedItem?.PublicVariablesList);
                    }

                    GUILayout.Space(20);

                    if (showExportedSymbols = EditorGUILayout.Foldout(showExportedSymbols, "Exported Symbols"))
                    {
                        GUILayout.TextArea(SelectedItem?.UdonProgramExportedSymbolList);
                    }

                    GUILayout.Space(20);

                    if (showSymbols = EditorGUILayout.Foldout(showSymbols, "Symbols"))
                    {
                        GUILayout.TextArea(SelectedItem?.UdonProgramSymbolList);
                    }

                    GUILayout.Space(20);

                    if (showExportedEntryPoints = EditorGUILayout.Foldout(showExportedEntryPoints, "Exported Entry Points"))
                    {
                        GUILayout.TextArea(SelectedItem?.UdonProgramExportedEntryPointSymbolList);
                    }

                    GUILayout.Space(20);

                    if (showEntryPoints = EditorGUILayout.Foldout(showEntryPoints, "Entry Points"))
                    {
                        GUILayout.TextArea(SelectedItem?.UdonProgramEntryPointSymbolList);
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
