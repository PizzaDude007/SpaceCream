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
using System.Linq;
using UnityEngine;

namespace PluginMaster
{
    public class PrefabPalette : UnityEditor.EditorWindow, ISerializationCallbackReceiver
    {
        #region COMMON
        private GUISkin _skin = null;

        [SerializeField] private PaletteManager _paletteManager = null;
        private bool _loadFromFile = false;
        private bool _undoRegistered = false;

        private static PrefabPalette _instance = null;
        public static PrefabPalette instance => _instance;
        [UnityEditor.MenuItem("Tools/Plugin Master/Prefab World Builder/Palette...", false, 1110)]
        public static void ShowWindow() => _instance = GetWindow<PrefabPalette>("Palette");
        private static bool _repaint = false;
        public static void RepainWindow()
        {
            if (_instance != null) _instance.Repaint();
            _repaint = true;
        }

        public static void OnChangeRepaint()
        {
            if (_instance != null)
            {
                _instance.OnPaletteChange();
                RepainWindow();
            }
        }
        public static void CloseWindow()
        {
            if (_instance != null) _instance.Close();
        }

        private void OnEnable()
        {
            _instance = this;
            _paletteManager = PaletteManager.instance;
            _skin = Resources.Load<GUISkin>("PWBSkin");
            if (_skin == null)
            {
                CloseWindow();
                return;
            }
            _toggleStyle = _skin.GetStyle("PaletteToggle");
            _loadingIcon = Resources.Load<Texture2D>("Sprites/Loading");
            _toggleStyle.margin = new RectOffset(4, 4, 4, 4);
            _dropdownIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/DropdownArrow"));
            _labelIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Label"), "Filter by label");
            _selectionFilterIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/SelectionFilter"),
                "Filter by selection");
            _newBrushIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/New"), "New Brush");
            _deleteBrushIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Delete"), "Delete Brush");
            _pickerIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Picker"), "Brush Picker");
            _clearFilterIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Clear"));
            _settingsIcon = new GUIContent(Resources.Load<Texture2D>("Sprites/Settings"));
            _cursorStyle = _skin.GetStyle("Cursor");
            _visibleTabCount = PaletteManager.paletteNames.Length;
            autoRepaintOnSceneChange = true;
            UpdateLabelFilter();
            UpdateFilteredList(false);
            PaletteManager.ClearSelection(false);
            UnityEditor.Undo.undoRedoPerformed += OnPaletteChange;
            AutoSave.QuickSave();
            if (System.IO.Directory.GetFiles(PWBData.palettesDirectory, "txt").Length == 0)
                PaletteManager.instance.LoadPaletteFiles();
        }

        private void OnDisable() => UnityEditor.Undo.undoRedoPerformed -= OnPaletteChange;

        private void OnDestroy() => ToolManager.OnPaletteClosed();
        public static void ClearUndo()
        {
            if (_instance == null) return;
            UnityEditor.Undo.ClearUndo(_instance);
        }


        private void OnGUI()
        {
            if (_skin == null)
            {
                Close();
                return;
            }
            if (_loadFromFile && Event.current.type == EventType.Repaint)
            {
                _loadFromFile = false;
                if (!PWBCore.staticData.saving) PWBCore.LoadFromFile();
                UpdateFilteredList(false);
                return;
            }
            if (_contextBrushAdded)
            {
                RegisterUndo("Add Brush");
                PaletteManager.selectedPalette.AddBrush(_newContextBrush);
                _newContextBrush = null;
                PaletteManager.selectedBrushIdx = PaletteManager.selectedPalette.brushes.Length - 1;
                _contextBrushAdded = false;
                OnPaletteChange();
                return;
            }
            try
            {
                TabBar();
                if (PaletteManager.paletteData.Length == 0) return;
                SearchBar();
                Palette();
            }
            catch
            {
                RepainWindow();
            }
            var eventType = Event.current.rawType;
            if (eventType == EventType.MouseMove || eventType == EventType.MouseUp)
            {
                _moveBrush.to = -1;
                draggingBrush = false;
                _showCursor = false;
            }
            else if (PWBSettings.shortcuts.paletteDeleteBrush.Check()) OnDelete();
        }

        private void Update()
        {
            if (mouseOverWindow != this)
            {
                _moveBrush.to = -1;
                _showCursor = false;
            }
            else if (draggingBrush) _showCursor = true;
            if (_repaint)
            {
                _repaint = false;
                Repaint();
            }
            if (_frameSelectedBrush && _newSelectedPositionSet) DoFrameSelectedBrush();
            if (PaletteManager.savePending) PaletteManager.SaveIfPending();
        }

        private void RegisterUndo(string name)
        {
            _undoRegistered = true;
            if (PWBCore.staticData.undoPalette) UnityEditor.Undo.RegisterCompleteObjectUndo(this, name);
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            _repaint = true;
            if (!_undoRegistered) _loadFromFile = true;
            PaletteManager.ClearSelection(false);
        }

        public void UpdateAllThumbnails() => PaletteManager.UpdateAllThumbnails();
        #endregion

        #region PALETTE
        private Vector2 _scrollPosition;
        private Rect _scrollViewRect;
        private Vector2 _prevSize;
        private int _columnCount = 1;
        private GUIStyle _toggleStyle = null;
        private const int MIN_ICON_SIZE = 24;
        private const int MAX_ICON_SIZE = 256;
        public const int DEFAULT_ICON_SIZE = 64;
        private int _prevIconSize = DEFAULT_ICON_SIZE;

        private GUIContent _dropdownIcon = null;
        private bool _draggingBrush = false;
        private bool _showCursor = false;
        private Rect _cursorRect;
        private GUIStyle _cursorStyle = null;
        private (int from, int to, bool perform) _moveBrush = (0, 0, false);

        private bool draggingBrush
        {
            get => _draggingBrush;
            set
            {
                _draggingBrush = value;
                wantsMouseMove = value;
                wantsMouseEnterLeaveWindow = value;
            }
        }

        private void Palette()
        {
            UpdateColumnCount();

            _prevIconSize = PaletteManager.iconSize;

            if (_moveBrush.perform)
            {
                RegisterUndo("Change Brush Order");
                var selection = PaletteManager.idxSelection;
                PaletteManager.selectedPalette.Swap(_moveBrush.from, _moveBrush.to, ref selection);
                PaletteManager.idxSelection = selection;
                if (selection.Length == 1) PaletteManager.selectedBrushIdx = selection[0];
                _moveBrush.perform = false;
                UpdateFilteredList(false);
            }
            BrushInputData toggleData = null;

            using (var scrollView = new UnityEditor.EditorGUILayout.ScrollViewScope(_scrollPosition, false, false,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, _skin.box))
            {
                _scrollPosition = scrollView.scrollPosition;
                Brushes(ref toggleData);
                if (_showCursor) GUI.Box(_cursorRect, string.Empty, _cursorStyle);
            }
            _scrollViewRect = GUILayoutUtility.GetLastRect();
            if (PaletteManager.selectedPalette.brushCount == 0) DropBox();

            Bottom();

            BrushMouseEventHandler(toggleData);
            PaletteContext();
            DropPrefab();
        }

