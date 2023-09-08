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

using UnityEngine;
using System.Linq;

namespace PluginMaster
{
    #region DATA & SETTINGS
    [System.Serializable]
    public class ReplacerSettings : CircleToolBase, IModifierTool
    {
        [SerializeField] private ModifierToolSettings _modifierTool = new ModifierToolSettings();
        public ReplacerSettings() => _modifierTool.OnDataChanged += DataChanged;
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

        [SerializeField] private bool _keepTargetSize = false;
        [SerializeField] private bool _maintainProportions = false;
        [SerializeField] private bool _outermostPrefabFilter = true;
        [SerializeField] private bool _overwriteBrushProperties = false;
        [SerializeField] private BrushSettings _brushProperties = new BrushSettings();

        public bool keepTargetSize
        {
            get => _keepTargetSize;
            set
            {
                if (_keepTargetSize == value) return;
                _keepTargetSize = value;
                DataChanged();
            }
        }
        public bool maintainProportions
        {
            get => _maintainProportions;
            set
            {
                if (_maintainProportions == value) return;
                _maintainProportions = value;
                DataChanged();
            }
        }
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

        public bool overwriteBrushProperties
        {
            get => _overwriteBrushProperties;
            set
            {
                if (_overwriteBrushProperties == value) return;
                _overwriteBrushProperties = value;
                DataChanged();
            }
        }
        public BrushSettings brushProperties => _brushProperties;
        public override void Copy(IToolSettings other)
        {
            var otherReplacer = other as ReplacerSettings;
            if (otherReplacer == null) return;
            base.Copy(other);
            _modifierTool.Copy(otherReplacer);
            _keepTargetSize = otherReplacer._keepTargetSize;
            _maintainProportions = otherReplacer._maintainProportions;
            _outermostPrefabFilter = otherReplacer._outermostPrefabFilter;
            _overwriteBrushProperties = otherReplacer._overwriteBrushProperties;
            _brushProperties.Copy(otherReplacer._brushProperties);
        }
    }

    [System.Serializable]
    public class ReplacerManager : ToolManagerBase<ReplacerSettings> { }
    #endregion

    #region PWBIO
    public static partial class PWBIO
    {
        private static System.Collections.Generic.List<GameObject> _toReplace
            = new System.Collections.Generic.List<GameObject>();
        private static System.Collections.Generic.List<Renderer> _replaceRenderers
            = new System.Collections.Generic.List<Renderer>();
        private static bool _replaceAllSelected = false;
        private static void ReplacerMouseEvents()
        {
            var settings = ReplacerManager.settings;
            if (Event.current.button == 0 && !Event.current.alt
                && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag))
            {
                Replace();
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

        private static void ReplacerDuringSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (PaletteManager.selectedBrushIdx < 0) return;
            if (_replaceAllSelected)
            {
                BrushstrokeManager.UpdateBrushstroke();
                _paintStroke.Clear();
                _toReplace.Clear();
                _replaceAllSelected = false;
                _toReplace.AddRange(SelectionManager.topLevelSelection);
                foreach (var selected in _toReplace)
                    ReplacePreview(sceneView.camera, selected.transform);
                Replace();
                return;
            }
            ReplacerMouseEvents();

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
            DrawReplacerCircle(center, mouseRay, sceneView.camera);

        }
        public static void ResetReplacer()
        {
            foreach (var renderer in _replaceRenderers)
            {
                if (renderer == null) continue;
                renderer.enabled = true;
            }
            _toReplace.Clear();
            _replaceRenderers.Clear();
            _paintStroke.Clear();
        }

