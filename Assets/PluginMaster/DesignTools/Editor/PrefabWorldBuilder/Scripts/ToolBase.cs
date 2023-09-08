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
    #region MANAGER
    public static class ToolProfile
    {
        public const string DEFAULT = "Default";
    }

    public interface IToolManager
    {
        string selectedProfileName { get; set; }
        string[] profileNames { get; }
        void SaveProfile();
        void SaveProfileAs(string name);
        void DeleteProfile();
        void Revert();
        void FactoryReset();
    }

    [System.Serializable]
    public class ToolManagerBase<TOOL_SETTINGS> : IToolManager, ISerializationCallbackReceiver
        where TOOL_SETTINGS : IToolSettings, new()
    {
        protected static ToolManagerBase<TOOL_SETTINGS> _instance = null;
        private static System.Collections.Generic.Dictionary<string, TOOL_SETTINGS> _staticProfiles
            = new System.Collections.Generic.Dictionary<string, TOOL_SETTINGS>
        { { ToolProfile.DEFAULT, new TOOL_SETTINGS() } };
        [SerializeField] private string[] _profileKeys = { ToolProfile.DEFAULT };
        [SerializeField] private TOOL_SETTINGS[] _profileValues = { new TOOL_SETTINGS() };
        private static string _staticSelectedProfileName = ToolProfile.DEFAULT;
        [SerializeField] private string _selectedProfileName = _staticSelectedProfileName;
        private static TOOL_SETTINGS _staticUnsavedProfile = new TOOL_SETTINGS();
        [SerializeField] private TOOL_SETTINGS _unsavedProfile = _staticUnsavedProfile;

        protected ToolManagerBase() { }
        public static ToolManagerBase<TOOL_SETTINGS> instance
        {
            get
            {
                if (_instance == null) _instance = new ToolManagerBase<TOOL_SETTINGS>();
                return _instance;
            }
        }
        public static TOOL_SETTINGS settings => _staticUnsavedProfile;

        private void UpdateUnsaved()
        {
            var tool = ToolManager.GetToolFromSettings(settings);
            if (ToolManager.tool == ToolManager.PaintTool.NONE || tool != ToolManager.tool) return;
            if (!_staticProfiles.ContainsKey(_staticSelectedProfileName))
                _staticSelectedProfileName = ToolProfile.DEFAULT;
            _staticUnsavedProfile.Copy(_staticProfiles[_staticSelectedProfileName]);
        }

        public string selectedProfileName
        {
            get => _staticSelectedProfileName;
            set
            {
                if (_staticSelectedProfileName == value) return;
                _staticSelectedProfileName = value;
                UpdateUnsaved();
                _staticUnsavedProfile.DataChanged();
            }
        }

        public string[] profileNames => _staticProfiles.Keys.ToArray();
        public void SaveProfile()
        {
            _staticProfiles[_staticSelectedProfileName].Copy(_staticUnsavedProfile);
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void SaveProfileAs(string name)
        {
            if (!_staticProfiles.ContainsKey(name))
            {
                var newProfile = new TOOL_SETTINGS();
                newProfile.Copy(_unsavedProfile);
                _staticProfiles.Add(name, newProfile);
            }
            else _staticProfiles[name].Copy(_staticUnsavedProfile);
            _staticSelectedProfileName = name;
            UpdateUnsaved();
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void DeleteProfile()
        {
            if (_staticSelectedProfileName == ToolProfile.DEFAULT) return;
            _staticProfiles.Remove(_staticSelectedProfileName);
            _staticSelectedProfileName = ToolProfile.DEFAULT;
            _staticUnsavedProfile.Copy(_staticProfiles[ToolProfile.DEFAULT]);
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void Revert()
        {
            UpdateUnsaved();
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }

        public void FactoryReset()
        {
            _staticUnsavedProfile = new TOOL_SETTINGS();
            _staticUnsavedProfile.DataChanged();
            PWBCore.staticData.SaveAndUpdateVersion();
        }

        public void CopyToolSettings(TOOL_SETTINGS value) => _staticUnsavedProfile.Copy(value);
        public virtual void OnBeforeSerialize()
        {
            _selectedProfileName = _staticSelectedProfileName;
            _profileKeys = _staticProfiles.Keys.ToArray();
            _profileValues = _staticProfiles.Values.ToArray();
        }

        public virtual void OnAfterDeserialize()
        {
            _staticSelectedProfileName = _selectedProfileName;
            if (_profileKeys.Length > 1)
            {
                _staticProfiles.Clear();
                for (int i = 0; i < _profileKeys.Length; ++i) _staticProfiles.Add(_profileKeys[i], _profileValues[i]);
            }
        }
    }

    [System.Serializable]
    public class PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>
        : ToolManagerBase<TOOL_SETTINGS>
        where TOOL_NAME : IToolName, new()
        where TOOL_SETTINGS : ICloneableToolSettings, new()
        where CONTROL_POINT : ControlPoint, new()
        where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
        where SCENE_DATA : SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>, new()
    {
        private static System.Collections.Generic.List<SCENE_DATA> _staticSceneItems = null;
        [SerializeField] private System.Collections.Generic.List<SCENE_DATA> _sceneItems = _staticSceneItems;
        protected PersistentToolManagerBase() { }
        public new static PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA> instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PersistentToolManagerBase
                        <TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>();
                return _instance as PersistentToolManagerBase<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA, SCENE_DATA>;
            }
        }
        public void AddPersistentItem(string sceneGUID, TOOL_DATA data)
        {
            if (_staticSceneItems == null)
                _staticSceneItems = new System.Collections.Generic.List<SCENE_DATA>();
            var sceneItem = _staticSceneItems.Find(i => i.sceneGUID == sceneGUID);
            if (sceneItem == null)
            {
                sceneItem = new SCENE_DATA();
                sceneItem.sceneGUID = sceneGUID;
                _staticSceneItems.Add(sceneItem);
            }
            if (sceneItem.items != null)
            {
                var item = sceneItem.items.Find(i => i.id == data.id);
                if (item != null) return;
            }
            sceneItem.AddItem(data);
            PWBCore.staticData.SaveAndUpdateVersion();
        }


        public TOOL_DATA[] GetPersistentItems()
        {
            var items = new System.Collections.Generic.List<TOOL_DATA>();
            var openedSceneCount = UnityEditor.SceneManagement.EditorSceneManager.sceneCount;
            if (_staticSceneItems != null)
            {
                for (int i = 0; i < openedSceneCount; ++i)
                {
                    string sceneGUID = UnityEditor.AssetDatabase.AssetPathToGUID
                        (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path);
                    var data = _staticSceneItems.Find(item => item.sceneGUID == sceneGUID);
                    if (data == null)
                    {
                        _staticSceneItems.Remove(data);
                        continue;
                    }
                    items.AddRange(data.items);
                }
            }
            return items.ToArray();
        }

        public bool ReplaceObject(GameObject target, GameObject obj)
        {
            var items = GetPersistentItems();
            foreach(var item in items)
                if (item.ReplaceObject(target, obj)) return true;
            return false;
        }

        public void RemovePersistentItem(long itemId)
        {
            foreach (var item in _staticSceneItems) item.RemoveItemData(itemId);
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public void DeletePersistentItem(long itemId, bool deleteObjects)
        {
            ToolProperties.RegisterUndo("Delete Item");
            var parents = new System.Collections.Generic.List<GameObject>();
            foreach (var item in _staticSceneItems)
            {
                var itemParents = item.GetParents(itemId);
                foreach (var parent in itemParents)
                    if (!parents.Contains(parent)) parents.Add(parent);
                item.DeleteItemData(itemId, deleteObjects);
            }

            foreach (var parent in parents)
            {
                var components = parent.GetComponentsInChildren<Component>();
                if (components.Length == 1) UnityEditor.Undo.DestroyObjectImmediate(parent);
            }
            PWBCore.staticData.SaveAndUpdateVersion();
        }
        public TOOL_DATA GetItem(long itemId)
        {
            var items = GetPersistentItems();
            foreach (var item in items)
                if (item.id == itemId) return item;
            return null;
        }

        public TOOL_DATA GetItem(string hexItemId)
        {
            var splittedId = hexItemId.Split('_');
            if (splittedId.Length != 2) return null;
            var itemId = long.Parse(splittedId[1], System.Globalization.NumberStyles.AllowHexSpecifier);
            return GetItem(itemId);
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            _sceneItems = _staticSceneItems;
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _staticSceneItems = _sceneItems;
        }
    }
    #endregion
    #region SETTINGS
    public interface IToolSettings
    {
        void DataChanged();
        void Copy(IToolSettings other);
    }

    public interface ICloneableToolSettings : IToolSettings
    {
        void Clone(ICloneableToolSettings clone);
    }

    [System.Serializable]
    public class CircleToolBase : IToolSettings
    {
        [SerializeField] private float _radius = 1f;

        public float radius
        {
            get => _radius;
            set
            {
                value = Mathf.Max(value, 0.05f);
                if (_radius == value) return;
                _radius = value;
                DataChanged();
            }
        }

        public virtual void Copy(IToolSettings other)
        {
            var otherCircleToolBase = other as CircleToolBase;
            if (otherCircleToolBase == null) return;
            _radius = otherCircleToolBase._radius;
        }


        public virtual void DataChanged() => PWBCore.SetSavePending();


    }

    [System.Serializable]
    public class BrushToolBase : CircleToolBase, IPaintToolSettings
    {
        [SerializeField] private PaintToolSettings _paintTool = new PaintToolSettings();
        public enum BrushShape { POINT, CIRCLE, SQUARE }
        [SerializeField] protected BrushShape _brushShape = BrushShape.CIRCLE;
        [SerializeField] private int _density = 50;
        [SerializeField] private bool _orientAlongBrushstroke = false;
        [SerializeField] private Vector3 _additionalOrientationAngle = Vector3.zero;
        public enum SpacingType { AUTO, CUSTOM }
        [SerializeField] private SpacingType _spacingType = SpacingType.AUTO;
        [SerializeField] protected float _minSpacing = 1f;
        [SerializeField] private bool _randomizePositions = true;
        [SerializeField] private float _randomness = 1f;
        public BrushToolBase() : base() => _paintTool.OnDataChanged += DataChanged;

        public BrushShape brushShape
        {
            get => _brushShape;
            set
            {
                if (_brushShape == value) return;
                _brushShape = value;
                DataChanged();
            }
        }
        public int density
        {
            get => _density;
            set
            {
                value = Mathf.Clamp(value, 0, 100);
                if (_density == value) return;
                _density = value;
                DataChanged();
            }
        }
        public bool orientAlongBrushstroke
        {
            get => _orientAlongBrushstroke;
            set
            {
                if (_orientAlongBrushstroke == value) return;
                _orientAlongBrushstroke = value;
                DataChanged();
            }
        }
        public Vector3 additionalOrientationAngle
        {
            get => _additionalOrientationAngle;
            set
            {
                if (_additionalOrientationAngle == value) return;
                _additionalOrientationAngle = value;
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
        public float minSpacing
        {
            get => _minSpacing;
            set
            {
                if (_minSpacing == value) return;
                _minSpacing = value;
                DataChanged();
            }
        }
        public bool randomizePositions
        {
            get => _randomizePositions;
            set
            {
                if (_randomizePositions == value) return;
                _randomizePositions = value;
                DataChanged();
            }
        }

        public float randomness
        {
            get => _randomness;
            set
            {
                value = Mathf.Clamp01(value);
                if (_randomness == value) return;
                _randomness = value;
                DataChanged();
            }
        }

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
            set => _paintTool.overwriteBrushProperties = value;
        }

        public BrushSettings brushSettings => _paintTool.brushSettings;

        public override void DataChanged()
        {
            base.DataChanged();
            BrushstrokeManager.UpdateBrushstroke();
        }

        public override void Copy(IToolSettings other)
        {
            var otherBrushToolBase = other as BrushToolBase;
            if (otherBrushToolBase == null) return;
            base.Copy(other);
            _paintTool.Copy(otherBrushToolBase._paintTool);
            _brushShape = otherBrushToolBase._brushShape;
            _density = otherBrushToolBase.density;
            _orientAlongBrushstroke = otherBrushToolBase._orientAlongBrushstroke;
            _additionalOrientationAngle = otherBrushToolBase._additionalOrientationAngle;
            _spacingType = otherBrushToolBase._spacingType;
            _minSpacing = otherBrushToolBase._minSpacing;
            _randomizePositions = otherBrushToolBase._randomizePositions;
        }
    }

    public interface IPaintToolSettings
    {
        bool autoCreateParent { get; set; }
        bool setSurfaceAsParent { get; set; }

        bool createSubparentPerPalette { get; set; }
        bool createSubparentPerTool { get; set; }
        bool createSubparentPerBrush { get; set; }
        bool createSubparentPerPrefab { get; set; }
        Transform parent { get; set; }
        bool overwritePrefabLayer { get; set; }
        int layer { get; set; }
        bool overwriteBrushProperties { get; set; }
        BrushSettings brushSettings { get; }
    }

    [System.Serializable]
    public class PaintToolSettings : IPaintToolSettings, ISerializationCallbackReceiver, IToolSettings
    {
        private Transform _parent = null;
        [SerializeField] private string _parentGlobalId = null;
        [SerializeField] private bool _autoCreateParent = true;
        [SerializeField] private bool _setSurfaceAsParent = false;
        [SerializeField] private bool _createSubparentPerPalette = true;
        [SerializeField] private bool _createSubparentPerTool = true;
        [SerializeField] private bool _createSubparentPerBrush = false;
        [SerializeField] private bool _createSubparentPerPrefab = true;
        [SerializeField] private bool _overwritePrefabLayer = false;
        [SerializeField] private int _layer = 0;
        [SerializeField] private bool _overwriteBrushProperties = false;
        [SerializeField] private BrushSettings _brushSettings = new BrushSettings();

        public System.Action OnDataChanged;

        public PaintToolSettings()
        {
            OnDataChanged += DataChanged;
            _brushSettings.OnDataChangedAction += DataChanged;
        }

        public Transform parent
        {
            get
            {
                if (_parent == null && _parentGlobalId != null)
                {
                    if (UnityEditor.GlobalObjectId.TryParse(_parentGlobalId, out UnityEditor.GlobalObjectId id))
                    {
                        var obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                        if (obj == null) _parentGlobalId = null;
                        else _parent = obj.transform;
                    }
                }
                return _parent;
            }
            set
            {
                if (_parent == value) return;
                _parent = value;
                _parentGlobalId = _parent == null ? null
                    : UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(_parent.gameObject).ToString();
                OnDataChanged();
            }
        }
        public bool autoCreateParent
        {
            get => _autoCreateParent;
            set
            {
                if (_autoCreateParent == value) return;
                _autoCreateParent = value;
                OnDataChanged();
            }
        }

        public bool setSurfaceAsParent
        {
            get => _setSurfaceAsParent;
            set
            {
                if (_setSurfaceAsParent == value) return;
                _setSurfaceAsParent = value;
                OnDataChanged();
            }
        }
        public bool createSubparentPerPalette
        {
            get => _createSubparentPerPalette;
            set
            {
                if (_createSubparentPerPalette == value) return;
                _createSubparentPerPalette = value;
                OnDataChanged();
            }
        }
        public bool createSubparentPerTool
        {
            get => _createSubparentPerTool;
            set
            {
                if (_createSubparentPerTool == value) return;
                _createSubparentPerTool = value;
                OnDataChanged();
            }
        }
        public bool createSubparentPerBrush
        {
            get => _createSubparentPerBrush;
            set
            {
                if (_createSubparentPerBrush == value) return;
                _createSubparentPerBrush = value;
                OnDataChanged();
            }
        }

        public bool createSubparentPerPrefab
        {
            get => _createSubparentPerPrefab;
            set
            {
                if (_createSubparentPerPrefab == value) return;
                _createSubparentPerPrefab = value;
                OnDataChanged();
            }
        }
        public bool overwritePrefabLayer
        {
            get => _overwritePrefabLayer;
            set
            {
                if (_overwritePrefabLayer == value) return;
                _overwritePrefabLayer = value;
                OnDataChanged();
            }
        }

        public int layer
        {
            get => _layer;
            set
            {
                if (_layer == value) return;
                _layer = value;
                OnDataChanged();
            }
        }

        public bool overwriteBrushProperties
        {
            get => _overwriteBrushProperties;
            set
            {
                if (_overwriteBrushProperties == value) return;
                _overwriteBrushProperties = value;
                OnDataChanged();
            }
        }

        public BrushSettings brushSettings => _brushSettings;

        public virtual void DataChanged() => PWBCore.SetSavePending();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => _parent = null;

        public virtual void Copy(IToolSettings other)
        {
            var otherPaintToolSettings = other as PaintToolSettings;
            if (otherPaintToolSettings == null) return;
            _parent = otherPaintToolSettings._parent;
            _parentGlobalId = otherPaintToolSettings._parentGlobalId;
            _overwritePrefabLayer = otherPaintToolSettings._overwritePrefabLayer;
            _layer = otherPaintToolSettings._layer;
            _autoCreateParent = otherPaintToolSettings._autoCreateParent;
            _createSubparentPerPrefab = otherPaintToolSettings._createSubparentPerPrefab;
            _overwriteBrushProperties = otherPaintToolSettings._overwriteBrushProperties;
            _brushSettings.Copy(otherPaintToolSettings._brushSettings);
        }
    }

    public interface IPaintOnSurfaceToolSettings
    {
        bool paintOnMeshesWithoutCollider { get; set; }
        bool paintOnSelectedOnly { get; set; }
        bool paintOnPalettePrefabs { get; set; }
    }

    public abstract class PaintOnSurfaceToolSettingsBase : IPaintOnSurfaceToolSettings
    {
        public abstract bool paintOnMeshesWithoutCollider { get; set; }
        public abstract bool paintOnSelectedOnly { get; set; }
        public abstract bool paintOnPalettePrefabs { get; set; }
        public enum PaintMode
        {
            AUTO,
            ON_SURFACE,
            ON_SHAPE
        }
    }

    [System.Serializable]
    public class PaintOnSurfaceToolSettings : PaintOnSurfaceToolSettingsBase,
        ISerializationCallbackReceiver, ICloneableToolSettings
    {
        [SerializeField] private bool _paintOnMeshesWithoutCollider = false;
        [SerializeField] private bool _paintOnSelectedOnly = false;
        [SerializeField] private bool _paintOnPalettePrefabs = false;
        [SerializeField] private PaintMode _mode = PaintMode.AUTO;
        [SerializeField] private bool _paralellToTheSurface = true;

        private bool _updateMeshColliders = false;
        public System.Action OnDataChanged;

        public PaintOnSurfaceToolSettings() => OnDataChanged += DataChanged;
        public PaintMode mode
        {
            get => _mode;
            set
            {
                if (_mode == value) return;
                _mode = value;
                OnDataChanged();
            }
        }
        public bool perpendicularToTheSurface
        {
            get => _paralellToTheSurface;
            set
            {
                if (_paralellToTheSurface == value) return;
                _paralellToTheSurface = value;
                OnDataChanged();
            }
        }

        public override bool paintOnMeshesWithoutCollider
        {
            get
            {
                if (PWBCore.staticData.tempCollidersAction == PWBData.TempCollidersAction.NEVER_CREATE)
                    return false;
                if (_updateMeshColliders)
                {
                    _updateMeshColliders = false;
                    PWBCore.UpdateTempColliders();
                }
                return _paintOnMeshesWithoutCollider;
            }
            set
            {
                if (_paintOnMeshesWithoutCollider == value) return;
                _paintOnMeshesWithoutCollider = value;
                OnDataChanged();
                if (_paintOnMeshesWithoutCollider) PWBCore.UpdateTempColliders();
            }
        }
        public override bool paintOnSelectedOnly
        {
            get => _paintOnSelectedOnly;
            set
            {
                if (_paintOnSelectedOnly == value) return;
                _paintOnSelectedOnly = value;
                OnDataChanged();
            }
        }

        public override bool paintOnPalettePrefabs
        {
            get => _paintOnPalettePrefabs;
            set
            {
                if (_paintOnPalettePrefabs == value) return;
                _paintOnPalettePrefabs = value;
                OnDataChanged();
            }
        }

        public virtual void Copy(IToolSettings other)
        {
            var otherPaintOnSurfaceToolSettings = other as PaintOnSurfaceToolSettings;
            if (otherPaintOnSurfaceToolSettings == null) return;
            _paintOnMeshesWithoutCollider = otherPaintOnSurfaceToolSettings._paintOnMeshesWithoutCollider;
            _paintOnSelectedOnly = otherPaintOnSurfaceToolSettings._paintOnSelectedOnly;
            _paintOnPalettePrefabs = otherPaintOnSurfaceToolSettings._paintOnPalettePrefabs;
            _mode = otherPaintOnSurfaceToolSettings._mode;
            _paralellToTheSurface = otherPaintOnSurfaceToolSettings._paralellToTheSurface;
        }
        public virtual void DataChanged() => PWBCore.SetSavePending();
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => _updateMeshColliders = _paintOnMeshesWithoutCollider;

        public virtual void Clone(ICloneableToolSettings clone)
        {
            if (clone == null && !(clone is PaintToolSettings)) clone = new PaintOnSurfaceToolSettings();
            var PaintOnSurfaceToolClone = clone as PaintOnSurfaceToolSettings;
            PaintOnSurfaceToolClone.Copy(this);
        }
    }

    [System.Serializable]
    public class SelectionToolBaseBasic : ICloneableToolSettings
    {
        [SerializeField] private bool _embedInSurface = false;
        [SerializeField] private bool _embedAtPivotHeight = false;
        [SerializeField] private float _surfaceDistance = 0f;
        [SerializeField] private bool _createTempColliders = true;

        public bool embedInSurface
        {
            get => _embedInSurface;
            set
            {
                if (_embedInSurface == value) return;
                _embedInSurface = value;
                DataChanged();
            }
        }

        public bool embedAtPivotHeight
        {
            get => _embedAtPivotHeight;
            set
            {
                if (_embedAtPivotHeight == value) return;
                _embedAtPivotHeight = value;
                DataChanged();
            }
        }

        public float surfaceDistance
        {
            get => _surfaceDistance;
            set
            {
                if (_surfaceDistance == value) return;
                _surfaceDistance = value;
                DataChanged();
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
        public virtual void Clone(ICloneableToolSettings clone)
        {
            if (clone == null || !(clone is SelectionToolBaseBasic)) clone = new SelectionToolBaseBasic();
            var selectionToolClone = clone as SelectionToolBaseBasic;
            selectionToolClone.Copy(this);
        }

        public virtual void Copy(IToolSettings other)
        {
            var otherSelectionTool = other as SelectionToolBaseBasic;
            if (otherSelectionTool == null) return;
            _embedInSurface = otherSelectionTool._embedInSurface;
            _embedAtPivotHeight = otherSelectionTool._embedAtPivotHeight;
            _surfaceDistance = otherSelectionTool._surfaceDistance;
            _createTempColliders = otherSelectionTool._createTempColliders;
        }

        public virtual void DataChanged() => PWBCore.SetSavePending();
    }

    [System.Serializable]
    public class SelectionToolBase : SelectionToolBaseBasic
    {
        [SerializeField] private bool _rotateToTheSurface = false;

        public bool rotateToTheSurface
        {
            get => _rotateToTheSurface;
            set
            {
                if (_rotateToTheSurface == value) return;
                _rotateToTheSurface = value;
                DataChanged();
            }
        }

        public override void Clone(ICloneableToolSettings clone)
        {
            if (clone == null || !(clone is SelectionToolBase)) clone = new SelectionToolBase();
            var selectionToolClone = clone as SelectionToolBase;
            selectionToolClone.Copy(this);
        }
        public override void Copy(IToolSettings other)
        {
            var otherSelectionTool = other as SelectionToolBase;
            if (otherSelectionTool == null) return;
            base.Copy(other);
            _rotateToTheSurface = otherSelectionTool._rotateToTheSurface;
        }
    }

    public interface IModifierTool
    {
        ModifierToolSettings.Command command { get; set; }
        bool modifyAllButSelected { get; set; }
        bool onlyTheClosest { get; set; }
    }
    [System.Serializable]
    public class ModifierToolSettings : IModifierTool, IToolSettings
    {
        public enum Command
        {
            MODIFY_ALL,
            MODIFY_PALETTE_PREFABS,
            MODIFY_BRUSH_PREFABS
        }

        [SerializeField] private Command _command = Command.MODIFY_ALL;
        [SerializeField] private bool _allButSelected = false;
        [SerializeField] private bool _onlyTheClosest = false;
        public System.Action OnDataChanged;

        public ModifierToolSettings() => OnDataChanged += DataChanged;
        public Command command
        {
            get => _command;
            set
            {
                if (_command == value) return;
                _command = value;
                DataChanged();
            }
        }

        public bool modifyAllButSelected
        {
            get => _allButSelected;
            set
            {
                if (_allButSelected == value) return;
                _allButSelected = value;
                DataChanged();
            }
        }

        public bool onlyTheClosest
        {
            get => _onlyTheClosest;
            set
            {
                if (_onlyTheClosest == value) return;
                _onlyTheClosest = value;
                DataChanged();
            }
        }
        public void DataChanged() => PWBCore.SetSavePending();

        public virtual void Copy(IToolSettings other)
        {
            var otherModifier = other as IModifierTool;
            if (otherModifier == null) return;
            _command = otherModifier.command;
            _allButSelected = otherModifier.modifyAllButSelected;
        }
    }
    #endregion
    #region DATA
    [System.Serializable]
    public class ControlPoint
    {
        public Vector3 position = Vector3.zero;
        public ControlPoint() { }

        public ControlPoint(Vector3 position) => this.position = position;
        public ControlPoint(ControlPoint other) => position = other.position;

        public virtual void Copy(ControlPoint other)
        {
            position = other.position;
        }
        public static implicit operator ControlPoint(Vector3 position) => new ControlPoint(position);
        public static implicit operator Vector3(ControlPoint point) => point.position;
        public static Vector3[] PointArrayToVectorArray(ControlPoint[] array)
            => array.Select(point => point.position).ToArray();
        public static ControlPoint[] VectorArrayToPointArray(Vector3[] array)
            => array.Select(position => new ControlPoint(position)).ToArray();
    }

    [System.Serializable]
    public class ObjectId : System.IEquatable<ObjectId>
    {
        [SerializeField] private int _instanceId;
        [SerializeField] private string _globalObjId;

        public int instanceId { get => _instanceId; set => value = _instanceId; }
        public string globalObjId { get => _globalObjId; set => _globalObjId = value; }

        public ObjectId(GameObject gameObject)
        {
            if (gameObject == null)
            {
                _instanceId = -1;
                _globalObjId = null;
                return;
            }
            _instanceId = gameObject.GetInstanceID();
            _globalObjId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
        }

        public ObjectId(int instanceId, string globalObjId)
        {
            _instanceId = instanceId;
            _globalObjId = globalObjId;
        }
        public override int GetHashCode()
        {
            int hashCode = 917907199;
            hashCode = hashCode * -1521134295 + _instanceId.GetHashCode();
            hashCode = hashCode * -1521134295
                + System.Collections.Generic.EqualityComparer<string>.Default.GetHashCode(_globalObjId);
            return hashCode;
        }
        public bool Equals(ObjectId other) => _instanceId == other._instanceId || _globalObjId == other._globalObjId;
        public override bool Equals(object obj) => obj is ObjectId other && this.Equals(other);
        public static bool operator ==(ObjectId lhs, ObjectId rhs) => lhs.Equals(rhs);
        public static bool operator !=(ObjectId lhs, ObjectId rhs) => !lhs.Equals(rhs);

        public void Copy(ObjectId other)
        {
            _instanceId = other._instanceId;
            _globalObjId = other._globalObjId;
        }

        public static GameObject FindObject(ObjectId objId)
        {
            var obj = UnityEditor.EditorUtility.InstanceIDToObject(objId.instanceId) as GameObject;
            if (obj == null)
            {
                if (UnityEditor.GlobalObjectId.TryParse(objId.globalObjId, out UnityEditor.GlobalObjectId id))
                {
                    obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (obj != null) objId.instanceId = obj.GetInstanceID();
                }
            }
            return obj;
        }
    }

    [System.Serializable]
    public class ObjectPose
    {
        [SerializeField] private ObjectId _id = null;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _localRotation;
        [SerializeField] private Vector3 _localScale;
        private GameObject _object = null;

        public ObjectPose(ObjectId id, Vector3 position, Quaternion localRotation, Vector3 localScale)
        {
            _id = id;
            _position = position;
            _localRotation = localRotation;
            _localScale = localScale;
            _object = ObjectId.FindObject(_id);
        }


        public ObjectId id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                _object = ObjectId.FindObject(_id);
            }
        }

        public GameObject obj
        {
            get
            {
                if (_object == null) _object = ObjectId.FindObject(_id);
                return _object;
            }
        }
        public Vector3 position { get => _position; set => _position = value; }
        public Quaternion localRotation { get => _localRotation; set => _localRotation = value; }
        public Vector3 localScale { get => _localScale; set => _localScale = value; }
        public ObjectPose Clone() => new ObjectPose(_id, _position, _localRotation, _localScale);
        public void Copy(ObjectPose other)
        {
            _position = other._position;
            _localRotation = other._localRotation;
            _localScale = other._localScale;
        }
    }

    public interface IToolName { string value { get; } }
    [System.Serializable]
    public class PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> : ISerializationCallbackReceiver
        where TOOL_NAME : IToolName, new()
        where TOOL_SETTINGS : ICloneableToolSettings, new()
        where CONTROL_POINT : ControlPoint, new()
    {
        #region ID
        private static long _nextId = System.DateTime.Now.Ticks;
        [SerializeField] protected long _id = _nextId;
        public static long nextId => _nextId;
        public static string HexId(long value) => (new TOOL_NAME()).value + "_" + value.ToString("X");
        public static string nextHexId => HexId(_nextId);
        public static void SetNextId() => _nextId = System.DateTime.Now.Ticks;
        public long id => _id;
        public string hexId => HexId(id);
        #endregion
        #region OBJECT POSES
        [SerializeField]
        protected System.Collections.Generic.List<ObjectPose> _objectPoses
            = new System.Collections.Generic.List<ObjectPose>();
        public void UpdateObjects()
        {
            var objPos = _objectPoses.ToArray();
            foreach (var item in objPos)
            {
                var obj = item.obj;
                if (obj == null) _objectPoses.Remove(item);
            }
        }

        public void UpdatePoses()
        {
            var objPos = _objectPoses.ToArray();
            foreach (var item in objPos)
            {
                var obj = item.obj;
                if (obj == null)
                {
                    _objectPoses.Remove(item);
                    continue;
                }
                item.position = obj.transform.position;
                item.localRotation = obj.transform.localRotation;
                item.localScale = obj.transform.localScale;
            }
        }

        public void AddObjects(GameObject[] objects)
        {
            for (int i = 0; i < objects.Length; ++i)
                _objectPoses.Add(new ObjectPose(new ObjectId(objects[i]),
                    objects[i].transform.position, objects[i].transform.localRotation, objects[i].transform.localScale));
        }

        public bool ReplaceObject(GameObject target, GameObject obj)
        {
            int targetIdx = -1;
            var targetId = new ObjectId(target);
            for (int i = 0; i < _objectPoses.Count; ++i)
            {
                var objId = _objectPoses[i].id;
                if(targetId == objId)
                {
                    targetIdx = i;
                    break;
                }
            }
            if (targetIdx == -1) return false;
            _objectPoses.Insert(targetIdx, new ObjectPose(new ObjectId(obj),
                    obj.transform.position, obj.transform.localRotation, obj.transform.localScale));
            _objectPoses.RemoveAt(targetIdx + 1);
            return true;
        }

        public int objectCount => _objectPoses.Count;
        public ObjectPose[] objectPoses => _objectPoses.ToArray();
        public GameObject[] objects
        {
            get
            {
                var objs = new System.Collections.Generic.List<GameObject>();
                foreach (var item in _objectPoses) objs.Add(item.obj);
                return objs.ToArray();
            }
        }

        public System.Collections.Generic.List<GameObject> objectList
        {
            get
            {
                var list = new System.Collections.Generic.List<GameObject>();
                var objPos = _objectPoses.ToArray();
                _objectPoses.Clear();
                for (int i = 0; i < objPos.Length; ++i)
                {
                    var item = objPos[i];
                    if (item == null) continue;
                    var obj = item.obj;
                    if (obj == null) continue;
                    _objectPoses.Add(item);
                    list.Add(obj);
                }
                return list;
            }
        }

        public void Delete()
        {
            var objList = objectList;
            foreach (var obj in objectList) UnityEditor.Undo.DestroyObjectImmediate(obj);
        }

        public virtual void ResetPoses(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> initialData)
        {
            var initialPoses = initialData.objectPoses;
            foreach (var initialPose in initialPoses)
            {
                var pose = _objectPoses.Find(p => p.id == initialPose.id);
                if (pose == null) continue;
                pose.Copy(initialPose);
                if (pose.obj == null) continue;
                UnityEditor.Undo.RecordObject(pose.obj.transform, RESET_COMMAND_NAME);
                pose.obj.transform.position = pose.position;
                pose.obj.transform.localRotation = pose.localRotation;
                pose.obj.transform.localScale = pose.localScale;
                pose.obj.SetActive(true);
            }
            Copy(initialData);
        }

        public GameObject GetParent()
        {
            var parents = new System.Collections.Generic.List<GameObject>();
            var objList = objectList;
            void GetParentList()
            {
                parents.Clear();
                foreach (var obj in objList)
                {
                    if (obj.transform.parent != null)
                    {
                        if (parents.Contains(obj.transform.parent.gameObject)) continue;
                        parents.Add(obj.transform.parent.gameObject);
                    }
                    else
                    {
                        parents.Clear();
                        return;
                    }
                }
            }
            do
            {
                GetParentList();
                objList = parents.ToList();
            }
            while (parents.Count > 1);
            if (parents.Count == 0) return null;
            return parents[0];
        }
        #endregion
        #region CONTROL POINTS
        [SerializeField]
        protected System.Collections.Generic.List<CONTROL_POINT> _controlPoints
            = new System.Collections.Generic.List<CONTROL_POINT>();
        private int _selectedPointIdx = -1;
        protected System.Collections.Generic.List<int> _selection = new System.Collections.Generic.List<int>();
        protected Vector3[] _pointPositions = null;
        private static string _commandName = null;
        public const string RESET_COMMAND_NAME = "Reset persistent pose";
        public static string COMMAND_NAME
        {
            get
            {
                if (_commandName == null) _commandName = "Edit " + (new TOOL_NAME()).value;
                return _commandName;
            }
        }
        public Vector3[] points => _pointPositions;
        public int pointsCount => _pointPositions.Length;
        public Vector3 GetPoint(int idx)
        {
            if (idx < 0) idx += _pointPositions.Length;
            return _pointPositions[idx];
        }
        public Vector3 selectedPoint => _pointPositions[_selectedPointIdx];
        public bool IsSelected(int idx) => _selection.Contains(idx);
        public int selectionCount => _selection.Count;
        public virtual void SetPoint(int idx, Vector3 value, bool registerUndo)
        {
            if (_pointPositions.Length <= 1) Initialize();
            if (idx < 0 || idx >= _pointPositions.Length) return;
            if (_pointPositions[idx] == value) return;
            if (registerUndo) ToolProperties.RegisterUndo(COMMAND_NAME);
            var delta = value - _pointPositions[idx];
            _pointPositions[idx] = _controlPoints[idx].position = value;

            foreach (var selectedIdx in _selection)
            {
                if (selectedIdx == idx) continue;
                _controlPoints[selectedIdx].position += delta;
                _pointPositions[selectedIdx] = _controlPoints[selectedIdx].position;
            }
        }

        public void AddDeltaToSelection(Vector3 delta)
        {
            foreach (var selectedIdx in _selection)
            {
                _controlPoints[selectedIdx].position += delta;
                _pointPositions[selectedIdx] = _controlPoints[selectedIdx].position;
            }
        }

        public void AddValue(int idx, Vector3 value)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints[idx].position += value;
            _pointPositions[idx] = _controlPoints[idx].position;
        }

        protected virtual void UpdatePoints()
            => _pointPositions = ControlPoint.PointArrayToVectorArray(_controlPoints.ToArray());

        public void RemoveSelectedPoints()
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            var toRemove = new System.Collections.Generic.List<int>(_selection);
            if (!toRemove.Contains(_selectedPointIdx)) toRemove.Add(_selectedPointIdx);
            toRemove.Sort();
            if (toRemove.Count >= _pointPositions.Length - 1)
            {
                Initialize();
                return;
            }
            for (int i = toRemove.Count - 1; i >= 0; --i) _controlPoints.RemoveAt(toRemove[i]);
            _selectedPointIdx = -1;
            _selection.Clear();
            UpdatePoints();
        }

        public void InsertPoint(int idx, CONTROL_POINT point)
        {
            if (idx < 0) return;
            idx = Mathf.Max(idx, 1);
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.Insert(idx, point);
            UpdatePoints();
        }

        protected void AddPoint(CONTROL_POINT point, bool registerUndo = true)
        {
            if (registerUndo) ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.Add(point);
            UpdatePoints();
        }
        protected void AddPointRange(System.Collections.Generic.IEnumerable<CONTROL_POINT> collection)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.AddRange(collection);
            UpdatePoints();
        }
        protected void PointsRemoveRange(int index, int count)
        {
            ToolProperties.RegisterUndo(COMMAND_NAME);
            _controlPoints.RemoveRange(index, count);
            UpdatePoints();
        }
        protected CONTROL_POINT[] PointsGetRange(int index, int count) => _controlPoints.GetRange(index, count).ToArray();
        public int selectedPointIdx
        {
            get => _selectedPointIdx;
            set
            {
                if (_selectedPointIdx == value) return;
                _selectedPointIdx = value;
            }
        }
        public void AddToSelection(int idx)
        {
            if (!_selection.Contains(idx)) _selection.Add(idx);
        }
        public void SelectAll()
        {
            _selection.Clear();
            for (int i = 0; i < pointsCount; ++i) _selection.Add(i);
            if (_selectedPointIdx < 0) _selectedPointIdx = 0;
        }
        public void RemoveFromSelection(int idx)
        {
            if (_selection.Contains(idx)) _selection.Remove(idx);
        }
        public void ClearSelection() => _selection.Clear();
        public void Reset() => Initialize();
        #endregion
        #region SETTINGS
        [SerializeField] protected TOOL_SETTINGS _settings = new TOOL_SETTINGS();
        public TOOL_SETTINGS settings { get => _settings; set => _settings = value; }
        #endregion
        #region STATE
        [SerializeField] private ToolManager.ToolState _state = ToolManager.ToolState.NONE;
        public virtual ToolManager.ToolState state
        {
            get => _state;
            set
            {
                if (_state == value) return;
                ToolProperties.RegisterUndo(COMMAND_NAME);
                _state = value;
            }
        }
        #endregion
        #region COMMON
        protected virtual void Initialize()
        {
            _selectedPointIdx = -1;
            _selection.Clear();
            _state = ToolManager.ToolState.NONE;
            _controlPoints.Clear();
            UpdatePoints();
        }

        [SerializeField] protected long _initialBrushId = -1;

        public PersistentData() => Initialize();
        public PersistentData(GameObject[] objects, long initialBrushId,
            PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> data)
        {
            Copy(data);
            _settings = new TOOL_SETTINGS();
            data._settings.Clone(_settings);
            _id = nextId;
            SetNextId();
            _initialBrushId = initialBrushId;
            _selectedPointIdx = -1;
            _selection.Clear();
            _state = ToolManager.ToolState.PERSISTENT;
            if (objects == null || objects.Length == 0) return;
            _objectPoses = new System.Collections.Generic.List<ObjectPose>();
            AddObjects(objects);
        }

        public long initialBrushId => _initialBrushId;
        public void SetInitialBrushId(long value) => _initialBrushId = value;

        protected void Clone(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> clone)
        {
            if (clone == null) clone = new PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>();
            clone._id = id;
            clone._controlPoints.Clear();
            foreach (var point in _controlPoints)
            {
                var pointClone = new CONTROL_POINT();
                pointClone.Copy(point);
                clone._controlPoints.Add(pointClone);
            }
            clone._pointPositions = _pointPositions == null ? null : _pointPositions.ToArray();

            clone._objectPoses = new System.Collections.Generic.List<ObjectPose>();
            foreach (var objPos in _objectPoses) clone._objectPoses.Add(objPos.Clone());
            clone._initialBrushId = _initialBrushId;
            _settings.Clone(clone._settings);

            clone._selectedPointIdx = -1;
            clone._selection.Clear();
        }
        public virtual void Copy(PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT> other)
        {
            _controlPoints.Clear();
            foreach (var point in other._controlPoints)
            {
                var pointClone = new CONTROL_POINT();
                pointClone.Copy(point);
                _controlPoints.Add(pointClone);
            }
            _selectedPointIdx = other._selectedPointIdx;
            _selection = other._selection.ToList();
            _pointPositions = other._pointPositions == null ? null : other._pointPositions.ToArray();

            _settings = other._settings;
            _objectPoses = new System.Collections.Generic.List<ObjectPose>();
            foreach (var objPos in other._objectPoses) _objectPoses.Add(objPos.Clone());
            _initialBrushId = other._initialBrushId;
        }

        private bool _deserializing = false;
        protected bool deserializing { get => _deserializing; set => _deserializing = value; }
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            deserializing = true;
            UpdatePoints();
            deserializing = false;
            PWBIO.repaint = true;
        }
        #endregion
    }

    [System.Serializable]
    public class SceneData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT, TOOL_DATA>
        where TOOL_NAME : IToolName, new()
        where TOOL_SETTINGS : ICloneableToolSettings, new()
        where CONTROL_POINT : ControlPoint, new()
        where TOOL_DATA : PersistentData<TOOL_NAME, TOOL_SETTINGS, CONTROL_POINT>, new()
    {
        [SerializeField] private string _sceneGUID = null;
        [SerializeField] private System.Collections.Generic.List<TOOL_DATA> _items = null;

        public string sceneGUID { get => _sceneGUID; set => _sceneGUID = value; }
        public System.Collections.Generic.List<TOOL_DATA> items => _items;

        public SceneData() { }
        public SceneData(string sceneGUID) => _sceneGUID = sceneGUID;

        public void AddItem(TOOL_DATA data)
        {
            if (_items == null) _items = new System.Collections.Generic.List<TOOL_DATA>();
            _items.Add(data);
        }

        public void RemoveItemData(long itemId) => _items.RemoveAll(i => i.id == itemId);

        public void DeleteItemData(long itemId, bool deleteObjects)
        {
            var item = GetItem(itemId);
            if (item == null) return;
            if (deleteObjects) item.Delete();
            RemoveItemData(itemId);
        }
        public TOOL_DATA GetItem(long itemId) => _items.Find(i => i.id == itemId);

        public GameObject[] GetParents(long itemId)
        {
            var parents = new System.Collections.Generic.List<GameObject>();
            var item = GetItem(itemId);
            if (item == null) return parents.ToArray();
            var objs = item.objects;
            foreach (var obj in objs)
            {
                if (obj == null) continue;
                if (obj.transform.parent == null) continue;
                var parent = obj.transform.parent.gameObject;
                if (parents.Contains(parent)) continue;
                parents.Add(parent);
                do
                {
                    if (parent.transform.parent == null) parent = null;
                    else
                    {
                        parent = parent.transform.parent.gameObject;
                        if (!parents.Contains(parent)) parents.Add(parent);
                    }
                }
                while (parent != null);
            }


            return parents.ToArray();
        }
    }
    #endregion
}