        private void UpdateColumnCount()
        {
            if (PaletteManager.paletteCount == 0) return;
            var paletteData = PaletteManager.selectedPalette;
            var brushes = paletteData.brushes;
            if (_scrollViewRect.width > MIN_ICON_SIZE)
            {
                if (_prevSize != position.size || _prevIconSize != PaletteManager.iconSize || _repaint)
                {
                    var iconW = (float)((PaletteManager.iconSize + 4) * brushes.Length + 6) / brushes.Length;
                    _columnCount = Mathf.Max((int)(_scrollViewRect.width / iconW), 1);
                    var rowCount = Mathf.CeilToInt((float)brushes.Length / _columnCount);
                    var h = rowCount * (PaletteManager.iconSize + 4) + 42;

                    if (h > _scrollViewRect.height)
                    {
                        iconW = (float)((PaletteManager.iconSize + 4) * brushes.Length + 17) / brushes.Length;
                        _columnCount = Mathf.Max((int)(_scrollViewRect.width / iconW), 1);
                    }
                }
                _prevSize = position.size;
            }
        }

        public void OnPaletteChange()
        {
            UpdateLabelFilter();
            UpdateFilteredList(false);
            _repaint = true;
            UpdateColumnCount();
            Repaint();
        }
        #endregion

        #region BOTTOM
        private GUIContent _newBrushIcon = null;
        private GUIContent _deleteBrushIcon = null;
        private GUIContent _pickerIcon = null;
        private GUIContent _settingsIcon = null;
        private void Bottom()
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar, GUILayout.Height(18)))
            {
                if (PaletteManager.selectedPalette.brushCount > 0)
                {
                    var sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                    sliderStyle.margin.top = 0;
                    PaletteManager.iconSize = (int)GUILayout.HorizontalSlider(
                        (float)PaletteManager.iconSize,
                        (float)MIN_ICON_SIZE,
                        (float)MAX_ICON_SIZE,
                        sliderStyle,
                        GUI.skin.horizontalSliderThumb,
                        GUILayout.MaxWidth(128));
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_newBrushIcon, UnityEditor.EditorStyles.toolbarButton)) PaletteContextMenu();
                using (new UnityEditor.EditorGUI.DisabledGroupScope(PaletteManager.selectionCount == 0))
                {
                    if (GUILayout.Button(_deleteBrushIcon, UnityEditor.EditorStyles.toolbarButton)) OnDelete();
                }
                PaletteManager.pickingBrushes = GUILayout.Toggle(PaletteManager.pickingBrushes,
                    _pickerIcon, UnityEditor.EditorStyles.toolbarButton);
                if (GUILayout.Button(_settingsIcon, UnityEditor.EditorStyles.toolbarButton)) SettingsContextMenu();
            }
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.DragUpdated
                    || Event.current.type == EventType.MouseDrag || Event.current.type == EventType.DragPerform)
                    Event.current.Use();
            }
        }

        private void OnDelete()
        {
            RegisterUndo("Delete Brush");
            DeleteBrushSelection();
            PaletteManager.ClearSelection();
            OnPaletteChange();
        }

        public void Reload(bool clearSelection)
        {
            if (PaletteManager.selectedPaletteIdx >= PaletteManager.paletteCount) PaletteManager.selectedPaletteIdx = 0;
            if (clearSelection)
            {
                PaletteManager.ClearSelection(true);
                _lastVisibleIdx = PaletteManager.paletteCount - 1;
            }
            _updateTabBar = true;
            OnPaletteChange();
        }

        private void SettingsContextMenu()
        {
            var menu = new UnityEditor.GenericMenu();
            menu.AddItem(new GUIContent(PaletteManager.viewList ? "Grid View" : "List View"), false,
                () => PaletteManager.viewList = !PaletteManager.viewList);
            if (!PaletteManager.viewList)
                menu.AddItem(new GUIContent("Show Brush Name"), PaletteManager.showBrushName,
                () => PaletteManager.showBrushName = !PaletteManager.showBrushName);
            if (PaletteManager.selectedPalette.brushCount > 1)
            {
                menu.AddItem(new GUIContent("Ascending Sort"), false,
                    () => { PaletteManager.selectedPalette.AscendingSort(); });
                menu.AddItem(new GUIContent("Descending Sort"), false,
                    () => { PaletteManager.selectedPalette.DescendingSort(); });
            }
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Rename palette..."), false, ShowRenamePaletteWindow,
                       new RenameData(PaletteManager.selectedPaletteIdx, PaletteManager.selectedPalette.name,
                       position.position + Event.current.mousePosition));
            menu.AddItem(new GUIContent("Delete palette"), false, ShowDeleteConfirmation,
                PaletteManager.selectedPaletteIdx);
            menu.AddItem(new GUIContent("Cleanup palette"), false, () =>
            {
                PaletteManager.Cleanup();
                OnPaletteChange();
                UpdateTabBar();
                Repaint();
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Update all thumbnails"), false, UpdateAllThumbnails);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Brush creation settings..."), false,
                BrushCreationSettingsWindow.ShowWindow);
            menu.ShowAsContext();
        }
        #endregion

        #region BRUSHES
        private Vector3 _selectedBrushPosition = Vector3.zero;
        private bool _frameSelectedBrush = false;
        private bool _newSelectedPositionSet = false;
        private Texture2D _loadingIcon = null;
        public void FrameSelectedBrush()
        {
            _frameSelectedBrush = true;
            _newSelectedPositionSet = false;
        }

        private void DoFrameSelectedBrush()
        {
            _frameSelectedBrush = false;
            if (_scrollPosition.y > _selectedBrushPosition.y
                || _scrollPosition.y + _scrollViewRect.height < _selectedBrushPosition.y)
                _scrollPosition.y = _selectedBrushPosition.y - 4;
            RepainWindow();
        }

        private void Brushes(ref BrushInputData toggleData)
        {
            if (Event.current.control && Event.current.keyCode == KeyCode.A && _filteredBrushList.Count > 0)
            {
                PaletteManager.ClearSelection();
                foreach (var brush in _filteredBrushList) PaletteManager.AddToSelection(brush.index);
                PaletteManager.selectedBrushIdx = _filteredBrushList[0].index;
                Repaint();
            }
            if (PaletteManager.selectedPalette.brushCount == 0) return;
            if (filteredBrushListCount == 0) return;

            var filteredBrushes = filteredBrushList.ToArray();
            int filterBrushIdx = 0;

            var nameStyle = GUIStyle.none;
            nameStyle.margin = new RectOffset(2, 2, 0, 1);
            nameStyle.clipping = TextClipping.Clip;
            nameStyle.fontSize = 8;
            nameStyle.normal.textColor = Color.white;

            MultibrushSettings brushSettings = null;
            int brushIdx = -1;
            Texture2D icon = null;

            void GetBrushSettings(ref GUIStyle style)
            {
                brushSettings = filteredBrushes[filterBrushIdx].brush;
                brushIdx = filteredBrushes[filterBrushIdx].index;
                if (PaletteManager.SelectionContains(brushIdx))
                    style.normal = _toggleStyle.onNormal;
                icon = brushSettings.thumbnail;
                if (icon == null) icon = _loadingIcon;
            }

            void GetInputData(ref BrushInputData inputData)
            {
                var rect = GUILayoutUtility.GetLastRect();
                if (rect.Contains(Event.current.mousePosition))
                    inputData = new BrushInputData(brushIdx, rect, Event.current.type,
                        Event.current.control, Event.current.shift, Event.current.mousePosition.x);
                if (Event.current.type != EventType.Layout && PaletteManager.selectedBrushIdx == brushIdx)
                {
                    _selectedBrushPosition = rect.position;
                    _newSelectedPositionSet = true;
                }
            }

            void GridViewRow(ref BrushInputData inputData)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int col = 0; col < _columnCount && filterBrushIdx < filteredBrushes.Length; ++col)
                    {
                        var style = new GUIStyle(_toggleStyle);
                        GetBrushSettings(ref style);
                        using (new GUILayout.VerticalScope(style))
                        {
                            if (PaletteManager.showBrushName)
                                GUILayout.Box(new GUIContent(brushSettings.name, brushSettings.name),
                                    nameStyle, GUILayout.Width(PaletteManager.iconSize));
                            GUILayout.Box(new GUIContent(icon, brushSettings.name), GUIStyle.none,
                                GUILayout.Width(PaletteManager.iconSize),
                            GUILayout.Height(PaletteManager.iconSize));
                        }
                        GetInputData(ref inputData);
                        ++filterBrushIdx;
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            void ListView(ref BrushInputData inputData)
            {
                var style = new GUIStyle(_toggleStyle);
                style.padding = new RectOffset(0, 0, 0, 0);
                GetBrushSettings(ref style);
                using (new GUILayout.HorizontalScope(style))
                {
                    GUILayout.Box(new GUIContent(icon, brushSettings.name), GUIStyle.none,
                        GUILayout.Width(PaletteManager.iconSize),
                        GUILayout.Height(PaletteManager.iconSize));
                    GUILayout.Space(4);
                    using (new GUILayout.VerticalScope())
                    {
                        var span = (PaletteManager.iconSize - 16) / 2;
                        GUILayout.Space(span);
                        GUILayout.Box(new GUIContent(brushSettings.name, brushSettings.name), nameStyle);
                        GUILayout.Space(span);
                    }
                }
                GetInputData(ref inputData);
                ++filterBrushIdx;
            }
            if (PaletteManager.viewList)
            {
                nameStyle.fontSize = 12;

            }
            while (filterBrushIdx < filteredBrushes.Length)
            {
                if (PaletteManager.viewList) ListView(ref toggleData);
                else GridViewRow(ref toggleData);
            }
        }

        public void DeselectAllButThis(int index)
        {
            if (PaletteManager.selectedBrushIdx == index && PaletteManager.selectionCount == 1) return;
            PaletteManager.ClearSelection();
            if (index < 0) return;
            PaletteManager.AddToSelection(index);
            PaletteManager.selectedBrushIdx = index;
        }

        private void DeleteBrushSelection()
        {
            var descendingSelection = PaletteManager.idxSelection;
            System.Array.Sort<int>(descendingSelection, new System.Comparison<int>((i1, i2) => i2.CompareTo(i1)));
            foreach (var i in descendingSelection) PaletteManager.selectedPalette.RemoveBrushAt(i);
        }
        private void DeleteBrush(object idx)
        {
            RegisterUndo("Delete Brush");
            if (PaletteManager.SelectionContains((int)idx)) DeleteBrushSelection();
            else PaletteManager.selectedPalette.RemoveBrushAt((int)idx);
            PaletteManager.ClearSelection();
            OnPaletteChange();
        }

        private void CopyBrushSettings(object idx)
            => PaletteManager.clipboardSetting = PaletteManager.selectedPalette.brushes[(int)idx].CloneMainSettings();

        private void PasteBrushSettings(object idx)
        {
            RegisterUndo("Paste Brush Settings");
            PaletteManager.selectedPalette.brushes[(int)idx].Copy(PaletteManager.clipboardSetting);
            if (BrushProperties.instance != null) BrushProperties.instance.Repaint();
            PaletteManager.selectedPalette.Save();
        }

        private void DuplicateBrush(object idx)
        {
            RegisterUndo("Duplicate Brush");
            if (PaletteManager.SelectionContains((int)idx))
            {
                var descendingSelection = PaletteManager.idxSelection;
                System.Array.Sort<int>(descendingSelection, new System.Comparison<int>((i1, i2) => i2.CompareTo(i1)));
                for (int i = 0; i < descendingSelection.Length; ++i)
                {
                    PaletteManager.selectedPalette.DuplicateBrush(descendingSelection[i]);
                    descendingSelection[i] += descendingSelection.Length - 1 - i;
                }
                PaletteManager.idxSelection = descendingSelection;
            }
            else PaletteManager.selectedPalette.DuplicateBrush((int)idx);
            OnPaletteChange();
        }

        private void MergeBrushes()
        {
            RegisterUndo("Merge Brushes");
            var selection = new System.Collections.Generic.List<int>(PaletteManager.idxSelection);
            selection.Sort();
            var resultIdx = selection[0];
            var lastIdx = selection.Last() + 1;
            PaletteManager.selectedPalette.DuplicateBrushAt(resultIdx, selection.Last() + 1);
            resultIdx = lastIdx;
            var result = PaletteManager.selectedPalette.GetBrush(resultIdx);
            var firstItem = result.GetItemAt(0);
            if (!firstItem.overwriteSettings) firstItem.Copy(result);
            firstItem.overwriteSettings = true;
            result.name += "_merged";
            
            selection.RemoveAt(0);
            for (int i = 0; i < selection.Count; ++i)
            {
                var idx = selection[i];
                var other = PaletteManager.selectedPalette.GetBrush(idx);
                var otherItems = other.items;
                foreach (var item in otherItems)
                {
                    var clone = new MultibrushItemSettings(item.prefab, result);
                    if (item.overwriteSettings) clone.Copy(item);
                    else clone.Copy(other);
                    clone.overwriteSettings = true;
                    result.AddItem(clone);
                }
            }
            result.Reset();
            PaletteManager.ClearSelection();
            PaletteManager.AddToSelection(resultIdx);
            PaletteManager.selectedBrushIdx = resultIdx;
            OnPaletteChange();
        }

        private void OnMergeBrushesContext()
        {
            RegisterUndo("Merge Brushes");
            var selection = new System.Collections.Generic.List<int>(PaletteManager.idxSelection);
            selection.Sort();
            var resultIdx = selection[0];
            selection.RemoveAt(0);
            selection.Reverse();
            var result = PaletteManager.selectedPalette.GetBrush(resultIdx);
            for (int i = 0; i < selection.Count; ++i)
            {
                var idx = selection[i];
                var other = PaletteManager.selectedPalette.GetBrush(idx);
                var otherItems = other.items;
                foreach (var item in otherItems)
                {
                    var clone = item.Clone() as MultibrushItemSettings;
                    clone.parentSettings = result;
                    result.AddItem(clone);
                }
                PaletteManager.selectedPalette.RemoveBrushAt(idx);
            }
            PaletteManager.ClearSelection();
            PaletteManager.AddToSelection(resultIdx);
            PaletteManager.selectedBrushIdx = resultIdx;
            OnPaletteChange();
        }


        private void SelectPrefabs(object idx)
        {
            var prefabs = new System.Collections.Generic.List<GameObject>();
            if (PaletteManager.SelectionContains((int)idx))
            {
                foreach (int selectedIdx in PaletteManager.idxSelection)
                {
                    var brush = PaletteManager.selectedPalette.GetBrush(selectedIdx);
                    foreach (var item in brush.items)
                    {
                        if (item.prefab != null) prefabs.Add(item.prefab);
                    }
                }
            }
            else
            {
                var brush = PaletteManager.selectedPalette.GetBrush((int)idx);
                foreach (var item in brush.items)
                {
                    if (item.prefab != null) prefabs.Add(item.prefab);
                }
            }
            UnityEditor.Selection.objects = prefabs.ToArray();
        }

        private void OpenPrefab(object idx)
            => UnityEditor.AssetDatabase.OpenAsset(PaletteManager.selectedPalette.GetBrush((int)idx).items[0].prefab);

        private void SelectReferences(object idx)
        {
            var items = PaletteManager.selectedPalette.GetBrush((int)idx).items;
            var itemsprefabIds = new System.Collections.Generic.List<int>();
            foreach (var item in items)
            {
                if (item.prefab != null) itemsprefabIds.Add(item.prefab.GetInstanceID());
            }
            var selection = new System.Collections.Generic.List<GameObject>();
            var objects = GameObject.FindObjectsOfType<Transform>();
            foreach (var obj in objects)
            {
                var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (source == null) continue;
                var sourceIdx = source.gameObject.GetInstanceID();
                if (itemsprefabIds.Contains(sourceIdx)) selection.Add(obj.gameObject);
            }
            UnityEditor.Selection.objects = selection.ToArray();
        }

        private void UpdateThumbnail(object idx) => PaletteManager.UpdateSelectedThumbnails();

        private void EditThumbnail(object idx)
        {
            var brushIdx = (int)idx;
            var brush = PaletteManager.selectedPalette.GetBrush(brushIdx);
            ThumbnailEditorWindow.ShowWindow(brush, brushIdx);
        }

        private void CopyThumbnailSettings(object idx)
        {
            var brush = PaletteManager.selectedPalette.brushes[(int)idx];
            PaletteManager.clipboardThumbnailSettings = brush.thumbnailSettings.Clone();
            PaletteManager.clipboardOverwriteThumbnailSettings = PaletteManager.Trit.SAME;
        }
        private void PasteThumbnailSettings(object idx)
        {
            if (PaletteManager.clipboardThumbnailSettings == null) return;
            RegisterUndo("Paste Thumbnail Settings");
            void Paste(MultibrushSettings brush)
            {
                brush.thumbnailSettings.Copy(PaletteManager.clipboardThumbnailSettings);
                ThumbnailUtils.UpdateThumbnail(brush);
            }
            if (PaletteManager.SelectionContains((int)idx))
            {
                foreach (var i in PaletteManager.idxSelection) Paste(PaletteManager.selectedPalette.brushes[i]);
            }
            else Paste(PaletteManager.selectedPalette.brushes[(int)idx]);
            PaletteManager.selectedPalette.Save();
        }

        private void BrushContext(int idx)
        {
            var menu = new UnityEditor.GenericMenu();
            var brush = PaletteManager.selectedPalette.GetBrush(idx);
            menu.AddItem(new GUIContent("Select Prefab" + (PaletteManager.selectionCount > 1
                || brush.itemCount > 1 ? "s" : "")), false, SelectPrefabs, idx);
            if (brush.itemCount == 1) menu.AddItem(new GUIContent("Open Prefab"), false, OpenPrefab, idx);
            menu.AddItem(new GUIContent("Select References In Scene"), false, SelectReferences, idx);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Update Thumbnail"), false, UpdateThumbnail, idx);
            menu.AddItem(new GUIContent("Edit Thumbnail"), false, EditThumbnail, idx);
            menu.AddItem(new GUIContent("Copy Thumbnail Settings"), false, CopyThumbnailSettings, idx);
            if (PaletteManager.clipboardThumbnailSettings != null)
                menu.AddItem(new GUIContent("Paste Thumbnail Settings"), false, PasteThumbnailSettings, idx);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete"), false, DeleteBrush, idx);
            menu.AddItem(new GUIContent("Duplicate"), false, DuplicateBrush, idx);
            if (PaletteManager.selectionCount > 1) menu.AddItem(new GUIContent("Merge"), false, OnMergeBrushesContext);
            if (PaletteManager.selectionCount == 1)
                menu.AddItem(new GUIContent("Copy Brush Settings"), false, CopyBrushSettings, idx);
            if (PaletteManager.clipboardSetting != null)
                menu.AddItem(new GUIContent("Paste Brush Settings"), false, PasteBrushSettings, idx);
            menu.AddSeparator(string.Empty);
            PaletteContextAddMenuItems(menu);
            menu.ShowAsContext();
        }

        private void BrushMouseEventHandler(BrushInputData data)
        {
            void DeselectAllButCurrent()
            {
                PaletteManager.ClearSelection();
                PaletteManager.selectedBrushIdx = data.index;
                PaletteManager.AddToSelection(data.index);
            }
            if (data == null) return;
            if (data.eventType == EventType.MouseMove) Event.current.Use();
            if (data.eventType == EventType.MouseDown && Event.current.button == 0)
            {
                void DeselectAll() => PaletteManager.ClearSelection();
                void ToggleCurrent()
                {
                    if (PaletteManager.SelectionContains(data.index)) PaletteManager.RemoveFromSelection(data.index);
                    else PaletteManager.AddToSelection(data.index);
                    PaletteManager.selectedBrushIdx = PaletteManager.selectionCount == 1
                        ? PaletteManager.idxSelection[0] : -1;
                }
                if (data.shift)
                {
                    var selectedIdx = PaletteManager.selectedBrushIdx;
                    var sign = (int)Mathf.Sign(data.index - selectedIdx);
                    if (sign != 0)
                    {
                        PaletteManager.ClearSelection();
                        for (int i = selectedIdx; i != data.index; i += sign)
                        {
                            if (FilteredListContains(i)) PaletteManager.AddToSelection(i);
                        }
                        PaletteManager.AddToSelection(data.index);
                        PaletteManager.selectedBrushIdx = selectedIdx;
                    }
                    else DeselectAllButCurrent();
                }
                else
                {
                    if (data.control && PaletteManager.selectionCount < 2)
                    {
                        if (PaletteManager.selectedBrushIdx == data.index) DeselectAll();
                        else ToggleCurrent();
                    }
                    else if (data.control && PaletteManager.selectionCount > 1) ToggleCurrent();
                    else if (!data.control && PaletteManager.selectionCount < 2)
                    {
                        if (PaletteManager.selectedBrushIdx == data.index) DeselectAll();
                        else DeselectAllButCurrent();
                    }
                    else if (!data.control && PaletteManager.selectionCount > 1) DeselectAllButCurrent();
                }
                Event.current.Use();
                Repaint();
            }
            else if (data.eventType == EventType.ContextClick)
            {
                BrushContext(data.index);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                if (!PaletteManager.SelectionContains(data.index)) DeselectAllButCurrent();
                UnityEditor.DragAndDrop.PrepareStartDrag();
                PWBIO.sceneDragReceiver.brushId = data.index;
                SceneDragAndDrop.StartDrag(PWBIO.sceneDragReceiver, "Dragging brush");
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                draggingBrush = true;
                _moveBrush.from = data.index;
                _moveBrush.perform = false;
                _moveBrush.to = -1;
            }
            else if (data.eventType == EventType.DragUpdated)
            {
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                var size = new Vector2(4, PaletteManager.iconSize);
                var min = data.rect.min;
                bool toTheRight = data.mouseX - data.rect.center.x > 0;
                min.x = toTheRight ? data.rect.max.x : min.x - size.x;
                _cursorRect = new Rect(min, size);
                _showCursor = true;
                _moveBrush.to = data.index;
                if (toTheRight) ++_moveBrush.to;
            }
            else if (data.eventType == EventType.DragPerform)
            {
                _moveBrush.to = data.index;
                bool toTheRight = data.mouseX - data.rect.center.x > 0;
                if (toTheRight) ++_moveBrush.to;
                if (draggingBrush)
                {
                    _moveBrush.perform = _moveBrush.from != _moveBrush.to;
                    draggingBrush = false;
                }
                _showCursor = false;
            }
            else if (data.eventType == EventType.DragExited)
            {
                _showCursor = false;
                draggingBrush = false;
                _moveBrush.to = -1;
            }
        }
        #endregion

        #region PALETTE CONTEXT
        private int _currentPickerId = -1;
        private bool _contextBrushAdded = false;
        private MultibrushSettings _newContextBrush = null;

        private void PaletteContext()
        {
            if (_scrollViewRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.ContextClick)
                {
                    PaletteContextMenu();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    PaletteManager.ClearSelection();
                    Repaint();
                }
            }

            if (Event.current.commandName == "ObjectSelectorClosed"
                && UnityEditor.EditorGUIUtility.GetObjectPickerControlID() == _currentPickerId)
            {
                var obj = UnityEditor.EditorGUIUtility.GetObjectPickerObject();
                if (obj != null)
                {
                    var prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(obj);
                    if (prefabType == UnityEditor.PrefabAssetType.Regular
                        || prefabType == UnityEditor.PrefabAssetType.Variant)
                    {
                        _contextBrushAdded = true;
                        var gameObj = obj as GameObject;
                        AddLabels(gameObj);
                        _newContextBrush = new MultibrushSettings(gameObj);
                    }
                }
                _currentPickerId = -1;
            }
        }

        private void PaletteContextAddMenuItems(UnityEditor.GenericMenu menu)
        {
            menu.AddItem(new GUIContent("New Brush From Prefab"), false, CreateBrushFromPrefab);
            menu.AddItem(new GUIContent("New MultiBrush From Folder"), false, CreateBrushFromFolder);
            menu.AddItem(new GUIContent("New Brush From Each Prefab In Folder"), false,
                CreateBrushFromEachPrefabInFolder);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("New MultiBrush From Selection"), false, CreateBrushFromSelection);
            menu.AddItem(new GUIContent("New Brush From Each Prefab Selected"), false,
                CreateBushFromEachPrefabSelected);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Update all thumbnails"), false, UpdateAllThumbnails);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Brush Creation And Drop Settings"), false,
                BrushCreationSettingsWindow.ShowWindow);
        }
        private void PaletteContextMenu()
        {
            var menu = new UnityEditor.GenericMenu();
            PaletteContextAddMenuItems(menu);
            menu.ShowAsContext();
        }

        private void CreateBrushFromPrefab()
        {
            _currentPickerId = GUIUtility.GetControlID(FocusType.Passive) + 100;
            UnityEditor.EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "t:Prefab", _currentPickerId);
        }

        private void CreateBrushFromFolder()
        {
            var items = PluginMaster.DropUtils.GetFolderItems();
            if (items == null) return;
            RegisterUndo("Add Brush");
            var brush = new MultibrushSettings(items[0].obj);
            AddLabels(items[0].obj);
            PaletteManager.selectedPalette.AddBrush(brush);
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            for (int i = 1; i < items.Length; ++i)
            {
                var item = new MultibrushItemSettings(items[i].obj, brush);
                AddLabels(items[i].obj);
                brush.AddItem(item);
            }
            OnPaletteChange();
        }

        private void CreateBrushFromEachPrefabInFolder()
        {
            var items = PluginMaster.DropUtils.GetFolderItems();
            if (items == null) return;
            foreach (var item in items)
            {
                if (item.obj == null) continue;
                RegisterUndo("Add Brush");
                AddLabels(item.obj);
                var brush = new MultibrushSettings(item.obj);
                PaletteManager.selectedPalette.AddBrush(brush);
            }
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            OnPaletteChange();
        }

        private string GetPrefabFolder(GameObject obj)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            var folders = path.Split(new char[] { '\\', '/' });
            var subFolder = folders[folders.Length - 2];
            return subFolder;
        }

        public void CreateBrushFromSelection()
        {
            if (PaletteManager.selectionCount > 1)
            {
                MergeBrushes();
                return;
            }

            var selectionPrefabs = SelectionManager.GetSelectionPrefabs();
            CreateBrushFromSelection(selectionPrefabs);
        }

        public void CreateBrushFromSelection(GameObject[] selectionPrefabs)
        {
            if (selectionPrefabs.Length == 0) return;

            RegisterUndo("Add Brush");
            AddLabels(selectionPrefabs[0]);
            var brush = new MultibrushSettings(selectionPrefabs[0]);
            PaletteManager.selectedPalette.AddBrush(brush);
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            for (int i = 1; i < selectionPrefabs.Length; ++i)
            {
                AddLabels(selectionPrefabs[i]);
                brush.AddItem(new MultibrushItemSettings(selectionPrefabs[i], brush));
            }
            OnPaletteChange();
        }

        public void CreateBrushFromSelection(GameObject selectedPrefab)
            => CreateBrushFromSelection(new GameObject[] { selectedPrefab });

        public void CreateBushFromEachPrefabSelected()
        {
            var selectionPrefabs = SelectionManager.GetSelectionPrefabs();
            if (selectionPrefabs.Length == 0) return;
            foreach (var obj in selectionPrefabs)
            {
                if (obj == null) continue;
                RegisterUndo("Add Brush");
                var brush = new MultibrushSettings(obj);
                AddLabels(obj);
                PaletteManager.selectedPalette.AddBrush(brush);
            }
            DeselectAllButThis(PaletteManager.selectedPalette.brushes.Length - 1);
            OnPaletteChange();
        }
        #endregion

        #region DROPBOX
        private void DropBox()
        {
            GUIStyle dragAndDropBoxStyle = new GUIStyle();
            dragAndDropBoxStyle.alignment = TextAnchor.MiddleCenter;
            dragAndDropBoxStyle.fontStyle = FontStyle.Italic;
            dragAndDropBoxStyle.fontSize = 12;
            dragAndDropBoxStyle.normal.textColor = Color.white;
            dragAndDropBoxStyle.wordWrap = true;
            GUI.Box(_scrollViewRect, "Drag and Drop Prefabs Or Folders Here", dragAndDropBoxStyle);
        }

        private void AddLabels(GameObject obj)
        {
            if (!PaletteManager.selectedPalette.brushCreationSettings.addLabelsToDroppedPrefabs) return;
            var labels = new System.Collections.Generic.HashSet<string>(UnityEditor.AssetDatabase.GetLabels(obj));
            int labelCount = labels.Count;
            if (PaletteManager.selectedPalette.brushCreationSettings.addLabelsToDroppedPrefabs)
                labels.UnionWith(PaletteManager.selectedPalette.brushCreationSettings.labels);
            if (labelCount != labels.Count) UnityEditor.AssetDatabase.SetLabels(obj, labels.ToArray());
        }

        private void DropPrefab()
        {
            if (_scrollViewRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    bool paletteChanged = false;
                    var items = DropUtils.GetDroppedPrefabs();
                    if (items.Length > 0) PaletteManager.ClearSelection();
                    var i = 0;
                    foreach (var item in items)
                    {
                        AddLabels(item.obj);
                        var brush = new MultibrushSettings(item.obj);
                        RegisterUndo("Add Brush");
                        if (_moveBrush.to < 0)
                        {
                            PaletteManager.selectedPalette.AddBrush(brush);
                            PaletteManager.selectedBrushIdx = PaletteManager.selectedPalette.brushes.Length - 1;
                        }
                        else
                        {
                            var idx = _moveBrush.to + i++;
                            PaletteManager.selectedPalette.InsertBrushAt(brush, idx);
                            PaletteManager.selectedBrushIdx = _moveBrush.to;
                        }
                        paletteChanged = true;
                    }
                    if (paletteChanged) OnPaletteChange();
                    if (draggingBrush && _moveBrush.to >= 0)
                    {
                        _moveBrush.perform = _moveBrush.from != _moveBrush.to;
                        draggingBrush = false;
                    }
                    _showCursor = false;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragExited) _showCursor = false;
            }
        }
        #endregion

        #region TAB BAR
        #region RENAME
        private class RenamePaletteWindow : UnityEditor.EditorWindow
        {
            private string _currentName = string.Empty;
            private int _paletteIdx = -1;
            private System.Action<string, int> _onDone;
            public static void ShowWindow(RenameData data, System.Action<string, int> onDone)
            {
                var window = GetWindow<RenamePaletteWindow>(true, "Rename Palette");
                window._currentName = data.currentName;
                window._paletteIdx = data.paletteIdx;
                window._onDone = onDone;
                window.position = new Rect(data.mousePosition.x + 50, data.mousePosition.y + 50, 160, 50);
            }

            private void OnGUI()
            {
                UnityEditor.EditorGUIUtility.labelWidth = 70;
                UnityEditor.EditorGUIUtility.fieldWidth = 70;
                using (new GUILayout.HorizontalScope())
                {
                    _currentName = UnityEditor.EditorGUILayout.TextField("New Name:", _currentName);
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Apply", GUILayout.Width(50)))
                    {
                        _onDone(_currentName, _paletteIdx);
                        Close();
                    }
                }
            }
        }

        private struct RenameData
        {
            public readonly int paletteIdx;
            public readonly string currentName;
            public readonly Vector2 mousePosition;

            public RenameData(int paletteIdx, string currentName, Vector2 mousePosition)
                => (this.paletteIdx, this.currentName, this.mousePosition) = (paletteIdx, currentName, mousePosition);
        }
        private void ShowRenamePaletteWindow(object obj)
        {
            if (!(obj is RenameData)) return;
            var data = (RenameData)obj;
            RenamePaletteWindow.ShowWindow(data, RenamePalette);
        }
        private void RenamePalette(string paletteName, int paletteIdx)
        {
            RegisterUndo("Rename Palette");
            PaletteManager.paletteData[paletteIdx].name = paletteName;
            _updateTabBarWidth = true;
            Repaint();
        }
        #endregion


        private void ShowDeleteConfirmation(object obj)
        {
            int paletteIdx = (int)obj;
            var palette = PaletteManager.paletteData[paletteIdx];
            if (UnityEditor.EditorUtility.DisplayDialog("Delete Palette: " + palette.name,
                "Are you sure you want to delete this palette?\n" + palette.name, "Delete", "Cancel"))
            {
                RegisterUndo("Remove Palette");
                PaletteManager.RemovePaletteAt(paletteIdx);
                if (PaletteManager.paletteCount == 0) CreatePalette();
                else if (PaletteManager.selectedPaletteIdx >= PaletteManager.paletteCount) SelectPalette(0);
                --_visibleTabCount;
                if (lastVisibleIdx >= _visibleTabCount) lastVisibleIdx = _visibleTabCount - 1;
                PaletteManager.selectedBrushIdx = -1;
                _updateTabBarWidth = true;
                _updateTabBar = true;
                UpdateFilteredList(false);
                Repaint();
            }
        }

        #region TAB BUTTONS
        private float _prevWidth = 0f;
        private bool _updateTabBarWidth = true;
        private bool _updateTabBar = false;
        private int _lastVisibleIdx = 0;
        private int _visibleTabCount = 0;
        private Rect _dropdownRect;

        public static void UpdateTabBar()
        {
            if (instance == null) return;
            instance._updateTabBar = true;
            instance._updateTabBarWidth = true;
        }
        private int lastVisibleIdx
        {
            get
            {
                if (_lastVisibleIdx >= PaletteManager.paletteCount) _lastVisibleIdx = 0;
                return _lastVisibleIdx;
            }
            set => _lastVisibleIdx = value;
        }
        public void SelectPalette(int idx)
        {
            if (PaletteManager.selectedPaletteIdx == idx) return;
            PaletteManager.selectedPaletteIdx = idx;
            PaletteManager.selectedBrushIdx = -1;
            PaletteManager.ClearSelection();
            _updateTabBar = true;
            OnPaletteChange();
        }
        private void SelectPalette(object obj) => SelectPalette((int)obj);

        private void CreatePalette()
        {
            _lastVisibleIdx = PaletteManager.paletteCount;
            RegisterUndo("Add Palette");
            PaletteManager.AddPalette(new PaletteData("Palette" + (PaletteManager.paletteCount + 1),
                System.DateTime.Now.ToBinary()));
            SelectPalette(lastVisibleIdx);
            _updateTabBarWidth = true;
            _updateTabBar = true;
        }

        private void ToggleMultipleRows()
            => PaletteManager.showTabsInMultipleRows = !PaletteManager.showTabsInMultipleRows;

        private System.Collections.Generic.List<Rect> _tabRects = new System.Collections.Generic.List<Rect>();
        private System.Collections.Generic.Dictionary<long, float> _tabSize
            = new System.Collections.Generic.Dictionary<long, float>();
        private void TabBar()
        {
            float visibleW = 0;
            int lastVisibleIdx = 0;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                for (int i = 0; i < _tabRects.Count; ++i)
                {
                    if (_tabRects[i].Contains(Event.current.mousePosition))
                    {
                        var name = PaletteManager.paletteNames[i];
                        var menu = new UnityEditor.GenericMenu();
                        menu.AddItem(new GUIContent("Rename"), false, ShowRenamePaletteWindow,
                            new RenameData(i, name, position.position + Event.current.mousePosition));
                        menu.AddItem(new GUIContent("Delete"), false, ShowDeleteConfirmation, i);
                        menu.ShowAsContext();
                    }
                }
            }
            var names = PaletteManager.paletteNames;
            var paletteIds = PaletteManager.paletteIds;

            int Tabs(int from, int to)
            {
                var lastVisible = to;
                for (int i = from; i <= to; ++i)
                {
                    var isSelected = PaletteManager.selectedPaletteIdx == i;
                    var name = names[i];


                    if (GUILayout.Toggle(isSelected, name, UnityEditor.EditorStyles.toolbarButton)
                        && Event.current.button == 0)
                    {
                        if (!isSelected) SelectPalette(i);
                        isSelected = true;
                    }

                    var toggleRect = GUILayoutUtility.GetLastRect();
                    var id = paletteIds[i];
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (_tabSize.ContainsKey(id)) _tabSize[id] = toggleRect.width;
                        else _tabSize.Add(id, toggleRect.width);
                    }

                    if (Event.current.type == EventType.Repaint) _tabRects.Add(toggleRect);
                    if (Event.current.type == EventType.Repaint && toggleRect.xMax < position.width)
                    {
                        lastVisible = i;
                        visibleW = toggleRect.xMax;
                    }
                }
                GUILayout.FlexibleSpace();
                return lastVisible;
            }

            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
            {
                if (GUILayout.Button(_dropdownIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    var menu = new UnityEditor.GenericMenu();
                    menu.AddItem(new GUIContent("New palette"), false, CreatePalette);
                    menu.AddSeparator(string.Empty);
                    menu.AddItem(new GUIContent("Show tabs in multiple rows"),
                        PaletteManager.showTabsInMultipleRows, ToggleMultipleRows);
                    menu.AddSeparator(string.Empty);
                    var namesDic = PaletteManager.paletteNames.Select((name, index) => new { name, index })
                        .ToDictionary(item => item.index, item => item.name);
                    var sortedDic = (from item in namesDic orderby item.Value ascending select item)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                    var repeatedNameCount = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (var item in sortedDic)
                    {
                        var name = item.Value;
                        if (repeatedNameCount.ContainsKey(item.Value)) name += "(" + repeatedNameCount[item.Value] + ")";
                        menu.AddItem(new GUIContent(name), PaletteManager.selectedPaletteIdx == item.Key,
                            SelectPalette, item.Key);
                        if (repeatedNameCount.ContainsKey(item.Value)) repeatedNameCount[item.Value] += 1;
                        else repeatedNameCount.Add(item.Value, 1);
                    }
                    menu.ShowAsContext();
                }
                if (Event.current.type == EventType.Repaint) _tabRects.Clear();
                if (PaletteManager.paletteCount == 0) return;

                lastVisibleIdx = Tabs(0, this.lastVisibleIdx);
                if (Event.current.type == EventType.Repaint)
                {
                    if (_updateTabBarWidth && _visibleTabCount == PaletteManager.paletteCount)
                    {
                        _updateTabBarWidth = false;
                        _lastVisibleIdx = lastVisibleIdx;
                        _updateTabBar = true;
                    }
                    else if (_updateTabBarWidth && _visibleTabCount != PaletteManager.paletteCount)
                    {
                        _lastVisibleIdx = PaletteManager.paletteCount - 1;
                        _updateTabBar = true;
                    }
                    if (_prevWidth != position.width)
                    {
                        if (_prevWidth < position.width) _updateTabBarWidth = true;

                        _lastVisibleIdx = lastVisibleIdx;
                        _prevWidth = position.width;
                        _updateTabBar = true;
                    }
                }
            }
            if (PaletteManager.showTabsInMultipleRows)
            {
                var rowItemCount = new System.Collections.Generic.List<int>();
                float tabsWidth = 0;
                var tabItemCount = 0;
                for (int i = _visibleTabCount; i < PaletteManager.paletteCount; ++i)
                {
                    var id = paletteIds[i];
                    if (!_tabSize.ContainsKey(id))
                    {
                        _updateTabBarWidth = true;
                        _updateTabBar = true;
                        continue;
                    }
                    var w = _tabSize[id];
                    tabsWidth += w;
                    if (tabsWidth > position.width)
                    {
                        rowItemCount.Add(Mathf.Max(tabItemCount, 1));
                        tabsWidth = tabItemCount > 0 ? w : 0;
                        if (tabItemCount == 0) continue;
                        tabItemCount = 0;
                    }
                    ++tabItemCount;
                }
                if (tabItemCount > 0) rowItemCount.Add(tabItemCount);
                if (rowItemCount.Count > 0)
                {
                    if (_visibleTabCount == PaletteManager.paletteCount)
                        _updateTabBar = true;
                    int fromIdx = _visibleTabCount;
                    int toIdx = _visibleTabCount;
                    foreach (var itemCount in rowItemCount)
                    {
                        toIdx = fromIdx + itemCount - 1;
                        using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
                        {
                            Tabs(fromIdx, toIdx);
                        }
                        fromIdx = toIdx + 1;
                        if (fromIdx >= PaletteManager.paletteCount) break;
                    }
                }
            }
            if (_updateTabBar && PaletteManager.paletteCount > 1)
            {
                if (!PaletteManager.showTabsInMultipleRows && PaletteManager.selectedPaletteIdx > this.lastVisibleIdx)
                {
                    PaletteManager.SwapPalette(PaletteManager.selectedPaletteIdx, this.lastVisibleIdx);
                    PaletteManager.selectedPaletteIdx = this.lastVisibleIdx;
                }
                _visibleTabCount = this.lastVisibleIdx + 1;
                _updateTabBar = false;
                Repaint();
            }
        }
        #endregion
        #endregion

        #region SEARCH BAR
        private string _filterText = string.Empty;
        private GUIContent _labelIcon = null;
        private GUIContent _selectionFilterIcon = null;
        private GUIContent _clearFilterIcon = null;

        private struct FilteredBrush
        {
            public readonly MultibrushSettings brush;
            public readonly int index;
            public FilteredBrush(MultibrushSettings brush, int index) => (this.brush, this.index) = (brush, index);
        }
        private System.Collections.Generic.List<FilteredBrush> _filteredBrushList
            = new System.Collections.Generic.List<FilteredBrush>();
        private System.Collections.Generic.List<FilteredBrush> filteredBrushList
        {
            get
            {
                if (_filteredBrushList == null) _filteredBrushList = new System.Collections.Generic.List<FilteredBrush>();
                return _filteredBrushList;
            }
        }
        public bool FilteredBrushListContains(int index) => _filteredBrushList.Exists(brush => brush.index == index);
        private System.Collections.Generic.Dictionary<string, bool> _labelFilter
            = new System.Collections.Generic.Dictionary<string, bool>();
        public System.Collections.Generic.Dictionary<string, bool> labelFilter
        {
            get
            {
                if (_labelFilter == null) _labelFilter = new System.Collections.Generic.Dictionary<string, bool>();
                return _labelFilter;
            }
            set => _labelFilter = value;
        }

        private bool _updateLabelFilter = true;
        public int filteredBrushListCount => filteredBrushList.Count;

        public string filterText
        {
            get
            {
                if (_filterText == null) _filterText = string.Empty;
                return _filterText;
            }
            set => _filterText = value;
        }

        private void ClearLabelFilter()
        {
            foreach (var key in labelFilter.Keys.ToArray()) labelFilter[key] = false;
        }

        private void SearchBar()
        {
            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();

                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                {
#if UNITY_2019_1_OR_NEWER
                    var searchFieldStyle = UnityEditor.EditorStyles.toolbarSearchField;
#else
                    var searchFieldStyle = EditorStyles.toolbarTextField;
#endif
                    GUILayout.Space(2);
                    filterText = UnityEditor.EditorGUILayout.TextField(filterText, searchFieldStyle).Trim();
                    if (check.changed) UpdateFilteredList(true);
                }
                if (filterText != string.Empty)
                {
                    if (GUILayout.Button(_clearFilterIcon, UnityEditor.EditorStyles.toolbarButton))
                    {
                        filterText = string.Empty;
                        ClearLabelFilter();
                        UpdateFilteredList(true);
                        GUI.FocusControl(null);
                    }
                }

                if (GUILayout.Button(_labelIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    UpdateLabelFilter();
                    var menu = new UnityEditor.GenericMenu();
                    if (labelFilter.Count == 0)
                        menu.AddItem(new GUIContent("No labels Found"), false, null);
                    else
                        foreach (var labelItem in labelFilter.OrderBy(item => item.Key))
                            menu.AddItem(new GUIContent(labelItem.Key), labelItem.Value,
                                SelectLabelFilter, labelItem.Key);
                    menu.ShowAsContext();
                }

                if (GUILayout.Button(_selectionFilterIcon, UnityEditor.EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    FilterBySelection();
                }
            }
            if (_updateLabelFilter)
            {
                _updateLabelFilter = false;
                UpdateLabelFilter();
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        private bool FilteredListContains(int index)
        {
            foreach (var filtered in filteredBrushList)
            {
                if (filtered.index == index) return true;
            }
            return false;
        }

        private void UpdateFilteredList(bool textCanged)
        {
            filteredBrushList.Clear();
            void RemoveFromSelection(int index)
            {
                PaletteManager.RemoveFromSelection(index);
                if (PaletteManager.selectedBrushIdx == index) PaletteManager.selectedBrushIdx = -1;
                if (PaletteManager.selectionCount == 1)
                    PaletteManager.selectedBrushIdx = PaletteManager.idxSelection[0];
            }

            //filter by label
            var filterTextArray = filterText.Split(',');
            var filterTextList = new System.Collections.Generic.List<string>();
            ClearLabelFilter();
            bool filterByLabel = false;
            for (int i = 0; i < filterTextArray.Length; ++i)
            {
                var filterText = filterTextArray[i].Trim();
                if (filterText.Length >= 2 && filterText.Substring(0, 2) == "l:")
                {
                    filterText = filterText.Substring(2);
                    if (labelFilter.ContainsKey(filterText))
                    {
                        labelFilter[filterText] = true;
                        filterByLabel = true;
                    }
                    else return;
                    continue;
                }
                filterTextList.Add(filterText);
            }

            var tempFilteredBrushList = new System.Collections.Generic.List<FilteredBrush>();
            var brushes = PaletteManager.selectedPalette.brushes;
            if (!filterByLabel)
                for (int i = 0; i < brushes.Length; ++i)
                {
                    if (brushes[i].containMissingPrefabs) continue;
                    tempFilteredBrushList.Add(new FilteredBrush(brushes[i], i));
                }
            else
            {
                for (int i = 0; i < brushes.Length; ++i)
                {
                    var brush = brushes[i];
                    if (brush.containMissingPrefabs) continue;
                    bool itemContainsFilter = false;
                    foreach (var item in brush.items)
                    {
                        if (item.prefab == null) continue;
                        var labels = UnityEditor.AssetDatabase.GetLabels(item.prefab);
                        foreach (var label in labels)
                        {
                            if (labelFilter[label])
                            {
                                itemContainsFilter = true;
                                break;
                            }
                        }
                        if (itemContainsFilter) break;
                    }
                    if (itemContainsFilter) tempFilteredBrushList.Add(new FilteredBrush(brush, i));
                    else RemoveFromSelection(i);
                }
            }
            //filter by name
            var listIsEmpty = filterTextList.Count == 0;
            if (!listIsEmpty)
            {
                listIsEmpty = true;
                foreach (var filter in filterTextList)
                {
                    if (filter != string.Empty)
                    {
                        listIsEmpty = false;
                        break;
                    }
                }
            }
            if (listIsEmpty)
            {
                filteredBrushList.AddRange(tempFilteredBrushList);
                return;
            }

            foreach (var filteredItem in tempFilteredBrushList.ToArray())
            {
                for (int i = 0; i < filterTextList.Count; ++i)
                {
                    var filterText = filterTextList[i].Trim();
                    bool wholeWordOnly = false;
                    if (filterText == string.Empty) continue;
                    if (filterText.Length >= 2 && filterText.Substring(0, 2) == "w:")
                    {
                        wholeWordOnly = true;
                        filterText = filterText.Substring(2);
                    }
                    if (filterText == string.Empty) continue;
                    filterText = filterText.ToLower();
                    var brush = filteredItem.brush;
                    if ((!wholeWordOnly && brush.name.ToLower().Contains(filterText))
                        || (wholeWordOnly && brush.name.ToLower() == filterText))
                        filteredBrushList.Add(filteredItem);
                    else RemoveFromSelection(filteredItem.index);
                }
            }
        }

        private void UpdateLabelFilter()
        {
            foreach (var brush in PaletteManager.selectedPalette.brushes)
            {
                foreach (var item in brush.items)
                {
                    if (item.prefab == null) continue;
                    var labels = UnityEditor.AssetDatabase.GetLabels(item.prefab);
                    foreach (var label in labels)
                    {
                        if (labelFilter.ContainsKey(label)) continue;
                        labelFilter.Add(label, false);
                    }
                }
            }
        }

        private void SelectLabelFilter(object key)
        {
            labelFilter[(string)key] = !labelFilter[(string)key];
            foreach (var pair in labelFilter)
            {
                if (!pair.Value) continue;
                var labelFilter = "l:" + pair.Key;
                if (filterText.Contains(labelFilter)) continue;
                if (filterText.Length > 0) filterText += ", ";
                filterText += labelFilter;
            }
            var filterTextArray = filterText.Split(',');
            filterText = string.Empty;
            for (int i = 0; i < filterTextArray.Length; ++i)
            {
                var filter = filterTextArray[i].Trim();
                if (filter.Length >= 2 && filter.Substring(0, 2) == "l:")
                {
                    var label = filter.Substring(2);
                    if (!labelFilter.ContainsKey(label)) continue;
                    if (!labelFilter[label]) continue;
                    if (filterText.Contains(filter)) continue;
                }
                if (filter == string.Empty) continue;
                filterText += filter + ", ";
            }
            if (filterText != string.Empty) filterText = filterText.Substring(0, filterText.Length - 2);
            UpdateFilteredList(false);
            Repaint();
        }

        public int FilterBySelection()
        {
            var selection = SelectionManager.GetSelectionPrefabs();
            filterText = string.Empty;
            for (int i = 0; i < selection.Length; ++i)
            {
                filterText += "w:" + selection[i].name;
                if (i < selection.Length - 1) filterText += ", ";
            }
            UpdateFilteredList(false);
            return filteredBrushListCount;
        }

        public void SelectFirstBrush()
        {
            if (filteredBrushListCount == 0) return;
            DeselectAllButThis(filteredBrushList[0].index);
        }
        #endregion
    }
}
