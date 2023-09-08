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
    #region BRUSH SETTINGS

    [System.Serializable]
    public class BrushSettings : ISerializationCallbackReceiver
    {
        [SerializeField] private long _id = -1;
        [SerializeField] private float _surfaceDistance = 0f;
        [SerializeField] private bool _randomSurfaceDistance = false;
        [SerializeField] private RandomUtils.Range _randomSurfaceDistanceRange = new RandomUtils.Range(-0.005f, 0.005f);
        [SerializeField] protected bool _embedInSurface = false;
        [SerializeField] protected bool _embedAtPivotHeight = true;
        [SerializeField] protected Vector3 _localPositionOffset = Vector3.zero;
        [SerializeField] private bool _rotateToTheSurface = true;
        [SerializeField] private Vector3 _eulerOffset = Vector3.zero;
        [SerializeField] private bool _addRandomRotation = false;
        [SerializeField] private float _rotationFactor = 90;
        [SerializeField] private bool _rotateInMultiples = false;
        [SerializeField]
        private RandomUtils.Range3 _randomEulerOffset = new RandomUtils.Range3(Vector3.zero, Vector3.zero);
        [SerializeField] private bool _separateScaleAxes = false;
        [SerializeField] private Vector3 _scaleMultiplier = Vector3.one;
        [SerializeField] private bool _randomScaleMultiplier = false;
        [SerializeField]
        private RandomUtils.Range3 _randomScaleMultiplierRange = new RandomUtils.Range3(Vector3.one, Vector3.one);

        public enum FlipAction { NONE, FLIP, RANDOM }
        [SerializeField] private FlipAction _flipX = FlipAction.NONE;
        [SerializeField] private FlipAction _flipY = FlipAction.NONE;

        [SerializeField] private ThumbnailSettings _thumbnailSettings = new ThumbnailSettings();
        [field: System.NonSerialized] private Texture2D _thumbnail = null;
        public System.Action OnDataChangedAction;
        private void OnDataChanged()
        {
            if(OnDataChangedAction != null) OnDataChangedAction();
        }
        public long id => _id;
        public virtual float surfaceDistance
        {
            get => _surfaceDistance;
            set
            {
                if (_surfaceDistance == value) return;
                _surfaceDistance = value;
                OnDataChanged();
            }
        }

        public virtual bool randomSurfaceDistance
        {
            get => _randomSurfaceDistance;
            set
            {
                if (_randomSurfaceDistance == value) return;
                _randomSurfaceDistance = value;
                OnDataChanged();
            }
        }

        public virtual RandomUtils.Range randomSurfaceDistanceRange
        {
            get => _randomSurfaceDistanceRange;
            set
            {
                if (_randomSurfaceDistanceRange == value) return;
                _randomSurfaceDistanceRange = value;
                OnDataChanged();
            }
        }
        public virtual bool embedInSurface
        {
            get => _embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                OnDataChanged();
            }
        }
        public virtual bool embedAtPivotHeight
        {
            get => _embedAtPivotHeight;
            set
            {
                if (_embedAtPivotHeight == value) return;
                _embedAtPivotHeight = value;
                OnDataChanged();
            }
        }
        public virtual void UpdateBottomVertices() { }
        public virtual Vector3 localPositionOffset
        {
            get => _localPositionOffset;
            set
            {
                if (_localPositionOffset == value) return;
                _localPositionOffset = value;
                OnDataChanged();
            }
        }
        public virtual bool rotateToTheSurface
        {
            get => _rotateToTheSurface;
            set
            {
                if (_rotateToTheSurface == value) return;
                _rotateToTheSurface = value;
                OnDataChanged();
            }
        }
        public virtual Vector3 eulerOffset
        {
            get => _eulerOffset;
            set
            {
                if (_eulerOffset == value) return;
                _eulerOffset = value;
                _randomEulerOffset.v1 = _randomEulerOffset.v2 = Vector3.zero;
                OnDataChanged();
            }
        }
        public virtual bool addRandomRotation
        {
            get => _addRandomRotation;
            set
            {
                if (_addRandomRotation == value) return;
                _addRandomRotation = value;
                OnDataChanged();
            }
        }
        public virtual float rotationFactor
        {
            get => _rotationFactor;
            set
            {
                value = Mathf.Max(value, 0f);
                if (_rotationFactor == value) return;
                _rotationFactor = value;
                OnDataChanged();
            }
        }
        public virtual bool rotateInMultiples
        {
            get => _rotateInMultiples;
            set
            {
                if (_rotateInMultiples == value) return;
                _rotateInMultiples = value;
                OnDataChanged();
            }
        }
        public virtual RandomUtils.Range3 randomEulerOffset
        {
            get => _randomEulerOffset;
            set
            {
                if (_randomEulerOffset == value) return;
                _randomEulerOffset = value;
                _eulerOffset = Vector3.zero;
                OnDataChanged();
            }
        }
        public virtual bool separateScaleAxes
        {
            get => _separateScaleAxes;
            set
            {
                if (_separateScaleAxes == value) return;
                _separateScaleAxes = value;
                OnDataChanged();
            }
        }
        public virtual Vector3 scaleMultiplier
        {
            get => _scaleMultiplier;
            set
            {
                if (_scaleMultiplier == value) return;
                _scaleMultiplier = value;
                _randomScaleMultiplierRange.v1 = _randomScaleMultiplierRange.v2 = Vector3.one;
                OnDataChanged();
            }
        }
        public virtual RandomUtils.Range3 randomScaleMultiplierRange
        {
            get => _randomScaleMultiplierRange;
            set
            {
                if (_randomScaleMultiplierRange == value) return;
                _randomScaleMultiplierRange = value;
                _scaleMultiplier = Vector3.one;
                OnDataChanged();
            }
        }
        public virtual bool randomScaleMultiplier
        {
            get => _randomScaleMultiplier;
            set
            {
                if (_randomScaleMultiplier == value) return;
                _randomScaleMultiplier = value;
                _randomScaleMultiplierRange.v1 = _randomScaleMultiplierRange.v2 = _scaleMultiplier = Vector3.one;
                OnDataChanged();
            }
        }

        public virtual bool isAsset2D { get; set; }
        public virtual FlipAction flipX
        {
            get => _flipX;
            set
            {
                if (_flipX == value) return;
                _flipX = value;
                OnDataChanged();
            }
        }
        public virtual FlipAction flipY
        {
            get => _flipY;
            set
            {
                if (_flipY == value) return;
                _flipY = value;
                OnDataChanged();
            }
        }
        public virtual string thumbnailPath { get; }
        public virtual ThumbnailSettings thumbnailSettings
        {
            get => _thumbnailSettings;
            set => _thumbnailSettings.Copy(value);
        }

        public Texture2D thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    var filePath = thumbnailPath;
                    if (filePath != null)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            var fileData = System.IO.File.ReadAllBytes(filePath);
                            _thumbnail = new Texture2D(ThumbnailUtils.SIZE, ThumbnailUtils.SIZE);
                            _thumbnail.LoadImage(fileData);
                        }
                        else UpdateThumbnail();
                    }
                }
                if (_thumbnail == null) UpdateThumbnail();
                return _thumbnail;
            }
        }

        public Texture2D thumbnailTexture
        {
            get
            {
                if (_thumbnail == null) _thumbnail = new Texture2D(ThumbnailUtils.SIZE, ThumbnailUtils.SIZE);
                return _thumbnail;
            }
        }

        public void UpdateThumbnail() => ThumbnailUtils.UpdateThumbnail(this);

        public virtual BrushSettings Clone()
        {
            var clone = new BrushSettings();
            clone.Copy(this);
            clone._thumbnail = _thumbnail;
            return clone;
        }

        public virtual void Copy(BrushSettings other)
        {
            _surfaceDistance = other._surfaceDistance;
            _randomSurfaceDistance = other._randomSurfaceDistance;
            _randomSurfaceDistanceRange = other._randomSurfaceDistanceRange;
            _embedInSurface = other._embedInSurface;
            _embedAtPivotHeight = other._embedAtPivotHeight;
            _localPositionOffset = other._localPositionOffset;
            _rotateToTheSurface = other._rotateToTheSurface;
            _addRandomRotation = other._addRandomRotation;
            _eulerOffset = other._eulerOffset;
            _randomEulerOffset = new RandomUtils.Range3(other._randomEulerOffset);
            _randomScaleMultiplier = other._randomScaleMultiplier;
            _separateScaleAxes = other._separateScaleAxes;
            _scaleMultiplier = other._scaleMultiplier;
            _randomScaleMultiplierRange = new RandomUtils.Range3(other._randomScaleMultiplierRange);
            _thumbnailSettings.Copy(other._thumbnailSettings);
            _rotationFactor = other._rotationFactor;
            _rotateInMultiples = other._rotateInMultiples;
            _flipX = other._flipX;
            _flipY = other._flipY;
        }
        public void Reset()
        {
            _surfaceDistance = 0f;
            _randomSurfaceDistance = false;
            _randomSurfaceDistanceRange = new RandomUtils.Range(-0.005f, 0.005f);
            _embedInSurface = false;
            _embedAtPivotHeight = true;
            _localPositionOffset = Vector3.zero;
            _rotateToTheSurface = true;
            _addRandomRotation = false;
            _eulerOffset = Vector3.zero;
            _randomEulerOffset = new RandomUtils.Range3(Vector3.zero, Vector3.zero);
            _randomScaleMultiplier = false;
            _separateScaleAxes = false;
            _scaleMultiplier = Vector3.one;
            _randomScaleMultiplierRange = new RandomUtils.Range3(Vector3.one, Vector3.one);
            _thumbnailSettings = new ThumbnailSettings();
            _rotationFactor = 90;
            _rotateInMultiples = false;
            _flipX = FlipAction.NONE;
            _flipY = FlipAction.NONE;
        }

        private static long _prevId = 0;
        protected void SetId()
        {
            _id = System.DateTime.Now.Ticks;
            if (_id <= _prevId) _id = _prevId + 1;
            _prevId = _id;
        }
        public BrushSettings() { }
        public BrushSettings(BrushSettings other) => Copy(other);

        public virtual void OnBeforeSerialize() { }

        public virtual void OnAfterDeserialize()
        {
            _thumbnail = null;
            if (id == -1)
            {
                SetId();
                PaletteManager.SetSavePending();
            }
        }
    }

    public static class SelectionUtils
    {
        public static void Swap<T>(int fromIdx, int toIdx, ref int[] selection, System.Collections.Generic.List<T> list)
        {
            if (fromIdx == toIdx) return;
            var newOrder = new System.Collections.Generic.List<T>();
            var newSelection = selection.ToArray();
            for (int idx = 0; idx <= list.Count; ++idx)
            {
                if (idx == toIdx)
                {
                    System.Array.Sort(selection);
                    int newSelectionIdx = 0;
                    foreach (var selectionIdx in selection)
                    {
                        newOrder.Add(list[selectionIdx]);
                        newSelection[newSelectionIdx++] = newOrder.Count - 1;
                    }
                    if (idx < list.Count && !selection.Contains(idx)) newOrder.Add(list[idx]);
                }
                else if (selection.Contains(idx)) continue;
                else if (idx < list.Count) newOrder.Add(list[idx]);
            }
            selection = newSelection;
            list.Clear();
            list.AddRange(newOrder);
            PWBCore.staticData.Save();
        }
    }

    [System.Serializable]
    public class ThumbnailSettings
    {
        [SerializeField] private Color _backgroudColor = Color.gray;
        [SerializeField] private Vector2 _lightEuler = new Vector2(130, -165);
        [SerializeField] private Color _lightColor = Color.white;
        [SerializeField] private float _lightIntensity = 1;
        [SerializeField] private float _zoom = 1;
        [SerializeField] private Vector3 _targetEuler = new Vector3(0, 125, 0);
        [SerializeField] private Vector2 _targetOffset = Vector2.zero;

        public Color backgroudColor { get => _backgroudColor; set => _backgroudColor = value; }
        public Vector2 lightEuler { get => _lightEuler; set => _lightEuler = value; }
        public Color lightColor { get => _lightColor; set => _lightColor = value; }
        public float lightIntensity { get => _lightIntensity; set => _lightIntensity = value; }
        public float zoom { get => _zoom; set => _zoom = value; }
        public Vector3 targetEuler { get => _targetEuler; set => _targetEuler = value; }
        public Vector2 targetOffset { get => _targetOffset; set => _targetOffset = value; }
        public ThumbnailSettings() { }
        public ThumbnailSettings(Color backgroudColor, Vector3 lightEuler, Color lightColor, float lightIntensity,
            float zoom, Vector3 targetEuler, Vector2 targetOffset)
        {
            _backgroudColor = backgroudColor;
            _lightEuler = lightEuler;
            _lightColor = lightColor;
            _lightIntensity = lightIntensity;
            _zoom = zoom;
            _targetEuler = targetEuler;
            _targetOffset = targetOffset;
        }

        public ThumbnailSettings(ThumbnailSettings other) => Copy(other);
        public void Copy(ThumbnailSettings other)
        {
            _backgroudColor = other._backgroudColor;
            _lightEuler = other._lightEuler;
            _lightColor = other._lightColor;
            _lightIntensity = other._lightIntensity;
            _zoom = other._zoom;
            _targetEuler = other._targetEuler;
            _targetOffset = other._targetOffset;
        }

        public ThumbnailSettings Clone()
        {
            var clone = new ThumbnailSettings();
            clone.Copy(this);
            return clone;
        }
    }

    [System.Serializable]
    public class MultibrushItemSettings : BrushSettings
    {
        [SerializeField] private bool _overwriteSettings = false;
        [SerializeField] private string _guid = string.Empty;
        [SerializeField] private string _prefabPath = string.Empty;
        [SerializeField] private float _frequency = 1;
        [SerializeField] private long _parentId = -1;
        [SerializeField] private bool _overwriteThumbnailSettings = false;
        [SerializeField] private bool _includeInThumbnail = true;
        [SerializeField] private bool _isAsset2D = false;
        private Vector3[] _bottomVertices = null;
        private float _bottomMagnitude = 0;
        private float _height = 1f;
        private Vector3 _size = Vector3.zero;
        private GameObject _prefab = null;

        [System.NonSerialized] private MultibrushSettings _parentSettings = null;
        public MultibrushSettings parentSettings
        {
            get
            {
                if (_parentSettings == null) _parentSettings = PaletteManager.GetBrushById(_parentId);
                return _parentSettings;
            }
            set
            {
                if (value == null)
                {
                    _parentId = -1;
                    _parentSettings = null;
                    return;
                }
                _parentSettings = value;
                _parentId = value.id;
            }
        }

        private void SavePalette()
        {
            if (parentSettings == null) return;
            parentSettings.SavePalette();
        }
        public MultibrushItemSettings(GameObject prefab, MultibrushSettings parentSettings) : base()
        {
            SetId();
            _prefab = prefab;
            _parentId = parentSettings.id;
            _parentSettings = parentSettings;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_prefab, out _guid, out long localId);
            if (_prefab == null) return;
            _prefabPath = UnityEditor.AssetDatabase.GetAssetPath(_prefab);
            _bottomVertices = BoundsUtils.GetBottomVertices(prefab.transform);
            _height = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation).size.y;
            _size = BoundsUtils.GetBoundsRecursive(prefab.transform).size;
            _bottomMagnitude = BoundsUtils.GetBottomMagnitude(prefab.transform);
            UpdateAssetType();
        }

        public void InitializeParentSettings(MultibrushSettings parentSettings)
        {
            _parentId = parentSettings.id;
            _parentSettings = parentSettings;
            this.parentSettings.UpdateTotalFrequency();
        }

        public override string thumbnailPath
        {
            get
            {
                if (parentSettings == null) return null;
                var parentPath = parentSettings.thumbnailPath;
                if (parentPath == null) return null;
                var path = parentPath.Insert(parentPath.Length - 4, "_" + id.ToString("X"));
                return path;
            }
        }

        public bool overwriteSettings
        {
            get => _overwriteSettings;
            set
            {
                if (_overwriteSettings == value) return;
                _overwriteSettings = value;
                SavePalette();
            }
        }

        public float frequency
        {
            get => _frequency;
            set
            {
                value = Mathf.Max(value, 0);
                if (_frequency == value) return;
                _frequency = value;
                if (parentSettings != null) parentSettings.UpdateTotalFrequency();
            }
        }
        public GameObject prefab
        {
            get
            {
                if (_prefab == null)
                    _prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>
                        (UnityEditor.AssetDatabase.GUIDToAssetPath(_guid));
                if (_prefab == null)
                {
                    _prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                    if (_prefab != null)
                        UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_prefab, out _guid, out long localId);
                }
                else _prefabPath = UnityEditor.AssetDatabase.GetAssetPath(_prefab);
                return _prefab;
            }
        }
        public string prefabPath => _prefabPath;
        public override float surfaceDistance
            => _overwriteSettings || parentSettings == null ? base.surfaceDistance : parentSettings.surfaceDistance;

        public override bool randomSurfaceDistance
            => _overwriteSettings || parentSettings == null
            ? base.randomSurfaceDistance : parentSettings.randomSurfaceDistance;

        public override RandomUtils.Range randomSurfaceDistanceRange
            => _overwriteSettings || parentSettings == null
            ? base.randomSurfaceDistanceRange : parentSettings.randomSurfaceDistanceRange;

        public override bool embedInSurface
        {
            get => _overwriteSettings || parentSettings == null ? base.embedInSurface
                : parentSettings.embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                if (_embedInSurface) UpdateBottomVertices();
            }
        }

        public override bool embedAtPivotHeight
        {
            get => _overwriteSettings || parentSettings == null ? base.embedAtPivotHeight : parentSettings.embedAtPivotHeight;
            set
            {
                if (_embedAtPivotHeight == value) return;
                _embedAtPivotHeight = value;
            }
        }

        public override Vector3 localPositionOffset
            => _overwriteSettings || parentSettings == null ? base.localPositionOffset : parentSettings.localPositionOffset;
        public override bool rotateToTheSurface
            => _overwriteSettings || parentSettings == null ? base.rotateToTheSurface : parentSettings.rotateToTheSurface;
        public override Vector3 eulerOffset
            => _overwriteSettings || parentSettings == null ? base.eulerOffset : parentSettings.eulerOffset;
        public override bool addRandomRotation
            => _overwriteSettings || parentSettings == null ? base.addRandomRotation : parentSettings.addRandomRotation;
        public override RandomUtils.Range3 randomEulerOffset
            => _overwriteSettings || parentSettings == null ? base.randomEulerOffset : parentSettings.randomEulerOffset;
        public override float rotationFactor
            => _overwriteSettings || parentSettings == null ? base.rotationFactor : parentSettings.rotationFactor;
        public override bool rotateInMultiples
            => _overwriteSettings || parentSettings == null ? base.rotateInMultiples : parentSettings.rotateInMultiples;
        public override bool separateScaleAxes
            => _overwriteSettings || parentSettings == null ? base.separateScaleAxes : parentSettings.separateScaleAxes;
        public override Vector3 scaleMultiplier
            => _overwriteSettings || parentSettings == null ? base.scaleMultiplier : parentSettings.scaleMultiplier;
        public override RandomUtils.Range3 randomScaleMultiplierRange
            => _overwriteSettings || parentSettings == null ? base.randomScaleMultiplierRange
            : parentSettings.randomScaleMultiplierRange;
        public override bool randomScaleMultiplier
            => _overwriteSettings || parentSettings == null ? base.randomScaleMultiplier
            : parentSettings.randomScaleMultiplier;
        public override FlipAction flipX
            => _overwriteSettings || parentSettings == null ? base.flipX : parentSettings.flipX;
        public override FlipAction flipY
            => _overwriteSettings || parentSettings == null ? base.flipY : parentSettings.flipY;
        public Vector3 maxScaleMultiplier
            => randomScaleMultiplier ? randomScaleMultiplierRange.max : scaleMultiplier;
        public Vector3 minScaleMultiplier
            => randomScaleMultiplier ? randomScaleMultiplierRange.min : scaleMultiplier;
        public virtual bool overwriteThumbnailSettings
        {
            get => _overwriteThumbnailSettings;
            set
            {
                if (_overwriteThumbnailSettings == value) return;
                _overwriteThumbnailSettings = value;
            }
        }
        public override ThumbnailSettings thumbnailSettings
        {
            get => _overwriteThumbnailSettings || parentSettings == null
                ? base.thumbnailSettings : parentSettings.thumbnailSettings;
            set => base.thumbnailSettings = value;
        }
        public bool includeInThumbnail
        {
            get => _includeInThumbnail;
            set
            {
                if (_includeInThumbnail == value) return;
                _includeInThumbnail = value;
            }
        }

        public override bool isAsset2D { get => _isAsset2D; set => _isAsset2D = value; }

        public void UpdateAssetType() => _isAsset2D = Utils2D.Is2DAsset(prefab);
        public override void Copy(BrushSettings other)
        {
            if (other is MultibrushItemSettings)
            {
                var otherItemSettings = other as MultibrushItemSettings;
                _overwriteSettings = otherItemSettings._overwriteSettings;
                _frequency = otherItemSettings._frequency;
                _overwriteThumbnailSettings = otherItemSettings._overwriteThumbnailSettings;
                _includeInThumbnail = otherItemSettings._includeInThumbnail;
                _isAsset2D = otherItemSettings._isAsset2D;
            }
            base.Copy(other);
        }

        public MultibrushItemSettings() : base() { }
        public MultibrushItemSettings(MultibrushItemSettings other) : base() => Copy(other);
        public override BrushSettings Clone()
        {
            var clone = new MultibrushItemSettings();
            clone._prefab = _prefab;
            clone._guid = _guid;
            clone._parentId = parentSettings.id;
            clone._parentSettings = parentSettings;
            clone._bottomVertices = bottomVertices == null ? null : bottomVertices.ToArray();
            clone._bottomMagnitude = bottomMagnitude;
            clone._height = height;
            clone.Copy(this);
            return clone;
        }

        public override void OnBeforeSerialize() => base.OnBeforeSerialize();

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _prefab = null;
        }

        public Vector3[] bottomVertices
        {
            get
            {
                if (_bottomVertices == null) UpdateBottomVertices();
                return _bottomVertices;
            }
        }

        public float bottomMagnitude
        {
            get
            {
                if (prefab == null) return 0f;
                if (_bottomMagnitude == 0) _bottomMagnitude = BoundsUtils.GetBottomMagnitude(prefab.transform);
                return _bottomMagnitude;
            }
        }

        public float height => _height;
        public Vector3 size
        {
            get
            {
                if (prefab == null) return Vector3.zero;
                if (_size == Vector3.zero) _size = BoundsUtils.GetBoundsRecursive(prefab.transform).size;
                return _size;
            }
        }
        public override void UpdateBottomVertices()
        {
            if (prefab == null) return;
            _bottomVertices = BoundsUtils.GetBottomVertices(prefab.transform);
            _height = BoundsUtils.GetBoundsRecursive(prefab.transform, prefab.transform.rotation).size.y;
            _size = BoundsUtils.GetBoundsRecursive(prefab.transform).size;
            _bottomMagnitude = BoundsUtils.GetBottomMagnitude(prefab.transform);
        }
    }

    [System.Serializable]
    public class MultibrushSettings : BrushSettings
    {
        public enum FrecuencyMode { RANDOM, PATTERN }
        [SerializeField] private string _name = null;
        [SerializeField]
        private System.Collections.Generic.List<MultibrushItemSettings> _items
            = new System.Collections.Generic.List<MultibrushItemSettings>();
        [SerializeField] private FrecuencyMode _frequencyMode = FrecuencyMode.RANDOM;
        [SerializeField] private string _pattern = "1...";
        [SerializeField] private bool _restartPatternForEachStroke = true;

        [field: System.NonSerialized] private float _totalFrequency = -1;
        [field: System.NonSerialized] private PatternMachine _patternMachine = null;
        [field: System.NonSerialized] private PaletteData _palette = null;

        public string name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
            }
        }

        public FrecuencyMode frequencyMode
        {
            get => _frequencyMode;
            set
            {
                if (_frequencyMode == value) return;
                _frequencyMode = value;
            }
        }

        public string pattern
        {
            get => _pattern;
            set
            {
                if (_pattern == value) return;
                _pattern = value;
            }
        }

        public PatternMachine patternMachine
        {
            get
            {
                return _patternMachine;
            }
            set => _patternMachine = value;
        }

        public bool restartPatternForEachStroke
        {
            get => _restartPatternForEachStroke;
            set
            {
                if (_restartPatternForEachStroke == value) return;
                _restartPatternForEachStroke = value;
            }
        }

        public PaletteData palette
        {
            get
            {
                if (_palette == null) _palette = PaletteManager.GetPalette(this);
                return _palette;
            }
            set => _palette = value;
        }

        public override bool isAsset2D
        {
            get => _items.Exists(i => i.isAsset2D);
            set
            {
                foreach (var item in _items) item.isAsset2D = value;
            }
        }

        public void SavePalette()
        {
            if (palette != null) palette.Save();
        }
        public MultibrushSettings(GameObject prefab) : base()
        {
            SetId();
            _items.Add(new MultibrushItemSettings(prefab, this));
            _name = prefab.name;
            Copy(PaletteManager.selectedPalette.brushCreationSettings.defaultBrushSettings);
            thumbnailSettings.Copy(PaletteManager.selectedPalette.brushCreationSettings.defaultThumbnailSettings);
        }

        public override string thumbnailPath
            => palette == null ? null : palette.thumbnailsPath + "/" + id.ToString("X") + ".png";

        public void AddItem(MultibrushItemSettings item)
        {
            _items.Add(item);
            OnItemCountChange();
        }

        private void RemoveFromPalette()
        {
            if (palette != null) palette.RemoveBrush(this);
        }

        public void RemoveItemAt(int index)
        {
            _items.RemoveAt(index);
            OnItemCountChange();
            if (_items.Count == 0) RemoveFromPalette();
        }

        public void RemoveItem(MultibrushItemSettings item)
        {
            if (!_items.Contains(item)) return;
            _items.Remove(item);
            OnItemCountChange();
            if (_items.Count == 0) RemoveFromPalette();
        }

        public MultibrushItemSettings GetItemAt(int index)
        {
            if (index >= _items.Count) return null;
            return _items[index];
        }

        public bool ItemExist(long itemId) => _items.Exists(i => i.id == itemId);
        public MultibrushItemSettings GetItemById(long itemId)
        {
            var items = _items.Where(i => i.id == itemId).ToArray();
            if (items.Length == 0) return null;
            return items[0];
        }

        public void InsertItemAt(MultibrushItemSettings item, int index)
        {
            _items.Insert(index, item);
            OnItemCountChange();
        }

        private void OnItemCountChange()
        {
            UpdateTotalFrequency();
            UpdatePatternMachine();
            PWBCore.staticData.SaveAndUpdateVersion();
            BrushstrokeManager.UpdateBrushstroke();
            SavePalette();
            UpdateThumbnail();
        }

        public void Swap(int fromIdx, int toIdx, ref int[] selection)
            => SelectionUtils.Swap<MultibrushItemSettings>(fromIdx, toIdx, ref selection, _items);

        public MultibrushItemSettings[] items => _items.ToArray();

        public int itemCount => _items.Count;

        public int notNullItemCount => _items.Where(i => i.prefab != null).Count();
        public bool containMissingPrefabs
        {
            get
            {
                foreach (var item in _items)
                    if (item.prefab == null) return true;
                return false;
            }
        }

        public bool allPrefabMissing
        {
            get
            {
                foreach (var item in _items)
                    if (item.prefab != null) return false;
                return true;
            }
        }
        public void UpdateTotalFrequency()
        {
            _totalFrequency = 0;
            foreach (var item in _items) _totalFrequency += item.frequency;
        }

        public float totalFrecuency
        {
            get
            {
                if (_totalFrequency == -1) UpdateTotalFrequency();
                return _totalFrequency;
            }
        }
        public int nextItemIndex
        {
            get
            {
                if (frequencyMode == FrecuencyMode.RANDOM)
                {
                    if (_items.Count == 1) return 0;
                    var rand = UnityEngine.Random.Range(0f, totalFrecuency);
                    float sum = 0;
                    for (int i = 0; i < _items.Count; ++i)
                    {
                        sum += _items[i].frequency;
                        if (rand <= sum) return i;
                    }
                    return -1;
                }
                if (_patternMachine == null)
                {
                    if (PatternMachine.Validate(_pattern, _items.Count, out PatternMachine.Token[] tokens)
                        == PatternMachine.ValidationResult.VALID) _patternMachine = new PatternMachine(tokens);
                }
                return _patternMachine == null ? -2 : _patternMachine.nextIndex - 1;
            }
        }

        private void UpdatePatternMachine()
        {
            if (PatternMachine.Validate(_pattern, _items.Count, out PatternMachine.Token[] tokens)
                != PatternMachine.ValidationResult.VALID)
                _patternMachine = null;
        }

        public override void Copy(BrushSettings other)
        {
            
            if (other is MultibrushSettings)
            {
                var otherMulti = other as MultibrushSettings;
                _items.Clear();
                foreach (var item in otherMulti._items)
                {
                    var clone = item.Clone() as MultibrushItemSettings;
                    clone.parentSettings = this;
                    _items.Add(clone);
                }
                _name = otherMulti._name;
                _frequencyMode = otherMulti._frequencyMode;
                _pattern = otherMulti._pattern;
                _restartPatternForEachStroke = otherMulti._restartPatternForEachStroke;
                _totalFrequency = otherMulti._totalFrequency;
            }
            base.Copy(other);
        }

        private MultibrushSettings() : base() { }
        public override BrushSettings Clone()
        {
            var clone = new MultibrushSettings();
            clone.Copy(this);
            return clone;
        }

        public BrushSettings CloneMainSettings()
        {
            var clone = new BrushSettings();
            clone.Copy(this);
            return clone;
        }

        public void Duplicate(int index)
        {
            var clone = _items[index].Clone();
            _items.Insert(index, clone as MultibrushItemSettings);
            OnItemCountChange();
        }

        public void DuplicateAt(int indexToDuplicate, int at)
        {
            var clone = _items[indexToDuplicate].Clone();
            _items.Insert(at, clone as MultibrushItemSettings);
            OnItemCountChange();
        }

        public override void UpdateBottomVertices()
        {
            foreach (var item in _items) item.UpdateBottomVertices();
        }

        public override bool embedInSurface
        {
            get => _embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                if (_embedInSurface) UpdateBottomVertices();
            }
        }
        public bool ContainsPrefab(int prefabId)
            => _items.Exists(item => item.prefab != null && item.prefab.GetInstanceID() == prefabId);

        public bool ContainsPrefabPath(string path) => _items.Exists(item => item.prefabPath == path);
        public bool ContainsSceneObject(GameObject obj)
        {
            if (obj == null) return false;
            var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (outermostPrefab == null) return false;
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
            if (prefab == null) return false;
            return ContainsPrefab(prefab.GetInstanceID());
        }

        public Vector3 minBrushSize
        {
            get
            {
                var min = Vector3.one * float.MaxValue;
                foreach (var item in _items)
                    min = Vector3.Min(min, item.size);
                return min;
            }
        }

        public float minBrushMagnitude
        {
            get
            {
                var min = minBrushSize;
                return Mathf.Min(min.x, min.y, min.z);
            }
        }

        public Vector3 maxBrushSize
        {
            get
            {
                var max = Vector3.one * float.MinValue;
                foreach (var item in _items)
                    max = Vector3.Max(max, item.size);
                return max;
            }
        }

        public float maxBrushMagnitude
        {
            get
            {
                var max = maxBrushSize;
                return Mathf.Min(max.x, max.y, max.z);
            }
        }

        public void UpdateAssetTypes()
        {
            foreach (var item in _items) item.UpdateAssetType();
        }

        public void Cleanup()
        {
            foreach (var item in items) if (item.prefab == null) RemoveItem(item);
        }
    }

    [System.Serializable]
    public class BrushCreationSettings
    {
        [SerializeField] private bool _includeSubfolders = true;
        [SerializeField] private bool _addLabelsToDroppedPrefabs = false;
        [SerializeField] private string _labelsCSV = null;
        private string[] _labels = null;
        [SerializeField] private BrushSettings _defaultBrushSettings = new BrushSettings();
        [SerializeField] private ThumbnailSettings _defaultThumbnailSettings = new ThumbnailSettings();

        public bool includeSubfolders
        {
            get => _includeSubfolders;
            set
            {
                if (_includeSubfolders == value) return;
                _includeSubfolders = value;
            }
        }

        public bool addLabelsToDroppedPrefabs
        {
            get => _addLabelsToDroppedPrefabs;
            set
            {
                if (_addLabelsToDroppedPrefabs == value) return;
                _addLabelsToDroppedPrefabs = value;
            }
        }

        private void SplitCSV() => _labels = _labelsCSV.Replace(", ", ",").Split(',');

        public string[] labels
        {
            get
            {
                if (_labels == null || (_labels.Length == 0 && _labelsCSV != null && _labelsCSV != string.Empty))
                    SplitCSV();
                return _labels;
            }
        }

        public string labelsCSV
        {
            get => _labelsCSV;
            set
            {
                if (_labelsCSV == value) return;
                if (value == string.Empty)
                {
                    _labelsCSV = string.Empty;
                    _labels = new string[0];
                    return;
                }
                var trimmed = System.Text.RegularExpressions.Regex.Replace(value.Trim(), "[( *, +)]+", ", ");
                if (trimmed.Last() == ' ') trimmed = trimmed.Substring(0, trimmed.Length - 2);
                if (trimmed.First() == ',') trimmed = trimmed.Substring(1);
                if (_labelsCSV == trimmed) return;
                _labelsCSV = trimmed;
                SplitCSV();
            }
        }

        public BrushSettings defaultBrushSettings => _defaultBrushSettings;
        public void FactoryResetDefaultBrushSettings() => _defaultBrushSettings = new BrushSettings();

        public ThumbnailSettings defaultThumbnailSettings => _defaultThumbnailSettings;
        public void FactoryResetDefaultThumbnailSettings() => _defaultThumbnailSettings = new ThumbnailSettings();

        public BrushCreationSettings Clone()
        {
            var clone = new BrushCreationSettings();
            clone.Copy(this);
            return clone;
        }

        public void Copy(BrushCreationSettings other)
        {
            _includeSubfolders = other._includeSubfolders;
            _addLabelsToDroppedPrefabs = other._addLabelsToDroppedPrefabs;
            _labelsCSV = other._labelsCSV;
            if (other._labels != null)
            {
                _labels = new string[other._labels.Length];
                System.Array.Copy(other._labels, _labels, other._labels.Length);
            }
            _defaultBrushSettings.Copy(other._defaultBrushSettings);
            _defaultThumbnailSettings.Copy(other._defaultThumbnailSettings);
        }
    }
    public class BrushInputData
    {
        public readonly int index;
        public readonly Rect rect;
        public readonly EventType eventType;
        public readonly bool control;
        public readonly bool shift;
        public readonly float mouseX;
        public BrushInputData(int index, Rect rect, EventType eventType, bool control, bool shift, float mouseX)
        {
            this.index = index;
            this.rect = rect;
            this.eventType = eventType;
            this.control = !shift && control;
            this.shift = shift;
            this.mouseX = mouseX;
        }
    }

    #endregion
    [System.Serializable]
    public class PaletteData
    {
        [SerializeField] private string _version = PWBData.VERSION;
        [SerializeField]
        private System.Collections.Generic.List<MultibrushSettings> _brushes
            = new System.Collections.Generic.List<MultibrushSettings>();
        [SerializeField] private string _name = null;
        [SerializeField] private long _id = -1;
        [SerializeField] BrushCreationSettings _brushCreationSettings = new BrushCreationSettings();
        private string _filePath = null;
        private bool _saving = false;
        public PaletteData(string name, long id) => (_name, _id) = (name, id);

        public string name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                Save();
            }
        }
        public long id => _id;

        public MultibrushSettings[] brushes => _brushes.Where(b => !b.allPrefabMissing).ToArray();

        public int brushCount => _brushes.Count;

        public BrushCreationSettings brushCreationSettings => _brushCreationSettings;

        public string filePath
        {
            get
            {
                void SetFilePath() => _filePath = PWBData.palettesDirectory + "/" + GetFileNameFromData(this);
                if (_filePath == null) SetFilePath();
                else if (!System.IO.File.Exists(_filePath)) SetFilePath();
                return _filePath;
            }
            set => _filePath = value;
        }
        public string thumbnailsPath
        {
            get
            {
                var path = filePath.Substring(0, filePath.Length - 4);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                return path;
            }
        }

        public string version { get => _version; set => _version = value; }
        public bool saving => _saving;
        public static string GetFileNameFromData(PaletteData data) => "PWB_" + data._id.ToString("X") + ".txt";

        public MultibrushSettings GetBrush(int idx)
        {
            if (idx < 0 || idx >= _brushes.Count) return null;
            if (_brushes[idx].allPrefabMissing) return null;
            return _brushes[idx];
        }

        public void UpdateAllThumbnails()
        {
            foreach (var brush in _brushes) brush.UpdateThumbnail();
        }
        private void SetSpritesThumbnailSettings(MultibrushSettings brush)
        {
            foreach (var item in brush.items)
            {
                if (item.isAsset2D)
                {
                    item.thumbnailSettings.targetEuler = new Vector3(17.5f, 0f, 0f);
                    item.thumbnailSettings.zoom = 1.47f;
                    item.thumbnailSettings.targetOffset = new Vector2(0f, -0.06f);
                }
            }
            brush.rotateToTheSurface = false;
        }

        public void AddBrush(MultibrushSettings brush)
        {
            _brushes.Add(brush);
            SetSpritesThumbnailSettings(brush);
            brush.palette = this;
            PWBCore.staticData.SaveAndUpdateVersion();
            Save();
        }

        public void RemoveBrushAt(int idx)
        {
            _brushes.RemoveAt(idx);
            PWBCore.staticData.SaveAndUpdateVersion();
            BrushstrokeManager.UpdateBrushstroke();
            Save();
        }

        public void RemoveBrush(MultibrushSettings brush)
        {
            _brushes.Remove(brush);
            PWBCore.staticData.SaveAndUpdateVersion();
            BrushstrokeManager.UpdateBrushstroke();
            PrefabPalette.OnChangeRepaint();
            Save();
        }

        public void InsertBrushAt(MultibrushSettings brush, int idx)
        {
            _brushes.Insert(idx, brush);
            SetSpritesThumbnailSettings(brush);
            brush.palette = this;
            PWBCore.staticData.SaveAndUpdateVersion();
            Save();
        }

        public void Swap(int fromIdx, int toIdx, ref int[] selection)
            => SelectionUtils.Swap(fromIdx, toIdx, ref selection, _brushes);

        public void AscendingSort()
        {
            _brushes.Sort(delegate (MultibrushSettings x, MultibrushSettings y) { return x.name.CompareTo(y.name); });
            PaletteManager.ClearSelection();
            PWBCore.staticData.SaveAndUpdateVersion();
            PrefabPalette.OnChangeRepaint();
        }

        public void DescendingSort()
        {
            _brushes.Sort(delegate (MultibrushSettings x, MultibrushSettings y) { return y.name.CompareTo(x.name); });
            PaletteManager.ClearSelection();
            PWBCore.staticData.SaveAndUpdateVersion();
            PrefabPalette.OnChangeRepaint();
        }

        public void DuplicateBrush(int index) => DuplicateBrushAt(index, index);

        public void DuplicateBrushAt(int indexToDuplicate, int at)
        {
            var clone = _brushes[indexToDuplicate].Clone();
            _brushes.Insert(at, clone as MultibrushSettings);
            PWBCore.staticData.SaveAndUpdateVersion();
            Save();
        }

        public bool ContainsSceneObject(GameObject obj)
        {
            if (obj == null) return false;
            var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (outermostPrefab == null) return false;
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
            if (prefab == null) return false;
            return _brushes.Exists(brush => brush.ContainsPrefab(prefab.GetInstanceID()));
        }

        public bool ContainsPrefab(GameObject prefab)
        {
            if (prefab == null) return false;
            return _brushes.Exists(brush => brush.ContainsPrefab(prefab.GetInstanceID()));
        }

        public bool ContainsPrefabPath(string path) => _brushes.Exists(brush => brush.ContainsPrefabPath(path));
        public int FindBrushIdx(GameObject obj)
        {
            if (obj == null) return -1;
            var outermostPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (outermostPrefab == null) return -1;
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefab);
            if (prefab == null) return -1;
            var idx = _brushes.FindIndex(brush => brush.ContainsPrefab(prefab.GetInstanceID()) && brush.itemCount == 1);
            if (idx == -1) idx = _brushes.FindIndex(brush => brush.ContainsPrefab(prefab.GetInstanceID()));
            return idx;
        }

        public bool ContainsBrush(MultibrushSettings brush)
            => _brushes.Contains(brush) || _brushes.Exists(b => b.id == brush.id);

        public string Save()
        {
            _saving = true;
            var jsonString = JsonUtility.ToJson(this);
            var fileExist = System.IO.File.Exists(filePath);
            System.IO.File.WriteAllText(filePath, jsonString);
            if (!fileExist) PWBCore.AssetDatabaseRefresh();
            ThumbnailUtils.DeleteUnusedThumbnails();
            _saving = false;
            return filePath;
        }

        public void Copy(PaletteData other)
        {
            _brushes.Clear();
            _brushes.AddRange(other.brushes.ToArray());
            _name = other.name;
            _brushCreationSettings.Copy(other._brushCreationSettings);
        }
        public void ReloadFromFile()
        {
            var fileText = System.IO.File.ReadAllText(_filePath);
            if (string.IsNullOrEmpty(fileText)) return;
            var paletteData = JsonUtility.FromJson<PaletteData>(fileText);
            if (paletteData == null) return;
            Copy(paletteData);
        }

        public void Cleanup()
        {
            foreach (var brush in _brushes.ToArray()) brush.Cleanup();
            Save();
        }
    }

    [System.Serializable]
    public class PaletteManager : ISerializationCallbackReceiver
    {
        private System.Collections.Generic.List<PaletteData> _paletteDataList
            = new System.Collections.Generic.List<PaletteData>()
        { new PaletteData("Palette", System.DateTime.Now.ToBinary()) };
        public static PaletteData[] paletteData => instance.paletteDataList.ToArray();

        [SerializeField] private int _selectedPaletteIdx = 0;
        [SerializeField] private int _selectedBrushIdx = -1;
        [SerializeField] private bool _showBrushName = false;
        [SerializeField] private bool _viewList = false;
        [SerializeField] private bool _showTabsInMultipleRows = false;
        private System.Collections.Generic.HashSet<int> _idxSelection = new System.Collections.Generic.HashSet<int>();

        public static System.Action OnBrushChanged;
        public static System.Action OnSelectionChanged;
        public static System.Action OnPaletteChanged;

        private bool _pickingBrushes = false;
        private bool _loadPaletteFiles = false;

        [SerializeField] private int _iconSize = PrefabPalette.DEFAULT_ICON_SIZE;

        public System.Collections.Generic.List<PaletteData> paletteDataList
        {
            get
            {
                if (_loadPaletteFiles)
                {
                    _loadPaletteFiles = false;
                    PWBCore.staticData.VersionUpdate();
                    LoadPaletteFiles();
                }
                if (_paletteDataList.Count == 0)
                {
                    AddPalette(new PaletteData("Palette", System.DateTime.Now.ToBinary()));
                    _selectedPaletteIdx = 0;
                    _selectedBrushIdx = -1;
                }
                return _paletteDataList;
            }
        }

        private static PaletteManager _instance = null;

        private PaletteManager() { }
        public static PaletteManager instance
        {
            get
            {
                if (_instance == null) _instance = new PaletteManager();
                return _instance;
            }
        }

        public void LoadPaletteFiles()
        {
            var txtPaths = System.IO.Directory.GetFiles(PWBData.palettesDirectory, "*.txt");
            if (txtPaths.Length == 0)
            {
                if (_paletteDataList.Count == 0)
                    _paletteDataList = new System.Collections.Generic.List<PaletteData>()
                            { new PaletteData("Palette", System.DateTime.Now.ToBinary()) };
                _paletteDataList[0].filePath = _paletteDataList[0].Save();
            }
            bool clearList = true;
            foreach (var path in txtPaths)
            {
                var fileText = System.IO.File.ReadAllText(path);
                if (string.IsNullOrEmpty(fileText)) continue;
                var paletteData = JsonUtility.FromJson<PaletteData>(fileText);
                if (paletteData == null) continue;
                if (clearList)
                {
                    _paletteDataList.Clear();
                    clearList = false;
                }
                paletteData.filePath = path;
                _paletteDataList.Add(paletteData);
            }
        }

        public static void Clear()
        {
            instance.paletteDataList.Clear();
            instance.paletteDataList.Add(new PaletteData("Palette", System.DateTime.Now.ToBinary()));
            instance._selectedPaletteIdx = 0;
            instance._selectedBrushIdx = -1;
            instance._idxSelection.Clear();
            instance._pickingBrushes = false;
        }

        public static bool showBrushName
        {
            get => instance._showBrushName;
            set
            {
                if (instance._showBrushName == value) return;
                instance._showBrushName = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }

        public static bool viewList
        {
            get => instance._viewList;
            set
            {
                if (instance._viewList == value) return;
                instance._viewList = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }
        public static bool showTabsInMultipleRows
        {
            get => instance._showTabsInMultipleRows;
            set
            {
                if (instance._showTabsInMultipleRows == value) return;
                instance._showTabsInMultipleRows = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }
        public static void ClearPaletteList() => instance._paletteDataList.Clear();
        public static void AddPalette(PaletteData palette)
        {
            instance._paletteDataList.Add(palette);
            palette.filePath = PWBData.palettesDirectory + "/" + PaletteData.GetFileNameFromData(palette);
            palette.Save();
        }

        public static void RemovePaletteAt(int paletteIdx)
        {
            var filePath = instance._paletteDataList[paletteIdx].filePath;
            var thumbnailFolderPath = instance._paletteDataList[paletteIdx].thumbnailsPath;
            instance._paletteDataList.RemoveAt(paletteIdx);
            var metapath = filePath + ".meta";
            if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            metapath = thumbnailFolderPath + ".meta";
            if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
            if (System.IO.Directory.Exists(thumbnailFolderPath))
                System.IO.Directory.Delete(thumbnailFolderPath, true);
            PWBCore.AssetDatabaseRefresh();
        }

        public static void SwapPalette(int from, int to)
        {
            if (from == to) return;
            instance.paletteDataList.Insert(to, instance.paletteDataList[from]);
            var removeIdx = from;
            if (from > to) ++removeIdx;
            instance.paletteDataList.RemoveAt(removeIdx);
        }

        public static void SelectNextPalette()
        {
            if (PrefabPalette.instance == null) return;
            if (paletteCount <= 1) return;
            instance._idxSelection.Clear();

            var idsDic = paletteData.Select((palette, index) => new { palette, index })
                        .ToDictionary(item => item.index, item => item.palette);
            var sortedDic = (from item in idsDic orderby item.Value.name ascending select item)
                .ToDictionary(pair => pair.Key, pair => pair.Value.id);

            var selectedId = selectedPalette.id;
            int sortedIdx = -1;
            var stop = false;

            foreach (var idx in sortedDic.Keys)
            {
                if (sortedIdx == -1) sortedIdx = idx;
                if (stop)
                {
                    sortedIdx = idx;
                    break;
                }
                stop = sortedDic[idx] == selectedId;
            }

            PrefabPalette.instance.SelectPalette(sortedIdx);
            selectedBrushIdx = 0;
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
            PrefabPalette.RepainWindow();
        }

        public static void SelectPreviousPalette()
        {
            if (PrefabPalette.instance == null) return;
            if (paletteCount <= 1) return;
            instance._idxSelection.Clear();

            var idsDic = paletteData.Select((palette, index) => new { palette, index })
                        .ToDictionary(item => item.index, item => item.palette);
            var sortedDic = (from item in idsDic orderby item.Value.name descending select item)
                .ToDictionary(pair => pair.Key, pair => pair.Value.id);

            var selectedId = selectedPalette.id;
            int sortedIdx = -1;
            var stop = false;

            foreach (var idx in sortedDic.Keys)
            {
                if (sortedIdx == -1) sortedIdx = idx;
                if (stop)
                {
                    sortedIdx = idx;
                    break;
                }
                stop = sortedDic[idx] == selectedId;
            }

            PrefabPalette.instance.SelectPalette(sortedIdx);
            selectedBrushIdx = 0;
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
            PrefabPalette.RepainWindow();
        }

        public static string[] paletteNames => instance.paletteDataList.Select(p => p.name).ToArray();
        public static long[] paletteIds => instance.paletteDataList.Select(p => p.id).ToArray();

        public static int selectedPaletteIdx
        {
            get
            {
                instance._selectedPaletteIdx = Mathf.Clamp(instance._selectedPaletteIdx, 0,
                    Mathf.Max(instance.paletteDataList.Count - 1, 0));
                return instance._selectedPaletteIdx;
            }
            set
            {
                value = Mathf.Max(value, 0);
                if (instance._selectedPaletteIdx == value) return;
                instance._selectedPaletteIdx = value;
                if (OnPaletteChanged != null) OnPaletteChanged();
            }
        }

        public static int selectedBrushIdx
        {
            get => instance._selectedBrushIdx;
            set
            {
                if (instance._selectedBrushIdx == value) return;
                instance._selectedBrushIdx = value;
                if (selectedBrush != null)
                {
                    selectedBrush.UpdateBottomVertices();
                    selectedBrush.UpdateAssetTypes();
                }
                else instance._selectedBrushIdx = -1;
                BrushstrokeManager.UpdateBrushstroke(true);
                if (ToolManager.tool == ToolManager.PaintTool.PIN) PWBIO.ResetPinValues();
                if (OnBrushChanged != null) OnBrushChanged();
            }
        }

        public static bool pickingBrushes
        {
            get => instance._pickingBrushes;
            set
            {
                if (instance._pickingBrushes == value) return;
                instance._pickingBrushes = value;
                if (instance._pickingBrushes)
                {
                    PWBCore.UpdateTempColliders();
                    PWBIO.repaint = true;
                    UnityEditor.SceneView.RepaintAll();
                }
                PrefabPalette.RepainWindow();
            }
        }

        public static int iconSize
        {
            get => instance._iconSize;
            set
            {
                if (instance._iconSize == value) return;
                instance._iconSize = value;
                PWBCore.staticData.SaveAndUpdateVersion();
            }
        }

        public static void SelectBrush(int idx)
        {
            if (PrefabPalette.instance == null) return;
            if (selectedPalette.brushCount == 0) return;
            if (!PrefabPalette.instance.FilteredBrushListContains(idx)) return;
            instance._idxSelection.Clear();
            selectedBrushIdx = idx;
            if (selectedBrush != null)
            {
                selectedBrush.UpdateBottomVertices();
                selectedBrush.UpdateAssetTypes();
            }
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
            PrefabPalette.RepainWindow();
        }

        public static void SelectNextBrush()
        {
            if (PrefabPalette.instance == null) return;
            if (selectedPalette.brushCount <= 1) return;
            instance._idxSelection.Clear();
            int selectedIdx = instance._selectedBrushIdx;
            int count = 0;
            do
            {
                selectedIdx = (selectedIdx + 1) % selectedPalette.brushCount;
                if (++count > selectedPalette.brushCount) return;
            }
            while (!PrefabPalette.instance.FilteredBrushListContains(selectedIdx));
            selectedBrushIdx = selectedIdx;
            if (selectedBrush != null)
            {
                selectedBrush.UpdateBottomVertices();
                selectedBrush.UpdateAssetTypes();
            }
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
        }
        public static void SelectPreviousBrush()
        {
            if (PrefabPalette.instance == null) return;
            if (selectedPalette.brushCount <= 1) return;
            instance._idxSelection.Clear();
            int selectedIdx = instance._selectedBrushIdx;
            int count = 0;
            do
            {
                selectedIdx = (selectedIdx == 0 ? selectedPalette.brushCount : selectedIdx) - 1;
                if (++count > selectedPalette.brushCount) return;
            }
            while (!PrefabPalette.instance.FilteredBrushListContains(selectedIdx));
            selectedBrushIdx = selectedIdx;
            if (selectedBrush != null)
            {
                selectedBrush.UpdateBottomVertices();
                selectedBrush.UpdateAssetTypes();
            }
            AddToSelection(selectedBrushIdx);
            PrefabPalette.instance.FrameSelectedBrush();
        }

        public static MultibrushSettings selectedBrush
            => instance._selectedBrushIdx < 0 ? null : selectedPalette.GetBrush(instance._selectedBrushIdx);

        public static PaletteData selectedPalette => instance.paletteDataList[selectedPaletteIdx];

        public static int paletteCount => instance.paletteDataList.Count;

        public static MultibrushSettings GetBrushById(long id)
        {
            foreach (var palette in instance.paletteDataList)
                foreach (var brush in palette.brushes)
                    if (brush.id == id) return brush;
            return null;
        }

        public static MultibrushSettings GetBrushByItemId(long id)
        {
            foreach (var palette in instance.paletteDataList)
                foreach (var brush in palette.brushes)
                    foreach (var item in brush.items)
                        if (item.id == id) return brush;
            return null;
        }

        public static bool BrushExist(long id) => instance.paletteDataList.Exists(b => b.id == id);
        public static int GetBrushIdx(long id)
        {
            var palette = selectedPalette;
            var brushes = palette.brushes;
            for (int i = 0; i < brushes.Length; ++i)
                if (brushes[i].id == id) return i;
            return -1;
        }

        public static int[] idxSelection
        {
            get => instance._idxSelection.ToArray();
            set
            {
                instance._idxSelection = new System.Collections.Generic.HashSet<int>(value);
                if (OnSelectionChanged != null) OnSelectionChanged();
            }
        }
        public static int selectionCount
        {
            get
            {
                if (instance._idxSelection.Count == 0 && instance._selectedBrushIdx > 0 && selectedBrush != null)
                {
                    instance._idxSelection.Add(instance._selectedBrushIdx);
                    if (OnSelectionChanged != null) OnSelectionChanged();
                }
                return instance._idxSelection.Count;
            }
        }
        public static void AddToSelection(int index)
        {
            instance._idxSelection.Add(index);
            if (OnSelectionChanged != null) OnSelectionChanged();
        }
        public static bool SelectionContains(int index) => instance._idxSelection.Contains(index);
        public static void RemoveFromSelection(int index)
        {
            instance._idxSelection.Remove(index);
            if (OnSelectionChanged != null) OnSelectionChanged();
        }
        public static void ClearSelection(bool updateBrushProperties = true)
        {
            selectedBrushIdx = -1;
            instance._idxSelection.Clear();
            if (!updateBrushProperties) return;
            if (OnSelectionChanged != null) OnSelectionChanged();
            BrushProperties.RepaintWindow();
        }

        public static void UpdateSelectedThumbnails()
        {
            foreach (var idx in instance._idxSelection) selectedPalette.GetBrush(idx).UpdateThumbnail();
        }

        public static void UpdateAllThumbnails()
        {
            var palettes = instance.paletteDataList.ToArray();
            foreach (var palette in palettes) palette.UpdateAllThumbnails();
        }
        public static PaletteData GetPalette(MultibrushSettings brush)
        {
            foreach (var palette in instance.paletteDataList)
                if (palette.ContainsBrush(brush)) return palette;
            return null;
        }

        public static PaletteData GetPalette(long id)
        {
            foreach (var palette in instance.paletteDataList)
                if (palette.id == id) return palette;
            return null;
        }

        public static string[] GetPaletteThumbnailFolderPaths()
        {
            var paths = new string[instance.paletteDataList.Count];
            for (int i = 0; i < paths.Length; ++i) paths[i] = instance.paletteDataList[i].thumbnailsPath;
            return paths;
        }
        public static void Cleanup()
        {
            foreach (var palette in instance.paletteDataList.ToArray()) palette.Cleanup();
        }
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _loadPaletteFiles = true;
        }

        private static bool _savePending = false;
        public static bool savePending => _savePending;
        public static void SetSavePending() => _savePending = true;

        private static void SavePalettes()
        {
            foreach (var palette in instance.paletteDataList) palette.Save();
        }
        public static void SaveIfPending()
        {
            if (_savePending) SavePalettes();
            _savePending = false;
        }

        #region CLIPBOARD
        private static BrushSettings _clipboardSettings = null;
        private static ThumbnailSettings _clipboardThumbnailSettings = null;
        public enum Trit { FALSE, TRUE, SAME }
        private static Trit _clipboardOverwriteThumbnailSettings = Trit.FALSE;
        public static BrushSettings clipboardSetting { get => _clipboardSettings; set => _clipboardSettings = value; }
        public static ThumbnailSettings clipboardThumbnailSettings
        { get => _clipboardThumbnailSettings; set => _clipboardThumbnailSettings = value; }
        public static Trit clipboardOverwriteThumbnailSettings
        { get => _clipboardOverwriteThumbnailSettings; set => _clipboardOverwriteThumbnailSettings = value; }

        #endregion
    }

    public class PaletteAssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool repaintPalette = false;
            foreach (var path in importedAssets)
            {
                if (PaletteManager.selectedPalette.ContainsPrefabPath(path))
                {
                    repaintPalette = true;
                    break;
                }
            }
            foreach (var path in deletedAssets)
            {
                if (PaletteManager.selectedPalette.ContainsPrefabPath(path))
                {
                    PaletteManager.Cleanup();
                    PaletteManager.ClearSelection();
                    repaintPalette = true;
                    break;
                }
            }
            if (repaintPalette) PrefabPalette.OnChangeRepaint();
        }
    }
}
