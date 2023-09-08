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
    public class LineSettings : PaintOnSurfaceToolSettings, IPaintToolSettings
    {
        public enum SpacingType { BOUNDS, CONSTANT }

        [SerializeField] private Vector3 _projectionDirection = Vector3.down;
        [SerializeField] private bool _objectsOrientedAlongTheLine = true;
        [SerializeField] private AxesUtils.Axis _axisOrientedAlongTheLine = AxesUtils.Axis.X;
        [SerializeField] private SpacingType _spacingType = SpacingType.BOUNDS;
        [SerializeField] private float _gapSize = 0f;
        [SerializeField] private float _spacing = 10f;


        public Vector3 projectionDirection
        {
            get => _projectionDirection;
            set
            {
                if (_projectionDirection == value) return;
                _projectionDirection = value;
                OnDataChanged();
            }
        }
        public void UpdateProjectDirection(Vector3 value) => _projectionDirection = value;

        public bool objectsOrientedAlongTheLine
        {
            get => _objectsOrientedAlongTheLine;
            set
            {
                if (_objectsOrientedAlongTheLine == value) return;
                _objectsOrientedAlongTheLine = value;
                OnDataChanged();
            }
        }

        public AxesUtils.Axis axisOrientedAlongTheLine
        {
            get => _axisOrientedAlongTheLine;
            set
            {
                if (_axisOrientedAlongTheLine == value) return;
                _axisOrientedAlongTheLine = value;
                OnDataChanged();
            }
        }

        public SpacingType spacingType
        {
            get => _spacingType;
            set
            {
                if (_spacingType == value) return;
                _spacingType = value;
                OnDataChanged();
            }
        }

        public float spacing
        {
            get => _spacing;
            set
            {
                value = Mathf.Max(value, 0.01f);
                if (_spacing == value) return;
                _spacing = value;
                OnDataChanged();
            }
        }

        public float gapSize
        {
            get => _gapSize;
            set
            {
                if (_gapSize == value) return;
                _gapSize = value;
                OnDataChanged();
            }
        }

        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public Transform parent { get => _paintTool.parent; set => _paintTool.parent = value; }
        public bool overwritePrefabLayer
        { get => _paintTool.overwritePrefabLayer; set => _paintTool.overwritePrefabLayer = value; }
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
        { get => _paintTool.overwriteBrushProperties; set => _paintTool.overwriteBrushProperties = value; }
        public BrushSettings brushSettings => _paintTool.brushSettings;

        public LineSettings() : base() => _paintTool.OnDataChanged += DataChanged;

        public override void DataChanged()
        {
            base.DataChanged();
            UpdateStroke();
            UnityEditor.SceneView.RepaintAll();
        }

        protected virtual void UpdateStroke() => PWBIO.UpdateStroke();

        public override void Copy(IToolSettings other)
        {
            var otherLineSettings = other as LineSettings;
            if (otherLineSettings == null) return;
            base.Copy(other);
            _projectionDirection = otherLineSettings._projectionDirection;
            _objectsOrientedAlongTheLine = otherLineSettings._objectsOrientedAlongTheLine;
            _axisOrientedAlongTheLine = otherLineSettings._axisOrientedAlongTheLine;
            _spacingType = otherLineSettings._spacingType;
            _spacing = otherLineSettings._spacing;
            _paintTool.Copy(otherLineSettings._paintTool);
            _gapSize = otherLineSettings._gapSize;
        }

        public override void Clone(ICloneableToolSettings clone)
        {
            if (clone == null || !(clone is LineSettings)) clone = new LineSettings();
            clone.Copy(this);
        }
    }

    [System.Serializable]
    public class LineSegment
    {
        public enum SegmentType { STRAIGHT, CURVE }
        public SegmentType type = SegmentType.CURVE;
        [SerializeField]
        private System.Collections.Generic.List<LinePoint> _linePoints = new System.Collections.Generic.List<LinePoint>();

        public Vector3[] points => _linePoints.Select(p => p.position).ToArray();
        public float[] scales => _linePoints.Select(p => p.scale).ToArray();

        public void AddPoint(Vector3 position, float scale = 0.25f) => _linePoints.Add(new LinePoint(position, scale));
    }

    [System.Serializable]
    public class LinePoint : ControlPoint
    {
        public LineSegment.SegmentType type = LineSegment.SegmentType.CURVE;
        public float scale = 0.25f;
        public LinePoint() { }
        public LinePoint(Vector3 position = new Vector3(), float scale = 0.25f,
             LineSegment.SegmentType type = LineSegment.SegmentType.CURVE)
            : base(position) => (this.type, this.scale) = (type, scale);
        public LinePoint(LinePoint other) : base((ControlPoint)other) => Copy(other);
        public override void Copy(ControlPoint other)
        {
            base.Copy(other);
            var otherLinePoint = other as LinePoint;
            if (otherLinePoint == null) return;
            type = otherLinePoint.type;
            scale = otherLinePoint.scale;
        }
    }

    [System.Serializable]
    public class LineData : PersistentData<LineToolName, LineSettings, LinePoint>
    {
        [SerializeField] private bool _closed = false;
        private float _lenght = 0f;
        private System.Collections.Generic.List<Vector3> _midpoints = new System.Collections.Generic.List<Vector3>();
        private System.Collections.Generic.List<Vector3> _pathPoints = new System.Collections.Generic.List<Vector3>();
        private System.Collections.Generic.List<Vector3> _onSurfacePathPoints = new System.Collections.Generic.List<Vector3>();
        public override ToolManager.ToolState state
        {
            get => base.state;
            set
            {
                if (state == value) return;
                base.state = value;
                UpdatePath();
            }
        }
        public override void SetPoint(int idx, Vector3 value, bool registerUndo)
        {
            base.SetPoint(idx, value, registerUndo);
            UpdatePath();
        }

        public void AddPoint(Vector3 point, bool registerUndo = true)
        {
            var linePoint = new LinePoint(point);
            base.AddPoint(linePoint, registerUndo);
            UpdatePath();
        }

        protected override void UpdatePoints()
        {
            base.UpdatePoints();
            UpdatePath();
        }
        public void ToggleSegmentType()
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            for (int i = 0; i < _selection.Count; ++i)
            {
                var idx = _selection[i];
                _controlPoints[idx].type = _controlPoints[idx].type == LineSegment.SegmentType.CURVE
                    ? LineSegment.SegmentType.STRAIGHT : LineSegment.SegmentType.CURVE;
            }
        }
        public LineSegment[] GetSegments()
        {
            var segments = new System.Collections.Generic.List<LineSegment>();
            if (_controlPoints == null || _controlPoints.Count == 0) return segments.ToArray();
            var type = _controlPoints[0].type;
            for (int i = 0; i < pointsCount; ++i)
            {
                var segment = new LineSegment();
                segments.Add(segment);
                segment.type = type;
                segment.AddPoint(_controlPoints[i].position);

                do
                {
                    ++i;
                    if (i >= pointsCount) break;
                    type = _controlPoints[i].type;
                    if (type == segment.type) segment.AddPoint(_controlPoints[i].position);
                } while (type == segment.type);
                if (i >= pointsCount) break;
                i -= 2;
            }
            if (_closed)
            {
                if (_controlPoints[0].type == _controlPoints.Last().type)
                    segments.Last().AddPoint(_controlPoints[0].position);
                else
                {
                    var segment = new LineSegment();
                    segment.type = _controlPoints[0].type;
                    segment.AddPoint(_controlPoints.Last().position);
                    segment.AddPoint(_controlPoints[0].position);
                    segments.Add(segment);
                }
            }
            return segments.ToArray();
        }

        public void ToggleClosed()
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _closed = !_closed;
        }

        public bool closed => _closed;

        protected override void Initialize()
        {
            base.Initialize();
            for (int i = 0; i < 2; ++i) _controlPoints.Add(new LinePoint(Vector3.zero));
            deserializing = true;
            UpdatePoints();
            deserializing = false;
        }
        public LineData() : base() { }
        public LineData(GameObject[] objects, long initialBrushId, LineData lineData)
            : base(objects, initialBrushId, lineData) { }

        //for compatibility with version 1.9
        public LineData(long id, LinePoint[] controlPoints, ObjectPose[] objectPoses,
            long initialBrushId, bool closed, LineSettings settings)
        {
            _id = id;
            _controlPoints = new System.Collections.Generic.List<LinePoint>(controlPoints);
            _initialBrushId = initialBrushId;
            _closed = closed;
            _settings = settings;
            base.UpdatePoints();
            UpdatePath(true);
            if (objectPoses == null || objectPoses.Length == 0) return;
            _objectPoses = new System.Collections.Generic.List<ObjectPose>(objectPoses);
        }

        private static LineData _instance = null;
        public static LineData instance
        {
            get
            {
                if (_instance == null) _instance = new LineData();
                if (_instance.points == null || _instance.points.Length == 0)
                {
                    _instance.Initialize();
                    _instance._settings = LineManager.settings;
                }
                return _instance;
            }
        }

        private void CopyLineData(LineData other)
        {
            _closed = other._closed;
            _lenght = other.lenght;
            _midpoints = other._midpoints.ToList();
            _pathPoints = other._pathPoints.ToList();
        }

        public LineData Clone()
        {
            var clone = new LineData();
            base.Clone(clone);
            clone.CopyLineData(this);
            return clone;
        }
        public override void Copy(PersistentData<LineToolName, LineSettings, LinePoint> other)
        {
            base.Copy(other);
            var otherLineData = other as LineData;
            if (otherLineData == null) return;
            CopyLineData(otherLineData);
        }
        private float GetLineLength(Vector3[] points, out float[] lengthFromFirstPoint)
        {
            float lineLength = 0f;
            lengthFromFirstPoint = new float[points.Length];
            var segmentLength = new float[points.Length];
            lengthFromFirstPoint[0] = 0f;
            for (int i = 1; i < points.Length; ++i)
            {
                segmentLength[i - 1] = (points[i] - points[i - 1]).magnitude;
                lineLength += segmentLength[i - 1];
                lengthFromFirstPoint[i] = lineLength;
            }
            return lineLength;
        }

        private Vector3[] GetLineMidpoints(Vector3[] points)
        {
            if (points.Length == 0) return new Vector3[0];
            var midpoints = new System.Collections.Generic.List<Vector3>();
            var subSegments = new System.Collections.Generic.List<System.Collections.Generic.List<Vector3>>();
            var pathPoints = _pointPositions;
            bool IsAPathPoint(Vector3 point) => pathPoints.Contains(point);
            subSegments.Add(new System.Collections.Generic.List<Vector3>());
            subSegments.Last().Add(points[0]);
            for (int i = 1; i < points.Length - 1; ++i)
            {
                var point = points[i];
                subSegments.Last().Add(point);
                if (IsAPathPoint(point))
                {
                    subSegments.Add(new System.Collections.Generic.List<Vector3>());
                    subSegments.Last().Add(point);
                }
            }
            subSegments.Last().Add(points.Last());
            Vector3 GetLineMidpoint(Vector3[] subSegmentPoints)
            {
                var midpoint = subSegmentPoints[0];
                float[] lengthFromFirstPoint = null;
                var halfLineLength = GetLineLength(subSegmentPoints, out lengthFromFirstPoint) / 2f;
                for (int i = 1; i < subSegmentPoints.Length; ++i)
                {
                    if (lengthFromFirstPoint[i] < halfLineLength) continue;
                    var dir = (subSegmentPoints[i] - subSegmentPoints[i - 1]).normalized;
                    var localLength = halfLineLength - lengthFromFirstPoint[i - 1];
                    midpoint = subSegmentPoints[i - 1] + dir * localLength;
                    break;
                }
                return midpoint;
            }
            foreach (var subSegment in subSegments) midpoints.Add(GetLineMidpoint(subSegment.ToArray()));
            return midpoints.ToArray();
        }

        public void UpdatePath(bool forceUpdate = false)
        {
            if (!forceUpdate && !ToolManager.editMode && state != ToolManager.ToolState.EDIT) return;
            _lenght = 0;
            _pathPoints.Clear();
            _midpoints.Clear();
            _onSurfacePathPoints.Clear();
            var segments = GetSegments();
            foreach (var segment in segments)
            {
                var segmentPoints = new System.Collections.Generic.List<Vector3>();
                if (segment.type == LineSegment.SegmentType.STRAIGHT) segmentPoints.AddRange(segment.points);
                else segmentPoints.AddRange(BezierPath.GetBezierPoints(segment.points, segment.scales));
                _pathPoints.AddRange(segmentPoints);
                if (segmentPoints.Count == 0) continue;
                _midpoints.AddRange(GetLineMidpoints(segmentPoints.ToArray()));
            }

            for (int i = 0; i < _pathPoints.Count; ++i)
            {
                float distance = 10000f;
                if (ToolManager.tool == ToolManager.PaintTool.LINE && !deserializing)
                {
                    var ray = new Ray(_pathPoints[i] - settings.projectionDirection * distance, settings.projectionDirection);
                    var onSurfacePoint = _pathPoints[i];
                    if (PWBIO.MouseRaycast(ray, out RaycastHit hit, out GameObject collider, distance * 2, -1, false, true,
                        null, null, null, objects))
                    {
                        onSurfacePoint = hit.point;
                    }
                    _onSurfacePathPoints.Add(onSurfacePoint);
                }
                if (i == 0) continue;
                _lenght += (_pathPoints[i] - _pathPoints[i - 1]).magnitude;
            }
        }

        public static bool SphereSegmentIntersection(Vector3 segmentStart, Vector3 segmentEnd,
            Vector3 sphereCenter, float sphereRadius, out Vector3 intersection)
        {
            var r = sphereRadius;
            var d = segmentEnd - segmentStart;
            var f = segmentStart - sphereCenter;
            var a = Vector3.Dot(d, d);
            var b = 2 * Vector3.Dot(f, d);
            var c = Vector3.Dot(f, f) - r * r;
            float discriminant = b * b - 4 * a * c;
            float t = -1;
            intersection = segmentStart;
            if (discriminant < 0) return false;
            else
            {
                discriminant = Mathf.Sqrt(discriminant);
                var t1 = (-b - discriminant) / (2 * a);
                var t2 = (-b + discriminant) / (2 * a);
                if (t1 >= 0 && t1 <= 1 && t1 > t2) t = t1;
                else if (t2 >= 0 && t2 <= 1 && t2 > t1) t = t2;
            }
            if (t == -1) return false;
            intersection += d * t;
            return true;
        }
        public static Vector3 NearestPathPoint(Vector3 startPoint, float minPathLenght,
            Vector3[] pathPoints, out int nearestPointIdx)
        {
            nearestPointIdx = pathPoints.Length - 1;
            var result = pathPoints.Last();
            for (int i = 1; i < pathPoints.Length; ++i)
            {
                var start = pathPoints[i - 1];
                var end = pathPoints[i];
                if (SphereSegmentIntersection(start, end, startPoint, minPathLenght, out Vector3 intersection))
                {
                    result = intersection;
                    nearestPointIdx = i - 1;
                    return result;
                }
            }
            return result;
        }


        public float lenght => _lenght;
        public Vector3[] pathPoints => _pathPoints.ToArray();
        public Vector3[] onSurfacePathPoints => _onSurfacePathPoints.ToArray();
        public Vector3 lastPathPoint => _pathPoints.Last();
        public Vector3[] midpoints => _midpoints.ToArray();
        public Vector3 lastTangentPos { get; set; }

        public bool showHandles { get; set; }
    }

    public class LineToolName : IToolName { public string value => "Line"; }

    [System.Serializable]
    public class LineSceneData : SceneData<LineToolName, LineSettings, LinePoint, LineData>
    {
        public LineSceneData() : base() { }
        public LineSceneData(string sceneGUID) : base(sceneGUID) { }
    }

    [System.Serializable]
    public class LineManager : PersistentToolManagerBase<LineToolName, LineSettings, LinePoint, LineData, LineSceneData> { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static LineData _lineData = LineData.instance;
        private static bool _selectingLinePoints = false;
        private static Rect _selectionRect = new Rect();
        private static System.Collections.Generic.List<GameObject> _disabledObjects
            = new System.Collections.Generic.List<GameObject>();
        private static bool _editingPersistentLine = false;
        private static LineData _initialPersistentLineData = null;
        private static LineData _selectedPersistentLineData = null;
        private static string _createProfileName = ToolProfile.DEFAULT;

        public static bool selectingLinePoints
        {
            get => _selectingLinePoints;
            set
            {
                if (value == _selectingLinePoints) return;
                _selectingLinePoints = value;
            }
        }

        private static void LineInitializeOnLoad()
        {
            LineManager.settings.OnDataChanged += OnLineSettingsChanged;
        }
        public static void ResetLineState(bool askIfWantToSave = true)
        {
            if (_lineData.state == ToolManager.ToolState.NONE) return;
            if (askIfWantToSave)
            {
                void Save()
                {
                    if (UnityEditor.SceneView.lastActiveSceneView != null)
                        LineStrokePreview(UnityEditor.SceneView.lastActiveSceneView, _lineData, false, true);
                    CreateLine();
                }
                AskIfWantToSave(_lineData.state, Save);
            }
            _snappedToVertex = false;
            selectingLinePoints = false;
            _lineData.Reset();
        }

        private static void DeselectPersistentLines()
        {
            var persistentLines = LineManager.instance.GetPersistentItems();
            foreach (var l in persistentLines)
            {
                l.selectedPointIdx = -1;
                l.ClearSelection();
            }
        }

        private static void OnLineToolModeChanged()
        {
            DeselectPersistentLines();
            if (!ToolManager.editMode)
            {
                if (_createProfileName != null)
                    ToolProperties.SetProfile(new ToolProperties.ProfileData(LineManager.instance, _createProfileName));
                ToolProperties.RepainWindow();
                return;
            }
            ResetLineState();
            ResetSelectedPersistentLine();
        }

        private static void OnLineSettingsChanged()
        {
            repaint = true;
            if (!ToolManager.editMode)
            {
                _lineData.settings = LineManager.settings;
                updateStroke = true;
                return;
            }
            if (_selectedPersistentLineData == null) return;
            _selectedPersistentLineData.settings.Copy(LineManager.settings);
            PreviewPersistentLine(_selectedPersistentLineData);
        }

        private static void ClearLineStroke()
        {
            _paintStroke.Clear();
            BrushstrokeManager.ClearBrushstroke();
            if (ToolManager.editMode && _selectedPersistentLineData != null)
            {
                _selectedPersistentLineData.UpdatePath(true);
                PreviewPersistentLine(_selectedPersistentLineData);
                UnityEditor.SceneView.RepaintAll();
                repaint = true;
            }
        }
        private static void OnUndoLine() => ClearLineStroke();

        private static void LineDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (LineManager.settings.paintOnMeshesWithoutCollider)
                PWBCore.CreateTempCollidersWithinFrustum(sceneView.camera);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (_lineData.state == ToolManager.ToolState.EDIT && _lineData.selectedPointIdx > 0)
                {
                    _lineData.selectedPointIdx = -1;
                    _lineData.ClearSelection();
                }
                else if (_lineData.state == ToolManager.ToolState.NONE && !ToolManager.editMode)
                    ToolManager.DeselectTool();
                else if (ToolManager.editMode)
                {
                    if (_editingPersistentLine) ResetSelectedPersistentLine();
                    else
                    {
                        ToolManager.DeselectTool();
                    }
                    DeselectPersistentLines();
                    _initialPersistentLineData = null;
                    _selectedPersistentLineData = null;
                    ToolProperties.SetProfile(new ToolProperties.ProfileData(LineManager.instance, _createProfileName));
                    ToolProperties.RepainWindow();
                    ToolManager.editMode = false;
                }
                else ResetLineState(false);
                OnUndoLine();
                UpdateStroke();
                BrushstrokeManager.ClearBrushstroke();
            }
            LineToolEditMode(sceneView);
            if (ToolManager.editMode) return;
            switch (_lineData.state)
            {
                case ToolManager.ToolState.NONE:
                    LineStateNone(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.PREVIEW:
                    LineStateStraightLine(sceneView.in2DMode);
                    break;
                case ToolManager.ToolState.EDIT:
                    LineStateBezier(sceneView);
                    break;
            }
        }

        private static bool DrawLineControlPoints(LineData lineData, bool showHandles,
            out bool clickOnPoint, out bool multiSelection, out bool addToSelection,
            out bool removedFromSelection, out bool wasEdited, out Vector3 delta)
        {
            delta = Vector3.zero;
            clickOnPoint = false;
            wasEdited = false;
            multiSelection = false;
            addToSelection = false;
            removedFromSelection = false;
            bool leftMouseDown = Event.current.button == 0 && Event.current.type == EventType.MouseDown;
            for (int i = 0; i < lineData.pointsCount; ++i)
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (selectingLinePoints)
                {
                    var GUIPos = UnityEditor.HandleUtility.WorldToGUIPoint(lineData.GetPoint(i));
                    var rect = _selectionRect;
                    if (_selectionRect.size.x < 0 || _selectionRect.size.y < 0)
                    {
                        var max = Vector2.Max(_selectionRect.min, _selectionRect.max);
                        var min = Vector2.Min(_selectionRect.min, _selectionRect.max);
                        var size = max - min;
                        rect = new Rect(min, size);
                    }
                    if (rect.Contains(GUIPos))
                    {
                        if (!Event.current.control && lineData.selectedPointIdx < 0) lineData.selectedPointIdx = i;
                        lineData.AddToSelection(i);
                        clickOnPoint = true;
                        multiSelection = true;
                    }
                }
                else if (!clickOnPoint)
                {
                    if (showHandles)
                    {
                        float distFromMouse
                            = UnityEditor.HandleUtility.DistanceToRectangle(lineData.GetPoint(i), Quaternion.identity, 0f);
                        UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                        if (leftMouseDown && UnityEditor.HandleUtility.nearestControl == controlId)
                        {
                            if (!Event.current.control)
                            {
                                lineData.selectedPointIdx = i;
                                lineData.ClearSelection();
                            }
                            if (Event.current.control || lineData.selectionCount == 0)
                            {
                                if (lineData.IsSelected(i))
                                {
                                    lineData.RemoveFromSelection(i);
                                    lineData.selectedPointIdx = -1;
                                    removedFromSelection = true;
                                }
                                else
                                {
                                    lineData.AddToSelection(i);
                                    lineData.showHandles = true;
                                    lineData.selectedPointIdx = i;
                                    if (Event.current.control) addToSelection = true;
                                }
                            }
                            clickOnPoint = true;
                            Event.current.Use();
                        }
                    }
                }
                if (Event.current.type != EventType.Repaint) continue;
                DrawDotHandleCap(lineData.GetPoint(i), 1, 1, lineData.IsSelected(i));
            }
            var midpoints = lineData.midpoints;
            for (int i = 0; i < midpoints.Length; ++i)
            {
                var point = midpoints[i];

                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                if (showHandles)
                {
                    float distFromMouse
                           = UnityEditor.HandleUtility.DistanceToRectangle(point, Quaternion.identity, 0f);
                    UnityEditor.HandleUtility.AddControl(controlId, distFromMouse);
                }
                DrawDotHandleCap(point, 0.4f);
                if (showHandles && UnityEditor.HandleUtility.nearestControl == controlId)
                {
                    DrawDotHandleCap(point);
                    if (leftMouseDown)
                    {
                        lineData.InsertPoint(i + 1, new LinePoint(point));
                        lineData.selectedPointIdx = i + 1;
                        lineData.ClearSelection();
                        updateStroke = true;
                        clickOnPoint = true;
                        Event.current.Use();
                    }
                }
            }
            if (showHandles && lineData.showHandles && lineData.selectedPointIdx >= 0)
            {
                var selectedPoint = lineData.selectedPoint;
                if (_updateHandlePosition)
                {
                    selectedPoint = _handlePosition;
                    _updateHandlePosition = false;
                }
                var prevPosition = lineData.selectedPoint;
                lineData.SetPoint(lineData.selectedPointIdx,
                    UnityEditor.Handles.PositionHandle(selectedPoint, Quaternion.identity), true);
                var point = _snapToVertex ? LinePointSnapping(lineData.selectedPoint)
                    : SnapAndUpdateGridOrigin(lineData.selectedPoint, SnapManager.settings.snappingEnabled,
                        LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider,
                        false, Vector3.down);
                lineData.SetPoint(lineData.selectedPointIdx, point, false);
                _handlePosition = lineData.selectedPoint;
                if (prevPosition != lineData.selectedPoint)
                {
                    wasEdited = true;
                    updateStroke = true;
                    delta = lineData.selectedPoint - prevPosition;
                    ToolProperties.RepainWindow();
                }
            }
            if (!showHandles) return false;
            return clickOnPoint || wasEdited;
        }

        private static Vector3 LinePointSnapping(Vector3 point)
        {
            const float snapSqrDistance = 400f;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var persistentLines = LineManager.instance.GetPersistentItems();
            var result = point;
            var minSqrDistance = snapSqrDistance;
            foreach (var lineData in persistentLines)
            {
                var controlPoints = lineData.points;
                foreach (var controlPoint in controlPoints)
                {
                    var intersection = mouseRay.origin + Vector3.Project(controlPoint - mouseRay.origin, mouseRay.direction);
                    var GUIControlPoint = UnityEditor.HandleUtility.WorldToGUIPoint(controlPoint);
                    var intersectionGUIPoint = UnityEditor.HandleUtility.WorldToGUIPoint(intersection);
                    var sqrDistance = (GUIControlPoint - intersectionGUIPoint).sqrMagnitude;
                    if (sqrDistance > 0 && sqrDistance < snapSqrDistance && sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        result = controlPoint;
                    }
                }
            }
            return result;
        }

        private static void LineToolEditMode(UnityEditor.SceneView sceneView)
        {
            var persistentLines = LineManager.instance.GetPersistentItems();
            var selectedLineId = _initialPersistentLineData == null ? -1 : _initialPersistentLineData.id;
            bool clickOnAnyPoint = false;
            bool someLinesWereEdited = false;
            var delta = Vector3.zero;
            var editedData = _selectedPersistentLineData;
            var deselectedLines = new System.Collections.Generic.List<LineData>(persistentLines);
            DrawSelectionRectangle();
            foreach (var lineData in persistentLines)
            {
                lineData.UpdateObjects();
                if (lineData.objectCount == 0)
                {
                    LineManager.instance.RemovePersistentItem(lineData.id);
                    continue;
                }
                DrawLine(lineData);

                if (DrawLineControlPoints(lineData, ToolManager.editMode, out bool clickOnPoint, out bool multiSelection,
                     out bool addToselection, out bool removedFromSelection, out bool wasEdited, out Vector3 localDelta))
                {
                    if (clickOnPoint)
                    {
                        clickOnAnyPoint = true;
                        _editingPersistentLine = true;
                        if (selectedLineId != lineData.id)
                        {
                            ApplySelectedPersistentLine(false);
                            if (selectedLineId == -1)
                                _createProfileName = LineManager.instance.selectedProfileName;
                            LineManager.instance.CopyToolSettings(lineData.settings);
                            ToolProperties.RepainWindow();
                            PWBCore.SetActiveTempColliders(lineData.objects, false);
                        }
                        _selectedPersistentLineData = lineData;
                        if (_initialPersistentLineData == null) _initialPersistentLineData = lineData.Clone();
                        else if (_initialPersistentLineData.id != lineData.id) _initialPersistentLineData = lineData.Clone();
                        if (!removedFromSelection) foreach (var l in persistentLines) l.showHandles = (l == lineData);
                        deselectedLines.Remove(lineData);
                    }
                    if (addToselection)
                    {
                        deselectedLines.Clear();
                        lineData.showHandles = true;
                    }
                    if (removedFromSelection) deselectedLines.Clear();
                    if (wasEdited)
                    {
                        _editingPersistentLine = true;
                        someLinesWereEdited = true;
                        delta = localDelta;
                        editedData = lineData;
                    }
                }
            }

            if (clickOnAnyPoint)
            {
                foreach (var lineData in deselectedLines)
                {
                    lineData.showHandles = false;
                    lineData.selectedPointIdx = -1;
                    lineData.ClearSelection();
                    PWBCore.SetActiveTempColliders(lineData.objects, true);
                }
            }
            var linesEdited = persistentLines.Where(i => i.selectionCount > 0).ToArray();

            if (someLinesWereEdited && linesEdited.Length > 0)
                _disabledObjects.Clear();
            if (someLinesWereEdited && linesEdited.Length > 1)
            {
                _paintStroke.Clear();
                foreach (var lineData in linesEdited)
                {
                    if (lineData != editedData) lineData.AddDeltaToSelection(delta);
                    lineData.UpdatePath();
                    PreviewPersistentLine(lineData);
                    LineStrokePreview(sceneView, lineData, true, true);
                }
                PWBCore.SetSavePending();
                return;
            }
            if (linesEdited.Length > 1)
            {
                PreviewPersistent(sceneView.camera);
            }

            if (!ToolManager.editMode) return;

            SelectionRectangleInput(clickOnAnyPoint);

            if ((!someLinesWereEdited && linesEdited.Length <= 1)
                && _editingPersistentLine && _selectedPersistentLineData != null)
            {
                var forceStrokeUpdate = updateStroke;
                if (updateStroke)
                {
                    _selectedPersistentLineData.UpdatePath();
                    PreviewPersistentLine(_selectedPersistentLineData);
                    updateStroke = false;
                    PWBCore.SetSavePending();
                }
                if (_brushstroke != null && !BrushstrokeManager.BrushstrokeEqual(BrushstrokeManager.brushstroke, _brushstroke))
                    _paintStroke.Clear();

                LineStrokePreview(sceneView, _selectedPersistentLineData, true, forceStrokeUpdate);
            }
            LineInput(true, sceneView);
        }

        private static void PreviewPersistentLine(LineData lineData)
        {
            PWBCore.UpdateTempCollidersIfHierarchyChanged();

            Vector3[] objPos = null;
            var objList = lineData.objectList;
            Vector3[] strokePos = null;
            var settings = lineData.settings;
            BrushstrokeManager.UpdatePersistentLineBrushstroke(lineData.pathPoints,
                settings, objList, out objPos, out strokePos);
            _disabledObjects.AddRange(lineData.objects.ToList());
            float pathLength = 0;
            var prevSegmentDir = Vector3.zero;

            BrushSettings brushSettings = PaletteManager.GetBrushById(lineData.initialBrushId);
            if (brushSettings == null && PaletteManager.selectedBrush != null)
            {
                brushSettings = PaletteManager.selectedBrush;
                lineData.SetInitialBrushId(brushSettings.id);
            }
            if (settings.overwriteBrushProperties) brushSettings = settings.brushSettings;
            if (brushSettings == null) brushSettings = new BrushSettings();
            var objArray = objList.ToArray();
            for (int objIdx = 0; objIdx < objPos.Length; ++objIdx)
            {
                var obj = objList[objIdx];

                obj.SetActive(true);
                if (objIdx > 0) pathLength += (objPos[objIdx] - objPos[objIdx - 1]).magnitude;

                var bounds = BoundsUtils.GetBoundsRecursive(obj.transform, obj.transform.rotation, true,
                    BoundsUtils.ObjectProperty.BOUNDING_BOX, true, true);

                var size = bounds.size;
                var pivotToCenter = Vector3.Scale(obj.transform.InverseTransformPoint(bounds.center),
                    obj.transform.lossyScale);
                var height = Mathf.Max(size.x, size.y, size.z) * 2;
                Vector3 segmentDir = Vector3.zero;
                var objOnLineSize = AxesUtils.GetAxisValue(size, settings.axisOrientedAlongTheLine);
                if (settings.objectsOrientedAlongTheLine && objPos.Length > 1)
                {
                    if (objIdx < objPos.Length - 1)
                    {
                        if (objIdx + 1 < objPos.Length) segmentDir = objPos[objIdx + 1] - objPos[objIdx];
                        else if (strokePos.Length > 0) segmentDir = strokePos[0] - objPos[objIdx];
                        prevSegmentDir = segmentDir;
                    }
                    else
                    {
                        var nearestPathPoint = LineData.NearestPathPoint(objPos[objIdx],
                            objOnLineSize, lineData.pathPoints, out int nearestPointIdx);
                        segmentDir = nearestPathPoint - objPos[objIdx];
                        segmentDir = segmentDir.normalized * prevSegmentDir.magnitude;
                    }
                }

                if (objPos.Length == 1) segmentDir = lineData.lastPathPoint - objPos[0];
                else if (objIdx == objPos.Length - 1)
                {
                    var onLineSize = objOnLineSize + settings.gapSize;
                    var segmentSize = segmentDir.magnitude;
                    if (segmentSize > onLineSize) segmentDir = segmentDir.normalized
                            * (settings.spacingType == LineSettings.SpacingType.BOUNDS ? onLineSize : settings.spacing);
                }
                if (settings.objectsOrientedAlongTheLine && !settings.perpendicularToTheSurface)
                {
                    var projectionAxis = ((AxesUtils.SignedAxis)(settings.projectionDirection)).axis;
                    segmentDir -= AxesUtils.GetVector(AxesUtils.GetAxisValue(segmentDir, projectionAxis), projectionAxis);
                }
                var normal = -settings.projectionDirection;
                var otherAxes = AxesUtils.GetOtherAxes((AxesUtils.SignedAxis)(-settings.projectionDirection));
                var tangetAxis = otherAxes[settings.objectsOrientedAlongTheLine ? 0 : 1];
                Vector3 itemTangent = (AxesUtils.SignedAxis)(tangetAxis);
                var itemRotation = Quaternion.LookRotation(itemTangent, normal);
                var lookAt = Quaternion.LookRotation((Vector3)(AxesUtils.SignedAxis)
                    (settings.axisOrientedAlongTheLine), Vector3.up);
                if (segmentDir != Vector3.zero) itemRotation = Quaternion.LookRotation(segmentDir, normal) * lookAt;
                var itemPosition = objPos[objIdx];
                var ray = new Ray(itemPosition + normal * height, -normal);
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit, out GameObject collider, float.MaxValue, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider,
                        null, null, null, objArray))
                    {
                        itemPosition = itemHit.point;
                        if (settings.perpendicularToTheSurface) normal = itemHit.normal;
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }

                if (settings.perpendicularToTheSurface && segmentDir != Vector3.zero)
                {
                    if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                    {
                        var bitangent = Vector3.Cross(segmentDir, normal);
                        var lineNormal = Vector3.Cross(bitangent, segmentDir);
                        itemRotation = Quaternion.LookRotation(segmentDir, lineNormal) * lookAt;
                    }
                    else
                    {
                        var plane = new Plane(normal, itemPosition);
                        var tangent = plane.ClosestPointOnPlane(segmentDir + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(tangent, normal) * lookAt;
                    }
                }
                else if (!settings.perpendicularToTheSurface && segmentDir != Vector3.zero)
                    itemRotation = Quaternion.LookRotation(segmentDir, normal) * lookAt;

                itemPosition += normal * brushSettings.surfaceDistance;
                itemPosition += itemRotation * brushSettings.localPositionOffset;
                var sizeOffset = itemRotation * (settings.axisOrientedAlongTheLine == AxesUtils.Axis.X
                    ? (Vector3.left * (size.x / 2))
                    : (Vector3.forward * (size.z / 2)));
                itemPosition += sizeOffset;
                itemPosition += itemRotation * (Vector3.up * (size.y / 2) - pivotToCenter);

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

                UnityEditor.Undo.RecordObject(obj.transform, LineData.COMMAND_NAME);
                obj.transform.SetPositionAndRotation(itemPosition, itemRotation);
                _disabledObjects.Remove(obj);
                lineData.lastTangentPos = objPos[objIdx];
            }
            _disabledObjects = _disabledObjects.Where(i => i != null).ToList();
            foreach (var obj in _disabledObjects) obj.SetActive(false);
        }

        private static void ResetSelectedPersistentLine()
        {
            _editingPersistentLine = false;
            if (_initialPersistentLineData == null) return;
            var selectedLine = LineManager.instance.GetItem(_initialPersistentLineData.id);
            if (selectedLine == null) return;
            selectedLine.ResetPoses(_initialPersistentLineData);
            selectedLine.selectedPointIdx = -1;
            selectedLine.ClearSelection();
        }

        private static void ApplySelectedPersistentLine(bool deselectPoint)
        {
            if (!ApplySelectedPersistentObject(deselectPoint, ref _editingPersistentLine, ref _initialPersistentLineData,
                ref _selectedPersistentLineData, LineManager.instance)) return;
            if (_initialPersistentLineData == null) return;
            var selected = LineManager.instance.GetItem(_initialPersistentLineData.id);
            _initialPersistentLineData = selected.Clone();
        }
        private static void LineStateNone(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _lineData.state = ToolManager.ToolState.PREVIEW;
                Event.current.Use();
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, LineManager.settings.mode, in2DMode,
                LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider, false))
            {
                point = _snapToVertex ? LinePointSnapping(point)
                    : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                    LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider,
                    false, Vector3.down);
                _lineData.SetPoint(0, point, false);
                _lineData.SetPoint(1, point, false);
            }
            DrawDotHandleCap(_lineData.GetPoint(0));
        }

        private static void LineStateStraightLine(bool in2DMode)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && !Event.current.alt)
            {
                _lineData.state = ToolManager.ToolState.EDIT;
                updateStroke = true;
            }
            if (MouseDot(out Vector3 point, out Vector3 normal, LineManager.settings.mode, in2DMode,
                LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider, false))
            {
                point = _snapToVertex ? LinePointSnapping(point)
                    : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                    LineManager.settings.paintOnPalettePrefabs, LineManager.settings.paintOnMeshesWithoutCollider,
                    false, Vector3.down);
                _lineData.SetPoint(1, point, false);
            }

            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, new Vector3[] { _lineData.GetPoint(0), _lineData.GetPoint(1) });
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(4, new Vector3[] { _lineData.GetPoint(0), _lineData.GetPoint(1) });
            DrawDotHandleCap(_lineData.GetPoint(0));
            DrawDotHandleCap(_lineData.GetPoint(1));
        }

        private static void DrawLine(LineData lineData)
        {
            var pathPoints = lineData.pathPoints;
            if (pathPoints.Length == 0) lineData.UpdatePath(true);
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            var surfacePathPoints = lineData.onSurfacePathPoints;
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, surfacePathPoints);
            UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.5f);
            UnityEditor.Handles.DrawAAPolyLine(4, surfacePathPoints);

            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(8, pathPoints);
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.7f);
            UnityEditor.Handles.DrawAAPolyLine(4, pathPoints);
        }

        private static void DrawSelectionRectangle()
        {
            if (!selectingLinePoints) return;
            var rays = new Ray[]
            {
                UnityEditor.HandleUtility.GUIPointToWorldRay(_selectionRect.min),
                UnityEditor.HandleUtility.GUIPointToWorldRay(new Vector2(_selectionRect.xMax, _selectionRect.yMin)),
                UnityEditor.HandleUtility.GUIPointToWorldRay(_selectionRect.max),
                UnityEditor.HandleUtility.GUIPointToWorldRay(new Vector2(_selectionRect.xMin, _selectionRect.yMax))
            };
            var verts = new Vector3[4];
            for (int i = 0; i < 4; ++i) verts[i] = rays[i].origin + rays[i].direction;
            UnityEditor.Handles.DrawSolidRectangleWithOutline(verts,
            new Color(0f, 0.5f, 0.5f, 0.3f), new Color(0f, 0.5f, 0.5f, 1f));
        }

        private static void SelectionRectangleInput(bool clickOnPoint)
        {
            bool leftMouseDown = Event.current.button == 1 && Event.current.type == EventType.MouseDown;
            if (!selectingLinePoints && Event.current.shift && leftMouseDown && !clickOnPoint)
            {
                selectingLinePoints = true;
                _selectionRect = new Rect(Event.current.mousePosition, Vector2.zero);
                Event.current.Use();
            }
            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove)
                && selectingLinePoints)
            {
                _selectionRect.size = Event.current.mousePosition - _selectionRect.position;
            }
            if (Event.current.button == 0 && (Event.current.type == EventType.MouseUp
                || Event.current.type == EventType.Ignore || Event.current.type == EventType.KeyUp))
                selectingLinePoints = false;
        }

        private static void CreateLine()
        {
            var nextLineId = LineData.nextHexId;
            var objDic = Paint(LineManager.settings, PAINT_CMD, true, false, nextLineId);
            if (objDic.Count != 1) return;
            var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            var sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID(scenePath);
            var initialBrushId = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.id : -1;
            var objs = objDic[nextLineId].ToArray();
            var persistentData = new LineData(objs, initialBrushId, _lineData);
            LineManager.instance.AddPersistentItem(sceneGUID, persistentData);
        }
        private static void LineInput(bool persistent, UnityEditor.SceneView sceneView)
        {
            var lineData = persistent ? _selectedPersistentLineData : _lineData;
            if (lineData == null) return;
            if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
            {
                if (persistent)
                {
                    DeleteDisabledObjects();
                    ApplySelectedPersistentLine(true);
                    ToolProperties.SetProfile(new ToolProperties.ProfileData(LineManager.instance, _createProfileName));
                    DeleteDisabledObjects();
                    ToolProperties.RepainWindow();
                }
                else
                {
                    CreateLine();
                    ResetLineState(false);
                }
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete
                && !Event.current.control && !Event.current.alt && !Event.current.shift)
            {
                lineData.RemoveSelectedPoints();
                if (persistent) PreviewPersistentLine(_selectedPersistentLineData);
                else updateStroke = true;
            }
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 1
                && Event.current.control && !Event.current.alt && !Event.current.shift)
            {
                if (MouseDot(out Vector3 point, out Vector3 normal, lineData.settings.mode, sceneView.in2DMode,
                lineData.settings.paintOnPalettePrefabs, lineData.settings.paintOnMeshesWithoutCollider, false))
                {
                    point = _snapToVertex ? LinePointSnapping(point)
                        : SnapAndUpdateGridOrigin(point, SnapManager.settings.snappingEnabled,
                        lineData.settings.paintOnPalettePrefabs, lineData.settings.paintOnMeshesWithoutCollider,
                        false, Vector3.down);
                    lineData.AddPoint(point, false);
                    if (persistent)
                    {
                        PreviewPersistentLine(_selectedPersistentLineData);
                        LineStrokePreview(sceneView, lineData, true, true);
                    }
                    else updateStroke = true;
                }
            }
            else if (PWBSettings.shortcuts.lineSelectAllPoints.Check()) lineData.SelectAll();
            else if (PWBSettings.shortcuts.lineDeselectAllPoints.Check())
            {
                lineData.selectedPointIdx = -1;
                lineData.ClearSelection();
            }
            else if (PWBSettings.shortcuts.lineToggleCurve.Check())
            {
                lineData.ToggleSegmentType();
                updateStroke = true;
            }
            else if (PWBSettings.shortcuts.lineToggleClosed.Check())
            {
                lineData.ToggleClosed();
                updateStroke = true;
            }
            else if (PWBSettings.shortcuts.lineEditGap.Check())
            {
                var deltaSign = Mathf.Sign(PWBSettings.shortcuts.lineEditGap.combination.delta);
                lineData.settings.gapSize += lineData.lenght * deltaSign * 0.001f;
                ToolProperties.RepainWindow();
            }
            if (!persistent) return;
            if (PWBSettings.shortcuts.editModeSelectParent.Check() && lineData != null)
            {
                var parent = lineData.GetParent();
                if (parent != null) UnityEditor.Selection.activeGameObject = parent;
            }
            else if (PWBSettings.shortcuts.editModeDeleteItemButNotItsChildren.Check())
                LineManager.instance.DeletePersistentItem(lineData.id, false);
            else if (PWBSettings.shortcuts.editModeDeleteItemAndItsChildren.Check())
                LineManager.instance.DeletePersistentItem(lineData.id, true);
        }

        private static void LineStateBezier(UnityEditor.SceneView sceneView)
        {
            var pathPoints = _lineData.pathPoints;
            var forceStrokeUpdate = updateStroke;
            if (updateStroke)
            {
                _lineData.UpdatePath();
                pathPoints = _lineData.pathPoints;
                BrushstrokeManager.UpdateLineBrushstroke(pathPoints);
                updateStroke = false;
            }
            LineStrokePreview(sceneView, _lineData, false, forceStrokeUpdate);
            DrawLine(_lineData);
            DrawSelectionRectangle();
            LineInput(false, sceneView);

            if (selectingLinePoints && !Event.current.control)
            {
                _lineData.selectedPointIdx = -1;
                _lineData.ClearSelection();
            }
            bool clickOnPoint, wasEdited;
            DrawLineControlPoints(_lineData, true, out clickOnPoint, out bool multiSelection, out bool addToselection,
                out bool removeFromSelection, out wasEdited, out Vector3 delta);
            if (wasEdited) updateStroke = true;
            SelectionRectangleInput(clickOnPoint);
        }
        private static void LineStrokePreview(UnityEditor.SceneView sceneView,
            LineData lineData, bool persistent, bool forceUpdate)
        {
            var settings = lineData.settings;
            var lastPoint = lineData.lastPathPoint;
            var objectCount = lineData.objectCount;
            var lastObjectTangentPosition = lineData.lastTangentPos;

            BrushstrokeItem[] brushstroke = null;

            if (PreviewIfBrushtrokestaysTheSame(out brushstroke, sceneView.camera, forceUpdate)) return;
            PWBCore.UpdateTempCollidersIfHierarchyChanged();

            if (!persistent) _paintStroke.Clear();
            for (int i = 0; i < brushstroke.Length; ++i)
            {
                var strokeItem = brushstroke[i];
                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                var bounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation);
                BrushSettings brushSettings = strokeItem.settings;
                if (LineManager.settings.overwriteBrushProperties) brushSettings = LineManager.settings.brushSettings;
                var size = Vector3.Scale(bounds.size, brushSettings.scaleMultiplier);

                var pivotToCenter = Vector3.Scale(
                    prefab.transform.InverseTransformDirection(bounds.center - prefab.transform.position),
                    brushSettings.scaleMultiplier);
                var height = Mathf.Max(size.x, size.y, size.z) * 2;
                Vector3 segmentDir = Vector3.zero;

                if (settings.objectsOrientedAlongTheLine && brushstroke.Length > 1)
                {
                    segmentDir = i < brushstroke.Length - 1
                        ? strokeItem.nextTangentPosition - strokeItem.tangentPosition
                        : lastPoint - strokeItem.tangentPosition;
                }
                if (brushstroke.Length == 1)
                {
                    segmentDir = lastPoint - brushstroke[0].tangentPosition;
                    if (persistent && objectCount > 0)
                        segmentDir = lastPoint - lastObjectTangentPosition;
                }
                if (i == brushstroke.Length - 1)
                {
                    var onLineSize = AxesUtils.GetAxisValue(size, settings.axisOrientedAlongTheLine)
                        + settings.gapSize;
                    var segmentSize = segmentDir.magnitude;
                    if (segmentSize > onLineSize) segmentDir = segmentDir.normalized
                            * (settings.spacingType == LineSettings.SpacingType.BOUNDS ? onLineSize : settings.spacing);
                }
                if (settings.objectsOrientedAlongTheLine && !settings.perpendicularToTheSurface)
                {
                    var projectionAxis = ((AxesUtils.SignedAxis)(settings.projectionDirection)).axis;
                    segmentDir -= AxesUtils.GetVector(AxesUtils.GetAxisValue(segmentDir, projectionAxis), projectionAxis);
                }
                var normal = -settings.projectionDirection;
                var otherAxes = AxesUtils.GetOtherAxes((AxesUtils.SignedAxis)(-settings.projectionDirection));
                var tangetAxis = otherAxes[settings.objectsOrientedAlongTheLine ? 0 : 1];
                Vector3 itemTangent = (AxesUtils.SignedAxis)(tangetAxis);
                var itemRotation = Quaternion.LookRotation(itemTangent, normal);
                var lookAt = Quaternion.LookRotation((Vector3)(AxesUtils.SignedAxis)
                    (settings.axisOrientedAlongTheLine), Vector3.up);

                var itemPosition = strokeItem.tangentPosition + segmentDir / 2;

                var ray = new Ray(itemPosition + normal * height, -normal);
                Transform surface = null;
                if (settings.mode != PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                {
                    if (MouseRaycast(ray, out RaycastHit itemHit,
                        out GameObject collider, float.MaxValue, -1,
                        settings.paintOnPalettePrefabs, settings.paintOnMeshesWithoutCollider))
                    {
                        itemPosition = itemHit.point;
                        if (settings.perpendicularToTheSurface) normal = itemHit.normal;
                        var colObj = PWBCore.GetGameObjectFromTempCollider(collider);
                        if (colObj != null) surface = colObj.transform;
                    }
                    else if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SURFACE) continue;
                }



                if (settings.perpendicularToTheSurface && segmentDir != Vector3.zero)
                {
                    if (settings.mode == PaintOnSurfaceToolSettingsBase.PaintMode.ON_SHAPE)
                    {
                        var bitangent = Vector3.Cross(segmentDir, normal);
                        var lineNormal = Vector3.Cross(bitangent, segmentDir);
                        itemRotation = Quaternion.LookRotation(segmentDir, lineNormal) * lookAt;
                    }
                    else
                    {
                        var plane = new Plane(normal, itemPosition);
                        var tangent = plane.ClosestPointOnPlane(segmentDir + itemPosition) - itemPosition;
                        itemRotation = Quaternion.LookRotation(tangent, normal) * lookAt;
                    }
                }
                else if (!settings.perpendicularToTheSurface && segmentDir != Vector3.zero)
                    itemRotation = Quaternion.LookRotation(segmentDir, normal) * lookAt;
                itemPosition += normal * strokeItem.surfaceDistance;
                itemRotation *= Quaternion.Euler(strokeItem.additionalAngle);

                itemPosition += itemRotation * brushSettings.localPositionOffset;
                itemPosition -= itemRotation * (pivotToCenter - Vector3.up * (size.y / 2));
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

                var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, strokeItem.scaleMultiplier)
                    * Matrix4x4.Rotate(Quaternion.Inverse(prefab.transform.rotation))
                    * Matrix4x4.Translate(-prefab.transform.position);
                var itemScale = Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier);
                var layer = settings.overwritePrefabLayer ? settings.layer : prefab.layer;

                Transform parentTransform = settings.parent;
                var paintItem = new PaintStrokeItem(prefab, itemPosition, itemRotation,
                    itemScale, layer, parentTransform, surface, strokeItem.flipX, strokeItem.flipY);
                paintItem.persistentParentId = persistent ? lineData.hexId : LineData.nextHexId;
                _paintStroke.Add(paintItem);
                PreviewBrushItem(prefab, rootToWorld, layer, sceneView.camera,
                    false, false, strokeItem.flipX, strokeItem.flipY);
                var prevData = new PreviewData(prefab, rootToWorld, layer, strokeItem.flipX, strokeItem.flipY);
                _previewData.Add(prevData);
            }
            if (_persistentPreviewData.ContainsKey(lineData.id)) _persistentPreviewData[lineData.id] = _previewData.ToArray();
            else _persistentPreviewData.Add(lineData.id, _previewData.ToArray());
        }
    }
    #endregion
}
