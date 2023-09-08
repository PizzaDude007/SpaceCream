/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;

namespace PluginMaster
{
    public class BrushCreationSettingsWindow : UnityEditor.EditorWindow
    {
        [SerializeField] PWBData _data = null;

        private bool _defaultBrushSettingsGroupOpen = false;
        private bool _defaultThumbnailSettingsGroupOpen = false;
        private bool _brushPosGroupOpen = false;
        private bool _brushRotGroupOpen = false;
        private bool _brushScaleGroupOpen = false;
        private bool _brushFlipGroupOpen = false;

        private Vector2 _mainScrollPosition = Vector2.zero;
        [UnityEditor.MenuItem("Tools/Plugin Master/Prefab World Builder/Brush Creation Settings...", false, 1140)]
        public static void ShowWindow() => GetWindow<BrushCreationSettingsWindow>();

        private static string UNDO_MSG = "Brush Creation Settings";

        private void OnEnable()
        {
            _data = PWBCore.staticData;
            UnityEditor.Undo.undoRedoPerformed += Repaint;
            titleContent = new GUIContent(PaletteManager.selectedPalette.name + " - Brush Creation Settings");
            
        }

        private void OnDisable() => UnityEditor.Undo.undoRedoPerformed -= Repaint;

        private void OnGUI()
        {
            if (PaletteManager.selectedPalette == null) return;
            UnityEditor.EditorGUIUtility.labelWidth = 60;
            var settings = PaletteManager.selectedPalette.brushCreationSettings.Clone();
            using (var scrollView = new UnityEditor.EditorGUILayout.ScrollViewScope(_mainScrollPosition,
                false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUIStyle.none))
            {
                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
                    _mainScrollPosition = scrollView.scrollPosition;
                    settings.includeSubfolders = UnityEditor.EditorGUILayout.ToggleLeft("Include subfolders",
                        settings.includeSubfolders);
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        settings.addLabelsToDroppedPrefabs = UnityEditor.EditorGUILayout.ToggleLeft("Add labels to prefabs",
                            settings.addLabelsToDroppedPrefabs);
                        using (new UnityEditor.EditorGUI.DisabledGroupScope(!settings.addLabelsToDroppedPrefabs))
                        {
                            UnityEditor.EditorGUIUtility.labelWidth = 40;
                            settings.labelsCSV = UnityEditor.EditorGUILayout.TextField("Labels:", settings.labelsCSV);
                        }
                    }

#if UNITY_2019_1_OR_NEWER
                    _defaultBrushSettingsGroupOpen
                        = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_defaultBrushSettingsGroupOpen,
                        "Default Brush Settings");
#else
                    _defaultBrushSettingsGroupOpen = EditorGUILayout.Foldout(_defaultBrushSettingsGroupOpen,
                    "Default Brush Settings");
#endif
                    if (_defaultBrushSettingsGroupOpen)
                    {
                        using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                        {
                            BrushProperties.BrushFields(settings.defaultBrushSettings, ref _brushPosGroupOpen,
                                ref _brushRotGroupOpen, ref _brushScaleGroupOpen, ref _brushFlipGroupOpen, this, UNDO_MSG);
                            GUILayout.Space(10);
                            if (GUILayout.Button("Reset to factory settings"))
                            {
                                settings.FactoryResetDefaultBrushSettings();
                                GUI.FocusControl(null);
                                Repaint();
                            }
                        }
                    }
#if UNITY_2019_1_OR_NEWER
                    UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
#endif
                    _defaultThumbnailSettingsGroupOpen
                        = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_defaultThumbnailSettingsGroupOpen,
                        "Default Thumbnail Settings");
                    if(_defaultThumbnailSettingsGroupOpen)
                    {
                        using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                        {
                            ThumbnailEditorWindow.ThumbnailSettingsGUI(settings.defaultThumbnailSettings);
                            GUILayout.Space(10);
                            if (GUILayout.Button("Reset to factory settings"))
                            {
                                settings.FactoryResetDefaultThumbnailSettings();
                                GUI.FocusControl(null);
                                Repaint();
                            }
                        }
                    }
                    UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        GUI.FocusControl(null);
                        Repaint();
                    }
                    if(check.changed)
                    {
                        UnityEditor.Undo.RegisterCompleteObjectUndo(this, UNDO_MSG);
                        PaletteManager.selectedPalette.brushCreationSettings.Copy(settings);
                        PWBCore.SetSavePending();
                    }
                }
            }
        }

        [UnityEditor.MenuItem("Assets/Clear Labels", false, 2000)]
        private static void ClearLabels()
        {
            var selection = UnityEditor.Selection.GetFiltered<Object>(UnityEditor.SelectionMode.Assets);
            foreach (var asset in selection) UnityEditor.AssetDatabase.ClearLabels(asset);
        }
    }
}
