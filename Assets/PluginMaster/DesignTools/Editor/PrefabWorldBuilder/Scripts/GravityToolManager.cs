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
    #region DATA & SETTINGS
    [System.Serializable]
    public class GravityToolSettings : BrushToolBase
    {
        [SerializeField] private SimulateGravityData _simData = new SimulateGravityData();
        [SerializeField] private float _height = 10f;
        [SerializeField] private bool _createTempColliders = true;
        public SimulateGravityData simData => _simData;
        public float height
        {
            get => _height;
            set
            {
                value = Mathf.Max(value, 0f);
                if (_height == value) return;
                _height = value;
            }
        }

        public bool createTempColliders
        {
            get
            {
                if (PWBCore.staticData.tempCollidersAction == PWBData.TempCollidersAction.NEVER_CREATE)
                    return false;
                return _createTempColliders;
            }
            set
            {
                if (_createTempColliders == value) return;
                _createTempColliders = value;
                DataChanged();
            }
        }
        public override void Copy(IToolSettings other)
        {
            var otherGravityToolSettings = other as GravityToolSettings;
            if (otherGravityToolSettings == null) return;
            base.Copy(other);
            _simData.Copy(otherGravityToolSettings._simData);
            _height = otherGravityToolSettings.height;
            _createTempColliders = otherGravityToolSettings.createTempColliders;
        }

        public GravityToolSettings Clone()
        {
            var clone = new GravityToolSettings();
            clone.Copy(this);
            return clone;
        }

        public GravityToolSettings() : base() => _brushShape = BrushShape.POINT;
    }

    [System.Serializable]
    public class GravityToolManager : ToolManagerBase<GravityToolSettings> 
    {
        private static float _surfaceDistanceSensitivityStatic = 1.0f;
        [SerializeField] private float _surfaceDistanceSensitivity = _surfaceDistanceSensitivityStatic;
        public static float surfaceDistanceSensitivity
        {
            get => _surfaceDistanceSensitivityStatic;
            set
            {
                value = Mathf.Clamp(value, 0f, 1f);
                if (_surfaceDistanceSensitivityStatic == value) return;
                _surfaceDistanceSensitivityStatic = value;
                PWBCore.staticData.Save();
            }
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            _surfaceDistanceSensitivity = _surfaceDistanceSensitivityStatic;
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _surfaceDistanceSensitivityStatic = _surfaceDistanceSensitivity;
        }
    }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static Mesh _gravityLinesMesh = null;
        private static Material _gravityLinesMaterial = null;
        private static readonly int OPACITY_PROP_ID = Shader.PropertyToID("_opacity");

        private static void GravityToolDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (GravityToolManager.settings.createTempColliders)
                PWBCore.CreateTempCollidersWithinFrustum(sceneView.camera);

            BrushstrokeMouseEvents(GravityToolManager.settings);

            var mousePos = Event.current.mousePosition;
            if (_pinned) mousePos = _pinMouse;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos);
            bool snappedToVertex = false;
            var closestVertexInfo = new RaycastHit();
            if (_snapToVertex)
                snappedToVertex = SnapToVertex(mouseRay, out closestVertexInfo, sceneView.in2DMode);
            if (snappedToVertex) mouseRay.origin = closestVertexInfo.point - mouseRay.direction;
            if (MouseRaycast(mouseRay, out RaycastHit hit, out GameObject c, float.MaxValue, -1, true, true))
                DrawGravityBrush(hit, sceneView.camera);
            else return;
            
            void AddHeight(float value)
            {
                GravityToolManager.settings.height += value;
                ToolProperties.RepainWindow();
            }

            if (Event.current.button == 0 && !Event.current.alt
                && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag))
            {
                var paintedObjectsDic = Paint(GravityToolManager.settings, PAINT_CMD, false);
                if (!paintedObjectsDic.ContainsKey(string.Empty)) return;
                var paintedObjects = paintedObjectsDic[string.Empty].ToArray();

                var finalPoses = GravityUtils.SimulateGravity(paintedObjects, GravityToolManager.settings.simData, false);

                for(int i = 0; i < paintedObjects.Length; ++i)
                {
                    var obj = paintedObjects[i];
                    var parent = obj.transform.parent;
                    var position = obj.transform.position;
                    var rotation = obj.transform.rotation;
                    var localScale = obj.transform.localScale;

                    var colliders = obj.GetComponentsInChildren<MeshCollider>();
                    foreach (var collider in colliders)
                    {
                        if (collider == null) continue;
                        if (UnityEditor.PrefabUtility.IsAddedComponentOverride(collider))
                            UnityEditor.PrefabUtility.RevertAddedComponent(collider,
                                UnityEditor.InteractionMode.AutomatedAction);
                    }
                    obj.transform.SetParent(parent);
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.transform.localScale = localScale;
                    PWBCore.AddTempCollider(obj, finalPoses[i]);
                }
            }
            if (PWBSettings.shortcuts.gravitySubtract1UnitFromSurfDist.Check()) AddHeight(-1f);
            else if (PWBSettings.shortcuts.gravityAdd1UnitToSurfDist.Check()) AddHeight(1f);
            else if (PWBSettings.shortcuts.gravitySubtract01UnitFromSurfDist.Check()) AddHeight(-0.1f);
            else if (PWBSettings.shortcuts.gravityAdd01UnitToSurfDist.Check()) AddHeight(0.1f);
            else if (PWBSettings.shortcuts.gravitySurfDist.Check())
            {
                var delta = Mathf.Sign(PWBSettings.shortcuts.gravitySurfDist.combination.delta)
                    * GravityToolManager.surfaceDistanceSensitivity;
                GravityToolManager.settings.height = Mathf.Max((GravityToolManager.settings.height + delta * 0.5f)
                    * (1f + delta * 0.02f), 0.05f);
                ToolProperties.RepainWindow();
            }
            if (Event.current.button == 1)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.control)
                {
                    _pinned = true;
                    _pinMouse = Event.current.mousePosition;
                    Event.current.Use();
                }
            }
        }

        private static void DrawGravityBrush(RaycastHit hit, Camera camera)
        {
            var settings = GravityToolManager.settings;

            PWBCore.UpdateTempCollidersIfHierarchyChanged();

            hit.point = SnapAndUpdateGridOrigin(hit.point, SnapManager.settings.snappingEnabled,
                true, true, false, Vector3.down);
            var tangent = GetTangent(Vector3.up);
            var bitangent = Vector3.Cross(hit.normal, tangent);

            if (settings.brushShape == BrushToolSettings.BrushShape.SQUARE)
            {
                DrawSquareIndicator(hit.point, hit.normal, settings.radius,
                    settings.height, tangent, bitangent, Vector3.up, true, true);
            }
            else
            {
                DrawCricleIndicator(hit.point, hit.normal,
                    settings.brushShape == BrushToolBase.BrushShape.POINT ? 0.1f : settings.radius,
                    settings.height, tangent, bitangent, Vector3.up, true, true);
            }

            if (_gravityLinesMesh == null)
            {
                _gravityLinesMesh = new Mesh();
                _gravityLinesMesh.SetVertices(new Vector3[] { new Vector3(-1, -1, 0), new Vector3(1, -1, 0),
                    new Vector3(1, 1, 0), new Vector3(-1, 1, 0) });
                _gravityLinesMesh.uv = new Vector2[] { new Vector2(1, 0), new Vector2(0, 0),
                    new Vector2(0, 1), new Vector2(1, 1) };
                _gravityLinesMesh.SetTriangles(new int[] { 0, 1, 2, 0, 2, 3 }, 0);
                _gravityLinesMesh.RecalculateNormals();
            }
            if (_gravityLinesMaterial == null)
                _gravityLinesMaterial = new Material(Resources.Load<Material>("Materials/GravityLines"));
            var camEulerY = Mathf.Abs(Vector3.SignedAngle(Vector3.up, camera.transform.up, camera.transform.forward));
            var gravityLinesOpacity = 1f - Mathf.Min((camEulerY > 90f ? 180f - camEulerY : camEulerY) / 60f, 1f);
            _gravityLinesMaterial.SetFloat(OPACITY_PROP_ID, gravityLinesOpacity);

            var hitToCamXZ = camera.transform.position - hit.point;
            hitToCamXZ.y = 0f;
            var gravityLinesRotation = Quaternion.AngleAxis(camera.transform.eulerAngles.y, Vector3.up);
            var radius = settings.brushShape == BrushToolBase.BrushShape.POINT
                ? 0.5F : settings.radius;
            var gravityLinesMatrix = Matrix4x4.TRS(hit.point + new Vector3(0f, 3f * radius, 0f),
                gravityLinesRotation, new Vector3(0.5f, 2f, 1f) * radius);
            Graphics.DrawMesh(_gravityLinesMesh, gravityLinesMatrix, _gravityLinesMaterial, 0, camera);

            Transform surface = null;
            if(hit.collider != null) surface = hit.collider.transform;

            GravityStrokePreview(hit.point + new Vector3(0f, settings.height, 0f), tangent,
                bitangent, camera, surface);
        }

        private static void GravityStrokePreview(Vector3 center, Vector3 tangent,
            Vector3 bitangent, Camera camera, Transform surface)
        {
            _paintStroke.Clear();

            foreach (var strokeItem in BrushstrokeManager.brushstroke)
            {
                var prefab = strokeItem.settings.prefab;
                if (prefab == null) continue;
                var h = strokeItem.settings.bottomMagnitude;
                BrushSettings brushSettings = strokeItem.settings;
                if (GravityToolManager.settings.overwriteBrushProperties)
                    brushSettings = GravityToolManager.settings.brushSettings;
                var strokePosition = TangentSpaceToWorld(tangent, bitangent,
                   new Vector2(strokeItem.tangentPosition.x, strokeItem.tangentPosition.y));
                var itemPosition = strokePosition + center + new Vector3(0f, h * strokeItem.scaleMultiplier.y, 0f);

                var itemRotation = Quaternion.AngleAxis(_brushAngle, Vector3.up)
                    * Quaternion.Euler(strokeItem.additionalAngle);
                if (GravityToolManager.settings.orientAlongBrushstroke)
                {
                    itemRotation = Quaternion.LookRotation(_strokeDirection, Vector3.up);
                    itemPosition = center + itemRotation * strokePosition;
                }
                itemPosition += itemRotation * brushSettings.localPositionOffset;


                var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, strokeItem.scaleMultiplier)
                    * Matrix4x4.Translate(-prefab.transform.position);
                var itemScale = Vector3.Scale(prefab.transform.localScale, strokeItem.scaleMultiplier);
                var layer = GravityToolManager.settings.overwritePrefabLayer
                    ? GravityToolManager.settings.layer : prefab.layer;
                Transform parentTransform = GetParent(GravityToolManager.settings, prefab.name, false, surface);
                _paintStroke.Add(new PaintStrokeItem(prefab, itemPosition,
                    itemRotation * Quaternion.Euler(prefab.transform.eulerAngles),
                    itemScale, layer, parentTransform, surface, strokeItem.flipX, strokeItem.flipY));
                PreviewBrushItem(prefab, rootToWorld, layer, camera,false, false, strokeItem.flipX, strokeItem.flipY);
            }
        }

        private static Vector3 GetObjectHalfSize(Transform transform)
        {
            var size = new Vector3(0.1f, 0.1f, 0.1f);
            var childrenRenderer = transform.GetComponentsInChildren<Renderer>();
            foreach (var child in childrenRenderer) size = Vector3.Max(size, child.bounds.size);
            return size / 2f;
        }
    }
    #endregion
}
