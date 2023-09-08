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
    public class BrushstrokeItem : System.IEquatable<BrushstrokeItem>
    {
        public readonly MultibrushItemSettings settings = null;
        public Vector3 tangentPosition = Vector3.zero;
        public readonly Vector3 additionalAngle = Vector3.zero;
        public readonly Vector3 scaleMultiplier = Vector3.zero;
        public Vector3 nextTangentPosition = Vector3.zero;
        public readonly bool flipX = false;
        public readonly bool flipY = false;
        public readonly float surfaceDistance = 0f;

        public BrushstrokeItem(MultibrushItemSettings settings, Vector3 tangentPosition,
            Vector3 additionalAngle, Vector3 scaleMultiplier, bool flipX, bool flipY, float surfaceDistance)
        {
            this.settings = settings;
            this.tangentPosition = tangentPosition;
            this.additionalAngle = additionalAngle;
            this.scaleMultiplier = scaleMultiplier;
            nextTangentPosition = tangentPosition;
            this.flipX = flipX;
            this.flipY = flipY;
            this.surfaceDistance = surfaceDistance;
        }

        public BrushstrokeItem Clone()
        {
            var clone = new BrushstrokeItem(settings, tangentPosition, additionalAngle,
                scaleMultiplier, flipX, flipY, surfaceDistance);
            clone.nextTangentPosition = nextTangentPosition;
            return clone;
        }

        public bool Equals(BrushstrokeItem other)
        {
            return settings == other.settings && tangentPosition == other.tangentPosition
                && additionalAngle == other.additionalAngle && scaleMultiplier == other.scaleMultiplier
                && nextTangentPosition == other.nextTangentPosition;
        }
        public static bool operator ==(BrushstrokeItem lhs, BrushstrokeItem rhs) => lhs.Equals(rhs);
        public static bool operator !=(BrushstrokeItem lhs, BrushstrokeItem rhs) => !lhs.Equals(rhs);

        public override bool Equals(object obj) => obj is BrushstrokeItem other && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = 861157388;
            hashCode = hashCode * -1521134295
                + System.Collections.Generic.EqualityComparer<MultibrushItemSettings>.Default.GetHashCode(settings);
            hashCode = hashCode * -1521134295 + tangentPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + additionalAngle.GetHashCode();
            hashCode = hashCode * -1521134295 + scaleMultiplier.GetHashCode();
            hashCode = hashCode * -1521134295 + nextTangentPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + flipX.GetHashCode();
            hashCode = hashCode * -1521134295 + flipY.GetHashCode();
            hashCode = hashCode * -1521134295 + surfaceDistance.GetHashCode();
            return hashCode;
        }
    }

    public static class BrushstrokeManager
    {
        private static System.Collections.Generic.List<BrushstrokeItem> _brushstroke
            = new System.Collections.Generic.List<BrushstrokeItem>();
        public static BrushstrokeItem[] brushstroke => _brushstroke.ToArray();

        public static void ClearBrushstroke() => _brushstroke.Clear();

        public static BrushstrokeItem[] brushstrokeClone
        {
            get
            {
                var clone = new BrushstrokeItem[_brushstroke.Count];
                for (int i = 0; i < clone.Length; ++i) clone[i] = _brushstroke[i].Clone();
                return clone;
            }
        }

        public static bool BrushstrokeEqual(BrushstrokeItem[] lhs, BrushstrokeItem[] rhs)
        {
            if (lhs.Length != rhs.Length) return false;
            for (int i = 0; i < lhs.Length; ++i)
                if (lhs[i] != rhs[i]) return false;
            return true;
        }
        private static void AddBrushstrokeItem(int index, Vector3 tangentPosition, Vector3 angle,
            IPaintToolSettings paintToolSettings)
        {
            if (index < 0 || index >= PaletteManager.selectedBrush.itemCount) return;

            BrushSettings brushSettings = PaletteManager.selectedBrush.items[index];
            if (paintToolSettings != null && paintToolSettings.overwriteBrushProperties)
                brushSettings = paintToolSettings.brushSettings;

            var additonalAngle = angle;
            if (brushSettings.addRandomRotation)
            {
                var randomAngle = brushSettings.randomEulerOffset.randomVector;
                if (brushSettings.rotateInMultiples)
                {
                    randomAngle = new Vector3(
                        Mathf.Round(randomAngle.x / brushSettings.rotationFactor) * brushSettings.rotationFactor,
                        Mathf.Round(randomAngle.y / brushSettings.rotationFactor) * brushSettings.rotationFactor,
                        Mathf.Round(randomAngle.z / brushSettings.rotationFactor) * brushSettings.rotationFactor);
                }
                additonalAngle += randomAngle;
            }
            else additonalAngle += brushSettings.eulerOffset;
            var scale = brushSettings.randomScaleMultiplier
                ? brushSettings.randomScaleMultiplierRange.randomVector : brushSettings.scaleMultiplier;
            if (!brushSettings.separateScaleAxes) scale.z = scale.y = scale.x;
            var flipX = brushSettings.flipX == BrushSettings.FlipAction.NONE ? false
                : brushSettings.flipX == BrushSettings.FlipAction.FLIP ? true : Random.value > 0.5;
            var flipY = brushSettings.flipY == BrushSettings.FlipAction.NONE ? false
               : brushSettings.flipY == BrushSettings.FlipAction.FLIP ? true : Random.value > 0.5;
            var surfaceDistance = brushSettings.randomSurfaceDistance
                ? brushSettings.randomSurfaceDistanceRange.randomValue : brushSettings.surfaceDistance;
            var strokeItem = new BrushstrokeItem(PaletteManager.selectedBrush.items[index],
                tangentPosition, additonalAngle, scale, flipX, flipY, surfaceDistance);
            if (_brushstroke.Count > 0) _brushstroke.Last().nextTangentPosition = tangentPosition;
            _brushstroke.Add(strokeItem);
        }

        public static float GetLineSpacing(int itemIdx, LineSettings settings)
        {
            float spacing = 0;
            if (itemIdx >= 0) spacing = settings.spacing;

            if (settings.spacingType == LineSettings.SpacingType.BOUNDS && itemIdx >= 0)
            {
                var item = PaletteManager.selectedBrush.items[itemIdx];
                if (item.prefab == null) return spacing;
                var bounds = BoundsUtils.GetBoundsRecursive(item.prefab.transform);
                var scale = item.scaleMultiplier;

                if (settings is ShapeSettings && ShapeManager.settings.overwriteBrushProperties)
                    scale = ShapeManager.settings.brushSettings.scaleMultiplier;
                else if (!(settings is ShapeSettings) && LineManager.settings.overwriteBrushProperties)
                    scale = LineManager.settings.brushSettings.scaleMultiplier;

                var size = Vector3.Scale(bounds.size, scale);
                var axis = settings.axisOrientedAlongTheLine;
                if (item.isAsset2D && UnityEditor.SceneView.currentDrawingSceneView.in2DMode
                    && axis == AxesUtils.Axis.Z) axis = AxesUtils.Axis.Y;
                spacing = AxesUtils.GetAxisValue(size, axis);
                if (spacing <= 0.0001) spacing = 0.5f;
            }
            spacing += settings.gapSize;
            return spacing;
        }
        private static void UpdateLineBrushstroke(Vector3[] points, LineSettings settings)
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;

            float lineLength = 0f;
            var lengthFromFirstPoint = new float[points.Length];
            var segmentLength = new float[points.Length];
            lengthFromFirstPoint[0] = 0f;
            for (int i = 1; i < points.Length; ++i)
            {
                segmentLength[i - 1] = (points[i] - points[i - 1]).magnitude;
                lineLength += segmentLength[i - 1];
                lengthFromFirstPoint[i] = lineLength;
            }

            float length = 0f;
            int segment = 0;
            float minSpace = lineLength / 1024f;
            if (PaletteManager.selectedBrush.patternMachine != null)
                PaletteManager.selectedBrush.patternMachine.Reset();

            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<int, float>();
            do
            {
                var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                while (lengthFromFirstPoint[segment + 1] < length)
                {
                    ++segment;
                    if (segment >= points.Length - 1) break;
                }
                if (segment >= points.Length - 1) break;
                var segmentDirection = (points[segment + 1] - points[segment]).normalized;
                var distance = length - lengthFromFirstPoint[segment];
                var position = points[segment] + segmentDirection * distance;
                float spacing = minSpace;
                if (settings.spacingType == LineSettings.SpacingType.BOUNDS)
                {
                    if (prefabSpacingDictionary.ContainsKey(nextIdx)) spacing = prefabSpacingDictionary[nextIdx];
                    else
                    {
                        spacing = GetLineSpacing(nextIdx, settings);
                        prefabSpacingDictionary.Add(nextIdx, spacing);
                    }
                }
                else spacing = GetLineSpacing(nextIdx, settings);

                var delta = Mathf.Max(spacing, minSpace);
                if (delta <= 0) break;
                length += Mathf.Max(spacing, minSpace);
                if (length > lineLength) break;
                AddBrushstrokeItem(nextIdx, position, Vector3.zero, settings);
            } while (length < lineLength);
        }

        public static void UpdateLineBrushstroke(Vector3[] pathPoints)
            => UpdateLineBrushstroke(pathPoints, LineManager.settings);


        private static float GetLineSpacing(Transform transform, LineSettings settings)
        {
            float spacing = settings.spacing;
            if (settings.spacingType == LineSettings.SpacingType.BOUNDS && transform != null)
            {
                var bounds = BoundsUtils.GetBoundsRecursive(transform, transform.rotation, false);

                var size = bounds.size;
                var axis = settings.axisOrientedAlongTheLine;
                if (Utils2D.Is2DAsset(transform.gameObject) && UnityEditor.SceneView.currentDrawingSceneView != null
                    && UnityEditor.SceneView.currentDrawingSceneView.in2DMode && axis == AxesUtils.Axis.Z)
                    axis = AxesUtils.Axis.Y;
                spacing = AxesUtils.GetAxisValue(size, axis);
                if (spacing <= 0.0001) spacing = 0.5f;
            }
            spacing += settings.gapSize;
            return spacing;
        }

        public static void UpdatePersistentLineBrushstroke(Vector3[] pathPoints,
            LineSettings settings, System.Collections.Generic.List<GameObject> lineObjects,
            out Vector3[] objPositions, out Vector3[] strokePositions)
        {
            _brushstroke.Clear();
            var objPositionsList = new System.Collections.Generic.List<Vector3>();
            var strokePositionsList = new System.Collections.Generic.List<Vector3>();
            float lineLength = 0f;
            var lengthFromFirstPoint = new float[pathPoints.Length];
            var segmentLength = new float[pathPoints.Length];
            lengthFromFirstPoint[0] = 0f;
            for (int i = 1; i < pathPoints.Length; ++i)
            {
                segmentLength[i - 1] = (pathPoints[i] - pathPoints[i - 1]).magnitude;
                lineLength += segmentLength[i - 1];
                lengthFromFirstPoint[i] = lineLength;
            }

            float length = 0f;
            int segment = 0;
            float minSpace = lineLength / 1024f;
            if (PaletteManager.selectedBrush != null)
                if (PaletteManager.selectedBrush.patternMachine != null)
                    PaletteManager.selectedBrush.patternMachine.Reset();
            int objIdx = 0;
            const float THRESHOLD = 0.0001f;
            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<int, float>();
            do
            {
                var nextIdx = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.nextItemIndex : -1;

                while (lengthFromFirstPoint[segment + 1] < length)
                {
                    ++segment;
                    if (segment >= pathPoints.Length - 1) break;
                }
                if (segment >= pathPoints.Length - 1) break;

                var segmentDirection = (pathPoints[segment + 1] - pathPoints[segment]).normalized;
                var distance = length - lengthFromFirstPoint[segment];

                var position = pathPoints[segment] + segmentDirection * distance;

                var objectExist = objIdx < lineObjects.Count;
                float spacing = 0;
                if (objectExist) spacing = GetLineSpacing(lineObjects[objIdx].transform, settings);
                else if (settings.spacingType == LineSettings.SpacingType.BOUNDS && nextIdx >= 0)
                {
                    if (prefabSpacingDictionary.ContainsKey(nextIdx))
                        spacing = prefabSpacingDictionary[nextIdx];
                    else
                    {
                        spacing = GetLineSpacing(nextIdx, settings);
                        prefabSpacingDictionary.Add(nextIdx, spacing);
                    }
                }
                else spacing = GetLineSpacing(nextIdx, settings);
                if (spacing == 0) break;
                spacing = Mathf.Max(spacing, minSpace);
                int nearestPathointIdx;
                var intersection = LineData.NearestPathPoint(position, spacing, pathPoints, out nearestPathointIdx);
                if (nearestPathointIdx > segment)
                    spacing = (pathPoints[nearestPathointIdx] - position).magnitude
                        + (intersection - pathPoints[nearestPathointIdx]).magnitude;
                length += spacing;
                if (lineLength - length < THRESHOLD) break;
                if (objectExist)
                {
                    ++objIdx;
                    objPositionsList.Add(position);
                }
                else if (PaletteManager.selectedBrush == null) break;
                else
                {
                    AddBrushstrokeItem(nextIdx, position, Vector3.zero, LineManager.settings);
                    strokePositionsList.Add(position);
                }

            } while (lineLength - length > THRESHOLD);
            objPositions = objPositionsList.ToArray();
            strokePositions = strokePositionsList.ToArray();
        }

        public static void UpdateShapeBrushstroke()
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            if (ShapeData.instance.state < ToolManager.ToolState.EDIT) return;
            var settings = ShapeManager.settings;
            var points = new System.Collections.Generic.List<Vector3>();
            var firstVertexIdx = ShapeData.instance.firstVertexIdxAfterIntersection;
            var lastVertexIdx = ShapeData.instance.lastVertexIdxBeforeIntersection;
            int sidesCount = settings.shapeType == ShapeSettings.ShapeType.POLYGON ? settings.sidesCount
                : ShapeData.instance.circleSideCount;
            int GetNextVertexIdx(int currentIdx) => currentIdx == sidesCount ? 1 : currentIdx + 1;
            int GetPrevVertexIdx(int currentIdx) => currentIdx == 1 ? sidesCount : currentIdx - 1;
            var firstPrev = GetPrevVertexIdx(firstVertexIdx);
            points.Add(ShapeData.instance.GetArcIntersection(0));
            if (lastVertexIdx != firstPrev || (lastVertexIdx == firstPrev && ShapeData.instance.arcAngle > 120))
            {
                var vertexIdx = firstVertexIdx;
                var nextVertexIdx = firstVertexIdx;
                do
                {
                    vertexIdx = nextVertexIdx;
                    points.Add(ShapeData.instance.GetPoint(vertexIdx));
                    nextVertexIdx = GetNextVertexIdx(nextVertexIdx);
                } while (vertexIdx != lastVertexIdx);
            }
            var lastPoint = ShapeData.instance.GetArcIntersection(1);
            if (points.Last() != lastPoint) points.Add(lastPoint);

            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<int, float>();
            void AddItemsToLine(Vector3 start, Vector3 end, ref int nextIdx)
            {
                if (nextIdx < 0) nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                var startToEnd = end - start;
                var lineLength = startToEnd.magnitude;
                float itemsSize = 0f;
                var items = new System.Collections.Generic.List<(int idx, float size)>();
                var minspacing = (lineLength * points.Count) / 1024f;
                do
                {
                    float itemSize;
                    if (prefabSpacingDictionary.ContainsKey(nextIdx)) itemSize = prefabSpacingDictionary[nextIdx];
                    else
                    {
                        itemSize = GetLineSpacing(nextIdx, settings);
                        prefabSpacingDictionary.Add(nextIdx, itemSize);
                    }
                    itemSize = Mathf.Max(itemSize, minspacing);
                    if (itemsSize + itemSize > lineLength) break;
                    itemsSize += itemSize;
                    items.Add((nextIdx, itemSize));
                    nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                } while (itemsSize < lineLength);
                var spacing = (lineLength - itemsSize) / (items.Count + 1);
                var distance = spacing;
                var direction = startToEnd.normalized;

                Vector3 itemDir = (settings.objectsOrientedAlongTheLine && direction != Vector3.zero)
                    ? direction : Vector3.forward;
                if (!settings.perpendicularToTheSurface)
                    itemDir = Vector3.ProjectOnPlane(itemDir, settings.projectionDirection);
                if (itemDir == Vector3.zero) itemDir = settings.projectionDirection;
                var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine), Vector3.up);
                var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                var angle = segmentRotation.eulerAngles;
                foreach (var item in items)
                {
                    var brushItem = PaletteManager.selectedBrush.items[item.idx];
                    if (brushItem.prefab == null) continue;
                    var position = start + direction * (distance + item.size / 2);
                    AddBrushstrokeItem(item.idx, position, angle, settings);
                    distance += item.size + spacing;
                }
            }
            int nexItemItemIdx = -1;

            if (ShapeManager.settings.shapeType == ShapeSettings.ShapeType.CIRCLE)
            {
                const float TAU = 2 * Mathf.PI;
                var perimeter = TAU * ShapeData.instance.radius;
                var items = new System.Collections.Generic.List<(int idx, float size)>();
                var minspacing = perimeter / 1024f;
                float itemsSize = 0f;

                var firstLocalArcIntersection = Quaternion.Inverse(ShapeData.instance.planeRotation)
                    * (ShapeData.instance.GetArcIntersection(0) - ShapeData.instance.center);
                var firstLocalAngle = Mathf.Atan2(firstLocalArcIntersection.z, firstLocalArcIntersection.x);
                if (firstLocalAngle < 0) firstLocalAngle += TAU;
                var secondLocalArcIntersection = Quaternion.Inverse(ShapeData.instance.planeRotation)
                   * (ShapeData.instance.GetArcIntersection(1) - ShapeData.instance.center);
                var secondLocalAngle = Mathf.Atan2(secondLocalArcIntersection.z, secondLocalArcIntersection.x);
                if (secondLocalAngle < 0) secondLocalAngle += TAU;
                if (secondLocalAngle <= firstLocalAngle) secondLocalAngle += TAU;
                var arcDelta = secondLocalAngle - firstLocalAngle;
                var arcPerimeter = arcDelta / TAU * perimeter;
                if (PaletteManager.selectedBrush.patternMachine != null &&
                    PaletteManager.selectedBrush.restartPatternForEachStroke)
                    PaletteManager.selectedBrush.patternMachine.Reset();
                do
                {
                    float itemSize;
                    var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                    if (prefabSpacingDictionary.ContainsKey(nextIdx)) itemSize = prefabSpacingDictionary[nextIdx];
                    else
                    {
                        itemSize = GetLineSpacing(nextIdx, settings);
                        prefabSpacingDictionary.Add(nextIdx, itemSize);
                    }
                    itemSize = Mathf.Max(itemSize, minspacing);
                    if (itemsSize + itemSize > arcPerimeter) break;
                    itemsSize += itemSize;
                    items.Add((nextIdx, itemSize));
                } while (itemsSize < arcPerimeter);

                var spacing = (arcPerimeter - itemsSize) / (items.Count);

                if (items.Count == 0) return;
                var distance = firstLocalAngle / TAU * perimeter + items[0].size / 2;

                for (int i = 0; i < items.Count; ++i)
                {
                    var item = items[i];
                    var arcAngle = distance / perimeter * TAU;
                    var LocalRadiusVector = new Vector3(Mathf.Cos(arcAngle), 0f, Mathf.Sin(arcAngle))
                        * ShapeData.instance.radius;
                    var radiusVector = ShapeData.instance.planeRotation * LocalRadiusVector;
                    var position = radiusVector + ShapeData.instance.center;
                    var itemDir = settings.objectsOrientedAlongTheLine
                        ? Vector3.Cross(ShapeData.instance.planeRotation * Vector3.up, radiusVector) : Vector3.forward;
                    if (!settings.perpendicularToTheSurface)
                        itemDir = Vector3.ProjectOnPlane(itemDir, settings.projectionDirection);
                    if (itemDir == Vector3.zero) itemDir = settings.projectionDirection;
                    var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine),
                        Vector3.up);
                    var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                    var angle = segmentRotation.eulerAngles;
                    AddBrushstrokeItem(item.idx, position, angle, settings);
                    var nextItem = items[(i + 1) % items.Count];
                    distance += item.size / 2 + nextItem.size / 2 + spacing;
                }
            }
            else
            {
                if (PaletteManager.selectedBrush.patternMachine != null &&
                    PaletteManager.selectedBrush.restartPatternForEachStroke)
                    PaletteManager.selectedBrush.patternMachine.Reset();
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    var start = points[i];
                    var end = points[i + 1];
                    AddItemsToLine(start, end, ref nexItemItemIdx);
                }
            }
        }

        public static void UpdatePersistentShapeBrushstroke(ShapeData data,
            System.Collections.Generic.List<GameObject> shapeObjects,
            out Pose[] objPoses)
        {
            _brushstroke.Clear();
            var objPosesList = new System.Collections.Generic.List<Pose>();
            var settings = data.settings;
            var points = new System.Collections.Generic.List<Vector3>();
            var firstVertexIdx = data.firstVertexIdxAfterIntersection;
            var lastVertexIdx = data.lastVertexIdxBeforeIntersection;
            int sidesCount = settings.shapeType == ShapeSettings.ShapeType.POLYGON ? settings.sidesCount
                : data.circleSideCount;
            int GetNextVertexIdx(int currentIdx) => currentIdx == sidesCount ? 1 : currentIdx + 1;
            int GetPrevVertexIdx(int currentIdx) => currentIdx == 1 ? sidesCount : currentIdx - 1;
            var firstPrev = GetPrevVertexIdx(firstVertexIdx);
            points.Add(data.GetArcIntersection(0));
            if (lastVertexIdx != firstPrev || (lastVertexIdx == firstPrev && data.arcAngle > 120))
            {
                var vertexIdx = firstVertexIdx;
                var nextVertexIdx = firstVertexIdx;
                do
                {
                    vertexIdx = nextVertexIdx;
                    points.Add(data.GetPoint(vertexIdx));
                    nextVertexIdx = GetNextVertexIdx(nextVertexIdx);
                } while (vertexIdx != lastVertexIdx);
            }
            var lastPoint = data.GetArcIntersection(1);
            if (points.Last() != lastPoint) points.Add(lastPoint);
            var prefabSpacingDictionary = new System.Collections.Generic.Dictionary<int, float>();
            int nextItemIdx = -1;
            int firstObjInSegmentIdx = 0;
            void AddItemsToLine(Vector3 start, Vector3 end)
            {
                int GetNextIdx() => PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.nextItemIndex : -1;
                if (nextItemIdx < 0) nextItemIdx = GetNextIdx();
                var startToEnd = end - start;
                var lineLength = startToEnd.magnitude;

                float itemsSize = 0f;
                var items = new System.Collections.Generic.List<(int idx, float size, bool objExist)>();
                var minspacing = (lineLength * points.Count) / 1024f;
                int objSegmentIdx = 0;
                var objIdx = firstObjInSegmentIdx + objSegmentIdx;

                do
                {
                    float itemSize;
                    var objectExist = objIdx < shapeObjects.Count;
                    if (objectExist)
                        itemSize = GetLineSpacing(shapeObjects[objIdx].transform, settings);
                    else if (prefabSpacingDictionary.ContainsKey(nextItemIdx)) itemSize = prefabSpacingDictionary[nextItemIdx];
                    else
                    {
                        itemSize = GetLineSpacing(nextItemIdx, settings);
                        prefabSpacingDictionary.Add(nextItemIdx, itemSize);
                    }
                    itemSize = Mathf.Max(itemSize, minspacing);
                    if (itemsSize + itemSize > lineLength) break;
                    itemsSize += itemSize;
                    items.Add((objectExist ? objIdx : nextItemIdx, itemSize, objectExist));
                    nextItemIdx = GetNextIdx();
                    if (objectExist) ++objIdx;
                } while (itemsSize < lineLength);


                var spacing = (lineLength - itemsSize) / (items.Count + 1);
                var distance = spacing;
                var direction = startToEnd.normalized;
                Vector3 itemDir = (settings.objectsOrientedAlongTheLine && direction != Vector3.zero)
                    ? direction : Vector3.forward;
                if (!settings.perpendicularToTheSurface)
                    itemDir = Vector3.ProjectOnPlane(itemDir, settings.projectionDirection);
                var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine), Vector3.up);
                var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                var angle = segmentRotation.eulerAngles;
                foreach (var item in items)
                {
                    var obj = item.objExist ? shapeObjects[item.idx]
                        : PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.items[item.idx].prefab : null;
                    if (obj == null) continue;
                    var position = start + direction * (distance + item.size / 2);
                    if (item.objExist) objPosesList.Add(new Pose(position, segmentRotation));
                    else AddBrushstrokeItem(item.idx, position, angle, settings);
                    distance += item.size + spacing;
                    ++firstObjInSegmentIdx;
                }
            }
            objPoses = objPosesList.ToArray();

            if (settings.shapeType == ShapeSettings.ShapeType.CIRCLE)
            {
                const float TAU = 2 * Mathf.PI;
                var perimeter = TAU * data.radius;
                var items = new System.Collections.Generic.List<(int idx, float size, bool objExist)>();
                var minspacing = perimeter / 1024f;
                float itemsSize = 0f;

                var firstLocalArcIntersection = Quaternion.Inverse(data.planeRotation)
                    * (data.GetArcIntersection(0) - data.center);
                var firstLocalAngle = Mathf.Atan2(firstLocalArcIntersection.z, firstLocalArcIntersection.x);
                if (firstLocalAngle < 0) firstLocalAngle += TAU;
                var secondLocalArcIntersection = Quaternion.Inverse(data.planeRotation)
                   * (data.GetArcIntersection(1) - data.center);
                var secondLocalAngle = Mathf.Atan2(secondLocalArcIntersection.z, secondLocalArcIntersection.x);
                if (secondLocalAngle < 0) secondLocalAngle += TAU;
                if (secondLocalAngle <= firstLocalAngle) secondLocalAngle += TAU;
                var arcDelta = secondLocalAngle - firstLocalAngle;
                var arcPerimeter = arcDelta / TAU * perimeter;

                var objIdx = 0;
                do
                {
                    float itemSize;
                    var objectExist = objIdx < shapeObjects.Count;
                    var nextIdx = PaletteManager.selectedBrush != null ? PaletteManager.selectedBrush.nextItemIndex : -1;
                    if (objectExist)
                        itemSize = GetLineSpacing(shapeObjects[objIdx].transform, settings);
                    else if (prefabSpacingDictionary.ContainsKey(nextIdx)) itemSize = prefabSpacingDictionary[nextIdx];
                    else
                    {
                        itemSize = GetLineSpacing(nextIdx, settings);
                        prefabSpacingDictionary.Add(nextIdx, itemSize);
                    }
                    itemSize = Mathf.Max(itemSize, minspacing);
                    if (itemsSize + itemSize > arcPerimeter) break;
                    itemsSize += itemSize;
                    items.Add((objectExist ? objIdx : nextIdx, itemSize, objectExist));
                    if (objectExist) ++objIdx;
                } while (itemsSize < arcPerimeter);
                var spacing = (arcPerimeter - itemsSize) / (items.Count + 1);

                if (items.Count == 0)
                {
                    return;
                }
                var distance = firstLocalAngle / TAU * perimeter + items[0].size / 2;
                foreach (var item in items)
                {
                    var obj = item.objExist ? shapeObjects[item.idx] : PaletteManager.selectedBrush.items[item.idx].prefab;
                    if (obj == null) continue;

                    var arcAngle = distance / perimeter * TAU;
                    var LocalRadiusVector = new Vector3(Mathf.Cos(arcAngle), 0f, Mathf.Sin(arcAngle))
                        * data.radius;
                    var radiusVector = data.planeRotation * LocalRadiusVector;
                    var position = radiusVector + data.center;
                    var itemDir = settings.objectsOrientedAlongTheLine
                        ? Vector3.Cross(data.planeRotation * Vector3.up, radiusVector) : Vector3.forward;
                    if (!settings.perpendicularToTheSurface)
                        itemDir = Vector3.ProjectOnPlane(itemDir, settings.projectionDirection);
                    if (itemDir == Vector3.zero) itemDir = settings.projectionDirection;
                    var lookAt = Quaternion.LookRotation((AxesUtils.SignedAxis)(settings.axisOrientedAlongTheLine),
                        Vector3.up);
                    var segmentRotation = Quaternion.LookRotation(itemDir, -settings.projectionDirection) * lookAt;
                    var angle = segmentRotation.eulerAngles;
                    if (item.objExist) objPosesList.Add(new Pose(position, segmentRotation));
                    else AddBrushstrokeItem(item.idx, position, angle, ShapeManager.settings);
                    distance += item.size + spacing;
                }
            }
            else
            {
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    var start = points[i];
                    var end = points[i + 1];
                    AddItemsToLine(start, end);
                }
            }
            objPoses = objPosesList.ToArray();
        }
        public static void UpdateTilingBrushstroke(Vector3[] cellCenters)
        {
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            for (int i = 0; i < cellCenters.Length; ++i)
            {
                var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                AddBrushstrokeItem(nextIdx, cellCenters[i], Vector3.zero, TilingManager.settings);
            }
        }

        public static void UpdatePersistentTilingBrushstroke(Vector3[] cellCenters, TilingSettings settings,
            System.Collections.Generic.List<GameObject> tilingObjects,
            out Vector3[] objPositions, out Vector3[] strokePositions)
        {
            _brushstroke.Clear();
            var objPositionsList = new System.Collections.Generic.List<Vector3>();
            var strokePositionsList = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < cellCenters.Length; ++i)
            {
                var objectExist = i < tilingObjects.Count;
                var position = cellCenters[i];
                if (objectExist) objPositionsList.Add(position);
                else
                {
                    if (PaletteManager.selectedBrush == null) break;
                    var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                    AddBrushstrokeItem(nextIdx, position, Vector3.zero, settings);
                    strokePositionsList.Add(position);
                }
            }
            objPositions = objPositionsList.ToArray();
            strokePositions = strokePositionsList.ToArray();
        }

        private static int _currentPinIdx = 0;
        public static void SetNextPinBrushstroke(int delta)
        {
            _currentPinIdx = _currentPinIdx + delta;
            var mod = _currentPinIdx % PaletteManager.selectedBrush.itemCount;
            _currentPinIdx = mod < 0 ? PaletteManager.selectedBrush.itemCount + mod : mod;
            _brushstroke.Clear();
            AddBrushstrokeItem(_currentPinIdx, Vector3.zero, Vector3.zero, PinManager.settings);
        }

        private static void UpdateBrushBaseStroke(BrushToolBase brushSettings)
        {
            if (brushSettings.spacingType == BrushToolBase.SpacingType.AUTO)
            {
                var maxSize = 0.1f;
                foreach (var item in PaletteManager.selectedBrush.items)
                {
                    if (item.prefab == null) continue;
                    var itemSize = BoundsUtils.GetBoundsRecursive(item.prefab.transform).size;
                    itemSize = Vector3.Scale(itemSize,
                        item.randomScaleMultiplier ? item.maxScaleMultiplier : item.scaleMultiplier);
                    maxSize = Mathf.Max(itemSize.x, itemSize.z, maxSize);
                }
                brushSettings.minSpacing = maxSize;
                ToolProperties.RepainWindow();
            }

            if (brushSettings.brushShape == BrushToolSettings.BrushShape.POINT)
            {
                var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                if (nextIdx == -1) return;
                if (PaletteManager.selectedBrush.frequencyMode == PluginMaster.MultibrushSettings.FrecuencyMode.PATTERN
                    && nextIdx == -2) return;
                _brushstroke.Clear();

                AddBrushstrokeItem(nextIdx, Vector3.zero, Vector3.zero, brushSettings);
                _currentPinIdx = Mathf.Clamp(nextIdx, 0, PaletteManager.selectedBrush.itemCount - 1);
            }
            else
            {
                var radius = brushSettings.radius;
                var radiusSqr = radius * radius;

                var minSpacing = brushSettings.minSpacing * 100f / brushSettings.density;
                if (brushSettings.randomizePositions)
                    minSpacing *= Mathf.Max(1 - (Random.value * brushSettings.randomness), 0.5f);

                var delta = minSpacing;
                var maxRandomOffset = delta * brushSettings.randomness;

                int halfSize = (int)Mathf.Ceil(radius / delta) + 1;
                const int MAX_SIZE = 32;
                if (halfSize > MAX_SIZE)
                {
                    halfSize = MAX_SIZE;
                    delta = radius / MAX_SIZE;
                    minSpacing = delta;
                    maxRandomOffset = delta * brushSettings.randomness;
                }
                int size = halfSize * 2;
                float col0x = -delta * halfSize;
                float row0y = -delta * halfSize;

                var takedCells = new System.Collections.Generic.List<(int row, int col)>();

                for (int row = 0; row < size; ++row)
                {
                    for (int col = 0; col < size; ++col)
                    {
                        var x = col0x + col * delta;
                        var y = row0y + row * delta;
                        if (brushSettings.randomizePositions)
                        {
                            if (Random.value < 0.4 * brushSettings.randomness) continue;
                            if (takedCells.Contains((row, col))) continue;
                            x += Random.Range(-maxRandomOffset, maxRandomOffset);
                            y += Random.Range(-maxRandomOffset, maxRandomOffset);
                            var randCol = Mathf.RoundToInt((x - col0x) / delta);
                            var randRow = Mathf.RoundToInt((y - row0y) / delta);
                            if (randRow < row) continue;
                            if (row != randRow || col != randRow) takedCells.Add((randRow, randCol));
                            takedCells.RemoveAll(pair => pair.row <= row);
                        }

                        if (brushSettings.brushShape == BrushToolBase.BrushShape.CIRCLE)
                        {
                            var distanceSqr = x * x + y * y;
                            if (distanceSqr >= radiusSqr) continue;
                        }
                        else if (brushSettings.brushShape == BrushToolBase.BrushShape.SQUARE)
                        {
                            if (Mathf.Abs(x) > radius || Mathf.Abs(y) > radius) continue;
                        }
                        var nextItemIdx = PaletteManager.selectedBrush.nextItemIndex;
                        var position = new Vector3(x, y, 0f);
                        if ((PaletteManager.selectedBrush.frequencyMode
                            == PluginMaster.MultibrushSettings.FrecuencyMode.RANDOM && nextItemIdx == -1)
                            || (PaletteManager.selectedBrush.frequencyMode
                            == PluginMaster.MultibrushSettings.FrecuencyMode.PATTERN && nextItemIdx == -2)) continue;
                        else if (nextItemIdx != -1) AddBrushstrokeItem(nextItemIdx, position, Vector3.zero, brushSettings);
                    }
                }
            }
        }
        private static void UpdatePinBrushstroke()
        {
            var nextIdx = PaletteManager.selectedBrush.nextItemIndex;
            if (nextIdx == -1) return;
            if (PaletteManager.selectedBrush.frequencyMode == PluginMaster.MultibrushSettings.FrecuencyMode.PATTERN
                && nextIdx == -2)
            {
                if (PaletteManager.selectedBrush.patternMachine != null) PaletteManager.selectedBrush.patternMachine.Reset();
                else return;
            }
            AddBrushstrokeItem(nextIdx, Vector3.zero, Vector3.zero, PinManager.settings);

            const int maxTries = 10;
            int tryCount = 0;
            while (_brushstroke.Count == 0 && ++tryCount < maxTries)
            {
                nextIdx = PaletteManager.selectedBrush.nextItemIndex;
                if (nextIdx >= 0)
                {
                    AddBrushstrokeItem(nextIdx, Vector3.zero, Vector3.zero, PinManager.settings);
                    break;
                }
            }
            _currentPinIdx = Mathf.Clamp(nextIdx, 0, PaletteManager.selectedBrush.itemCount - 1);
        }
        public static void UpdateBrushstroke(bool brushChange = false)
        {
            if (ToolManager.tool == ToolManager.PaintTool.SELECTION) return;
            if (ToolManager.tool == ToolManager.PaintTool.LINE
                || ToolManager.tool == ToolManager.PaintTool.SHAPE
                || ToolManager.tool == ToolManager.PaintTool.TILING)
            {
                PWBIO.UpdateStroke();
                return;
            }
            if (!brushChange && ToolManager.tool == ToolManager.PaintTool.PIN && PinManager.settings.repeat) return;
            _brushstroke.Clear();
            if (PaletteManager.selectedBrush == null) return;
            if (ToolManager.tool == ToolManager.PaintTool.BRUSH) UpdateBrushBaseStroke(BrushManager.settings);
            else if (ToolManager.tool == ToolManager.PaintTool.GRAVITY) UpdateBrushBaseStroke(GravityToolManager.settings);
            else if (ToolManager.tool == ToolManager.PaintTool.PIN) UpdatePinBrushstroke();
            else if (ToolManager.tool == ToolManager.PaintTool.REPLACER) UpdatePinBrushstroke();
        }
    }
}
