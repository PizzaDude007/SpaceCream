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
    [UnityEditor.InitializeOnLoad]
    public static partial class PWBIO
    {
        #region PWB WINDOWS
        public static void CloseAllWindows(bool closeToolbar = true)
        {
            BrushProperties.CloseWindow();
            ToolProperties.CloseWindow();
            PrefabPalette.CloseWindow();
            if (closeToolbar) PWBToolbar.CloseWindow();
        }
        #endregion

        #region SELECTION
        public static void UpdateSelection()
        {
            if (SelectionManager.topLevelSelection.Length == 0)
            {
                if (tool == ToolManager.PaintTool.EXTRUDE)
                {
                    _initialExtrudePosition = _extrudeHandlePosition = _selectionSize = Vector3.zero;
                    _extrudeDirection = Vector3Int.zero;
                }
                return;
            }
            if (tool == ToolManager.PaintTool.EXTRUDE)
            {
                var selectionBounds = ExtrudeManager.settings.space == Space.World
                    ? BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection)
                    : BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection,
                    ExtrudeManager.settings.rotationAccordingTo == ExtrudeSettings.RotationAccordingTo.FRIST_SELECTED
                    ? SelectionManager.topLevelSelection.First().transform.rotation
                    : SelectionManager.topLevelSelection.Last().transform.rotation);
                _initialExtrudePosition = _extrudeHandlePosition = selectionBounds.center;
                _selectionSize = selectionBounds.size;
                _extrudeDirection = Vector3Int.zero;
            }
            else if (tool == ToolManager.PaintTool.SELECTION)
            {
                _selectedBoxPointIdx = 10;
                _selectionRotation = Quaternion.identity;
                _selectionChanged = true;
                _editingSelectionHandlePosition = false;
                var rotation = GetSelectionRotation();
                _selectionBounds = BoundsUtils.GetSelectionBounds(SelectionManager.topLevelSelection, rotation);
                _selectionRotation = rotation;
            }
        }
        #endregion

        #region UNSAVED CHANGES
        private const string UNSAVED_CHANGES_TITLE = "Unsaved Changes";
        private const string UNSAVED_CHANGES_MESSAGE = "There are unsaved changes.\nWhat would you like to do?";
        private const string UNSAVED_CHANGES_OK = "Save";
        private const string UNSAVED_CHANGES_CANCEL = "Don't Save";

        private static void DisplaySaveDialog(System.Action Save)
        {
            if (UnityEditor.EditorUtility.DisplayDialog(UNSAVED_CHANGES_TITLE,
                UNSAVED_CHANGES_MESSAGE, UNSAVED_CHANGES_OK, UNSAVED_CHANGES_CANCEL)) Save();
            else repaint = true;
        }
        private static void AskIfWantToSave(ToolManager.ToolState state, System.Action Save)
        {
            switch (PWBCore.staticData.unsavedChangesAction)
            {
                case PWBData.UnsavedChangesAction.ASK:
                    if (state == ToolManager.ToolState.EDIT) DisplaySaveDialog(Save);
                    break;
                case PWBData.UnsavedChangesAction.SAVE:
                    if (state == ToolManager.ToolState.EDIT) Save();
                    BrushstrokeManager.ClearBrushstroke();
                    break;
                case PWBData.UnsavedChangesAction.DISCARD:
                    repaint = true;
                    return;
            }
        }

        #endregion

        #region COMMON
        private const float TAU = Mathf.PI * 2;
        private static int _controlId;
        public static int controlId { set => _controlId = value; }
        private static ToolManager.PaintTool tool => ToolManager.tool;

        private static UnityEditor.Tool _unityCurrentTool = UnityEditor.Tool.None;

        private static Camera _sceneViewCamera = null;

        public static bool repaint { get; set; }

        static PWBIO()
        {
            LineData.SetNextId();
            SelectionManager.selectionChanged += UpdateSelection;
            UnityEditor.Undo.undoRedoPerformed += OnUndoPerformed;
            UnityEditor.SceneView.duringSceneGui += DuringSceneGUI;
            PaletteManager.OnPaletteChanged += OnPaletteChanged;
            PaletteManager.OnBrushChanged += OnBrushChanged;
            ToolManager.OnToolModeChanged += OnEditModeChanged;
            LineInitializeOnLoad();
            ShapeInitializeOnLoad();
            TilingInitializeOnLoad();
        }

        private static void OnPaletteChanged() => ApplySelectionFilters();
        private static void OnBrushChanged()
        {
            switch (ToolManager.tool)
            {
                case ToolManager.PaintTool.LINE:

                    ClearLineStroke();
                    break;
                case ToolManager.PaintTool.SHAPE:
                    ClearShapeStroke();
                    break;
                case ToolManager.PaintTool.TILING:
                    ClearTilingStroke();
                    break;
                case ToolManager.PaintTool.SELECTION:
                    ApplySelectionFilters();
                    break;
            }
        }

        public static void SaveUnityCurrentTool() => _unityCurrentTool = UnityEditor.Tools.current;
        public static bool _wasPickingBrushes = false;
        public static void DuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (sceneView.in2DMode)
            {
                SnapManager.settings.gridOnZ = true;
                PWBToolbar.RepaintWindow();
            }
            if (repaint)
            {
                if (tool == ToolManager.PaintTool.SHAPE) BrushstrokeManager.UpdateShapeBrushstroke();
                sceneView.Repaint();
                repaint = false;
            }

            PaletteInput(sceneView);
            _sceneViewCamera = sceneView.camera;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape
                && (tool == ToolManager.PaintTool.PIN || tool == ToolManager.PaintTool.BRUSH
                || tool == ToolManager.PaintTool.GRAVITY || tool == ToolManager.PaintTool.ERASER
                || tool == ToolManager.PaintTool.REPLACER))
                ToolManager.DeselectTool();
            var repaintScene = _wasPickingBrushes == PaletteManager.pickingBrushes;
            _wasPickingBrushes = PaletteManager.pickingBrushes;
            if (PaletteManager.pickingBrushes)
            {
                UnityEditor.HandleUtility.AddDefaultControl(_controlId);
                if (repaintScene) UnityEditor.SceneView.RepaintAll();
                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown) Event.current.Use();
                return;
            }
            if (ToolManager.tool != ToolManager.PaintTool.NONE)
            {
                if (PWBSettings.shortcuts.editModeToggle.Check())
                {
                    switch (tool)
                    {
                        case ToolManager.PaintTool.LINE:
                        case ToolManager.PaintTool.SHAPE:
                        case ToolManager.PaintTool.TILING:
                            ToolManager.editMode = !ToolManager.editMode;
                            ToolProperties.RepainWindow();
                            break;
                        default: break;
                    }
                }
                if (PaletteManager.selectedBrushIdx == -1 && (tool == ToolManager.PaintTool.PIN
                    || tool == ToolManager.PaintTool.BRUSH || tool == ToolManager.PaintTool.GRAVITY
                    || ((tool == ToolManager.PaintTool.LINE || tool == ToolManager.PaintTool.SHAPE
                    || tool == ToolManager.PaintTool.TILING)
                    && !ToolManager.editMode)))
                {
                    if (tool == ToolManager.PaintTool.LINE && _lineData != null && _lineData.state != ToolManager.ToolState.NONE)
                        ResetLineState();
                    else if (tool == ToolManager.PaintTool.SHAPE
                        && _shapeData != null && _shapeData.state != ToolManager.ToolState.NONE)
                        ResetShapeState();
                    else if (tool == ToolManager.PaintTool.TILING
                        && _tilingData != null && _tilingData.state != ToolManager.ToolState.NONE)
                        ResetTilingState();
                    return;
                }

                if (Event.current.type == EventType.MouseEnterWindow) _pinned = false;

                if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                {
                    sceneView.Focus();
                }
                else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.V)
                    _snapToVertex = true;
                else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.V)
                    _snapToVertex = false;
                if (tool == ToolManager.PaintTool.BRUSH || tool == ToolManager.PaintTool.GRAVITY
                    || tool == ToolManager.PaintTool.ERASER || tool == ToolManager.PaintTool.REPLACER)
                {
                    var settings = ToolManager.GetSettingsFromTool(tool);
                    BrushRadiusShortcuts(settings as CircleToolBase);
                }
                if (PWBCore.staticData.tempCollidersAction == PWBData.TempCollidersAction.CREATE_WITHIN_FRUSTRUM)
                    PWBCore.CreateTempCollidersWithinFrustum(sceneView.camera);

                switch (tool)
                {
                    case ToolManager.PaintTool.PIN:
                        PinDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.BRUSH:
                        BrushDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.GRAVITY:
                        GravityToolDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.LINE:
                        LineDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.SHAPE:
                        ShapeDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.TILING:
                        TilingDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.ERASER:
                        EraserDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.REPLACER:
                        ReplacerDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.SELECTION:
                        SelectionDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.EXTRUDE:
                        ExtrudeDuringSceneGUI(sceneView);
                        break;
                    case ToolManager.PaintTool.MIRROR:
                        MirrorDuringSceneGUI(sceneView);
                        break;
                }

                if ((tool != ToolManager.PaintTool.EXTRUDE && tool != ToolManager.PaintTool.SELECTION
                    && tool != ToolManager.PaintTool.MIRROR) && Event.current.type == EventType.Layout && !ToolManager.editMode)
                {
                    UnityEditor.Tools.current = UnityEditor.Tool.None;
                    UnityEditor.HandleUtility.AddDefaultControl(_controlId);
                }
            }
            GridDuringSceneGui(sceneView);
        }

        private static float UpdateRadius(float radius)
            => Mathf.Max(radius * (1f + Mathf.Sign(Event.current.delta.y) * 0.05f), 0.05f);
        private static Vector3 TangentSpaceToWorld(Vector3 tangent, Vector3 bitangent, Vector2 tangentSpacePos)
            => (tangent * tangentSpacePos.x + bitangent * tangentSpacePos.y);

        private static void UpdateStrokeDirection(Vector3 hitPoint)
        {
            var dir = hitPoint - _prevMousePos;
            if (dir.sqrMagnitude > 0.3f)
            {
                _strokeDirection = hitPoint - _prevMousePos;
                _prevMousePos = hitPoint;
            }
        }

        public static void ResetUnityCurrentTool() => UnityEditor.Tools.current = _unityCurrentTool;

        private static bool MouseDot(out Vector3 point, out Vector3 normal,
            PaintOnSurfaceToolSettingsBase.PaintMode mode, bool in2DMode,
            bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, bool snapOnGrid)
        {
            point = Vector3.zero;
            normal = Vector3.up;
            var mousePos = Event.current.mousePosition;
            if (mousePos.x < 0 || mousePos.x >= Screen.width || mousePos.y < 0 || mousePos.y >= Screen.height) return false;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 SnapPoint(Vector3 hitPoint, ref Vector3 snapNormal)
            {
                if (_snapToVertex)
                {
                    if (SnapToVertex(mouseRay, out RaycastHit snappedHit, in2DMode))
                    {
                        _snappedToVertex = true;
                        hitPoint = snappedHit.point;
                        snapNormal = snappedHit.normal;
                    }
                }
                else if (SnapManager.settings.snappingEnabled)
                {
                    hitPoint = SnapPosition(hitPoint, snapOnGrid, true);
                    mouseRay.origin = hitPoint - mouseRay.direction;
                    if (Physics.Raycast(mouseRay, out RaycastHit hitInfo)) snapNormal = hitInfo.normal;
                    else if (MeshUtils.Raycast(mouseRay, out RaycastHit meshHitInfo, out GameObject c,
                        octree.GetNearby(mouseRay, 1), float.MaxValue)) snapNormal = meshHitInfo.normal;
                }
                return hitPoint;
            }

            RaycastHit surfaceHit;
            bool surfaceFound = MouseRaycast(mouseRay, out surfaceHit, out GameObject collider,
                float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider);
            if (mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE && surfaceFound)
            {
                normal = surfaceHit.normal;
                point = SnapPoint(surfaceHit.point, ref normal);
                return true;
            }
            if (mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE)
            {
                if (surfaceFound)
                {
                    point = SnapPoint(surfaceHit.point, ref normal);
                    var direction = SnapManager.settings.rotation * Vector3.down;
                    var ray = new Ray(point - direction, direction);
                    if (MouseRaycast(ray, out RaycastHit hitInfo, out collider, float.MaxValue, -1,
                        paintOnPalettePrefabs, castOnMeshesWithoutCollider)) point = hitInfo.point;
                    UpdateGridOrigin(point);
                    return true;
                }
                if (GridRaycast(mouseRay, out RaycastHit gridHit))
                {
                    point = SnapPoint(gridHit.point, ref normal);
                    return true;
                }
            }
            return false;
        }

        private static bool _updateStroke = false;
        public static bool updateStroke { get => _updateStroke; set => _updateStroke = value; }
        public static void UpdateStroke()
        {
            updateStroke = true;
            UnityEditor.SceneView.RepaintAll();
        }

        public static void UpdateSelectedPersistentObject()
        {
            BrushstrokeManager.UpdateBrushstroke(false);
            switch (tool)
            {
                case ToolManager.PaintTool.LINE:
                    if (_selectedPersistentLineData != null) _editingPersistentLine = true;
                    break;
                case ToolManager.PaintTool.SHAPE:
                    if (_selectedPersistentShapeData != null) _editingPersistentShape = true;
                    break;
                case ToolManager.PaintTool.TILING:
                    if (_selectedPersistentTilingData != null) _editingPersistentTiling = true;
                    break;
            }
            repaint = true;
        }
        public static int selectedPointIdx
        {
            get
            {
                switch (ToolManager.tool)
                {
                    case ToolManager.PaintTool.TILING:
                        if (ToolManager.editMode)
                        {
                            if (_selectedPersistentTilingData == null) return -1;
                            return _selectedPersistentTilingData.selectedPointIdx;
                        }
                        else if (_tilingData.state == ToolManager.ToolState.EDIT) return _tilingData.selectedPointIdx;
                        break;
                    case ToolManager.PaintTool.LINE:
                        if (ToolManager.editMode)
                        {
                            if (_selectedPersistentLineData == null) return -1;
                            return _selectedPersistentLineData.selectedPointIdx;
                        }
                        else if (_lineData.state == ToolManager.ToolState.EDIT) return _lineData.selectedPointIdx;
                        break;
                    case ToolManager.PaintTool.SHAPE:
                        if (ToolManager.editMode)
                        {
                            if (_selectedPersistentShapeData == null) return -1;
                            return _selectedPersistentShapeData.selectedPointIdx;
                        }
                        else if (_shapeData.state == ToolManager.ToolState.EDIT) return _shapeData.selectedPointIdx;
                        break;
                }
                return -1;
            }
        }

        private static bool _updateHandlePosition = false;
        private static Vector3 _handlePosition;
        public static void UpdateHandlePosition()
        {
            _updateHandlePosition = true;
            if (tool == ToolManager.PaintTool.TILING && tilingData != null) ApplyTilingHandlePosition(tilingData);
            BrushstrokeManager.UpdateBrushstroke(false);

        }
        public static Vector3 handlePosition { get => _handlePosition; set => _handlePosition = value; }

        private static bool _updateHandleRotation = false;
        private static Quaternion _handleRotation;
        public static void UpdateHandleRotation()
        {
            _updateHandleRotation = true;
            BrushstrokeManager.UpdateBrushstroke(false);
        }
        public static Quaternion handleRotation { get => _handleRotation; set => _handleRotation = value; }

        #endregion

        #region PERSISTENT OBJECTS
        public static void OnUndoPerformed()
        {
            _octree = null;
            if (tool == ToolManager.PaintTool.LINE && UnityEditor.Undo.GetCurrentGroupName() == LineData.COMMAND_NAME)
            {
                OnUndoLine();
                UpdateStroke();
            }
            else if (tool == ToolManager.PaintTool.SHAPE && UnityEditor.Undo.GetCurrentGroupName() == ShapeData.COMMAND_NAME)
            {
                OnUndoShape();
                UpdateStroke();
            }
            else if (tool == ToolManager.PaintTool.TILING && UnityEditor.Undo.GetCurrentGroupName() == TilingData.COMMAND_NAME)
            {
                OnUndoTiling();
                UpdateStroke();
            }

            if (ToolManager.tool != ToolManager.PaintTool.LINE
                && ToolManager.tool != ToolManager.PaintTool.SHAPE
                && ToolManager.tool != ToolManager.PaintTool.TILING)
                BrushstrokeManager.UpdateBrushstroke();
            UnityEditor.SceneView.RepaintAll();
        }

        public static void OnToolChange(ToolManager.PaintTool prevTool)
        {
            switch (prevTool)
            {
                case ToolManager.PaintTool.LINE:
                    ResetLineState();
                    break;
                case ToolManager.PaintTool.SHAPE:
                    ResetShapeState();
                    break;
                case ToolManager.PaintTool.TILING:
                    ResetTilingState();
                    break;
                case ToolManager.PaintTool.EXTRUDE:
                    ResetExtrudeState();
                    break;
                case ToolManager.PaintTool.MIRROR:
                    ResetMirrorState();
                    break;
                default: break;
            }
            _meshesAndRenderers.Clear();
            UnityEditor.SceneView.RepaintAll();
        }

        private static void OnEditModeChanged()
        {
            switch (tool)
            {
                case ToolManager.PaintTool.LINE:
                    OnLineToolModeChanged();
                    break;
                case ToolManager.PaintTool.SHAPE:
                    OnShapeToolModeChanged();
                    break;
                case ToolManager.PaintTool.TILING:
                    OnTilingToolModeChanged();
                    break;
                default: break;
            }
        }

        private static void DeleteDisabledObjects()
        {
            if (_disabledObjects == null) return;
            foreach (var obj in _disabledObjects)
            {
                if (obj == null) continue;
                obj.SetActive(true);
                UnityEditor.Undo.DestroyObjectImmediate(obj);
            }
        }

        private static void ResetSelectedPersistentObject<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
            (PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> manager,
            ref bool editingPersistentObject, TOOL_DATA initialPersistentData)
            where TOOL_NAME : IToolName, new()
            where TOOL_SETTINGS : ICloneableToolSettings, new()
            where CONTROL_POINT : ControlPoint, new()
            where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
            where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
        {
            editingPersistentObject = false;
            if (initialPersistentData == null) return;
            var selectedItem = manager.GetItem(initialPersistentData.id);
            if (selectedItem == null) return;
            selectedItem.ResetPoses(initialPersistentData);
            selectedItem.selectedPointIdx = -1;
            selectedItem.ClearSelection();
        }

        private static void DeselectPersistentItems<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
            (PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> manager)
            where TOOL_NAME : IToolName, new()
            where TOOL_SETTINGS : ICloneableToolSettings, new()
            where CONTROL_POINT : ControlPoint, new()
            where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
            where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
        {
            var persitentTilings = manager.GetPersistentItems();
            foreach (var i in persitentTilings)
            {
                i.selectedPointIdx = -1;
                i.ClearSelection();
            }
        }

        private static bool ApplySelectedPersistentObject<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
            (bool deselectPoint, ref bool editingPersistentObject, ref TOOL_DATA initialPersistentData,
            ref TOOL_DATA selectedPersistentData,
            PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> manager)
            where TOOL_NAME : IToolName, new()
            where TOOL_SETTINGS : ICloneableToolSettings, new()
            where CONTROL_POINT : ControlPoint, new()
            where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
            where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
        {
            editingPersistentObject = false;
            if (initialPersistentData == null) return false;
            var selected = manager.GetItem(initialPersistentData.id);
            if (selected == null)
            {
                initialPersistentData = null;
                selectedPersistentData = null;
                return false;
            }
            selected.UpdatePoses();
            if (_paintStroke.Count > 0)
            {
                var objDic = Paint(selected.settings as IPaintToolSettings, PAINT_CMD, true, true);
                foreach (var paintedItem in objDic)
                {
                    var persistentItem = manager.GetItem(paintedItem.Key);
                    if (persistentItem == null) continue;
                    persistentItem.AddObjects(paintedItem.Value.ToArray());
                }
            }
            if (deselectPoint)
            {
                DeselectPersistentItems(manager);
            }
            DeleteDisabledObjects();

            _persistentPreviewData.Clear();
            if (!deselectPoint) return true;
            var persistentObjects = manager.GetPersistentItems();
            foreach (var item in persistentObjects)
            {
                item.selectedPointIdx = -1;
                item.ClearSelection();
            }
            return true;
        }

        #endregion

        #region OCTREE
        private const float MIN_OCTREE_NODE_SIZE = 0.5f;
        private static PointOctree<GameObject> _octree = new PointOctree<GameObject>(10, Vector3.zero, MIN_OCTREE_NODE_SIZE);
        private static System.Collections.Generic.List<GameObject> _paintedObjects
            = new System.Collections.Generic.List<GameObject>();
        public static PointOctree<GameObject> octree
        {
            get
            {
                if (_octree == null) UpdateOctree();
                return _octree;
            }
            set => _octree = value;
        }
        public static void UpdateOctree()
        {
            if (PaletteManager.paletteCount == 0) return;
            if ((tool == ToolManager.PaintTool.PIN || tool == ToolManager.PaintTool.BRUSH
                || tool == ToolManager.PaintTool.GRAVITY || tool == ToolManager.PaintTool.LINE
                || tool == ToolManager.PaintTool.SHAPE || tool == ToolManager.PaintTool.TILING)
                && PaletteManager.selectedBrushIdx < 0) return;

            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            _octree = null;
            _paintedObjects.Clear();
            var allPrefabsPaths = new System.Collections.Generic.List<string>();
            bool AddPrefabPath(MultibrushItemSettings item)
            {
                if (item.prefab == null) return false;
                var path = UnityEditor.AssetDatabase.GetAssetPath(item.prefab);
                if (allPrefabsPaths.Contains(path)) return false;
                allPrefabsPaths.Add(path);
                return true;
            }
            if (tool == ToolManager.PaintTool.ERASER || tool == ToolManager.PaintTool.REPLACER)
            {
                IModifierTool settings = EraserManager.settings;
                if (tool == ToolManager.PaintTool.REPLACER) settings = ReplacerManager.settings;
                if (settings.command == ModifierToolSettings.Command.MODIFY_PALETTE_PREFABS)
                    foreach (var brush in PaletteManager.selectedPalette.brushes)
                        foreach (var item in brush.items) AddPrefabPath(item);
                else if (PaletteManager.selectedBrush != null
                    && settings.command == ModifierToolSettings.Command.MODIFY_BRUSH_PREFABS)
                    foreach (var item in PaletteManager.selectedBrush.items) AddPrefabPath(item);
                SelectionManager.UpdateSelection();
                bool modifyAll = settings.command == ModifierToolSettings.Command.MODIFY_ALL;
                bool modifyAllButSelected = settings.modifyAllButSelected;
                foreach (var obj in allObjects)
                {
                    if (!obj.activeInHierarchy) continue;
                    if (!modifyAll && !UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(obj)) continue;
                    var prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                    bool isBrush = allPrefabsPaths.Contains(prefabPath);
                    if (!isBrush && !modifyAll) continue;
                    if (modifyAllButSelected && SelectionManager.selection.Contains(obj)) continue;
                    AddPaintedObject(obj);
                }
            }
            else
            {
                foreach (var brush in PaletteManager.selectedPalette.brushes)
                    foreach (var item in brush.items) AddPrefabPath(item);
                foreach (var obj in allObjects)
                {
                    if (!obj.activeInHierarchy) continue;
                    if (!UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(obj)) continue;
                    var prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                    bool isBrush = allPrefabsPaths.Contains(prefabPath);
                    if (isBrush) AddPaintedObject(obj);
                }
            }
            if (_octree == null) _octree = new PointOctree<GameObject>(10, Vector3.zero, MIN_OCTREE_NODE_SIZE);
        }

        private static void AddPaintedObject(GameObject obj)
        {
            if (_octree == null) _octree = new PointOctree<GameObject>(10, obj.transform.position, MIN_OCTREE_NODE_SIZE);
            _octree.Add(obj, obj.transform.position);
            _paintedObjects.Add(obj);
        }

        public static bool OctreeContains(int objId) => octree.Contains(objId);
        #endregion

        #region STROKE & PAINT
        private const string PWB_OBJ_NAME = "Prefab World Builder";
        private static Vector3 _prevMousePos = Vector3.zero;
        private static Vector3 _strokeDirection = Vector3.forward;
        private static Transform _autoParent = null;
        private static System.Collections.Generic.Dictionary<string, Transform> _subParents
            = new System.Collections.Generic.Dictionary<string, Transform>();
        private static Mesh quadMesh;

        private class PaintStrokeItem
        {
            public readonly GameObject prefab = null;
            public readonly Vector3 position = Vector3.zero;
            public readonly Quaternion rotation = Quaternion.identity;
            public readonly Vector3 scale = Vector3.one;
            public readonly int layer = 0;
            public readonly bool flipX = false;
            public readonly bool flipY = false;
            private Transform _parent = null;
            private string _persistentParentId = string.Empty;


            private Transform _surface = null;
            public Transform parent { get => _parent; set => _parent = value; }
            public string persistentParentId { get => _persistentParentId; set => _persistentParentId = value; }
            public Transform surface { get => _surface; set => _surface = value; }
            public PaintStrokeItem(GameObject prefab, Vector3 position, Quaternion rotation,
                Vector3 scale, int layer, Transform parent, Transform surface, bool flipX, bool flipY)
            {
                this.prefab = prefab;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
                this.layer = layer;
                this.flipX = flipX;
                this.flipY = flipY;
                _parent = parent;
                _surface = surface;
            }
        }
        private static System.Collections.Generic.List<PaintStrokeItem> _paintStroke
            = new System.Collections.Generic.List<PaintStrokeItem>();

        private static void BrushRadiusShortcuts(CircleToolBase settings)
        {
            if (PWBSettings.shortcuts.brushRadius.Check())
            {
                var combi = PWBSettings.shortcuts.brushRadius.combination;
                var delta = Mathf.Sign(combi.delta);
                settings.radius = Mathf.Max(settings.radius * (1f + delta * 0.03f), 0.05f);
                if (settings is BrushToolSettings)
                {
                    if (BrushManager.settings.heightType == BrushToolSettings.HeightType.RADIUS)
                        BrushManager.settings.maxHeightFromCenter = BrushManager.settings.radius;
                }
                ToolProperties.RepainWindow();
            }
        }

        private static void BrushstrokeMouseEvents(BrushToolBase settings)
        {
            if (PaletteManager.selectedBrush == null) return;
            if (Event.current.button == 0 && !Event.current.alt && Event.current.type == EventType.MouseUp
                && PaletteManager.selectedBrush.patternMachine != null
                && PaletteManager.selectedBrush.restartPatternForEachStroke)
            {
                PaletteManager.selectedBrush.patternMachine.Reset();
                BrushstrokeManager.UpdateBrushstroke();
            }
            else if (PWBSettings.shortcuts.brushUpdatebrushstroke.Check())
            {
                BrushstrokeManager.UpdateBrushstroke();
                repaint = true;
            }
            else if (PWBSettings.shortcuts.brushResetRotation.Check()) _brushAngle = 0;
            else if (PWBSettings.shortcuts.brushDensity.Check()
                && settings.brushShape != BrushToolBase.BrushShape.POINT)
            {
                settings.density += (int)Mathf.Sign(PWBSettings.shortcuts.brushDensity.combination.delta);
                ToolProperties.RepainWindow();
            }
            else if (PWBSettings.shortcuts.brushRotate.Check())
                _brushAngle -= PWBSettings.shortcuts.brushRotate.combination.delta * 1.8f; //180deg/100px
            if (Event.current.button == 1)
            {
                if (Event.current.type == EventType.MouseDown && (Event.current.control || Event.current.shift))
                {
                    _pinned = true;
                    _pinMouse = Event.current.mousePosition;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp && !Event.current.control && !Event.current.shift)
                    _pinned = false;
            }
            if ((Event.current.keyCode == KeyCode.LeftControl || Event.current.keyCode == KeyCode.RightControl
                || Event.current.keyCode == KeyCode.RightShift || Event.current.keyCode == KeyCode.LeftShift)
                && Event.current.type == EventType.KeyUp) _pinned = false;
        }

        private struct MeshAndRenderer
        {
            public Mesh mesh;
            public Renderer renderer;

            public MeshAndRenderer(Mesh mesh, Renderer renderer)
            {
                this.mesh = mesh;
                this.renderer = renderer;
            }
        }

        private static System.Collections.Generic.Dictionary<int, MeshAndRenderer[]> _meshesAndRenderers
            = new System.Collections.Generic.Dictionary<int, MeshAndRenderer[]>();

        private static void PreviewBrushItem(GameObject prefab, Matrix4x4 rootToWorld, int layer,
            Camera camera, bool redMaterial, bool reverseTriangles, bool flipX, bool flipY)
        {
            var id = prefab.GetInstanceID();
            if (!_meshesAndRenderers.ContainsKey(id))
            {
                var meshesAndRenderers = new System.Collections.Generic.List<MeshAndRenderer>();
                var filters = prefab.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in filters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null) continue;
                    var renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer == null) continue;
                    meshesAndRenderers.Add(new MeshAndRenderer(mesh, renderer));
                }
                var skinedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in skinedMeshRenderers)
                {
                    var mesh = renderer.sharedMesh;
                    if (mesh == null) continue;
                    meshesAndRenderers.Add(new MeshAndRenderer(mesh, renderer));
                }
                _meshesAndRenderers.Add(id, meshesAndRenderers.ToArray());
            }
            foreach (var item in _meshesAndRenderers[id])
            {
                var mesh = item.mesh;
                var childToWorld = rootToWorld * item.renderer.transform.localToWorldMatrix;

                if (!redMaterial)
                {
                    if (item.renderer is SkinnedMeshRenderer)
                    {
                        var smr = (SkinnedMeshRenderer)item.renderer;
                        var rootBone = smr.rootBone;
                        if (rootBone != null)
                        {
                            while (rootBone.parent != null && rootBone.parent != prefab.transform) rootBone = rootBone.parent;
                            var rotation = rootBone.rotation;
                            var position = rootBone.position;
                            position.y = 0f;
                            var scale = rootBone.localScale;
                            childToWorld = rootToWorld * Matrix4x4.TRS(position, rotation, scale);
                        }
                    }
                    var materials = item.renderer.sharedMaterials;
                    if (materials == null && materials.Length > 0 && materials.Length >= mesh.subMeshCount) continue;
                    for (int subMeshIdx = 0; subMeshIdx < Mathf.Min(mesh.subMeshCount, materials.Length); ++subMeshIdx)
                    {
                        var material = materials[subMeshIdx];
                        if (reverseTriangles)
                        {
                            var tempMesh = (Mesh)GameObject.Instantiate(mesh);
                            tempMesh.SetTriangles(mesh.triangles.Reverse().ToArray(), subMeshIdx);
                            tempMesh.subMeshCount = mesh.subMeshCount;
                            int vCount = 0;
                            for (int i = 0; i < mesh.subMeshCount; ++i)
                            {
                                var desc = mesh.GetSubMesh(mesh.subMeshCount - i - 1);
                                desc.indexStart = vCount;
                                tempMesh.SetSubMesh(i, desc);
                                vCount += desc.indexCount;
                            }
                            material = materials[mesh.subMeshCount - subMeshIdx - 1];
                            Graphics.DrawMesh(tempMesh, childToWorld, material, layer, camera, subMeshIdx);
                            tempMesh = null;
                        }
                        else Graphics.DrawMesh(mesh, childToWorld, material, layer, camera, subMeshIdx);
                    }
                }
                else
                {
                    for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; ++subMeshIdx)
                        Graphics.DrawMesh(mesh, childToWorld, transparentRedMaterial, layer, camera, subMeshIdx);
                }
            }
            var SpriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>()
                .Where(r => r.enabled && r.sprite != null && r.gameObject.activeSelf).ToArray();
            if (SpriteRenderers.Length > 0)
            {
                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform);

                foreach (var spriteRenderer in SpriteRenderers)
                    DrawSprite(spriteRenderer, rootToWorld, camera, bounds, flipX, flipY);
            }
        }
        private static void DrawSprite(SpriteRenderer renderer, Matrix4x4 matrix,
            Camera camera, Bounds objectBounds, bool flipX, bool flipY)
        {
            if (quadMesh == null)
            {
                quadMesh = new Mesh
                {
                    vertices = new[] { new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0),
                      new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0) },
                    normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward },
                    triangles = new[] { 0, 2, 3, 3, 1, 0 }
                };
            }
            var minUV = new Vector2(float.MaxValue, float.MaxValue);
            var maxUV = new Vector2(float.MinValue, float.MinValue);
            foreach (var uv in renderer.sprite.uv)
            {
                minUV = Vector2.Min(minUV, uv);
                maxUV = Vector2.Max(maxUV, uv);
            }
            var uvs = new Vector2[] { new Vector2(minUV.x, maxUV.y),  new Vector2(maxUV.x, maxUV.y),
                new Vector2(minUV.x, minUV.y), new Vector2(maxUV.x, minUV.y)};
            void ToggleFlip(ref bool flip) => flip = !flip;
            if (renderer.flipX) ToggleFlip(ref flipX);
            if (renderer.flipY) ToggleFlip(ref flipY);
            if (flipX)
            {
                uvs[0].x = maxUV.x;
                uvs[1].x = minUV.x;
                uvs[2].x = maxUV.x;
                uvs[3].x = minUV.x;
            }
            if (flipY)
            {
                uvs[0].y = minUV.y;
                uvs[1].y = minUV.y;
                uvs[2].y = maxUV.y;
                uvs[3].y = maxUV.y;
            }
            quadMesh.uv = uvs;
            var pivotToCenter = (renderer.sprite.rect.size / 2 - renderer.sprite.pivot) / renderer.sprite.pixelsPerUnit;
            if (renderer.flipX) pivotToCenter.x = -pivotToCenter.x;
            if (renderer.flipY) pivotToCenter.y = -pivotToCenter.y;
            var mpb = new MaterialPropertyBlock();
            mpb.SetTexture("_MainTex", renderer.sprite.texture);
            mpb.SetColor("_Color", renderer.color);
            matrix *= Matrix4x4.Translate(pivotToCenter);
            matrix *= renderer.transform.localToWorldMatrix;
            matrix *= Matrix4x4.Scale(new Vector3(
                renderer.sprite.textureRect.width / renderer.sprite.pixelsPerUnit,
                renderer.sprite.textureRect.height / renderer.sprite.pixelsPerUnit, 1));
            Graphics.DrawMesh(quadMesh, matrix, renderer.sharedMaterial, 0, camera, 0, mpb);
        }
        public static bool painting { get; set; }
        private const string PAINT_CMD = "Paint";
        private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<GameObject>>
            Paint(IPaintToolSettings settings, string commandName = PAINT_CMD,
            bool addTempCollider = true, bool persistent = false, string toolObjectId = "")
        {
            painting = true;
            var paintedObjects = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.List<GameObject>>();
            if (_paintStroke.Count == 0)
            {
                if (BrushstrokeManager.brushstroke.Length == 0) BrushstrokeManager.UpdateBrushstroke();
                return paintedObjects;
            }

            foreach (var item in _paintStroke)
            {
                if (item.prefab == null) continue;
                var persistentParentId = persistent ? item.persistentParentId : toolObjectId;
                var type = UnityEditor.PrefabUtility.GetPrefabAssetType(item.prefab);
                GameObject obj = type == UnityEditor.PrefabAssetType.NotAPrefab ? GameObject.Instantiate(item.prefab)
                    : (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab
                    (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(item.prefab)
                    ? item.prefab : UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(item.prefab));
                if (settings.overwritePrefabLayer) obj.layer = settings.layer;
                obj.transform.SetPositionAndRotation(item.position, item.rotation);
                obj.transform.localScale = item.scale;
                var root = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                item.parent = GetParent(settings, item.prefab.name, true, item.surface, persistentParentId);
                if (addTempCollider) PWBCore.AddTempCollider(obj);
                if (!paintedObjects.ContainsKey(persistentParentId))
                    paintedObjects.Add(persistentParentId, new System.Collections.Generic.List<GameObject>());
                paintedObjects[persistentParentId].Add(obj);
                var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();

                foreach (var spriteRenderer in spriteRenderers)
                {
                    var flipX = spriteRenderer.flipX;
                    var flipY = spriteRenderer.flipY;
                    if (item.flipX) flipX = !flipX;
                    if (item.flipY) flipY = !flipY;
                    spriteRenderer.flipX = flipX;
                    spriteRenderer.flipY = flipY;
                    var center = BoundsUtils.GetBoundsRecursive(spriteRenderer.transform,
                        spriteRenderer.transform.rotation).center;
                    var pivotToCenter = center - spriteRenderer.transform.position;
                    var delta = Vector3.zero;
                    if (item.flipX) delta.x = pivotToCenter.x * -2;
                    if (item.flipY) delta.y = pivotToCenter.y * -2;
                    spriteRenderer.transform.position += delta;
                }

                AddPaintedObject(obj);
                UnityEditor.Undo.RegisterCreatedObjectUndo(obj, commandName);
                if (root != null) UnityEditor.Undo.SetTransformParent(root.transform, item.parent, commandName);
                else UnityEditor.Undo.SetTransformParent(obj.transform, item.parent, commandName);
            }
            if (_paintStroke.Count > 0) BrushstrokeManager.UpdateBrushstroke();
            _paintStroke.Clear();
            return paintedObjects;
        }

        public static void ResetAutoParent() => _autoParent = null;

        private const string NO_PALETTE_NAME = "<#PALETTE@>";
        private const string NO_TOOL_NAME = "<#TOOL@>";
        private const string NO_OBJ_ID = "<#ID@>";
        private const string NO_BRUSH_NAME = "<#BRUSH@>";
        private const string NO_PREFAB_NAME = "<#PREFAB@>";
        private const string PARENT_KEY_SEPARATOR = "<#@>";

        private static Transform GetParent(IPaintToolSettings settings, string prefabName,
            bool create, Transform surface, string toolObjectId = "")
        {
            if (!create) return settings.parent;
            if (settings.autoCreateParent)
            {
                var pwbObj = GameObject.Find(PWB_OBJ_NAME);
                if (pwbObj == null) _autoParent = new GameObject(PWB_OBJ_NAME).transform;
                else _autoParent = pwbObj.transform;
            }
            else _autoParent = settings.setSurfaceAsParent ? surface : settings.parent;
            if (!settings.createSubparentPerPalette && !settings.createSubparentPerTool
                && !settings.createSubparentPerBrush && !settings.createSubparentPerPrefab) return _autoParent;

            var _autoParentId = _autoParent == null ? -1 : _autoParent.gameObject.GetInstanceID();
            string GetSubParentKey(int parentId = -1, string palette = NO_PALETTE_NAME, string tool = NO_TOOL_NAME,
                string id = NO_OBJ_ID, string brush = NO_BRUSH_NAME, string prefab = NO_PREFAB_NAME)
                => parentId + PARENT_KEY_SEPARATOR + palette + PARENT_KEY_SEPARATOR + tool + PARENT_KEY_SEPARATOR
                + id + PARENT_KEY_SEPARATOR + brush + PARENT_KEY_SEPARATOR + prefab;

            string subParentKey = GetSubParentKey(_autoParentId,
                settings.createSubparentPerPalette ? PaletteManager.selectedPalette.name : NO_PALETTE_NAME,
                settings.createSubparentPerTool ? ToolManager.tool.ToString() : NO_TOOL_NAME,
                string.IsNullOrEmpty(toolObjectId) ? NO_OBJ_ID : toolObjectId,
                settings.createSubparentPerBrush ? PaletteManager.selectedBrush.name : NO_BRUSH_NAME,
                settings.createSubparentPerPrefab ? prefabName : NO_PREFAB_NAME);

            create = !(_subParents.ContainsKey(subParentKey));
            if (!create && _subParents[subParentKey] == null) create = true;
            if (!create) return _subParents[subParentKey];

            Transform CreateSubParent(string key, string name, Transform transformParent)
            {
                Transform subParentTransform = null;
                var subParentIsEmpty = true;
                if (transformParent != null)
                {
                    subParentTransform = transformParent.Find(name);
                    if (subParentTransform != null)
                        subParentIsEmpty = subParentTransform.GetComponents<Component>().Length == 1;
                }
                if (subParentTransform == null || !subParentIsEmpty)
                {
                    var obj = new GameObject(name);
                    var subParent = obj.transform;
                    subParent.SetParent(transformParent);
                    subParent.localPosition = Vector3.zero;
                    subParent.localRotation = Quaternion.identity;
                    subParent.localScale = Vector3.one;
                    if (_subParents.ContainsKey(key)) _subParents[key] = subParent;
                    else _subParents.Add(key, subParent);
                    return subParent;
                }
                return subParentTransform;
            }

            var parent = _autoParent;
            void CreateSubParentIfDoesntExist(string name, string palette = NO_PALETTE_NAME,
                string tool = NO_TOOL_NAME, string id = NO_OBJ_ID, string brush = NO_BRUSH_NAME,
                string prefab = NO_PREFAB_NAME)
            {
                var key = GetSubParentKey(_autoParentId, palette, tool, id, brush, prefab);
                var keyExist = _subParents.ContainsKey(key);
                var subParent = keyExist ? _subParents[key] : null;
                if (subParent != null) parent = subParent;
                if (!keyExist || subParent == null) parent = CreateSubParent(key, name, parent);
            }

            var keySplitted = subParentKey.Split(new string[] { PARENT_KEY_SEPARATOR },
                System.StringSplitOptions.None);
            var keyPlaletteName = keySplitted[1];
            var keyToolName = keySplitted[2];
            var keyToolObjId = keySplitted[3];
            var keyBrushName = keySplitted[4];
            var keyPrefabName = keySplitted[5];

            if (keyPlaletteName != NO_PALETTE_NAME)
                CreateSubParentIfDoesntExist(keyPlaletteName, keyPlaletteName);
            if (keyToolName != NO_TOOL_NAME)
            {
                CreateSubParentIfDoesntExist(keyToolName, keyPlaletteName, keyToolName);
                if (keyToolObjId != NO_OBJ_ID)
                    CreateSubParentIfDoesntExist(keyToolObjId, keyPlaletteName, keyToolName, keyToolObjId);
            }
            if (keyBrushName != NO_BRUSH_NAME)
                CreateSubParentIfDoesntExist(keyBrushName, keyPlaletteName, keyToolName, keyToolObjId, keyBrushName);
            if (keyPrefabName != NO_PREFAB_NAME)
                CreateSubParentIfDoesntExist(keyPrefabName, keyPlaletteName,
                    keyToolName, keyToolObjId, keyBrushName, keyPrefabName);
            return parent;
        }

        private static bool IsVisible(ref GameObject obj)
        {
            if (obj == null) return false;
            var parentRenderer = obj.GetComponentInParent<Renderer>();
            var parentTerrain = obj.GetComponentInParent<Terrain>();
            if (parentRenderer != null) obj = parentRenderer.gameObject;
            else if (parentTerrain != null) obj = parentTerrain.gameObject;
            else
            {
                var parent = obj.transform.parent;
                if (parent != null)
                {
                    var siblingRenderer = parent.GetComponentInChildren<Renderer>();
                    var siblingTerrain = parent.GetComponentInChildren<Terrain>();
                    if (siblingRenderer != null) obj = parent.gameObject;
                    else if (siblingTerrain != null) obj = parent.gameObject;

                }
            }
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                foreach (var renderer in renderers)
                    if (renderer.enabled) return true;
            }
            var terrains = obj.GetComponentsInChildren<Terrain>();
            if (terrains.Length > 0)
            {
                foreach (var terrain in terrains)
                    if (terrain.enabled) return true;
            }
            return false;
        }
        private static bool IsVisible(GameObject obj)
        {
            obj = PWBCore.GetGameObjectFromTempCollider(obj);
            return IsVisible(ref obj);
        }
        private struct TerrainDataSimple
        {
            public float[,,] alphamaps;
            public Vector3 size;
            public TerrainLayer[] layers;
            public TerrainDataSimple(float[,,] alphamaps, Vector3 size, TerrainLayer[] layers)
                => (this.alphamaps, this.size, this.layers) = (alphamaps, size, layers);
        }
        private static System.Collections.Generic.Dictionary<int, TerrainDataSimple> _terrainAlphamaps
            = new System.Collections.Generic.Dictionary<int, TerrainDataSimple>();
        public static bool MouseRaycast(Ray mouseRay, out RaycastHit mouseHit,
            out GameObject collider, float maxDistance, LayerMask layerMask,
            bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider, string[] tags = null,
            GameObject[] invisibleExeptions = null, TerrainLayer[] terrainLayers = null, GameObject[] exeptions = null)
        {
            mouseHit = new RaycastHit();
            collider = null;
            bool validHit = Physics.Raycast(mouseRay, out mouseHit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
            bool physicsHit = validHit;
            if (validHit && !castOnMeshesWithoutCollider)
            {
                var obj = mouseHit.collider.gameObject;
                var hitParent = obj.transform.parent;
                if (hitParent != null && hitParent.gameObject.GetInstanceID() == PWBCore.parentColliderId)
                    physicsHit = false;
            }

            GameObject[] nearbyObjects = null;
            if (!physicsHit)
            {
                if (!castOnMeshesWithoutCollider) return false;
                nearbyObjects = octree.GetNearby(mouseRay, 1f);
                validHit = MeshUtils.Raycast(mouseRay, out mouseHit, out collider, nearbyObjects, maxDistance);
            }
            else collider = mouseHit.collider.gameObject;
            if (!validHit) return false;

            RaycastHit[] hitArray = null;
            GameObject[] colliders = null;
            if (physicsHit) hitArray = Physics.RaycastAll(mouseRay, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
            else MeshUtils.RaycastAll(mouseRay, out hitArray, out colliders, nearbyObjects, maxDistance);
            validHit = false;
            var minDistance = float.MaxValue;
            for (int i = 0; i < hitArray.Length; ++i)
            {
                if (physicsHit && hitArray[i].collider == null) continue;
                if (!physicsHit && colliders[i] == null) continue;
                var obj = physicsHit ? hitArray[i].collider.gameObject : colliders[i];
                var hitParent = obj.transform.parent;
                if (hitParent != null && hitParent.gameObject.GetInstanceID() == PWBCore.parentColliderId)
                    obj = PWBCore.GetGameObjectFromTempColliderId(obj.GetInstanceID());
                if (obj == null) continue;
                if (tags != null) if (!tags.Contains(obj.tag)) continue;
                if (exeptions != null && exeptions.Contains(obj)) continue;
                if (!paintOnPalettePrefabs && PaletteManager.selectedPalette.ContainsSceneObject(obj)) continue;
                var checkInvisibility = invisibleExeptions == null || !invisibleExeptions.Contains(obj);
                if (checkInvisibility) if (!IsVisible(ref obj)) continue;
                if (terrainLayers != null && terrainLayers.Length > 0)
                {
                    var terrain = obj.GetComponent<Terrain>();
                    if (terrain != null)
                    {
                        var instanceId = terrain.GetInstanceID();
                        int alphamapW = 0;
                        int alphamapH = 0;
                        float[,,] alphamaps;
                        Vector3 terrainSize;
                        TerrainLayer[] layers;
                        if (_terrainAlphamaps.ContainsKey(instanceId))
                        {
                            alphamaps = _terrainAlphamaps[instanceId].alphamaps;
                            alphamapW = alphamaps.GetLength(1);
                            alphamapH = alphamaps.GetLength(0);
                            terrainSize = _terrainAlphamaps[instanceId].size;
                            layers = _terrainAlphamaps[instanceId].layers;
                        }
                        else
                        {
                            var terrainData = terrain.terrainData;
                            alphamapW = terrainData.alphamapWidth;
                            alphamapH = terrainData.alphamapHeight;
                            alphamaps = terrainData.GetAlphamaps(0, 0, alphamapW, alphamapH);
                            terrainSize = terrainData.size;
                            layers = terrainData.terrainLayers;
                            _terrainAlphamaps.Add(instanceId, new TerrainDataSimple(alphamaps, terrainSize, layers));

                        }
                        var numLayers = alphamaps.GetLength(2);

                        var localHit = terrain.transform.InverseTransformPoint(mouseHit.point);
                        var alphaHitX = Mathf.RoundToInt(localHit.x / terrainSize.x * alphamapW);
                        var alphaHitZ = Mathf.RoundToInt(localHit.z / terrainSize.z * alphamapH);

                        int layerUnderCursorIdx = 0;
                        for (int k = 1; k < numLayers; k++)
                        {
                            if (alphamaps[alphaHitZ, alphaHitX, k] > 0.5)
                            {
                                layerUnderCursorIdx = k;
                                break;
                            }
                        }
                        var layerUnderCursor = layers[layerUnderCursorIdx];
                        if (!terrainLayers.Contains(layerUnderCursor)) continue;
                    }
                }
                if (hitArray[i].distance < minDistance)
                {
                    minDistance = hitArray[i].distance;
                    validHit = true;
                    mouseHit = hitArray[i];
                    collider = obj;
                    continue;
                }

            }
            return validHit;
        }

        public static float GetBottomDistanceToSurface(Vector3[] bottomVertices, Matrix4x4 TRS,
            float bottomMagnitude, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider)
        {
            var positiveDistance = float.MinValue;
            var negativeDistance = float.MaxValue;
            var down = (TRS.rotation * Vector3.down).normalized;
            bool noSurfaceFound = true;
            void GetDistance(float height, Vector3 direction)
            {
                foreach (var vertex in bottomVertices)
                {
                    var origin = TRS.MultiplyPoint(vertex);

                    var ray = new Ray(origin - (direction * height), direction);
                    if (MouseRaycast(ray, out RaycastHit hitInfo, out GameObject collider,
                        float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider))
                    {

                        if (hitInfo.distance >= height)
                            positiveDistance = Mathf.Max(hitInfo.distance - height, positiveDistance);
                        else negativeDistance = Mathf.Min(height - hitInfo.distance, negativeDistance);
                        noSurfaceFound = false;
                    }
                }
            }
            float hMult = 100f;
            GetDistance(Mathf.Max(bottomMagnitude * hMult, hMult), down);
            if (noSurfaceFound) return 0f;
            if (positiveDistance < 0)
            {
                positiveDistance = float.MinValue;
                negativeDistance = float.MaxValue;
                noSurfaceFound = true;
                GetDistance(negativeDistance * 0.98f, down);
            }
            if (noSurfaceFound) return 0f;
            var distance = positiveDistance >= 0 ? positiveDistance : -negativeDistance;
            return distance;
        }

        public static float GetBottomDistanceToSurfaceSigned(Vector3[] bottomVertices, Matrix4x4 TRS,
            float maxDistance, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider)
        {
            float distance = 0f;
            var down = Vector3.down;
            foreach (var vertex in bottomVertices)
            {
                var origin = TRS.MultiplyPoint(vertex);
                var ray = new Ray(origin - down * maxDistance, down);
                if (MouseRaycast(ray, out RaycastHit hitInfo, out GameObject collider,
                    float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider))
                {
                    var d = hitInfo.distance - maxDistance;
                    if (Mathf.Abs(d) > Mathf.Abs(distance)) distance = d;
                }
            }
            return distance;
        }

        public static float GetPivotDistanceToSurfaceSigned(Vector3 pivot,
            float maxDistance, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider)
        {
            var ray = new Ray(pivot + Vector3.up * maxDistance, Vector3.down);
            if (MouseRaycast(ray, out RaycastHit hitInfo, out GameObject collider,
                    float.MaxValue, -1, paintOnPalettePrefabs, castOnMeshesWithoutCollider))
                return hitInfo.distance - maxDistance;
            return 0;
        }

        private static BrushstrokeItem[] _brushstroke = null;
        private struct PreviewData
        {
            public readonly GameObject prefab;
            public readonly Matrix4x4 rootToWorld;
            public readonly int layer;
            public readonly bool flipX;
            public readonly bool flipY;
            public PreviewData(GameObject prefab, Matrix4x4 rootToWorld, int layer, bool flipX, bool flipY)
            {
                this.prefab = prefab;
                this.rootToWorld = rootToWorld;
                this.layer = layer;
                this.flipX = flipX;
                this.flipY = flipY;
            }
        }
        private static System.Collections.Generic.List<PreviewData> _previewData
            = new System.Collections.Generic.List<PreviewData>();

        private static bool PreviewIfBrushtrokestaysTheSame(out BrushstrokeItem[] brushstroke,
            Camera camera, bool forceUpdate)
        {
            brushstroke = BrushstrokeManager.brushstroke;
            if (!forceUpdate && _brushstroke != null && BrushstrokeManager.BrushstrokeEqual(brushstroke, _brushstroke))
            {
                foreach (var previewItemData in _previewData)
                    PreviewBrushItem(previewItemData.prefab, previewItemData.rootToWorld,
                        previewItemData.layer, camera, false, false, previewItemData.flipX, previewItemData.flipY);
                return true;
            }
            _brushstroke = BrushstrokeManager.brushstrokeClone;
            _previewData.Clear();
            return false;
        }

        private static System.Collections.Generic.Dictionary<long, PreviewData[]> _persistentPreviewData
            = new System.Collections.Generic.Dictionary<long, PreviewData[]>();
        private static System.Collections.Generic.Dictionary<long, BrushstrokeItem[]> _persistentLineBrushstrokes
            = new System.Collections.Generic.Dictionary<long, BrushstrokeItem[]>();

        private static void PreviewPersistent(Camera camera)
        {
            foreach (var previewDataArray in _persistentPreviewData.Values)
                foreach (var previewItemData in previewDataArray)
                    PreviewBrushItem(previewItemData.prefab, previewItemData.rootToWorld,
                        previewItemData.layer, camera, false, false, previewItemData.flipX, previewItemData.flipY);
        }

        #endregion

        #region BRUSH SHAPE INDICATOR
        private static void DrawCricleIndicator(Vector3 hitPoint, Vector3 hitNormal,
            float radius, float height, Vector3 tangent, Vector3 bitangent,
            Vector3 normal, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider,
            int layerMask = -1, string[] tags = null)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            const float normalOffset = 0.01f;
            const float polygonSideSize = 0.3f;
            const int minPolygonSides = 12;
            const int maxPolygonSides = 36;
            var polygonSides = Mathf.Clamp((int)(TAU * radius / polygonSideSize), minPolygonSides, maxPolygonSides);

            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.5f);
            var periPoints = new System.Collections.Generic.List<Vector3>();
            var periPointsShadow = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < polygonSides; ++i)
            {
                var radians = TAU * i / (polygonSides - 1f);
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(tangent, bitangent, tangentDir);
                var periPoint = hitPoint + (worldDir * (radius));
                var periRay = new Ray(periPoint + normal * height, -normal);
                if (MouseRaycast(periRay, out RaycastHit periHit, out GameObject collider,
                    height * 2, layerMask, paintOnPalettePrefabs, castOnMeshesWithoutCollider, tags))
                {
                    var periHitPoint = periHit.point + hitNormal * normalOffset;
                    var shadowPoint = periHitPoint + worldDir * 0.2f;
                    periPoints.Add(periHitPoint);
                    periPointsShadow.Add(shadowPoint);
                }
                else
                {
                    if (periPoints.Count > 0 && i == polygonSides - 1)
                    {
                        periPoints.Add(periPoints[0]);
                        periPointsShadow.Add(periPointsShadow[0]);
                    }
                    else
                    {
                        float binSearchRadius = radius;
                        float delta = -binSearchRadius / 2;

                        for (int j = 0; j < 8; ++j)
                        {
                            binSearchRadius += delta;
                            periPoint = hitPoint + (worldDir * binSearchRadius);
                            periRay = new Ray(periPoint + normal * height, -normal);
                            if (MouseRaycast(periRay, out RaycastHit binSearchPeriHit,
                                out GameObject binSearchCollider, height * 2, layerMask,
                                paintOnPalettePrefabs, castOnMeshesWithoutCollider, tags))
                            {
                                delta = Mathf.Abs(delta) / 2;
                                periHit = binSearchPeriHit;
                            }
                            else delta = -Mathf.Abs(delta) / 2;
                            if (Mathf.Abs(delta) < 0.01) break;
                        }
                        if (periHit.point == Vector3.zero) continue;
                        var periHitPoint = periHit.point + hitNormal * normalOffset;
                        var shadowPoint = periHitPoint + worldDir * 0.2f;
                        periPoints.Add(periHitPoint);
                        periPointsShadow.Add(shadowPoint);
                    }
                }
            }
            if (periPoints.Count > 0)
            {
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.5f);
                UnityEditor.Handles.DrawAAPolyLine(3, periPoints.ToArray());
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.5f);
                UnityEditor.Handles.DrawAAPolyLine(6, periPointsShadow.ToArray());
            }
            else
            {
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.5f);
                UnityEditor.Handles.DrawWireDisc(hitPoint + hitNormal * normalOffset, hitNormal, radius);
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.5f);
                UnityEditor.Handles.DrawWireDisc(hitPoint + hitNormal * normalOffset, hitNormal, radius + 0.2f);
            }
        }

        private static void DrawSquareIndicator(Vector3 hitPoint, Vector3 hitNormal,
            float radius, float height, Vector3 tangent, Vector3 bitangent,
            Vector3 normal, bool paintOnPalettePrefabs, bool castOnMeshesWithoutCollider,
            int layerMask = -1, string[] tags = null)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            const float normalOffset = 0.01f;

            const int minSideSegments = 4;
            const int maxSideSegments = 15;
            var segmentsPerSide = Mathf.Clamp((int)(radius * 2 / 0.3f), minSideSegments, maxSideSegments);
            var segmentCount = segmentsPerSide * 4;
            float segmentSize = radius * 2f / segmentsPerSide;
            float SQRT2 = Mathf.Sqrt(2f);
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.5f);
            var periPoints = new System.Collections.Generic.List<Vector3>();

            for (int i = 0; i < segmentCount; ++i)
            {
                int sideIdx = i / segmentsPerSide;
                int segmentIdx = i % segmentsPerSide;
                var periPoint = hitPoint;
                if (sideIdx == 0) periPoint += tangent * (segmentSize * segmentIdx - radius) + bitangent * radius;
                else if (sideIdx == 1) periPoint += bitangent * (radius - segmentSize * segmentIdx) + tangent * radius;
                else if (sideIdx == 2) periPoint += tangent * (radius - segmentSize * segmentIdx) - bitangent * radius;
                else periPoint += bitangent * (segmentSize * segmentIdx - radius) - tangent * radius;
                var worldDir = (periPoint - hitPoint).normalized;
                var periRay = new Ray(periPoint + normal * height, -normal);
                if (MouseRaycast(periRay, out RaycastHit periHit, out GameObject collider,
                    height * 2, layerMask, paintOnPalettePrefabs, castOnMeshesWithoutCollider, tags))
                {
                    var periHitPoint = periHit.point + hitNormal * normalOffset;
                    periPoints.Add(periHitPoint);
                }
                else
                {
                    float binSearchRadius = radius * SQRT2;
                    float delta = -binSearchRadius / 2;

                    for (int j = 0; j < 8; ++j)
                    {
                        binSearchRadius += delta;
                        periPoint = hitPoint + (worldDir * binSearchRadius);
                        periRay = new Ray(periPoint + normal * height, -normal);
                        if (MouseRaycast(periRay, out RaycastHit binSearchPeriHit,
                            out GameObject binSearchCollider, height * 2, layerMask,
                            paintOnPalettePrefabs, castOnMeshesWithoutCollider, tags))
                        {
                            delta = Mathf.Abs(delta) / 2;
                            periHit = binSearchPeriHit;
                        }
                        else delta = -Mathf.Abs(delta) / 2;
                        if (Mathf.Abs(delta) < 0.01) break;
                    }
                    if (periHit.point == Vector3.zero)
                        continue;
                    var periHitPoint = periHit.point + hitNormal * normalOffset;
                    var shadowPoint = periHitPoint + worldDir * 0.2f;
                    periPoints.Add(periHitPoint);

                }
            }
            if (periPoints.Count > 0)
            {
                periPoints.Add(periPoints[0]);
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(8, periPoints.ToArray());

                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
                UnityEditor.Handles.DrawAAPolyLine(4, periPoints.ToArray());
            }
        }
        #endregion

        #region HANDLES
        private static void DrawDotHandleCap(Vector3 point, float alpha = 1f,
            float scale = 1f, bool selected = false)
        {
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f * alpha);
            var handleSize = UnityEditor.HandleUtility.GetHandleSize(point);
            var sizeDelta = handleSize * 0.0125f;
            UnityEditor.Handles.DotHandleCap(0, point, Quaternion.identity,
                handleSize * 0.0325f * scale * PWBCore.staticData.controPointSize, EventType.Repaint);
            var fillColor = selected ? Color.cyan : UnityEditor.Handles.preselectionColor;
            fillColor.a *= alpha;
            UnityEditor.Handles.color = fillColor;
            UnityEditor.Handles.DotHandleCap(0, point, Quaternion.identity,
                (handleSize * 0.0325f * scale - sizeDelta) * PWBCore.staticData.controPointSize, EventType.Repaint);
        }
        #endregion

        #region DRAG AND DROP
        public class SceneDragReceiver : ISceneDragReceiver
        {
            private int _brushID = -1;
            public int brushId { get => _brushID; set => _brushID = value; }
            public void PerformDrag(Event evt) { }
            public void StartDrag() { }
            public void StopDrag() { }
            public UnityEditor.DragAndDropVisualMode UpdateDrag(Event evt, EventType eventType)
            {
                PrefabPalette.instance.DeselectAllButThis(_brushID);
                ToolManager.tool = ToolManager.PaintTool.PIN;
                return UnityEditor.DragAndDropVisualMode.Generic;
            }
        }
        private static SceneDragReceiver _sceneDragReceiver = new SceneDragReceiver();
        public static SceneDragReceiver sceneDragReceiver => _sceneDragReceiver;




        #endregion

        #region PALETTE
        private static void PaletteInput(UnityEditor.SceneView sceneView)
        {
            void Repaint()
            {
                PrefabPalette.RepainWindow();
                sceneView.Repaint();
                repaint = true;
                AsyncRepaint();
            }
            if (PWBSettings.shortcuts.palettePreviousBrush.Check())
            {
                PaletteManager.SelectPreviousBrush();
                Repaint();
            }
            else if (PWBSettings.shortcuts.paletteNextBrush.Check())
            {
                PaletteManager.SelectNextBrush();
                Repaint();
            }
            if (PWBSettings.shortcuts.paletteNextBrushScroll.Check())
            {
                Event.current.Use();
                if (PWBSettings.shortcuts.paletteNextBrushScroll.combination.delta > 0) PaletteManager.SelectNextBrush();
                else PaletteManager.SelectPreviousBrush();
                Repaint();
            }
            if (PWBSettings.shortcuts.paletteNextPaletteScroll.Check())
            {
                Event.current.Use();
                if (Event.current.delta.y > 0) PaletteManager.SelectNextPalette();
                else PaletteManager.SelectPreviousPalette();
                Repaint();
            }
            if (PWBSettings.shortcuts.palettePreviousPalette.Check())
            {
                PaletteManager.SelectPreviousPalette();
                Repaint();
            }
            else if (PWBSettings.shortcuts.paletteNextPalette.Check())
            {
                PaletteManager.SelectNextPalette();
                Repaint();
            }
            var pickShortcutOn = PWBSettings.shortcuts.palettePickBrush.Check();
            var pickBrush = PaletteManager.pickingBrushes && Event.current.button == 0
                && Event.current.type == EventType.MouseDown;
            if (pickShortcutOn || pickBrush)
            {
                var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (MouseRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider,
                    float.MaxValue, -1, true, true))
                {
                    var target = collider.gameObject;
                    var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(target);
                    if (outermostPrefab != null) target = outermostPrefab;
                    var brushIdx = PaletteManager.selectedPalette.FindBrushIdx(target);
                    if (brushIdx >= 0) PaletteManager.SelectBrush(brushIdx);
                    else if (outermostPrefab != null)
                    {
                        var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
                        PrefabPalette.instance.CreateBrushFromSelection(prefabAsset);
                    }
                }
                Event.current.Use();
                if (!pickShortcutOn && pickBrush) PaletteManager.pickingBrushes = false;
            }
            if (PWBSettings.shortcuts.palettePickBrush.holdKeysAndClickCombination.holdingChanged)
                PaletteManager.pickingBrushes = PWBSettings.shortcuts.palettePickBrush.holdKeysAndClickCombination.holdingKeys;
        }
        async static void AsyncRepaint()
        {
            await System.Threading.Tasks.Task.Delay(500);
            repaint = true;
        }
        #endregion

        #region TOOLBAR
        public static void ToogleTool(ToolManager.PaintTool tool)
        {
#if UNITY_2021_2_OR_NEWER
#else
            if (PWBToolbar.instance == null) PWBToolbar.ShowWindow();
#endif
            ToolManager.tool = ToolManager.tool == tool ? ToolManager.PaintTool.NONE : tool;
            PWBToolbar.RepaintWindow();
        }
        /*private static void ToolbarInput()
        {
            if(PWBSettings.shortcuts.toolbarPinToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.PIN);
            else if (PWBSettings.shortcuts.toolbarBrushToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.BRUSH);
            else if (PWBSettings.shortcuts.toolbarGravityToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.GRAVITY);
            else if (PWBSettings.shortcuts.toolbarLineToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.LINE);
            else if (PWBSettings.shortcuts.toolbarShapeToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.SHAPE);
            else if (PWBSettings.shortcuts.toolbarTilingToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.TILING);
            else if (PWBSettings.shortcuts.toolbarReplacerToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.REPLACER);
            else if (PWBSettings.shortcuts.toolbarEraserToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.ERASER);
            else if (PWBSettings.shortcuts.toolbarSelectionToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.SELECTION);
            else if (PWBSettings.shortcuts.toolbarExtrudeToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.EXTRUDE);
            else if (PWBSettings.shortcuts.toolbarMirrorToggle.combination.Check())
                ToogleTool(ToolManager.PaintTool.MIRROR);
        }*/
        #endregion
    }
}