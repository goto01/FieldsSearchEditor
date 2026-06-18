using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FieldsSearchEditor
{
    public class SearchableEditor : UnityEditor.Editor
    {
        private const string SearchFilterPrefsKey = "FieldsSearchEditor_SearchFilter";

        private GUIStyle _searchTextFieldStyle;
        private GUIStyle _clearSearchButtonStyle;
        private GUIStyle _hiddenCountLabelStyle;
        private Texture2D _hiddenCountBackground;
        private string _searchFilter;
        private List<string> _filteredProperties;
        private int _totalPropertyCount;

        public string SearchFilter
        {
            get
            {
                if (_searchFilter == null) _searchFilter = EditorPrefs.GetString(SearchFilterPrefsKey, string.Empty);
                return _searchFilter;
            }
            set
            {
                if (_searchFilter == value) return;
                EditorPrefs.SetString(SearchFilterPrefsKey, _searchFilter = value.ToUpper());
                RefreshFilteredProperties();
            }
        }

        protected virtual void Reset()
        {
            RefreshFilteredProperties();
        }

        private void TryToInitialize()
        {
            if (_filteredProperties == null) RefreshFilteredProperties();
            if (string.IsNullOrEmpty(SearchFilter) && _filteredProperties.Count == 0) RefreshFilteredProperties();
            if (_searchTextFieldStyle == null)
            {
#if UNITY_6000_0_OR_NEWER
                _searchTextFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? GUI.skin.textField;
                _clearSearchButtonStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? GUI.skin.button;
#else
                _searchTextFieldStyle = GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.textField;
                _clearSearchButtonStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton") ?? GUI.skin.button;
#endif
            }
            if (_hiddenCountLabelStyle == null)
            {
                _hiddenCountBackground = new Texture2D(1, 1);
                _hiddenCountBackground.SetPixel(0, 0, new Color(0.25f, 0.6f, 0.9f, 0.85f));
                _hiddenCountBackground.Apply();
                _hiddenCountLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { background = _hiddenCountBackground, textColor = Color.white },
                    padding = new RectOffset(8, 8, 3, 3),
                    margin = new RectOffset(0, 0, 2, 2),
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
            }
        }

        public override void OnInspectorGUI()
        {
            TryToInitialize();
            DrawSearchEditor();
            EditorGUILayout.Space();
            DrawFields();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSearchEditor()
        {
            EditorGUILayout.BeginHorizontal();
            SearchFilter = EditorGUILayout.DelayedTextField(SearchFilter, _searchTextFieldStyle);
            if (GUILayout.Button(string.Empty, _clearSearchButtonStyle)) SearchFilter = string.Empty;
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(SearchFilter))
            {
                var hiddenCount = _totalPropertyCount - _filteredProperties.Count;
                if (hiddenCount > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(
                        $"{hiddenCount} field{(hiddenCount == 1 ? "" : "s")} hidden",
                        _hiddenCountLabelStyle);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawFields()
        {
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(false))
            {
                if (_filteredProperties.Contains(iterator.name))
                    EditorGUILayout.PropertyField(iterator, true);
            }
        }

        private void RefreshFilteredProperties()
        {
            if (_filteredProperties == null) _filteredProperties = new List<string>();
            _filteredProperties.Clear();
            _totalPropertyCount = 0;
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(false))
            {
                _totalPropertyCount++;
                if (iterator.displayName.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    _filteredProperties.Add(iterator.name);
            }
        }
    }
}