        private static void ReplacePreview(Camera camera, Transform target)
        {
            if (BrushstrokeManager.brushstroke.Length == 0) return;
            var strokeItem = BrushstrokeManager.brushstroke[0];
            var prefab = strokeItem.settings.prefab;
            if (prefab == null) return;
            var itemRotation = target.rotation;
            var targetBounds = BoundsUtils.GetBoundsRecursive(target, target.rotation);
            var strokeRotation = Quaternion.Euler(strokeItem.additionalAngle);
            var scaleMult = strokeItem.scaleMultiplier;
            if (ReplacerManager.settings.overwriteBrushProperties)
            {
                var brushSettings = ReplacerManager.settings.brushProperties;
                var additonalAngle = brushSettings.addRandomRotation
                    ? brushSettings.randomEulerOffset.randomVector : brushSettings.eulerOffset;
                strokeRotation *= Quaternion.Euler(additonalAngle);
                scaleMult = brushSettings.randomScaleMultiplier
                    ? brushSettings.randomScaleMultiplierRange.randomVector : brushSettings.scaleMultiplier;
            }
            var inverseStrokeRotation = Quaternion.Inverse(strokeRotation);
            itemRotation *= strokeRotation;
            var itemBounds = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation * strokeRotation);

            if (ReplacerManager.settings.keepTargetSize)
            {
                var targetSize = targetBounds.size;
                var itemSize = itemBounds.size;
                
                if (ReplacerManager.settings.maintainProportions)
                {
                    var targetMagnitude = Mathf.Max(targetSize.x, targetSize.y, targetSize.z);
                    var itemMagnitude = Mathf.Max(itemSize.x, itemSize.y, itemSize.z);
                    scaleMult = inverseStrokeRotation * (Vector3.one * (targetMagnitude / itemMagnitude));
                }
                else scaleMult = inverseStrokeRotation
                        * new Vector3(targetSize.x / itemSize.x, targetSize.y / itemSize.y, targetSize.z / itemSize.z);
                scaleMult = new Vector3(Mathf.Abs(scaleMult.x), Mathf.Abs(scaleMult.y), Mathf.Abs(scaleMult.z));
            }
            var itemPosition = targetBounds.center
                - itemRotation * Vector3.Scale(itemBounds.center - prefab.transform.position, scaleMult);
            
            var itemScale = Vector3.Scale(prefab.transform.localScale, scaleMult);

            var layer = target.gameObject.layer;
            Transform parentTransform = target.parent;
            _paintStroke.Add(new PaintStrokeItem(prefab, itemPosition,
                itemRotation * prefab.transform.rotation,
                itemScale, layer, parentTransform, null, false, false));
            var rootToWorld = Matrix4x4.TRS(itemPosition, itemRotation, scaleMult)
                * Matrix4x4.Translate(-prefab.transform.position);
            PreviewBrushItem(prefab, rootToWorld, layer, camera, false, false, strokeItem.flipX, strokeItem.flipY);
        }

