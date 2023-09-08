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
    #region DATA & SETTINGS
    [System.Serializable]
    public class ExtrudeSettings : SelectionToolBaseBasic, IToolSettings, IPaintToolSettings
    {
        [SerializeField] private Space _space = Space.World;
        [SerializeField] private Vector3 _spacing = Vector3.zero;
        public enum SpacingType { BOX_SIZE, CUSTOM }
        [SerializeField] private SpacingType _spacingType = SpacingType.CUSTOM;
        [SerializeField] private Vector3 _multiplier = Vector3.one;

        public enum RotationAccordingTo { FRIST_SELECTED, LAST_SELECTED }
        [SerializeField] private RotationAccordingTo _rotationAccordingTo = RotationAccordingTo.FRIST_SELECTED;

        [SerializeField] private bool _sameParentAsSource = true;

        [SerializeField] private Vector3 _eulerOffset = Vector3.zero;
        [SerializeField] private bool _addRandomRotation = false;
        [SerializeField] private float _rotationFactor = 90;
        [SerializeField] private bool _rotateInMultiples = false;
        [SerializeField]
        private RandomUtils.Range3 _randomEulerOffset = new RandomUtils.Range3(Vector3.zero, Vector3.zero);


        public Space space
        {
            get => _space;
            set
            {
                if (_space == value) return;
                _space = value;
                DataChanged();
            }
        }
        public Vector3 multiplier
        {
            get => _multiplier;
            set
            {
                if (_multiplier == value) return;
                _multiplier = value;
                DataChanged();
            }
        }

        public RotationAccordingTo rotationAccordingTo
        {
            get => _rotationAccordingTo;
            set
            {
                if (_rotationAccordingTo == value) return;
                _rotationAccordingTo = value;
                DataChanged();
            }
        }

        public Vector3 spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                DataChanged();
            }
        }
        public SpacingType spacingType
        {
            get => _spacingType;
            set
            {
                if (_spacingType == value) return;
                _spacingType = value;
                DataChanged();
            }
        }

        public ExtrudeSettings Clone()
        {
            var clone = new ExtrudeSettings();
            clone.Copy(this);
            return clone;
        }


        public Vector3 eulerOffset
        {
            get => _eulerOffset;
            set
            {
                if (_eulerOffset == value) return;
                _eulerOffset = value;
                _randomEulerOffset.v1 = _randomEulerOffset.v2 = Vector3.zero;
            }
        }
        public bool addRandomRotation
        {
            get => _addRandomRotation;
            set
            {
                if (_addRandomRotation == value) return;
                _addRandomRotation = value;
            }
        }
        public float rotationFactor
        {
            get => _rotationFactor;
            set
            {
                value = Mathf.Max(value, 0f);
                if (_rotationFactor == value) return;
                _rotationFactor = value;
            }
        }
        public bool rotateInMultiples
        {
            get => _rotateInMultiples;
            set
            {
                if (_rotateInMultiples == value) return;
                _rotateInMultiples = value;
            }
        }
        public RandomUtils.Range3 randomEulerOffset
        {
            get => _randomEulerOffset;
            set
            {
                if (_randomEulerOffset == value) return;
                _randomEulerOffset = value;
                _eulerOffset = Vector3.zero;
            }
        }

        public override void Copy(IToolSettings other)
        {
            var otherExtrudeSettings = other as ExtrudeSettings;
            if (otherExtrudeSettings == null) return;
            base.Copy(other);
            _paintTool.Copy(otherExtrudeSettings._paintTool);
            _sameParentAsSource = otherExtrudeSettings._sameParentAsSource;
            _space = otherExtrudeSettings._space;
            _multiplier = otherExtrudeSettings._multiplier;
            _rotationAccordingTo = otherExtrudeSettings._rotationAccordingTo;
            _spacing = otherExtrudeSettings._spacing;
            _spacingType = otherExtrudeSettings._spacingType;
            _eulerOffset = otherExtrudeSettings._eulerOffset;
            _addRandomRotation = otherExtrudeSettings._addRandomRotation;
            _rotationFactor = otherExtrudeSettings._rotationFactor;
            _rotateInMultiples = otherExtrudeSettings._rotateInMultiples;
            _randomEulerOffset = otherExtrudeSettings._randomEulerOffset;
        }

        public bool sameParentAsSource
        {
            get => _sameParentAsSource;
            set
            {
                if (_sameParentAsSource == value) return;
                _sameParentAsSource = value;
                DataChanged();
            }
        }

        #region PAINT TOOL
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
        #endregion
    }

    [System.Serializable]
    public class ExtrudeManager : ToolManagerBase<ExtrudeSettings> { }
    #endregion
    #region PWBIO
    public static partial class PWBIO
    {
        private static Vector3 _extrudeHandlePosition;
        private static Vector3Int _extrudeDirection;
        private static Vector3 _initialExtrudePosition;
        private static Vector3 _selectionSize;
        private static Vector3 _deltaSnapped;
        private static Vector3 _extrudeSpacing;
        private static int _extrudegPreviewObjectCount = 0;

        public static void ResetExtrudeState(bool askIfWantToSave = true)
        {
            if (askIfWantToSave && _extrudegPreviewObjectCount > 0) DisplaySaveDialog(CreateExtrudedObjects);
            _extrudegPreviewObjectCount = 0;
            ClearExtrudeAngles();
        }

        public static void ClearExtrudeAngles() => _extrudeAngles.Clear();
        private static void ExtrudeDuringSceneGUI(UnityEditor.SceneView sceneView)
        {

            if (ExtrudeManager.settings.createTempColliders)
                PWBCore.CreateTempCollidersWithinFrustum(sceneView.camera);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                ResetUnityCurrentTool();
                ResetExtrudeState(false);
                ToolManager.DeselectTool();
                return;
            }
            if (SelectionManager.topLevelSelection.Length == 0) return;
            ExtrudeInput();
            if (SelectionManager.topLevelSelection.Length == 0) return;
            var settings = ExtrudeManager.settings;
            if (UnityEditor.Tools.current != UnityEditor.Tool.View && UnityEditor.Tools.current != UnityEditor.Tool.None)
            {
                SaveUnityCurrentTool();
                UnityEditor.Tools.current = UnityEditor.Tool.None;
            }
            var anchor = settings.rotationAccordingTo == ExtrudeSettings.RotationAccordingTo.FRIST_SELECTED
                ? SelectionManager.topLevelSelection.First().transform
                : SelectionManager.topLevelSelection.Last().transform;
            var handlePosition = UnityEditor.Handles.PositionHandle(_extrudeHandlePosition,
                settings.space == Space.World ? Quaternion.identity : anchor.rotation);
            var handleDelta = handlePosition - _extrudeHandlePosition;
            _extrudeHandlePosition = handlePosition;
            var delta = _extrudeHandlePosition - _initialExtrudePosition;
            if (settings.space == Space.Self)
            {
                handleDelta = anchor.InverseTransformVector(handleDelta);
                delta = anchor.InverseTransformVector(delta);
            }

            if (delta.sqrMagnitude > 0.01)
            {
                var direction = Vector3Int.one;
                var absDelta = new Vector3(Mathf.Abs(handleDelta.x),
                    Mathf.Abs(handleDelta.y), Mathf.Abs(handleDelta.z));
                direction.x = (absDelta.x <= absDelta.y || absDelta.x <= absDelta.z) ? 0 : (int)Mathf.Sign(delta.x);
                direction.y = (absDelta.y <= absDelta.x || absDelta.y <= absDelta.z) ? 0 : (int)Mathf.Sign(delta.y);
                direction.z = (absDelta.z <= absDelta.x || absDelta.z <= absDelta.y) ? 0 : (int)Mathf.Sign(delta.z);
                var directionChanged = direction != Vector3Int.zero && _extrudeDirection != direction;
                if (handleDelta != Vector3.zero && directionChanged && _extrudeDirection != Vector3.zero
                    && _extrudeDirection != (direction * -1)) CreateExtrudedObjects(anchor);

                if (directionChanged) _extrudeDirection = direction;
                _extrudeSpacing = _selectionSize + (settings.spacingType == ExtrudeSettings.SpacingType.BOX_SIZE
                    ? Vector3.Scale(_selectionSize, settings.multiplier - Vector3.one)
                    : settings.spacing);
                _deltaSnapped = new Vector3(
                    Mathf.Floor((Mathf.Abs(delta.x) + _selectionSize.x / 2f) / _extrudeSpacing.x)
                    * _extrudeSpacing.x * Mathf.Sign(delta.x),
                    Mathf.Floor((Mathf.Abs(delta.y) + _selectionSize.y / 2f) / _extrudeSpacing.y)
                    * _extrudeSpacing.y * Mathf.Sign(delta.y),
                    Mathf.Floor((Mathf.Abs(delta.z) + _selectionSize.z / 2f) / _extrudeSpacing.z)
                    * _extrudeSpacing.z * Mathf.Sign(delta.z));
                if (_deltaSnapped != Vector3.zero) PreviewExtrudedObjects(sceneView.camera, anchor);
            }
        }

            private static Vector3 GetExtrudeStep(Transform anchor)
        {
            var step = Vector3.Scale(_extrudeSpacing, _extrudeDirection);
            if (ExtrudeManager.settings.space == Space.Self)
            {
                if (anchor.lossyScale.x != 0) step.x /= anchor.lossyScale.x;
                if (anchor.lossyScale.y != 0) step.y /= anchor.lossyScale.y;
                if (anchor.lossyScale.z != 0) step.z /= anchor.lossyScale.z;
            }
            return step;
        }

        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Vector3>>
            _extrudeAngles = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Vector3>>();
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Pose>>
            _extrudePoses = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Pose>>();

        private static void PreviewExtrudedObjects(Camera camera, Transform anchor)
        {
            var step = GetExtrudeStep(anchor);
            var settings = ExtrudeManager.settings;
            _extrudegPreviewObjectCount = 0;
            _extrudePoses.Clear();
            foreach (var obj in SelectionManager.topLevelSelection)
            {
                var objPose = new Pose(obj.transform.position, obj.transform.rotation);

                var delta = step;
                var objId = obj.GetInstanceID();
                _extrudePoses.Add(objId, new System.Collections.Generic.List<Pose>());
                System.Collections.Generic.List<Vector3> rotationList = null;
                if (_extrudeAngles.ContainsKey(objId))
                {
                    rotationList = _extrudeAngles[objId];
                }
                else
                {
                    rotationList = new System.Collections.Generic.List<Vector3>();
                    _extrudeAngles.Add(objId, rotationList);
                }
                int rotationIdx = 0;

                do
                {
                    var deltaPos = settings.space == Space.World ? delta : anchor.TransformVector(delta);
                    var localToWorld = Matrix4x4.Translate(deltaPos);

                    var additonalAngle = Vector3.zero;
                    if (settings.space == Space.World)
                    {
                        if (rotationIdx < rotationList.Count - 1)
                        {
                            additonalAngle = rotationList[rotationIdx];
                        }
                        else
                        {
                            if (settings.addRandomRotation)
                            {
                                var randomAngle = settings.randomEulerOffset.randomVector;
                                if (settings.rotateInMultiples)
                                {
                                    randomAngle = new Vector3(
                                        Mathf.Round(randomAngle.x / settings.rotationFactor) * settings.rotationFactor,
                                        Mathf.Round(randomAngle.y / settings.rotationFactor) * settings.rotationFactor,
                                        Mathf.Round(randomAngle.z / settings.rotationFactor) * settings.rotationFactor);
                                }
                                additonalAngle += randomAngle;
                            }
                            else additonalAngle += settings.eulerOffset;
                            rotationList.Add(additonalAngle);
                        }

                        if (additonalAngle != Vector3.zero)
                        {
                            var aditionalRotation = Quaternion.Euler(additonalAngle);
                            Vector3 additionalRotationAxis;
                            float additionalRotationAngle;
                            aditionalRotation.ToAngleAxis(out additionalRotationAngle, out additionalRotationAxis);
                            obj.transform.rotation = objPose.rotation;
                            obj.transform.position = objPose.position;
                            var center = BoundsUtils.GetBoundsRecursive(obj.transform).center;
                            obj.transform.RotateAround(center, additionalRotationAxis, additionalRotationAngle);
                        }
                    }
                    var surfaceDelta = Vector3.zero;
                    if (settings.embedInSurface)
                    {
                        var bottomVertices = BoundsUtils.GetBottomVertices(obj.transform, Space.Self);
                        var height = BoundsUtils.GetMagnitude(obj.transform) * 3;
                        Vector3 position = anchor.position + deltaPos;
                        var rotation = anchor.rotation;
                        var TRS = Matrix4x4.TRS(position, rotation, obj.transform.lossyScale);
                        var surfceDistance = settings.embedAtPivotHeight
                        ? GetPivotDistanceToSurfaceSigned(position, height, true, true)
                        : GetBottomDistanceToSurfaceSigned(bottomVertices, TRS, height, true, true);
                        surfceDistance -= settings.surfaceDistance;
                        position += new Vector3(0f, -surfceDistance, 0f);
                        deltaPos += new Vector3(0f, -surfceDistance, 0f);
                        surfaceDelta = new Vector3(0f, -surfceDistance, 0f);
                        localToWorld = Matrix4x4.Translate(deltaPos);
                    }
                    ++_extrudegPreviewObjectCount;
                    PreviewBrushItem(obj, localToWorld, obj.layer, camera, false, false, false, false);
                    var posePosition = obj.transform.position + surfaceDelta;
                    posePosition += settings.space == Space.World ? delta : obj.transform.rotation * delta;
                    _extrudePoses[objId].Add(new Pose(posePosition, obj.transform.rotation));
                    delta += step;
                    ++rotationIdx;
                } while (Mathf.Abs(delta.x) <= Mathf.Abs(_deltaSnapped.x)
                && Mathf.Abs(delta.y) <= Mathf.Abs(_deltaSnapped.y)
                && Mathf.Abs(delta.z) <= Mathf.Abs(_deltaSnapped.z));
                obj.transform.rotation = objPose.rotation;
                obj.transform.position = objPose.position;
            }
        }

        private static void CreateExtrudedObjects(Transform anchor)
        {
            _extrudegPreviewObjectCount = 0;
            if (SelectionManager.topLevelSelection.Length == 0 || _extrudeDirection == Vector3Int.zero
                || _deltaSnapped == Vector3.zero) return;
            var newSelection = new System.Collections.Generic.List<GameObject>();
            _initialExtrudePosition += Vector3.Scale(_extrudeDirection, _deltaSnapped);
            _extrudeHandlePosition = _initialExtrudePosition;
            var step = GetExtrudeStep(anchor);
            foreach (var obj in SelectionManager.topLevelSelection)
            {
                GameObject extruded = null;
                var parent = GetParent(ExtrudeManager.settings, obj.name, true, null);
                if (ExtrudeManager.settings.sameParentAsSource) parent = obj.transform.parent;
                foreach(var pose in _extrudePoses[obj.GetInstanceID()])
                {
                    extruded = UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(obj)
                         ? (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(
                             UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj))
                         : GameObject.Instantiate(obj);
                    extruded.transform.position = pose.position;
                    extruded.transform.rotation = pose.rotation;
                    extruded.transform.localScale = obj.transform.lossyScale;
                    if (ExtrudeManager.settings.overwritePrefabLayer)
                        extruded.layer = ExtrudeManager.settings.layer;
                    const string COMMAND_NAME = "Extrude";
                    UnityEditor.Undo.RegisterCreatedObjectUndo(extruded, COMMAND_NAME);
                    UnityEditor.Undo.SetTransformParent(extruded.transform, parent, COMMAND_NAME);
                }
                newSelection.Add(extruded);
            }
            UnityEditor.Selection.objects = newSelection.ToArray();
        }

        private static void CreateExtrudedObjects()
        {
            var anchor = ExtrudeManager.settings.rotationAccordingTo == ExtrudeSettings.RotationAccordingTo.FRIST_SELECTED
               ? SelectionManager.topLevelSelection.First().transform
               : SelectionManager.topLevelSelection.Last().transform;
            CreateExtrudedObjects(anchor);
        }
        private static void ExtrudeInput()
        {
            if (SelectionManager.topLevelSelection.First() == null || SelectionManager.topLevelSelection.Last() == null)
                SelectionManager.UpdateSelection();
            if (SelectionManager.topLevelSelection.Length == 0) return;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                CreateExtrudedObjects();
        }
    }
    #endregion
}
