/*
Copyright (c) 2022 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2022.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;
using System.Linq;

namespace PluginMaster
{
    public class PWBPreferences : UnityEditor.EditorWindow
    {
        #region COMMON
        private int _tab = 0;
        private Vector2 _mainScrollPosition = Vector2.zero;
        [UnityEditor.MenuItem("Tools/Plugin Master/Prefab World Builder/Preferences...", false, 1250)]
        public static void ShowWindow() => GetWindow<PWBPreferences>("PWB Preferences");

        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                _tab = GUILayout.Toolbar(_tab, new string[] { "General", "Shortcuts" });
                GUILayout.FlexibleSpace();
            }
            using (var scrollView = new UnityEditor.EditorGUILayout.ScrollViewScope(_mainScrollPosition,
                false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, UnityEditor.EditorStyles.helpBox))
            {
                _mainScrollPosition = scrollView.scrollPosition;
                if (_tab == 0) GeneralSettings();
                else Shortcuts();
            }
            UpdateCombination();
        }
        #endregion

        #region GENERAL SETTINGS
        private bool _dataGroupOpen = true;
        private bool _autoSaveGroupOpen = true;
        private bool _unsavedChangesGroupOpen = true;
        private bool _gizmosGroupOpen = true;
        private bool _toolbarGroupOpen = true;
        private bool _pinToolGroupOpen = true;
        private bool _gravityToolGroupOpen = true;
        private bool _thumbnailsGroupOpen = true;
        private bool _tempCollidersGroupOpen = true;

        private void GeneralSettings()
        {
            _dataGroupOpen
                = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_dataGroupOpen, "Data Settings");
            if (_dataGroupOpen) DataGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _autoSaveGroupOpen
                = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_autoSaveGroupOpen, "Auto-Save Settings");
            if (_autoSaveGroupOpen) AutoSaveGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _unsavedChangesGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_unsavedChangesGroupOpen,
                "Unsaved Changes");
            if (_unsavedChangesGroupOpen) UnsavedChangesGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _gizmosGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_gizmosGroupOpen, "Gizmos");
            if (_gizmosGroupOpen) GizmosGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _toolbarGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_toolbarGroupOpen, "Toolbar");
            if (_toolbarGroupOpen) ToolbarGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _tempCollidersGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_tempCollidersGroupOpen,
                "Temp Colliders");
            if (_tempCollidersGroupOpen) TempCollidersGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _pinToolGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_pinToolGroupOpen, "Pin Tool");
            if (_pinToolGroupOpen) PinToolGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _gravityToolGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_gravityToolGroupOpen, "Gravity Tool");
            if (_gravityToolGroupOpen) GravityToolGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();

            _thumbnailsGroupOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(_thumbnailsGroupOpen, "Thumnails");
            if (_thumbnailsGroupOpen) ThumbnailsGroup();
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DataGroup()
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 90;
                UnityEditor.EditorGUILayout.LabelField("Data directory:"
                    , PWBSettings.fullDataDir, UnityEditor.EditorStyles.textField);
                if (GUILayout.Button("...", GUILayout.Width(29), GUILayout.Height(20)))
                {
                    var directory = UnityEditor.EditorUtility.OpenFolderPanel("Select data directory...",
                    PWBSettings.fullDataDir, "Data");
                    if (System.IO.Directory.Exists(directory)) PWBSettings.SetDataDir(directory);
                }
            }
        }

        private void AutoSaveGroup()
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.helpBox))
            {
                GUILayout.Label("Auto-Save Every:");
                PWBCore.staticData.autoSavePeriodMinutes
                    = UnityEditor.EditorGUILayout.IntSlider(PWBCore.staticData.autoSavePeriodMinutes, 1, 10);
                GUILayout.Label("minutes");
                GUILayout.FlexibleSpace();
            }
        }

        private static readonly string[] _unsavedChangesActionNames = { "Ask if want to save", "Save", "Discard" };
        private void UnsavedChangesGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 45;
                PWBCore.staticData.unsavedChangesAction = (PWBData.UnsavedChangesAction)
                    UnityEditor.EditorGUILayout.Popup("Action:",
                    (int)PWBCore.staticData.unsavedChangesAction, _unsavedChangesActionNames);
            }
        }

        private void GizmosGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 110;
                PWBCore.staticData.controPointSize = UnityEditor.EditorGUILayout.IntSlider("Control Point Size:",
                    PWBCore.staticData.controPointSize, 1, 3);

            }
        }

        private void ToolbarGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                PWBCore.staticData.closeAllWindowsWhenClosingTheToolbar
                        = UnityEditor.EditorGUILayout.ToggleLeft("Close all windows when closing the toolbar",
                        PWBCore.staticData.closeAllWindowsWhenClosingTheToolbar);
            }
        }

        private void PinToolGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 155;
                PinManager.rotationSnapValue = UnityEditor.EditorGUILayout.Slider("Rotation snap value (Deg):",
                    PinManager.rotationSnapValue, 0f, 360f);
            }
        }

        private void GravityToolGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 182;
                GravityToolManager.surfaceDistanceSensitivity
                    = UnityEditor.EditorGUILayout.Slider("Distance to surface sensitivity:",
                     GravityToolManager.surfaceDistanceSensitivity, 0f, 1f);
            }
        }
        private void ThumbnailsGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                PWBCore.staticData.thumbnailLayer = UnityEditor.EditorGUILayout.IntField("Thumbnail Layer:",
                    PWBCore.staticData.thumbnailLayer);
            }
        }

        private static readonly string[] _tempCollidersActionNames = { "Never create temp colliders",
            "Create all temp colliders at once", "Create temp colliders within the frustum" };
        private void TempCollidersGroup()
        {
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                UnityEditor.EditorGUIUtility.labelWidth = 45;
                PWBCore.staticData.tempCollidersAction = (PWBData.TempCollidersAction)
                    UnityEditor.EditorGUILayout.Popup("Action:",
                    (int)PWBCore.staticData.tempCollidersAction, _tempCollidersActionNames);
            }
        }
        #endregion

        #region SHORTCUTS
        private bool _pinCategory = false;
        private bool _brushCategory = false;
        private bool _gravityCategory = false;
        private bool _lineCategory = false;
        private bool _shapeCategory = false;
        private bool _tilingCategory = false;
        private bool _eraserCategory = false;
        private bool _replacerCategory = false;
        private bool _selectionCategory = false;
        private bool _gridCategory = false;
        private bool _paletteCategory = false;
        private bool _toolbarCategory = true;

        private PWBKeyShortcut _selectedShortcut = null;
        private static Texture2D _warningTexture = null;
        private static Texture2D warningTexture
        {
            get
            {
                if (_warningTexture == null) _warningTexture = Resources.Load<Texture2D>("Sprites/Warning");
                return _warningTexture;
            }
        }

        private UnityEditor.IMGUI.Controls.MultiColumnHeaderState _multiColumnHeaderState;
        private UnityEditor.IMGUI.Controls.MultiColumnHeader _multiColumnHeader;
        private UnityEditor.IMGUI.Controls.MultiColumnHeaderState.Column[] _columns;

        private void InitializeMultiColumn()
        {
            _columns = new UnityEditor.IMGUI.Controls.MultiColumnHeaderState.Column[]
            {
                new UnityEditor.IMGUI.Controls.MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = 320,
                    width = 330,
                    canSort = false,
                    headerContent = new GUIContent("Command"),
                    headerTextAlignment = TextAlignment.Left,
                },
                new UnityEditor.IMGUI.Controls.MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = 266,
                    maxWidth = 266,
                    width = 266,
                    canSort = false,
                    headerContent = new GUIContent("Shortcut"),
                    headerTextAlignment = TextAlignment.Left,
                }
            };
            _multiColumnHeaderState = new UnityEditor.IMGUI.Controls.MultiColumnHeaderState(columns: _columns);
            _multiColumnHeader = new UnityEditor.IMGUI.Controls.MultiColumnHeader(state: _multiColumnHeaderState);
            _multiColumnHeader.visibleColumnsChanged += (multiColumnHeader) => multiColumnHeader.ResizeToFit();
            _multiColumnHeader.ResizeToFit();
        }

        private static readonly Color _lighterColor = Color.white * 0.3f;
        private static readonly Color _darkerColor = Color.white * 0.1f;

        private void SelectProfileItem(object value)
        {
            PWBSettings.selectedProfileIdx = (int)value;
            Repaint();
        }

        private static readonly EventModifiers[] _modifierOptions = new EventModifiers[]
        {
            EventModifiers.None,
            EventModifiers.Control,
            EventModifiers.Alt,
            EventModifiers.Shift,
            EventModifiers.Control | EventModifiers.Alt,
            EventModifiers.Control | EventModifiers.Shift,
            EventModifiers.Alt | EventModifiers.Shift,
            EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift,
        };


        private static readonly string[] _modifierDisplayedOptions = new string[]
        {
            "Disabled",
            "Ctrl",
            "Alt",
            "Shift",
            "Ctrl+Alt",
            "Ctrl+Shift",
            "Alt+Shift",
            "Ctrl+Alt+Shift"
        };

        private static readonly string[] _mouseEventsDisplayedOptions = new string[]
        {
            "Mouse scroll wheel",
            "R Btn horizontal drag",
            "R Btn vertical drag",
            "Mid Btn horizontal drag",
            "Mid Btn vertical drag"
        };

        private void Shortcuts()
        {
            if (_multiColumnHeader == null) InitializeMultiColumn();
            void SelectCategory(ref bool category)
            {
                _gridCategory = false;
                _pinCategory = false;
                _brushCategory = false;
                _gravityCategory = false;
                _lineCategory = false;
                _shapeCategory = false;
                _tilingCategory = false;
                _eraserCategory = false;
                _replacerCategory = false;
                _selectionCategory = false;
                _paletteCategory = false;
                _toolbarCategory = false;
                category = true;
            }
            string shortcutString(PWBKeyShortcut shortcut)
            {
                if ((object)shortcut == (object)_selectedShortcut) return string.Empty;
                return shortcut.combination.ToString();
            }
            GUIStyle shortcutStyle(PWBKeyShortcut shortcut)
            {
                if ((object)shortcut == (object)_selectedShortcut) return UnityEditor.EditorStyles.textField;
                return UnityEditor.EditorStyles.label;
            }

            var categoryButton = new GUIStyle(UnityEditor.EditorStyles.toolbarButton);
            categoryButton.alignment = TextAnchor.UpperLeft;

            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.helpBox))
            {
                GUILayout.Label("Profile:");
                if (GUILayout.Button(PWBSettings.shortcuts.profileName,
                    UnityEditor.EditorStyles.popup, GUILayout.MinWidth(100)))
                {
                    GUI.FocusControl(null);
                    var menu = new UnityEditor.GenericMenu();
                    var profileNames = PWBSettings.shotcutProfileNames;
                    for (int i = 0; i < profileNames.Length; ++i)
                        menu.AddItem(new GUIContent(profileNames[i]),
                            PWBSettings.selectedProfileIdx == i, SelectProfileItem, i);
                    menu.AddSeparator(string.Empty);
                    menu.AddItem(new GUIContent("Factory Reset Selected Profile"), false, PWBSettings.ResetSelectedProfile);
                    menu.ShowAsContext();
                }
                GUILayout.FlexibleSpace();
            }

            using (new GUILayout.HorizontalScope())
            {
                const int categoryColumnW = 100;
                using (new GUILayout.VerticalScope(GUILayout.Width(categoryColumnW)))
                {
                    if (GUILayout.Toggle(_toolbarCategory, "Toolbar", categoryButton)) SelectCategory(ref _toolbarCategory);
                    if (GUILayout.Toggle(_pinCategory, "Pin", categoryButton)) SelectCategory(ref _pinCategory);
                    if (GUILayout.Toggle(_brushCategory, "Brush", categoryButton)) SelectCategory(ref _brushCategory);
                    if (GUILayout.Toggle(_gravityCategory, "Gravity", categoryButton)) SelectCategory(ref _gravityCategory);
                    if (GUILayout.Toggle(_lineCategory, "Line", categoryButton)) SelectCategory(ref _lineCategory);
                    if (GUILayout.Toggle(_shapeCategory, "Shape", categoryButton)) SelectCategory(ref _shapeCategory);
                    if (GUILayout.Toggle(_tilingCategory, "Tiling", categoryButton)) SelectCategory(ref _tilingCategory);
                    if (GUILayout.Toggle(_eraserCategory, "Eraser", categoryButton)) SelectCategory(ref _eraserCategory);
                    if (GUILayout.Toggle(_replacerCategory, "Replacer", categoryButton)) SelectCategory(ref _replacerCategory);
                    if (GUILayout.Toggle(_selectionCategory, "Selection", categoryButton))
                        SelectCategory(ref _selectionCategory);
                    if (GUILayout.Toggle(_gridCategory, "Grid", categoryButton)) SelectCategory(ref _gridCategory);
                    if (GUILayout.Toggle(_paletteCategory, "Palette", categoryButton)) SelectCategory(ref _paletteCategory);

                    using (new UnityEditor.EditorGUI.DisabledGroupScope(true))
                        GUILayout.Box(new GUIContent(), new GUIStyle(categoryButton) { fixedHeight = 427 });
                }
                GUILayout.Space(2);
                using (new GUILayout.VerticalScope())
                {
                    var minX = categoryColumnW + 10;
                    var shorcutPanelRect = new Rect(minX, 28, position.width - categoryColumnW - 20, position.height);

                    float columnHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
                    Rect columnRectPrototype = new Rect(shorcutPanelRect) { height = columnHeight };

                    _multiColumnHeader.OnGUI(rect: columnRectPrototype, xScroll: 0.0f);

                    void ContextMenu(PWBShortcut shortcut, UnityEditor.GenericMenu.MenuFunction DisableFunction)
                    {
                        bool shortcutIsUSM = false;
                        if (shortcut is PWBKeyShortcut)
                        {
                            var keyShortcut = shortcut as PWBKeyShortcut;
                            shortcutIsUSM = keyShortcut.combination is PWBKeyCombinationUSM;
                        }
                        void ResetToDefault()
                        {
                            PWBSettings.ResetShortcutToDefault(shortcut);
                            if (shortcutIsUSM)
                            {
                                var keyShortcut = shortcut as PWBKeyShortcut;
                                (keyShortcut.combination as PWBKeyCombinationUSM).Reset();
                            }
                            PWBSettings.UpdateShrotcutsConflictsAndSaveFile();
                        }
                        var menu = new UnityEditor.GenericMenu();
                        menu.AddItem(new GUIContent("Reset to default"), false, ResetToDefault);
                        if (!shortcutIsUSM) menu.AddItem(new GUIContent("Disable shortcut"), false, DisableFunction);
                        menu.ShowAsContext();
                    }

                    int row = 0;
                    void ShortcutRow(PWBKeyShortcut shortcut)
                    {
                        Rect rowRect = new Rect(columnRectPrototype);

                        rowRect.y += columnHeight * (++row);
                        UnityEditor.EditorGUI.DrawRect(rowRect, row % 2 == 0 ? _darkerColor : _lighterColor);

                        Rect columnRect = _multiColumnHeader.GetColumnRect(0);
                        columnRect.y = rowRect.y;

                        var cellRect = _multiColumnHeader.GetCellRect(0, columnRect);
                        cellRect.x += minX;
                        UnityEditor.EditorGUI.LabelField(cellRect, new GUIContent(shortcut.name));

                        ////////////////
                        columnRect = _multiColumnHeader.GetColumnRect(1);
                        columnRect.y = rowRect.y;

                        cellRect = _multiColumnHeader.GetCellRect(1, columnRect);
                        cellRect.x += minX;
                        cellRect.width -= 20;
                        UnityEditor.EditorGUI.LabelField(cellRect, new GUIContent(shortcutString(shortcut)),
                            shortcutStyle(shortcut));

                        if (cellRect.Contains(Event.current.mousePosition)
                            && Event.current.type == EventType.MouseDown)
                        {
                            if (Event.current.button == 0)
                            {
                                _selectedShortcut = shortcut;
                                Repaint();
                            }
                            else if (Event.current.button == 1)
                            {
                                void Remove()
                                {
                                    shortcut.combination.Set(KeyCode.None);
                                    PWBSettings.UpdateShrotcutsConflictsAndSaveFile();
                                }
                                ContextMenu(shortcut, Remove);
                            }
                        }

                        if (!shortcut.conflicted) return;
                        cellRect.x += cellRect.width;
                        cellRect.width = 20;
                        UnityEditor.EditorGUI.LabelField(cellRect, new GUIContent(warningTexture));
                    }

                    void MouseShortcutRow(PWBMouseShortcut shortcut, bool scrollWheelOnly = false)
                    {
                        Rect rowRect = new Rect(columnRectPrototype);

                        rowRect.y += columnHeight * (++row);
                        UnityEditor.EditorGUI.DrawRect(rowRect, row % 2 == 0 ? _darkerColor : _lighterColor);

                        Rect columnRect = _multiColumnHeader.GetColumnRect(0);
                        columnRect.y = rowRect.y;

                        var cellRect = _multiColumnHeader.GetCellRect(0, columnRect);
                        cellRect.x += minX;
                        UnityEditor.EditorGUI.LabelField(cellRect, new GUIContent(shortcut.name));

                        ////////////////
                        columnRect = _multiColumnHeader.GetColumnRect(1);
                        columnRect.y = rowRect.y;

                        cellRect = _multiColumnHeader.GetCellRect(1, columnRect);
                        cellRect.x += minX;

                        if (cellRect.Contains(Event.current.mousePosition)
                           && Event.current.type == EventType.MouseDown && Event.current.button == 1)
                        {
                            void Remove()
                            {
                                shortcut.combination.Set(EventModifiers.None, PWBMouseCombination.MouseEvents.NONE);
                                PWBSettings.UpdateShrotcutsConflictsAndSaveFile();
                            }
                            ContextMenu(shortcut, Remove);
                        }

                        cellRect.width = 100;

                        int modId = System.Array.IndexOf(_modifierOptions, shortcut.combination.modifiers);
                        PWBMouseCombination.MouseEvents mouseEvent = shortcut.combination.mouseEvent;
                        void SetCombination()
                        {
                            shortcut.combination.Set(_modifierOptions[modId], mouseEvent);
                            PWBSettings.UpdateShrotcutsConflictsAndSaveFile();
                        }
                        using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                        {
                            modId = UnityEditor.EditorGUI.Popup(cellRect, modId,
                                _modifierDisplayedOptions);
                            if (check.changed)
                            {
                                var combi = new PWBMouseCombination(_modifierOptions[modId], mouseEvent);
                                var combiString = _modifierDisplayedOptions[modId];
                                if (modId > 0) combiString += " + Mouse scroll wheel";
                                if (PWBSettings.shortcuts.CheckMouseConflicts(combi, shortcut, out string conflicts))
                                {
                                    if (BindingConflictDialog(combiString, conflicts)) SetCombination();
                                }
                                else SetCombination();
                            }
                        }

                        cellRect.x += cellRect.width;
                        cellRect.width = 149;
                        if (shortcut.combination.modifiers != EventModifiers.None)
                        {
                            if (scrollWheelOnly) UnityEditor.EditorGUI.LabelField(cellRect, "+ Mouse scroll wheel");
                            else
                            {
                                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                                {
                                    mouseEvent = (PWBMouseCombination.MouseEvents)(UnityEditor.EditorGUI.Popup(cellRect,
                                        (int)mouseEvent - 1, _mouseEventsDisplayedOptions) + 1);
                                    if (check.changed)
                                    {
                                        var combi = new PWBMouseCombination(_modifierOptions[modId], mouseEvent);
                                        var combiString = _modifierDisplayedOptions[modId];
                                        if (modId > 0)
                                            combiString += " + " + _mouseEventsDisplayedOptions[(int)mouseEvent - 1];
                                        if (PWBSettings.shortcuts.CheckMouseConflicts(combi, shortcut, out string conflicts))
                                        {
                                            if (BindingConflictDialog(combiString, conflicts)) SetCombination();
                                        }
                                        else SetCombination();
                                    }
                                }
                            }
                        }

                        if (!shortcut.conflicted) return;
                        cellRect.x += cellRect.width;
                        cellRect.width = 20;
                        UnityEditor.EditorGUI.LabelField(cellRect, new GUIContent(warningTexture));
                    }

                    void EditModeRows()
                    {
                        ShortcutRow(PWBSettings.shortcuts.editModeToggle);
                        ShortcutRow(PWBSettings.shortcuts.editModeDeleteItemAndItsChildren);
                        ShortcutRow(PWBSettings.shortcuts.editModeDeleteItemButNotItsChildren);
                        ShortcutRow(PWBSettings.shortcuts.editModeSelectParent);
                    }
                    void SelectionRows()
                    {
                        ShortcutRow(PWBSettings.shortcuts.selectionRotate90XCW);
                        ShortcutRow(PWBSettings.shortcuts.selectionRotate90XCCW);
                        ShortcutRow(PWBSettings.shortcuts.selectionRotate90YCW);
                        ShortcutRow(PWBSettings.shortcuts.selectionRotate90YCCW);
                        ShortcutRow(PWBSettings.shortcuts.selectionRotate90ZCW);
                        ShortcutRow(PWBSettings.shortcuts.selectionRotate90ZCCW);
                    }
                    if (_toolbarCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.toolbarPinToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarBrushToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarGravityToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarLineToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarShapeToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarTilingToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarReplacerToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarEraserToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarSelectionToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarExtrudeToggle);
                        ShortcutRow(PWBSettings.shortcuts.toolbarMirrorToggle);
                    }
                    else if (_pinCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.pinMoveHandlesUp);
                        ShortcutRow(PWBSettings.shortcuts.pinMoveHandlesDown);
                        ShortcutRow(PWBSettings.shortcuts.pinSelectPrevHandle);
                        ShortcutRow(PWBSettings.shortcuts.pinSelectNextHandle);
                        ShortcutRow(PWBSettings.shortcuts.pinSelectPivotHandle);
                        ShortcutRow(PWBSettings.shortcuts.pinToggleRepeatItem);
                        ShortcutRow(PWBSettings.shortcuts.pinResetScale);

                        ShortcutRow(PWBSettings.shortcuts.pinRotate90YCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotate90YCCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotateAStepYCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotateAStepYCCW);

                        ShortcutRow(PWBSettings.shortcuts.pinRotate90XCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotate90XCCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotateAStepXCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotateAStepXCCW);

                        ShortcutRow(PWBSettings.shortcuts.pinRotate90ZCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotate90ZCCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotateAStepZCW);
                        ShortcutRow(PWBSettings.shortcuts.pinRotateAStepZCCW);

                        ShortcutRow(PWBSettings.shortcuts.pinResetRotation);

                        ShortcutRow(PWBSettings.shortcuts.pinAdd1UnitToSurfDist);
                        ShortcutRow(PWBSettings.shortcuts.pinSubtract1UnitFromSurfDist);
                        ShortcutRow(PWBSettings.shortcuts.pinAdd01UnitToSurfDist);
                        ShortcutRow(PWBSettings.shortcuts.pinSubtract01UnitFromSurfDist);

                        ShortcutRow(PWBSettings.shortcuts.pinResetSurfDist);

                        ShortcutRow(PWBSettings.shortcuts.pinSelectPreviousItem);
                        ShortcutRow(PWBSettings.shortcuts.pinSelectNextItem);

                        MouseShortcutRow(PWBSettings.shortcuts.pinSelectNextItemScroll, true);
                        MouseShortcutRow(PWBSettings.shortcuts.pinScale);

                        MouseShortcutRow(PWBSettings.shortcuts.pinRotateAroundY);
                        MouseShortcutRow(PWBSettings.shortcuts.pinRotateAroundYSnaped);
                        MouseShortcutRow(PWBSettings.shortcuts.pinRotateAroundX);
                        MouseShortcutRow(PWBSettings.shortcuts.pinRotateAroundXSnaped);
                        MouseShortcutRow(PWBSettings.shortcuts.pinRotateAroundZ);
                        MouseShortcutRow(PWBSettings.shortcuts.pinRotateAroundZSnaped);

                        MouseShortcutRow(PWBSettings.shortcuts.pinSurfDist);
                    }
                    else if (_brushCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.brushUpdatebrushstroke);
                        ShortcutRow(PWBSettings.shortcuts.brushResetRotation);

                        MouseShortcutRow(PWBSettings.shortcuts.brushRadius);
                        MouseShortcutRow(PWBSettings.shortcuts.brushDensity);
                        MouseShortcutRow(PWBSettings.shortcuts.brushRotate);
                    }
                    else if (_gravityCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.brushUpdatebrushstroke);
                        ShortcutRow(PWBSettings.shortcuts.brushResetRotation);

                        ShortcutRow(PWBSettings.shortcuts.gravityAdd1UnitToSurfDist);
                        ShortcutRow(PWBSettings.shortcuts.gravitySubtract1UnitFromSurfDist);
                        ShortcutRow(PWBSettings.shortcuts.gravityAdd01UnitToSurfDist);
                        ShortcutRow(PWBSettings.shortcuts.gravitySubtract01UnitFromSurfDist);

                        MouseShortcutRow(PWBSettings.shortcuts.gravitySurfDist);
                        MouseShortcutRow(PWBSettings.shortcuts.brushRadius);
                        MouseShortcutRow(PWBSettings.shortcuts.brushDensity);
                        MouseShortcutRow(PWBSettings.shortcuts.brushRotate);
                    }
                    else if (_lineCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.lineSelectAllPoints);
                        ShortcutRow(PWBSettings.shortcuts.lineDeselectAllPoints);
                        ShortcutRow(PWBSettings.shortcuts.lineToggleCurve);
                        ShortcutRow(PWBSettings.shortcuts.lineToggleClosed);
                        EditModeRows();
                        MouseShortcutRow(PWBSettings.shortcuts.lineEditGap);
                    }
                    else if (_shapeCategory)
                    {
                        EditModeRows();
                    }
                    else if (_tilingCategory)
                    {
                        SelectionRows();
                        EditModeRows();
                        MouseShortcutRow(PWBSettings.shortcuts.tilingEditSpacing1);
                        MouseShortcutRow(PWBSettings.shortcuts.tilingEditSpacing2);
                    }
                    else if (_eraserCategory)
                    {
                        MouseShortcutRow(PWBSettings.shortcuts.brushRadius);
                    }
                    else if (_replacerCategory)
                    {
                        MouseShortcutRow(PWBSettings.shortcuts.brushRadius);
                    }
                    else if (_selectionCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.selectionTogglePositionHandle);
                        ShortcutRow(PWBSettings.shortcuts.selectionToggleRotationHandle);
                        ShortcutRow(PWBSettings.shortcuts.selectionToggleScaleHandle);
                        ShortcutRow(PWBSettings.shortcuts.selectionEditCustomHandle);
                        SelectionRows();
                    }
                    else if (_gridCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.gridEnableShortcuts);
                        ShortcutRow(PWBSettings.shortcuts.gridToggle);
                        ShortcutRow(PWBSettings.shortcuts.gridToggleSnaping);
                        ShortcutRow(PWBSettings.shortcuts.gridToggleLock);
                        ShortcutRow(PWBSettings.shortcuts.gridSetOriginPosition);
                        ShortcutRow(PWBSettings.shortcuts.gridSetOriginRotation);
                        ShortcutRow(PWBSettings.shortcuts.gridSetSize);
                        ShortcutRow(PWBSettings.shortcuts.gridFrameOrigin);
                        ShortcutRow(PWBSettings.shortcuts.gridTogglePositionHandle);
                        ShortcutRow(PWBSettings.shortcuts.gridToggleRotationHandle);
                        ShortcutRow(PWBSettings.shortcuts.gridToggleSpacingHandle);
                        ShortcutRow(PWBSettings.shortcuts.gridMoveOriginUp);
                        ShortcutRow(PWBSettings.shortcuts.gridMoveOriginDown);
                    }
                    else if (_paletteCategory)
                    {
                        ShortcutRow(PWBSettings.shortcuts.paletteDeleteBrush);
                        ShortcutRow(PWBSettings.shortcuts.palettePreviousBrush);
                        ShortcutRow(PWBSettings.shortcuts.paletteNextBrush);
                        ShortcutRow(PWBSettings.shortcuts.palettePreviousPalette);
                        ShortcutRow(PWBSettings.shortcuts.paletteNextPalette);
                        ShortcutRow(PWBSettings.shortcuts.palettePickBrush);
                        MouseShortcutRow(PWBSettings.shortcuts.paletteNextBrushScroll, true);
                        MouseShortcutRow(PWBSettings.shortcuts.paletteNextPaletteScroll, true);
                    }
                    GUILayout.Space((row + 2) * columnHeight);
                    if (_gridCategory)
                    {
                        UnityEditor.EditorGUILayout.HelpBox("These shortcuts work in two steps."
                        + "\nFirst you have to activate the shortcuts with "
                        + PWBSettings.shortcuts.gridEnableShortcuts.combination
                        + ".\nFor example to toggle the grid you have to press "
                        + PWBSettings.shortcuts.gridEnableShortcuts.combination + " and then "
                        + PWBSettings.shortcuts.gridToggle.combination + ".",
                       UnityEditor.MessageType.Info);
                    }
                }
            }
        }

        private bool BindingConflictDialog(string combi, string conflicts)
            => UnityEditor.EditorUtility.DisplayDialog("Binding Conflict", "The key " + combi
                + " is already assigned to: \n" + conflicts + "\n Do you want to create the conflict?",
                "Create Conflict", "Cancel");


        private void UpdateCombination()
        {
            if (_selectedShortcut == null) return;
            if (Event.current == null) return;
            if (Event.current.type != EventType.KeyDown) return;
            if (Event.current.keyCode == KeyCode.Escape)
            {
                Repaint();
                _selectedShortcut = null;
                return;
            }
            if (Event.current.keyCode < KeyCode.Space || Event.current.keyCode > KeyCode.F15) return;
            var combi = new PWBKeyCombination(Event.current.keyCode, Event.current.modifiers);

            void SetCombination()
            {
                _selectedShortcut.combination.Set(Event.current.keyCode, Event.current.modifiers);
                if (_selectedShortcut.combination is PWBKeyCombinationUSM)
                    (_selectedShortcut.combination as PWBKeyCombinationUSM).Rebind(Event.current.keyCode,
                        Event.current.modifiers);
                PWBSettings.UpdateShrotcutsConflictsAndSaveFile();
            }
            if (PWBSettings.shortcuts.CheckConflicts(combi, _selectedShortcut, out string conflicts))
            {
                if (BindingConflictDialog(combi.ToString(), conflicts)) SetCombination();
            }
            else SetCombination();
            _selectedShortcut = null;
            Repaint();
        }
        #endregion
    }
}
