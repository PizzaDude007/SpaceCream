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

namespace PluginMaster
{
    [UnityEditor.InitializeOnLoad]
    public static class SelectionManager
    {
        private static GameObject[] _topLevelSelection = new GameObject[0];
        private static GameObject[] _topLevelSelectionWithPrefabs = new GameObject[0];
        private static GameObject[] _selection = new GameObject[0];
        public static System.Action selectionChanged;
        static SelectionManager() => UnityEditor.Selection.selectionChanged += UpdateSelection;

        private static void UpdateSelection(System.Collections.Generic.List<GameObject> list,
            bool filteredByTopLevel, bool excludePrefabs)
        {
            var newSet = new System.Collections.Generic.HashSet<GameObject>(
                UnityEditor.Selection.GetFiltered<GameObject>(UnityEditor.SelectionMode.Editable
                | (excludePrefabs ? UnityEditor.SelectionMode.ExcludePrefab : UnityEditor.SelectionMode.Unfiltered)
                | (filteredByTopLevel ? UnityEditor.SelectionMode.TopLevel : UnityEditor.SelectionMode.Unfiltered)));
            if (newSet.Count == 0)
            {
                list.Clear();
                return;
            }
            var unselectedSet = new System.Collections.Generic.HashSet<GameObject>(list);
            unselectedSet.ExceptWith(newSet);
            foreach (var obj in unselectedSet) list.Remove(obj);
            newSet.ExceptWith(list);
            foreach (var obj in newSet) list.Add(obj);
        }
        public static void UpdateSelection()
        {
            var selectionOrderedTopLevel = new System.Collections.Generic.List<GameObject>(_topLevelSelection);
            var selectionOrdered = new System.Collections.Generic.List<GameObject>(_selection);
            var selectionOrderedTopLevelWithPrefabs
                = new System.Collections.Generic.List<GameObject>(_topLevelSelectionWithPrefabs);
            UpdateSelection(selectionOrderedTopLevel, true, true);
            UpdateSelection(selectionOrdered, false, true);
            UpdateSelection(selectionOrderedTopLevelWithPrefabs, true, false);
            _selection = selectionOrdered.ToArray();
            _topLevelSelection = selectionOrderedTopLevel.ToArray();
            _topLevelSelectionWithPrefabs = selectionOrderedTopLevelWithPrefabs.ToArray();
            if (selectionChanged != null) selectionChanged();
        }
        public static GameObject[] GetSelection(bool filteredByTopLevel)
            => filteredByTopLevel ? _topLevelSelection : _selection;
        public static GameObject[] topLevelSelection => _topLevelSelection;
        public static GameObject[] topLevelSelectionWithPrefabs => _topLevelSelectionWithPrefabs;
        public static GameObject[] selection => _selection;

        public static GameObject[] GetSelectionPrefabs()
        {
            var result = new System.Collections.Generic.List<GameObject>();
            foreach (var obj in topLevelSelectionWithPrefabs)
            {
                if (obj == null) continue;
                var assetType = UnityEditor.PrefabUtility.GetPrefabAssetType(obj);
                if (assetType == UnityEditor.PrefabAssetType.NotAPrefab) continue;
                var prefab = obj;
                if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(obj))
                {
                    prefab = assetType == UnityEditor.PrefabAssetType.Variant ? obj
                        : UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(obj);
                }
                if (result.Contains(prefab)) continue;
                result.Add(prefab);
            }
            return result.ToArray();
        }
    }
}
