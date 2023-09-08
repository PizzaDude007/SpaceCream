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
    #region DATA & SETTIGNS
    [System.Serializable]
    public class EraserSettings : CircleToolBase, IModifierTool
    {
        [SerializeField] private ModifierToolSettings _modifierTool = new ModifierToolSettings();
        public EraserSettings() => _modifierTool.OnDataChanged += DataChanged;
        public ModifierToolSettings.Command command { get => _modifierTool.command; set => _modifierTool.command = value; }

        public bool modifyAllButSelected
        {
            get => _modifierTool.modifyAllButSelected;
            set => _modifierTool.modifyAllButSelected = value;
        }

        public bool onlyTheClosest
        {
            get => _modifierTool.onlyTheClosest;
            set => _modifierTool.onlyTheClosest = value;
        }

        [SerializeField] private bool _outermostPrefabFilter = true;
        public bool outermostPrefabFilter
        {
            get => _outermostPrefabFilter;
            set
            {
                if (_outermostPrefabFilter == value) return;
                _outermostPrefabFilter = value;
                DataChanged();
            }
        }
        public override void Copy(IToolSettings other)
        {
            var otherEraserSettings = other as EraserSettings;
            if (otherEraserSettings == null) return;
            base.Copy(other);
            _modifierTool.Copy(otherEraserSettings);
            _outermostPrefabFilter = otherEraserSettings._outermostPrefabFilter;
        }
    }

    [System.Serializable]
    public class EraserManager : ToolManagerBase<EraserSettings> { }
    #endregion
    #region PWBIO
    public static partial class PWBIO
    {
        private static float _lastHitDistance = 20f;

        private static Material _transparentRedMaterial = null;
        public static Material transparentRedMaterial
        {
            get
            {
                if (_transparentRedMaterial == null)
                    _transparentRedMaterial = new Material(Shader.Find("PluginMaster/TransparentRed"));
                return _transparentRedMaterial;
            }
        }
        private static System.Collections.Generic.List<GameObject> _toErase
            = new System.Collections.Generic.List<GameObject>();


        private static void EraserMouseEvents()
        {
            if (Event.current.button == 0 && !Event.current.alt
                && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag))
            {
                Erase();
                Event.current.Use();
            }
            if (Event.current.button == 1)
            {
                if (Event.current.type == EventType.MouseDown && (Event.current.control || Event.current.shift))
                {
                    _pinned = true;
                    _pinMouse = Event.current.mousePosition;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp) _pinned = false;
            }
        }

        private static void Erase()
        {
            void EraseObject(GameObject obj)
            {
                if (EraserManager.settings.outermostPrefabFilter)
                {
                    var root = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                    if (root != null) obj = root;
                }
                else
                {
                    var parent = obj.transform.parent.gameObject;
                    if (parent != null)
                    {
                        GameObject outermost = null;
                        do
                        {
                            outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                            if (outermost == null) break;
                            if (outermost == obj) break;
                            UnityEditor.PrefabUtility.UnpackPrefabInstance(outermost,
                                UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.UserAction);
                        } while (outermost != parent);
                    }
                }
                PWBCore.DestroyTempCollider(obj.GetInstanceID());
                UnityEditor.Undo.DestroyObjectImmediate(obj);
            }
            for (int i = 0; i < _toErase.Count; ++i)
            {
                var obj = _toErase[i];
                if (obj == null) continue;
                EraseObject(obj);
            }
            _toErase.Clear();
        }

        private static void EraserDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            EraserMouseEvents();
            var mousePos = Event.current.mousePosition;
            if (_pinned) mousePos = _pinMouse;
            var mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePos);

            var center = mouseRay.GetPoint(_lastHitDistance);
            if (MouseRaycast(mouseRay, out RaycastHit mouseHit, out GameObject collider,
                float.MaxValue, -1, true, true))
            {
                _lastHitDistance = mouseHit.distance;
                center = mouseHit.point;
                PWBCore.UpdateTempCollidersIfHierarchyChanged();
            }
            DrawEraserCircle(center, mouseRay, sceneView.camera);
        }
        private static void DrawEraserCircle(Vector3 center, Ray mouseRay, Camera camera)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.6f);

            const float polygonSideSize = 0.3f;
            const int minPolygonSides = 8;
            const int maxPolygonSides = 60;
            var polygonSides = Mathf.Clamp((int)(TAU * EraserManager.settings.radius / polygonSideSize),
                minPolygonSides, maxPolygonSides);

            var periPoints = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < polygonSides; ++i)
            {
                var radians = TAU * i / (polygonSides - 1f);
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(camera.transform.right, camera.transform.up, tangentDir);
                periPoints.Add(center + (worldDir * EraserManager.settings.radius));
            }
            UnityEditor.Handles.DrawAAPolyLine(6, periPoints.ToArray());

            var nearbyObjects = octree.GetNearby(mouseRay, EraserManager.settings.radius);

            _toErase.Clear();
            if (EraserManager.settings.outermostPrefabFilter)
            {
                foreach (var nearby in nearbyObjects)
                {
                    var outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(nearby);
                    if (outermost == null) _toErase.Add(nearby);
                    else if (!_toErase.Contains(outermost)) _toErase.Add(outermost);
                }
            }
            else _toErase.AddRange(nearbyObjects);

            var toErase = _toErase.ToArray();
            _toErase.Clear();

            var closestDistSqr = float.MaxValue;
            for (int i = 0; i < toErase.Length; ++i)
            {
                var obj = toErase[i];
                if (obj == null) continue;
                var magnitude = BoundsUtils.GetAverageMagnitude(obj.transform);
                if (EraserManager.settings.radius < magnitude / 2) continue;

                if (EraserManager.settings.onlyTheClosest)
                {
                    var pos = obj.transform.position;
                    var distSqr = (pos - camera.transform.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        _toErase.Clear();
                        _toErase.Add(obj);
                    }
                    continue;
                }
                _toErase.Add(obj);
            }
            foreach (var obj in _toErase)
            {
                var filters = obj.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in filters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null) continue;
                    for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; ++subMeshIdx)
                        Graphics.DrawMesh(mesh, filter.transform.localToWorldMatrix,
                            transparentRedMaterial, 0, camera, subMeshIdx);
                }
            }
        }
    }
    #endregion
}