        private static void Replace()
        {
            if (_toReplace.Count == 0) return;
            if (_paintStroke.Count != _toReplace.Count) return;
            const string COMMAND_NAME = "Replace";
            foreach (var renderer in _replaceRenderers) renderer.enabled = true;
            _replaceRenderers.Clear();
            for (int i = 0; i < _toReplace.Count; ++i)
            {
                var target = _toReplace[i];
                if (target == null) continue;
                var item = _paintStroke[i];
                if (item.prefab == null) continue;
                if (ReplacerManager.settings.outermostPrefabFilter)
                {
                    var nearestRoot = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(target);
                    if (nearestRoot != null) target = nearestRoot;
                }
                else
                {
                    var parent = target.transform.parent.gameObject;
                    if (parent != null)
                    {
                        GameObject outermost = null;
                        do
                        {
                            outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(target);
                            if (outermost == null) break;
                            if (outermost == target) break;
                            UnityEditor.PrefabUtility.UnpackPrefabInstance(outermost,
                                UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.UserAction);
                        } while (outermost != parent);
                    }
                }
                var type = UnityEditor.PrefabUtility.GetPrefabAssetType(item.prefab);
                GameObject obj = type == UnityEditor.PrefabAssetType.NotAPrefab ? GameObject.Instantiate(item.prefab)
                    : (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab
                    (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(item.prefab)
                    ? item.prefab : UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(item.prefab));
                obj.transform.SetPositionAndRotation(item.position, item.rotation);
                obj.transform.localScale = item.scale;
                var root = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                PWBCore.AddTempCollider(obj);
                AddPaintedObject(obj);

                if(!LineManager.instance.ReplaceObject(target, obj))
                    if(!ShapeManager.instance.ReplaceObject(target, obj))
                        TilingManager.instance.ReplaceObject(target, obj);

                BrushstrokeManager.UpdateBrushstroke();
                var tempColliders = PWBCore.GetTempColliders(target);
                if (tempColliders != null)
                    foreach (var tempCollider in tempColliders) UnityEditor.Undo.DestroyObjectImmediate(tempCollider);
                UnityEditor.Undo.DestroyObjectImmediate(target);
                UnityEditor.Undo.RegisterCreatedObjectUndo(obj, COMMAND_NAME);
                if (root != null) UnityEditor.Undo.SetTransformParent(root.transform, item.parent, COMMAND_NAME);
                else UnityEditor.Undo.SetTransformParent(obj.transform, item.parent, COMMAND_NAME);
            }
            _paintStroke.Clear();
            _toReplace.Clear();
        }

        public static void ReplaceAllSelected()
        {
            _replaceAllSelected = true;
        }

        private static void DrawReplacerCircle(Vector3 center, Ray mouseRay, Camera camera)
        {
            var settings = ReplacerManager.settings;
            const float polygonSideSize = 0.3f;
            const int minPolygonSides = 8;
            const int maxPolygonSides = 60;
            var polygonSides = Mathf.Clamp((int)(TAU * settings.radius / polygonSideSize),
                minPolygonSides, maxPolygonSides);

            var periPoints = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < polygonSides; ++i)
            {
                var radians = TAU * i / (polygonSides - 1f);
                var tangentDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var worldDir = TangentSpaceToWorld(camera.transform.right, camera.transform.up, tangentDir);
                periPoints.Add(center + (worldDir * settings.radius));
            }
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            UnityEditor.Handles.color = new Color(0f, 0f, 0f, 1f);
            UnityEditor.Handles.DrawAAPolyLine(6, periPoints.ToArray());
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.6f);
            UnityEditor.Handles.DrawAAPolyLine(4, periPoints.ToArray());

            var nearbyObjects = octree.GetNearby(mouseRay, settings.radius);

            _toReplace.Clear();
            _paintStroke.Clear();
            if (settings.outermostPrefabFilter)
            {
                foreach (var nearby in nearbyObjects)
                {
                    var outermost = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(nearby);
                    if (outermost == null)
                    {
                        var components = nearby.GetComponents<Component>();
                        if (components.Length > 1) _toReplace.Add(nearby);
                    }
                    else if (!_toReplace.Contains(outermost)) _toReplace.Add(outermost);
                }
            }
            else _toReplace.AddRange(nearbyObjects);

            var toReplace = _toReplace.ToArray();
            _toReplace.Clear();
            var closestDistSqr = float.MaxValue;
            for (int i = 0; i < toReplace.Length; ++i)
            {
                var obj = toReplace[i];
                if (obj == null) continue;
                var magnitude = BoundsUtils.GetAverageMagnitude(obj.transform);
                if (settings.radius < magnitude / 2) continue;
                if (ReplacerManager.settings.onlyTheClosest)
                {
                    var pos = obj.transform.position;
                    var distSqr = (pos - camera.transform.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        _toReplace.Clear();
                        _toReplace.Add(obj);
                    }
                    continue;
                }
                _toReplace.Add(obj);
            }

            foreach (var renderer in _replaceRenderers)
            {
                if (renderer == null) continue;
                renderer.enabled = true;
            }
            _replaceRenderers.Clear();
            toReplace = _toReplace.ToArray();
            _toReplace.Clear();
            for (int i = 0; i < toReplace.Length; ++i)
            {
                var obj = toReplace[i];
                var isChild = false;
                foreach (var listed in toReplace)
                {
                    if (obj.transform.IsChildOf(listed.transform) && listed != obj)
                    {
                        isChild = true;
                        break;
                    }
                }
                if (isChild) continue;
                _toReplace.Add(obj);
                _replaceRenderers.AddRange(obj.GetComponentsInChildren<Renderer>().Where(r => r.enabled == true));
                ReplacePreview(camera, obj.transform);
                foreach (var renderer in _replaceRenderers) renderer.enabled = false;
                BrushstrokeManager.UpdateBrushstroke();
            }
        }
    }

    #endregion
}