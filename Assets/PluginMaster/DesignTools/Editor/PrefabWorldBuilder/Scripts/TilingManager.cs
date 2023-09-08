/*
Copyright (c) 2021 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2021.

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
    #region DATA & SETTINGS
    [System.Serializable]
    public class TilingSettings : PaintOnSurfaceToolSettings, IPaintToolSettings
    {
        #region TILING SETTINGS

        public enum CellSizeType
        {
            SMALLEST_OBJECT,
            BIGGEST_OBJECT,
            CUSTOM
        }

        [SerializeField] private CellSizeType _cellSizeType = CellSizeType.SMALLEST_OBJECT;
        [SerializeField] private Vector2 _cellSize = Vector2.one;
        [SerializeField] private Quaternion _rotation = Quaternion.identity;
        [SerializeField] private Vector2 _spacing = Vector2.zero;
        [SerializeField] private AxesUtils.SignedAxis _axisAlignedWithNormal = AxesUtils.SignedAxis.UP;
        [SerializeField] private bool _showPreview = true;
        public System.Action<float, Vector3> OnRotationChanged;
        public Quaternion rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                var prevRotation = _rotation;
                _rotation = value;
                OnDataChanged();
                if (OnRotationChanged != null)
                {
                    var angle = Quaternion.Angle(prevRotation, _rotation);
                    var axis = Vector3.Cross(prevRotation * Vector3.forward, _rotation * Vector3.forward);
                    if (axis == Vector3.zero) axis = Vector3.Cross(prevRotation * Vector3.up, _rotation * Vector3.up);
                    axis.Normalize();
                    OnRotationChanged(angle, axis);
                }
            }
        }
        public CellSizeType cellSizeType
        {
            get => _cellSizeType;
            set
            {
                if (_cellSizeType == value) return;
                _cellSizeType = value;
                UpdateCellSize();
            }
        }
        public Vector2 cellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize == value) return;
                _cellSize = value;
                OnDataChanged();
            }
        }
        public Vector2 spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }
        public AxesUtils.SignedAxis axisAlignedWithNormal
        {
            get => _axisAlignedWithNormal;
            set
            {
                if (_axisAlignedWithNormal == value) return;
                _axisAlignedWithNormal = value;
                UpdateCellSize();
                OnDataChanged();
            }
        }
        public bool showPreview
        {
            get => _showPreview;
            set
            {
                if (_showPreview == value) return;
                _showPreview = value;
                OnDataChanged();
            }
        }
        public void UpdateCellSize()
        {
            if (ToolManager.tool != ToolManager.PaintTool.TILING) return;
            if (_cellSizeType != CellSizeType.CUSTOM)
            {
                var selectedBrush = PaletteManager.selectedBrush;
                if (selectedBrush == null) return;
                var cellSize = Vector3.one * (_cellSizeType == CellSizeType.SMALLEST_OBJECT
                    ? float.MaxValue : float.MinValue);

                foreach (var item in selectedBrush.items)
                {
                    var prefab = item.prefab;
                    if (prefab == null) continue;
                    var scaleMultiplier = _cellSizeType == CellSizeType.SMALLEST_OBJECT
                        ? item.minScaleMultiplier : item.maxScaleMultiplier;
                    if (overwriteBrushProperties)
                        scaleMultiplier = Vector3.Scale(scaleMultiplier, brushSettings.scaleMultiplier);
                    var itemSize = Vector3.Scale(BoundsUtils.GetBoundsRecursive(prefab.transform,
                        prefab.transform.rotation * Quaternion.Euler(selectedBrush.eulerOffset)).size, scaleMultiplier);
                    cellSize = _cellSizeType == CellSizeType.SMALLEST_OBJECT
                        ? Vector3.Min(cellSize, itemSize) : Vector3.Max(cellSize, itemSize);
                }

                if (_axisAlignedWithNormal.axis == AxesUtils.Axis.Y) cellSize.y = cellSize.z;
                else if (_axisAlignedWithNormal.axis == AxesUtils.Axis.X)
                {
                    cellSize.x = cellSize.y;
                    cellSize.y = cellSize.z;
                }

                if (cellSize.x == 0) cellSize.x = 0.5f;
                if (cellSize.y == 0) cellSize.y = 0.5f;
                if (cellSize.z == 0) cellSize.z = 0.5f;
                _cellSize = cellSize;
                ToolProperties.RepainWindow();
                UnityEditor.SceneView.RepaintAll();
            }
            OnDataChanged();
        }
        #endregion

        #region ON DATA CHANGED
        public TilingSettings() : base()
        {
            _paintTool.OnDataChanged += DataChanged;
            _paintTool.brushSettings.OnDataChangedAction += DataChanged;
        }

        public override void DataChanged()
        {
            base.DataChanged();
            PWBIO.UpdateStroke();
        }
        #endregion

        #region PAINT TOOL
        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public Transform parent { get => _paintTool.parent; set => _paintTool.parent = value; }
        public bool overwritePrefabLayer
        {
            get => _paintTool.overwritePrefabLayer;
            set => _paintTool.overwritePrefabLayer = value;
        }
        public int layer { get => _paintTool.layer; set => _paintTool.layer = value; }
        public bool autoCreateParent { get => _paintTool.autoCreateParent; set => _paintTool.autoCreateParent = value; }
        public bool setSurfaceAsParent { get => _paintTool.setSurfaceAsParent; set => _paintTool.setSurfaceAsParent = value; }
        public bool createSubparentPerPalette
        {
            get => _paintTool.createSubparentPerPalette;
            set => _paintTool.createSubparentPerPalette = value;
        }
        public bool createSubparentPerTool
        {
            get => _paintTool.createSubparentPerTool;
            set => _paintTool.createSubparentPerTool = value;
        }
        public bool createSubparentPerBrush
        {
            get => _paintTool.createSubparentPerBrush;
            set => _paintTool.createSubparentPerBrush = value;
        }
        public bool createSubparentPerPrefab
        {
            get => _paintTool.createSubparentPerPrefab;
            set => _paintTool.createSubparentPerPrefab = value;
        }
        public bool overwriteBrushProperties
        {
            get => _paintTool.overwriteBrushProperties;
            set
            {
                _paintTool.overwriteBrushProperties = value;
                OnDataChanged();
            }
        }
        public BrushSettings brushSettings => _paintTool.brushSettings;
        #endregion

        public override void Copy(IToolSettings other)
        {
            var otherTilingSettings = other as TilingSettings;
            base.Copy(other);
            _paintTool.Copy(otherTilingSettings._paintTool);
            _cellSizeType = otherTilingSettings._cellSizeType;
            _cellSize = otherTilingSettings._cellSize;
            _rotation = otherTilingSettings._rotation;
            _spacing = otherTilingSettings._spacing;
            _axisAlignedWithNormal = otherTilingSettings._axisAlignedWithNormal;

        }

        public TilingSettings Clone()
        {
            var clone = new TilingSettings();
            clone.Copy(this);
            return clone;
        }
    }

    public class TilingToolName : IToolName { public string value => "Tiling"; }

    [System.Serializable]
    public class TilingData : PersistentData<TilingToolName, TilingSettings, ControlPoint>
    {
        [System.NonSerialized]
        private System.Collections.Generic.List<Vector3> _tilingCenters
            = new System.Collections.Generic.List<Vector3>();
        public System.Collections.Generic.List<Vector3> tilingCenters => _tilingCenters;
        public TilingData() : base() { }
        public TilingData(GameObject[] objects, long initialBrushId, TilingData tilingData)
        : base(objects, initialBrushId, tilingData) { }

        private static TilingData _instance = null;
        public static TilingData instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TilingData();
                    _instance._settings = TilingManager.settings;
                }
                return _instance;
            }
        }
        protected override void Initialize()
        {
            base.Initialize();
            const int pointCount = 9;
            for (int i = 0; i < pointCount; i++) _controlPoints.Add(new ControlPoint());
            _pointPositions = new Vector3[pointCount];
        }
        public TilingData Clone()
        {
            var clone = new TilingData();
            base.Clone(clone);
            clone._tilingCenters = _tilingCenters.ToList();
            return clone;
        }
    }

    [System.Serializable]
    public class TilingSceneData : SceneData<TilingToolName, TilingSettings, ControlPoint, TilingData>
    {
        public TilingSceneData() : base() { }
        public TilingSceneData(string sceneGUID) : base(sceneGUID) { }
    }

    [System.Serializable]
    public class TilingManager
        : PersistentToolManagerBase<TilingToolName, TilingSettings, ControlPoint, TilingData, TilingSceneData>
    { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static TilingData _tilingData = TilingData.instance;
        private static TilingData _initialPersistentTilingData = null;
        private static TilingData _selectedPersistentTilingData = null;
        private static bool _editingPersistentTiling = true;
        private static void TilingInitializeOnLoad()
        {
            TilingManager.settings.OnDataChanged += OnTilingSettingsChanged;
            TilingManager.settings.OnRotationChanged += OnTilingRotationChanged;
        }

        public static void ResetTilingState(bool askIfWantToSave = true)
        {
            if (askIfWantToSave)
            {
                void Save()
                {
                    if (UnityEditor.SceneView.lastActiveSceneView != null)
                        TilingStrokePreview(UnityEditor.SceneView.lastActiveSceneView.camera, TilingData.nextHexId, true);
                    CreateTiling();
                }
                AskIfWantToSave(_tilingData.state, Save);
            }
            _snappedToVertex = false;
            _tilingData.Reset();
            _paintStroke.Clear();
        }

        private static void ClearTilingStroke()
        {
            _paintStroke.Clear();
            BrushstrokeManager.ClearBrushstroke();
            if (ToolManager.editMode)
            {
                _selectedPersistentTilingData.UpdatePoses();
                PreviewPersistentTiling(_selectedPersistentTilingData);
                UnityEditor.SceneView.RepaintAll();
            }
        }
        private static void OnUndoTiling() => ClearTilingStroke();

        private static void OnTilingToolModeChanged()
        {
            DeselectPersistentItems(TilingManager.instance);
            if (!ToolManager.editMode)
            {
                if (_createProfileName != null)
                    ToolProperties.SetProfile(new ToolProperties.ProfileData(TilingManager.instance, _createProfileName));
                ToolProperties.RepainWindow();
                return;
            }
            ResetTilingState();
            ResetSelectedPersistentObject(TilingManager.instance, ref _editingPersistentTiling, _initialPersistentTilingData);
        }

        private static void OnTilingSettingsChanged()
        {
            repaint = true;
            if (!ToolManager.editMode)
            {
                _tilingData.settings = TilingManager.settings;
                updateStroke = true;
                return;
            }
            if (_selectedPersistentTilingData == null) return;
            _selectedPersistentTilingData.settings.Copy(TilingManager.settings);
            PreviewPersistentTiling(_selectedPersistentTilingData);
        }


        private static void OnTilingRotationChanged(float angle, Vector3 axis)
        {
            if (!ToolManager.editMode) return;
            if (_selectedPersistentTilingData == null) return;
            RotateTiling(_selectedPersistentTilingData, angle, axis, false);
            DrawCells(_selectedPersistentTilingData);
            PreviewPersistentTiling(_selectedPersistentTilingData);
            repaint = true;
        }
        private static void TilingDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (TilingManager.settings.paintOnMeshesWithoutCollider)
                PWBCore.CreateTempCollidersWithinFrustum(sceneView.camera);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_tilingData.state == ToolManager.ToolState.EDIT && _tilingData.selectedPointIdx > 0)
                    _tilingData.selectedPointIdx = -1;
                else if (_tilingData.state == ToolManager.ToolState.NONE) ToolManager.DeselectTool();
                else ResetTilingState(false);
            }
            TilingToolEditMode(sceneView);
            if (ToolManager.editMode) return;
            switch (_tilingData.state)
            {
                case ToolManager.ToolState.NONE:
                    TilingStateNone(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.PREVIEW:
                    TilingStateRectangle(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.EDIT:
                    TilingStateEdit(sceneView.camera);
                    break;
            }
        }

        private static void TilingToolEditMode(UnityEditor.SceneView sceneView)
        {
            var persistentItems = TilingManager.instance.GetPersistentItems();
            var deselectedItems = new System.Collections.Generic.List<TilingData>(persistentItems);
            bool clickOnAnyPoint = false;
            bool selectedItemWasEdited = false;
            foreach (var itemData in persistentItems)
            {
                itemData.UpdateObjects();
                if (itemData.objectCount == 0)
                {
                    TilingManager.instance.RemovePersistentItem(itemData.id);
                    continue;
                }
                DrawCells(itemData);
                if (!ToolManager.editMode) continue;
                DrawTilingRectangle(itemData);

                var selectedTilingId = _initialPersistentTilingData == null ? -1 : _initialPersistentTilingData.id;
                if (DrawTilingControlPoints(itemData, out bool clickOnPoint, out bool wasEdited, out Vector3 delta))
                {
                    if (clickOnPoint)
                    {
                        clickOnAnyPoint = true;
                        _editingPersistentTiling = true;
                        if (selectedTilingId != itemData.id)
                        {
                            ApplySelectedPersistentTiling(false);
                            if (selectedTilingId == -1)
                                _createProfileName = TilingManager.instance.selectedProfileName;
                            TilingManager.instance.CopyToolSettings(itemData.settings);
                        }
                        _selectedPersistentTilingData = itemData;
                        if (_initialPersistentTilingData == null) _initialPersistentTilingData = itemData.Clone();
                        else if (_initialPersistentTilingData.id != itemData.id)
                            _initialPersistentTilingData = itemData.Clone();
                        deselectedItems.Remove(itemData);
                    }
                    if (wasEdited)
                    {
                        _editingPersistentTiling = true;
                        selectedItemWasEdited = true;
                    }
                }
            }
            if (clickOnAnyPoint)
            {
                foreach (var itemData in deselectedItems)
                {
                    itemData.selectedPointIdx = -1;
                    itemData.ClearSelection();
                }
            }
            if (!ToolManager.editMode) return;
            if (selectedItemWasEdited) PreviewPersistentTiling(_selectedPersistentTilingData);
            else if (_editingPersistentTiling && _selectedPersistentTilingData != null)
            {
                var forceStrokeUpdate = updateStroke;
                if (updateStroke)
                {
                    PreviewPersistentTiling(_selectedPersistentTilingData);
                    updateStroke = false;
                    PWBCore.SetSavePending();
                }
                if (_brushstroke != null
                    && !BrushstrokeManager.BrushstrokeEqual(BrushstrokeManager.brushstroke, _brushstroke))
                    _paintStroke.Clear();
                TilingStrokePreview(sceneView.camera, _selectedPersistentTilingData.hexId, forceStrokeUpdate);
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                ApplySelectedPersistentTiling(true);
                ToolProperties.SetProfile(new ToolProperties.ProfileData(TilingManager.instance, _createProfileName));
                ToolProperties.RepainWindow();
            }
            else if (PWBSettings.shortcuts.editModeSelectParent.Check()
                && _selectedPersistentTilingData != null)
            {
                var parent = _selectedPersistentTilingData.GetParent();
                if (parent != null) UnityEditor.Selection.activeGameObject = parent;
            }
            else if (PWBSettings.shortcuts.editModeDeleteItemButNotItsChildren.Check())
                TilingManager.instance.DeletePersistentItem(_selectedPersistentTilingData.id, false);
            else if (PWBSettings.shortcuts.editModeDeleteItemAndItsChildren.Check())
                TilingManager.instance.DeletePersistentItem(_selectedPersistentTilingData.id, true);
            if (TilingShortcuts(_selectedPersistentTilingData))
            {
                DrawCells(_selectedPersistentTilingData);
                PreviewPersistentTiling(_selectedPersistentTilingData);
                repaint = true;
            }
        }

        private static void PreviewPersistentTiling(TilingData data)
        {
            PWBCore.UpdateTempCollidersIfHierarchyChanged();
            Vector3[] objPos = null;
            var objList = data.objectList;
            var settings = data.settings;
            BrushstrokeManager.UpdatePersistentTilingBrushstroke(data.tilingCenters.ToArray(),
                settings, objList, out objPos, out Vector3[] strokePos);
            _disabledObjects.Clear();
            _disabledObjects.AddRange(data.objects.ToList());
            var objArray = objList.ToArray();
            for (int objIdx = 0; objIdx < objPos.Length; ++objIdx)
            {
                var obj = objList[objIdx];
                obj.SetActive(true);

                Bounds bounds = BoundsUtils.GetBoundsRecursive(obj.transform, obj.transform.rotation);
                var size = bounds.size;
                var height = Mathf.Max(size.x, size.y, size.z) * 2;

                var itemPosition = objPos[objIdx];
                var normal = Vector3.up;
                var ray = new Ray(itemPosition + Vector3.up * height, Vector3.down);
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit,
                        out GameObject collider, height * 2f, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider,
                        null, null, null, objArray))
                    {
                        itemPosition = itemHit.point;
                        normal = itemHit.normal;
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }
                var itemRotation = settings.rotation;
                Vector3 itemTangent = itemRotation * Vector3.forward;
                BrushSettings brushSettings = PaletteManager.GetBrushById(data.initialBrushId);
                if (settings.overwriteBrushProperties) brushSettings = settings.brushSettings;
                if (brushSettings.rotateToTheSurface
                    && settings.mode != PaintOnSurfaceToolSettings.PaintMode.ON_SHAPE)
                {
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * brushSettings.surfaceDistance;
                }
                else itemPosition += normal * brushSettings.surfaceDistance;
                var axisAlignedWithNormal = (Vector3)settings.axisAlignedWithNormal;
                if (settings.axisAlignedWithNormal.axis != AxesUtils.Axis.Y) axisAlignedWithNormal *= -1;
                itemRotation *= Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal);
                var previewRotation = itemRotation;

                itemPosition += itemRotation * brushSettings.localPositionOffset;

                UnityEditor.Undo.RecordObject(obj.transform, LineData.COMMAND_NAME);
                obj.transform.rotation = Quaternion.identity;
                obj.transform.position = Vector3.zero;
                bounds = BoundsUtils.GetBoundsRecursive(obj.transform, true, BoundsUtils.ObjectProperty.BOUNDING_BOX, false);
                obj.transform.rotation = itemRotation;
                var pivotToCenter = itemRotation * obj.transform.InverseTransformPoint(bounds.center);

                var axisDirection = Vector3.up;
                if (settings.axisAlignedWithNormal == AxesUtils.Axis.Z)
                {
                    size.x = bounds.size.y;
                    size.y = bounds.size.z;
                    size.z = bounds.size.x;
                    axisDirection = Vector3.forward;
                }
                else if (settings.axisAlignedWithNormal == AxesUtils.Axis.X)
                {
                    size.x = bounds.size.z;
                    size.y = bounds.size.x;
                    size.z = bounds.size.y;
                    axisDirection = Vector3.right;
                }

                itemPosition -= itemRotation * (pivotToCenter - axisDirection * (size.y / 2));
                if (brushSettings.embedInSurface
                    && settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    var bottomMagnitude = BoundsUtils.GetBottomMagnitude(obj.transform);
                    if (brushSettings.embedAtPivotHeight)
                        itemPosition += itemRotation * new Vector3(0f, bottomMagnitude, 0f);
                    else
                    {
                        var TRS = Matrix4x4.TRS(itemPosition, itemRotation, obj.transform.lossyScale);
                        var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform);
                        var bottomDistanceToSurfce = GetBottomDistanceToSurface(bottomVertices, TRS,
                            Mathf.Abs(bottomMagnitude), settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider);
                        itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                    }
                }
                obj.transform.position = itemPosition;
                _disabledObjects.Remove(obj);
            }
            _disabledObjects = _disabledObjects.Where(i => i != null).ToList();
            foreach (var obj in _disabledObjects) obj.SetActive(false);
        }

        private static void ApplySelectedPersistentTiling(bool deselectPoint)
        {
            if (!ApplySelectedPersistentObject(deselectPoint, ref _editingPersistentTiling, ref _initialPersistentTilingData,
                ref _selectedPersistentTilingData, TilingManager.instance)) return;
            if (_initialPersistentTilingData == null) return;
            var selectedTiling = TilingManager.instance.GetItem(_initialPersistentTilingData.id);
            _initialPersistentTilingData = selectedTiling.Clone();
        }
        private static void TilingStateNone(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _tilingData.state = ToolManager.ToolState.PREVIEW;
                TilingManager.settings.UpdateCellSize();
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, TilingManager.settings.mode, in2DMode,
                TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider, false))
            {
                point = SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                   TilingManager.settings.paintOnPalettePrefabs, TilingManager.settings.paintOnMeshesWithoutCollider
                   , false, Vector3.down);
                _tilingData.SetPoint(2, point, false);
                _tilingData.SetPoint(0, point, false);
            }
            if (_tilingData.pointsCount > 0) DrawDotHandleCap(_tilingData.GetPoint(0));
        }

        private static void DrawTilingRectangle(TilingData data)
        {
            var settings = data.settings;
            var cornerPoints = new Vector3[] { data.GetPoint(0), data.GetPoint(1),
                data.GetPoint(2), data.GetPoint(3), data.GetPoint(0) };
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, cornerPoints);
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(4, cornerPoints);
        }

        private static void UpdateMidpoints(TilingData data)
        {
            for (int i = 0; i < 4; ++i)
            {
                var nextI = (i + 1) % 4;
                var point = data.GetPoint(i);
                var nextPoint = data.GetPoint(nextI);
                data.SetPoint(i + 4, point + (nextPoint - point) / 2, false);
            }
            data.SetPoint(8, data.GetPoint(0)
                + (data.GetPoint(2) - data.GetPoint(0)) / 2, false);
        }

        private static void DrawCells(TilingData data) => UpdateCellCenters(data, true);

        private static void DrawTilingGrid(TilingData data)
        {
            DrawCells(data);
            DrawTilingRectangle(data);
        }

        public static TilingData tilingData => ToolManager.editMode ? _selectedPersistentTilingData : _tilingData;

        private static void ApplyTilingHandlePosition(TilingData data) => SetTilingSelectedPoint(data, _handlePosition);

        private static bool SetTilingSelectedPoint(TilingData data, Vector3 position)
        {
            if (data.selectedPointIdx < 0) return false;
            _handlePosition = position;
            var prevPosition = data.selectedPoint;
            var snappedPoint = SnapAndUpdateGridOrigin(_handlePosition, SnapManager.settings.snappingEnabled,
               data.settings.paintOnPalettePrefabs, data.settings.paintOnMeshesWithoutCollider,
               false, Vector3.down);
            data.SetPoint(data.selectedPointIdx, snappedPoint, true);
            _handlePosition = data.selectedPoint;
            if (prevPosition == data.selectedPoint) return false;

            updateStroke = true;
            var delta = data.selectedPoint - prevPosition;
            if (data.selectedPointIdx < 4)
            {
                var nextCornerIdx = (data.selectedPointIdx + 1) % 4;
                var oppositeCornerIdx = (data.selectedPointIdx + 2) % 4;
                var prevCornerIdx = (data.selectedPointIdx + 3) % 4;

                var nextVector = data.GetPoint(nextCornerIdx) - prevPosition;
                var prevVector = data.GetPoint(prevCornerIdx) - prevPosition;
                var deltaNext = Vector3.Project(delta, nextVector);
                var deltaPrev = Vector3.Project(delta, prevVector);
                var deltaNormal = delta - deltaNext - deltaPrev;
                data.AddValue(nextCornerIdx, deltaPrev + deltaNormal);
                data.AddValue(prevCornerIdx, deltaNext + deltaNormal);
                data.AddValue(oppositeCornerIdx, deltaNormal);
            }
            else if (data.selectedPointIdx < 8)
            {
                var prevCornerIdx = data.selectedPointIdx - 4;
                var nextCornerIdx = (data.selectedPointIdx - 3) % 4;
                var oppositeSideIdx = (data.selectedPointIdx - 2) % 4 + 4;
                var parallel = data.GetPoint(nextCornerIdx) - data.GetPoint(prevCornerIdx);
                var perpendicular = data.GetPoint(oppositeSideIdx) - prevPosition;
                var deltaParallel = Vector3.Project(delta, parallel);
                var deltaPerpendicular = Vector3.Project(delta, perpendicular);
                var deltaNormal = delta - deltaParallel - deltaPerpendicular;
                for (int i = 0; i < 4; ++i) data.AddValue(i, deltaParallel + deltaNormal);
                data.AddValue(prevCornerIdx, deltaPerpendicular);
                data.AddValue(nextCornerIdx, deltaPerpendicular);
            }
            else for (int i = 0; i < 4; ++i) data.AddValue(i, delta);
            UpdateMidpoints(data);
            UpdateCellCenters(data, false);
            return true;
        }

        private static bool SetTilingRotation(TilingData data, Quaternion rotation)
        {
            var prevRotation = data.settings.rotation;
            data.settings.rotation = rotation;
            if (data.settings.rotation == prevRotation) return false;

            var angle = Quaternion.Angle(prevRotation, data.settings.rotation);
            var axis = Vector3.Cross(prevRotation * Vector3.forward,
                data.settings.rotation * Vector3.forward);
            if (axis == Vector3.zero) axis = Vector3.Cross(prevRotation * Vector3.up,
                data.settings.rotation * Vector3.up);
            axis.Normalize();
            RotateTiling(data, angle, axis, false);
            ToolProperties.RepainWindow();
            UpdateCellCenters(data, false);
            return true;
        }
       
        public static void UpdateCellSize()
        {
            if (ToolManager.editMode)
            {
                _selectedPersistentTilingData.settings.UpdateCellSize();
                UpdateCellCenters(_selectedPersistentTilingData, true);
            }
            _tilingData.settings.UpdateCellSize();
            UpdateCellCenters(_tilingData, true);
        }


        private static void UpdateCellCenters(TilingData data, bool DrawCells)
        {
            data.tilingCenters.Clear();
            var settings = data.settings;
            var tangentDir = data.GetPoint(1) - data.GetPoint(0);
            var tangentSize = tangentDir.magnitude;
            tangentDir.Normalize();
            var bitangentDir = data.GetPoint(3) - data.GetPoint(0);
            var bitangentSize = bitangentDir.magnitude;
            bitangentDir.Normalize();
            var cellTangent = tangentDir * Mathf.Abs(settings.cellSize.x);
            var cellBitangent = bitangentDir * Mathf.Abs(settings.cellSize.y);
            var vertices = new Vector3[] { Vector3.zero, cellTangent, cellTangent + cellBitangent, cellBitangent };
            var offset = data.GetPoint(0);
            void SetTileCenter()
            {
                var linePoints = new Vector3[5];
                for (int i = 0; i <= 4; ++i) linePoints[i] = vertices[i % 4] + offset;
                var cellCenter = linePoints[0] + (linePoints[2] - linePoints[0]) / 2;
                data.tilingCenters.Add(cellCenter);
                if (!DrawCells) return;
                UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.3f);
                UnityEditor.Handles.DrawAAPolyLine(6, linePoints);
                UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.3f);
                UnityEditor.Handles.DrawAAPolyLine(2, linePoints);
            }
            var minCellSize = settings.cellSize + settings.spacing;
            minCellSize = Vector2.Max(minCellSize, Vector2.one * 0.001f);
            var cellSize = minCellSize - settings.spacing;
            float tangentOffset = 0;
            while (Mathf.Abs(tangentOffset) + Mathf.Abs(cellSize.x) < tangentSize)
            {
                float bitangentOffset = 0;
                while (Mathf.Abs(bitangentOffset) + Mathf.Abs(cellSize.y) < bitangentSize)
                {
                    SetTileCenter();
                    bitangentOffset += minCellSize.y;
                    offset = data.GetPoint(0) + tangentDir * Mathf.Abs(tangentOffset)
                        + bitangentDir * Mathf.Abs(bitangentOffset);
                }
                tangentOffset += minCellSize.x;
                offset = data.GetPoint(0) + tangentDir * Mathf.Abs(tangentOffset);
            }
        }

        private static bool DrawTilingControlPoints(TilingData data,
            out bool clickOnPoint, out bool wasEdited, out Vector3 delta)
        {
            delta = Vector3.zero;
            clickOnPoint = false;
            wasEdited = false;

            for (int i = 0; i < 9; ++i)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (!clickOnPoint)
                {
                    float distFromMouse
                        = UnityEditor.HandleUtility.DistanceToRectangle(data.GetPoint(i), Quaternion.identity, 0f);
                    UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                    if (Event.current.button == 0 && Event.current.type == EventType.MouseDown
                        && UnityEditor.HandleUtility.nearestControl == controlId)
                    {
                        data.selectedPointIdx = i;
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                }
                if (Event.current.type != EventType.Repaint) continue;
                DrawDotHandleCap(data.GetPoint(i));
            }
            if (clickOnPoint) ToolProperties.RepainWindow();
            if (data.selectedPointIdx < 0) return false;
            var prevPoint = data.selectedPoint;
            wasEdited = SetTilingSelectedPoint(data,
                UnityEditor.Handles.PositionHandle(data.selectedPoint, data.settings.rotation));
            if (prevPoint != data.selectedPoint) ToolProperties.RepainWindow();
            if (data.selectedPointIdx == 8)
            {
                var prevRotation = data.settings.rotation;
                wasEdited = SetTilingRotation(data,
                    UnityEditor.Handles.RotationHandle(data.settings.rotation, data.GetPoint(8)));
                if(prevRotation != data.settings.rotation) ToolProperties.RepainWindow();
            }
            return clickOnPoint || wasEdited;
        }

        private static void TilingStateRectangle(bool in2DMode)
        {
            var settings = TilingManager.settings;
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                UpdateMidpoints(_tilingData);
                _tilingData.state = ToolManager.ToolState.EDIT;
                updateStroke = true;
            }

            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var plane = new Plane(settings.rotation * Vector3.up, _tilingData.GetPoint(0));

            if (plane.Raycast(mouseRay, out float distance))
            {
                var point = mouseRay.GetPoint(distance);
                _tilingData.SetPoint(2, point, false);
                var diagonal = point - _tilingData.GetPoint(0);
                var tangent = Vector3.Project(diagonal, settings.rotation * Vector3.right);
                var bitangent = Vector3.Project(diagonal, settings.rotation * Vector3.forward);
                _tilingData.SetPoint(1, _tilingData.GetPoint(0) + tangent, false);
                _tilingData.SetPoint(3, _tilingData.GetPoint(0) + bitangent, false);
                DrawTilingGrid(_tilingData);
                for (int i = 0; i < 4; ++i) DrawDotHandleCap(_tilingData.GetPoint(i));
                return;
            }
            DrawDotHandleCap(_tilingData.GetPoint(0));
        }

        private static void CreateTiling()
        {
            var nextTilingId = TilingData.nextHexId;
            var objDic = Paint(TilingManager.settings, PAINT_CMD, true, false, nextTilingId);
            if (objDic.Count != 1)
                return;
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(scenePath);
            var initialBrushId = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.id : -1;
            var objs = objDic[nextTilingId].ToArray();
            var persistentData = new TilingData(objs, initialBrushId, _tilingData);
            TilingManager.instance.AddPersistentItem(sceneGUID, persistentData);
        }

        private static bool TilingShortcuts(TilingData data)
        {
            if (data == null) return false;
            var keyCode = Event.current.keyCode;

            var spacing1 = PWBSettings.shortcuts.tilingEditSpacing1.Check();
            var spacing2 = PWBSettings.shortcuts.tilingEditSpacing2.Check();
            if (spacing1 || spacing2)
            {
                var delta = spacing1 ? PWBSettings.shortcuts.tilingEditSpacing1.combination.delta
                    : -PWBSettings.shortcuts.tilingEditSpacing2.combination.delta;
                var deltaSign = -Mathf.Sign(delta);
                var otherAxes = AxesUtils.GetOtherAxes(AxesUtils.Axis.Y);
                var spacing = Vector3.zero;
                AxesUtils.SetAxisValue(ref spacing, otherAxes[0], data.settings.spacing.x);
                AxesUtils.SetAxisValue(ref spacing, otherAxes[1], data.settings.spacing.y);
                var axisIdx = spacing1 ? 1 : 0;
                var size = data.GetPoint(2) - data.GetPoint(axisIdx);
                var axisSize = AxesUtils.GetAxisValue(size, otherAxes[axisIdx]);
                AxesUtils.AddValueToAxis(ref spacing, otherAxes[axisIdx], axisSize * deltaSign * 0.005f);
                data.settings.spacing = new Vector2(AxesUtils.GetAxisValue(spacing, otherAxes[0]),
                    AxesUtils.GetAxisValue(spacing, otherAxes[1]));
                ToolProperties.RepainWindow();
                Event.current.Use();
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90XCCW.Check())
            {
                RotateTiling(data, 90, Vector3.right, true);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90XCW.Check())
            {
                RotateTiling(data, 90, Vector3.left, true);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90YCCW.Check())
            {
                RotateTiling(data, 90, Vector3.up, true);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90YCW.Check())
            {
                RotateTiling(data, 90, Vector3.down, true);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90ZCCW.Check())
            {
                RotateTiling(data, 90, Vector3.forward, true);
                return true;
            }
            if (PWBSettings.shortcuts.selectionRotate90ZCW.Check())
            {
                RotateTiling(data, 90, Vector3.back, true);
                return true;
            }
            return false;
        }

        private static void TilingStateEdit(Camera camera)
        {
            bool mouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
            TilingShortcuts(_tilingData);
            var forceStrokeUpdate = updateStroke;
            if (updateStroke)
            {
                BrushstrokeManager.UpdateTilingBrushstroke(_tilingData.tilingCenters.ToArray());
                updateStroke = false;
            }
            if (TilingManager.settings.showPreview) TilingStrokePreview(camera, TilingData.nextHexId, forceStrokeUpdate);

            DrawTilingGrid(_tilingData);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (!TilingManager.settings.showPreview) TilingStrokePreview(camera, TilingData.nextHexId, forceStrokeUpdate);
                CreateTiling();
                ResetTilingState(false);
            }
            DrawTilingControlPoints(_tilingData, out bool clickOnPoint, out bool wasEdited, out Vector3 delta);
        }

        private static void RotateTiling(TilingData data, float angle, Vector3 axis, bool updateDataRotation)
        {
            updateStroke = true;
            var delta = Quaternion.AngleAxis(angle, axis);
            for (int i = 0; i < 8; ++i)
            {
                var centerToPoint = data.GetPoint(i) - data.GetPoint(8);
                var rotatedPos = (delta * centerToPoint) + data.GetPoint(8);
                data.SetPoint(i, rotatedPos, false);
            }
            if (updateDataRotation) data.settings.rotation *= delta;
            DrawCells(data);
        }

        public static void UpdateTilingRotation(Quaternion rotation)
        {
            if (tilingData == null) return;
            updateStroke = true;
            SetTilingRotation(tilingData, rotation);
        }
        private static void TilingStrokePreview(Camera camera, string hexId, bool forceUpdate)
        {
            BrushstrokeItem[] brushstroke;
            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, camera, forceUpdate)) return;
            PWBCore.UpdateTempCollidersIfHierarchyChanged();
            _paintStroke.Clear();
            var settings = TilingManager.settings;

            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];

                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                Bounds bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation);
                BrushSettings brushSettings = strokeItem.settings;
                if (settings.overwriteBrushProperties) brushSettings = settings.brushSettings;
                var additionalRotation = Quaternion.Euler(strokeItem.additionalAngle);
                var size = additionalRotation * Vector3.Scale(bounds.size, brushSettings.scaleMultiplier);
                size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
                var pivotToCenter = prefab.transform.InverseTransformDirection(bounds.center - prefab.transform.position);
                if (TilingManager.settings.overwriteBrushProperties)
                    pivotToCenter = Vector3.Scale(pivotToCenter, TilingManager.settings.brushSettings.scaleMultiplier);
                pivotToCenter = additionalRotation * pivotToCenter;
                bounds = new Bounds(prefab.transform.position + pivotToCenter, size);
                var height = Mathf.Max(size.x, size.y, size.z) * 2;

                var itemPosition = strokeItem.tangentPosition;
                var normal = Vector3.up;
                var ray = new Ray(itemPosition + Vector3.up * height, Vector3.down);
                Transform surface = null;
                if (settings.mode != TilingSettings.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit,
                        out GameObject collider, height * 2f, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                    {
                        itemPosition = itemHit.point;
                        normal = itemHit.normal;
                        var colObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        if (colObj != null) surface = colObj.transform;
                    }
                    else if (settings.mode == TilingSettings.PaintMode.ON_SURFACE) continue;
                }
                var itemRotation = settings.rotation;
                Vector3 itemTangent = itemRotation * Vector3.forward;
                if (brushSettings != null) brushSettings = new BrushSettings();
                if (brushSettings.rotateToTheSurface
                    && settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    itemRotation = Quaternion.LookRotation(itemTangent, normal);
                    itemPosition += normal * strokeItem.surfaceDistance;
                }
                else itemPosition += normal * strokeItem.surfaceDistance;
                var axisAlignedWithNormal = (Vector3)settings.axisAlignedWithNormal;
                if (settings.axisAlignedWithNormal.axis != AxesUtils.Axis.Y) axisAlignedWithNormal *= -1;
                itemRotation *= Quaternion.FromToRotation(Vector3.up, axisAlignedWithNormal);

                itemRotation *= additionalRotation;
                var previewRotation = itemRotation;

                itemPosition += itemRotation * (brushSettings.localPositionOffset);

                
               
                var axisDirection = Vector3.up;
                if (settings.axisAlignedWithNormal == AxesUtils.Axis.Z)
                {
                    size.x = bounds.size.y;
                    size.y = bounds.size.z;
                    size.z = bounds.size.x;
                    size = Vector3.Scale(size, brushSettings.scaleMultiplier);
                    axisDirection = Vector3.forward;
                }
                else if (settings.axisAlignedWithNormal == AxesUtils.Axis.X)
                {
                    size.x = bounds.size.z;
                    size.y = bounds.size.x;
                    size.z = bounds.size.y;
                    size = Vector3.Scale(size, brushSettings.scaleMultiplier);
                    axisDirection = Vector3.right;
                }

                itemPosition -= itemRotation * (pivotToCenter - axisDirection * (size.y / 2));

                if (brushSettings.embedInSurface
                    && settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (brushSettings.embedAtPivotHeight)
                        itemPosition += itemRotation * new Vector3(0f, strokeItem.settings.bottomMagnitude, 0f);
                    else
                    {
                        var TRS = Matrix4x4.TRS(itemPosition, itemRotation,
                            Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier));
                        var bottomDistanceToSurfce = GetBottomDistanceToSurface(strokeItem.settings.bottomVertices,
                            TRS, Mathf.Abs(strokeItem.settings.bottomMagnitude), settings.paintOnPalettePrefabs,
                            settings.paintOnMeshesWithoutCollider);
                        itemPosition += itemRotation * new Vector3(0f, -bottomDistanceToSurfce, 0f);
                    }
                }

                var itemScale = Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier);
                var layer = settings.overwritePrefabLayer ? settings.layer : prefab.layer;
                Transform parentTransform = settings.parent;

                var paintItem = new PaintStrokeItem(prefab, itemPosition, itemRotation,
                    itemScale, layer, parentTransform, surface, strokeItem.flipX, strokeItem.flipY);
                paintItem.persistentParentId = hexId;

                _paintStroke.Add(paintItem);
                var previewRootToWorld = Matrix4x4.TRS(itemPosition, previewRotation, strokeItem.scaleMultiplier)
                    * Matrix4x4.Rotate(Quaternion.Inverse(prefab.transform.rotation))
                    * Matrix4x4.Translate(-prefab.transform.position);
                PreviewBrushItem(prefab, previewRootToWorld, layer, camera,false, false, strokeItem.flipX, strokeItem.flipY);
                _previewData.Add(new PreviewData(prefab, previewRootToWorld, layer, strokeItem.flipX, strokeItem.flipY));
            }
        }
    }
    #endregion
}
