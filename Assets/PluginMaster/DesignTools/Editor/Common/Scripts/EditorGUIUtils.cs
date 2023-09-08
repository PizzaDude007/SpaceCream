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
    public static class EditorGUIUtils
    {
        #region LAYER MASK FIELD
        public static LayerMask FieldToLayerMask(int field)
        {
            LayerMask mask = 0;
            var layers = UnityEditorInternal.InternalEditorUtility.layers;
            for (int layerIdx = 0; layerIdx < layers.Length; layerIdx++)
            {
                if ((field & (1 << layerIdx)) == 0) continue;
                mask |= 1 << LayerMask.NameToLayer(layers[layerIdx]);
            }
            return mask;
        }

        public static int LayerMaskToField(LayerMask mask)
        {
            int field = 0;
            var layers = UnityEditorInternal.InternalEditorUtility.layers;
            for (int layerIdx = 0; layerIdx < layers.Length; layerIdx++)
            {
                if ((mask & (1 << LayerMask.NameToLayer(layers[layerIdx]))) == 0) continue;
                field |= 1 << layerIdx;
            }
            return field;
        }
        #endregion

        #region CUSTOM FIELDS
        #region AXIS FIELD
        private static Vector3[] directions =
        {
            Vector3.right, Vector3.left,
            Vector3.up, Vector3.down,
            Vector3.forward, Vector3.back
        };
        private static string[] directionNames =
        {
            "+X", "-X",
            "+Y", "-Y",
            "+Z", "-Z"
        };
        public static Vector3 AxisField(string label, Vector3 value)
        {
            int selectedIndex = System.Array.IndexOf(directions, value);
            selectedIndex = UnityEditor.EditorGUILayout.Popup(label, selectedIndex, directionNames);
            return directions[selectedIndex];
        }
        #endregion
        
        #region RANGE FIELD
        public static RandomUtils.Range RangeField(string label, RandomUtils.Range value)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (label != string.Empty)
                {
                    GUILayout.Label(label);
                }
                var prevLabelW = UnityEditor.EditorGUIUtility.labelWidth;
                UnityEditor.EditorGUIUtility.labelWidth = 30;
                GUILayout.Label("Between:");
                value.v1 = UnityEditor.EditorGUILayout.FloatField(value.v1);
                value.v2 = UnityEditor.EditorGUILayout.FloatField(value.v2);
                UnityEditor.EditorGUIUtility.labelWidth = prevLabelW;
            }
            return value;
        }

        public static RandomUtils.Range3 Range3Field(string label, RandomUtils.Range3 value)
        {
            using (new GUILayout.VerticalScope())
            {
                if (label != string.Empty)
                {
                    GUILayout.Label(label);
                }

                GUILayout.Label("Between:");
                value.v1 = UnityEditor.EditorGUILayout.Vector3Field(string.Empty, value.v1);
                value.v2 = UnityEditor.EditorGUILayout.Vector3Field(string.Empty, value.v2);

            }

            return value;
        }
        #endregion

        #region MULTITAG FIELD
        public class MultiTagField
        {
            private const string NOTHING = "Nothing";
            private const string EVERYTHING = "Everything";
            private const string MIXED = "Mixed ...";

            private string _label = null;
            private System.Collections.Generic.List<string> _tags = null;
            private string _key = null;

            public System.Action<System.Collections.Generic.List<string>,
                System.Collections.Generic.List<string>, string> OnChange;

            private MultiTagField(string label, System.Collections.Generic.List<string> tags, string key)
                => (_label, _tags, _key) = (label, tags, key);

            private void SelectTag(object obj)
            {
                var originalList = new System.Collections.Generic.List<string>(_tags);
                var originalSet = new System.Collections.Generic.HashSet<string>(_tags);
                void CheckChange()
                {
                    var newSet = new System.Collections.Generic.HashSet<string>(_tags);
                    if (!originalSet.SetEquals(newSet)) OnChange(originalList, _tags, _key);
                }

                var tag = (string)obj;
                if (tag == NOTHING)
                {
                    _tags.Clear();
                    CheckChange();
                    return;
                }
                if (tag == EVERYTHING)
                {
                    _tags.Clear();
                    _tags.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
                    CheckChange();
                    return;
                }

                if (_tags.Contains(tag)) _tags.Remove(tag);
                else _tags.Add(tag);
                CheckChange();
            }

            private void Show()
            {
                var allTags = UnityEditorInternal.InternalEditorUtility.tags;
                var text = _tags.Count == 0
                    ? NOTHING
                    : _tags.Count == allTags.Length
                        ? EVERYTHING
                        : _tags.Count > 1 ? MIXED : _tags[0];

                using (new GUILayout.HorizontalScope())
                {
                    if (_label != null && _label != string.Empty)
                        GUILayout.Label(_label, GUILayout.Width(UnityEditor.EditorGUIUtility.labelWidth));
                    if (GUILayout.Button(text, UnityEditor.EditorStyles.popup,
                        GUILayout.MinWidth(UnityEditor.EditorGUIUtility.fieldWidth)))
                    {
                        var menu = new UnityEditor.GenericMenu();
                        menu.AddItem(new GUIContent(NOTHING), false, SelectTag, NOTHING);
                        menu.AddItem(new GUIContent(EVERYTHING), false, SelectTag, EVERYTHING);
                        foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
                            menu.AddItem(new GUIContent(tag), _tags.Contains(tag), SelectTag, tag);
                        menu.ShowAsContext();
                    }
                }
            }

            public static MultiTagField Instantiate(string label, System.Collections.Generic.List<string> tags, string key)
            {
                var field = new MultiTagField(label, tags, key);
                field.Show();
                return field;
            }
        }
        #endregion

        #region OBJECT ARRAY FIELD
        public static OBJ_TYPE[] ObjectArrayField<OBJ_TYPE>(string label, OBJ_TYPE[] objArray, ref bool foldout)
            where OBJ_TYPE : UnityEngine.Object
        {
            var size = objArray == null ? 0 : objArray.Length;
            OBJ_TYPE[] result = objArray;
            foldout = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            if (!foldout) return result;
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                using (new GUILayout.VerticalScope())
                {
                    UnityEditor.EditorGUIUtility.labelWidth = 40;
                    size = Mathf.Clamp(UnityEditor.EditorGUILayout.IntField("Size:", size), 0, 10);
                    result = new OBJ_TYPE[size];
                    for (int i = 0; i < size; ++i)
                    {
                        var obj = i < objArray.Length ? objArray[i] : null;
                        result[i] = (OBJ_TYPE)UnityEditor.EditorGUILayout.ObjectField(obj, typeof(OBJ_TYPE), true);
                    }
                }
            }
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
            return result;
        }
        #endregion

        #region OBJECT ARRAY FIELD WITH BUTTONS
        public static OBJ_TYPE[] ObjectArrayFieldWithButtons<OBJ_TYPE>(string label, OBJ_TYPE[] objArray,
            ref bool foldout, out bool changed)
            where OBJ_TYPE : UnityEngine.Object
        {
            var result =  new System.Collections.Generic.List<OBJ_TYPE>();
            int removeIdx = -1;
            foldout = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
            changed = false;
            if (!foldout) return objArray;
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                using (new GUILayout.VerticalScope())
                {
                    if (objArray != null)
                    {
                        foreach (var obj in objArray)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                using (var check = new UnityEditor.EditorGUI.ChangeCheckScope())
                                {
                                    result.Add((OBJ_TYPE)UnityEditor.EditorGUILayout.ObjectField(obj,
                                        typeof(OBJ_TYPE), true));
                                    if(check.changed) changed = true;
                                }
                                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                                {
                                    removeIdx = result.Count - 1;
                                    changed = true;
                                }
                            }
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Add", GUILayout.Width(70)))
                        {
                            result.Add(null);
                            changed = true;
                        }
                    }
                }
            }
            UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
            if (removeIdx >= 0) result.RemoveAt(removeIdx);
            return result.ToArray();
        }
        #endregion
        #endregion
    }
}
