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
    public static class DropUtils
    {
        public struct DroppedItem
        {
            public GameObject obj;
            public DroppedItem(GameObject obj) => this.obj = obj;
        }

        public static DroppedItem[] GetDirPrefabs(string dirPath)
        {
            var filePaths = System.IO.Directory.GetFiles(dirPath, "*.prefab");
            var subItemList = new System.Collections.Generic.List<DroppedItem>();
            var dirName = dirPath.Substring(Mathf.Max(dirPath.LastIndexOf('/'), dirPath.LastIndexOf('\\')) + 1);
            foreach (var filePath in filePaths)
            {
                DroppedItem item;
                item.obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                var prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(item.obj);
                if (prefabType != UnityEditor.PrefabAssetType.Regular
                    && prefabType != UnityEditor.PrefabAssetType.Variant) continue;
                subItemList.Add(item);
            }
            if (PaletteManager.selectedPalette.brushCreationSettings.includeSubfolders)
            {
                var subdirPaths = System.IO.Directory.GetDirectories(dirPath);
                foreach (var subdirPath in subdirPaths) subItemList.AddRange(GetDirPrefabs(subdirPath));
            }
            return subItemList.ToArray();
        }

        public static DroppedItem[] GetDroppedPrefabs()
        {
            var itemList = new System.Collections.Generic.List<DroppedItem>();
            for (int i = 0; i < UnityEditor.DragAndDrop.objectReferences.Length; ++i)
            {
                var objRef = UnityEditor.DragAndDrop.objectReferences[i];
                if (objRef is GameObject)
                {
                    if (objRef == null) continue;
                    if (UnityEditor.PrefabUtility.GetPrefabAssetType(objRef) == UnityEditor.PrefabAssetType.NotAPrefab)
                        continue;
                    var path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(objRef);
                    if (path == string.Empty) continue;
                    var prefab = objRef as GameObject;
                    var prefabInstance = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(objRef) as GameObject;
                    if (prefabInstance != null)
                    {
                        var assetType = UnityEditor.PrefabUtility.GetPrefabAssetType(prefabInstance);
                        if (assetType == UnityEditor.PrefabAssetType.NotAPrefab
                            || assetType == UnityEditor.PrefabAssetType.NotAPrefab) continue;
                        if (assetType == UnityEditor.PrefabAssetType.Variant) prefab = prefabInstance;
                        else prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(prefabInstance);
                    }
                    itemList.Add(new DroppedItem(prefab));
                }
                else
                {
                    var path = UnityEditor.DragAndDrop.paths[i];
                    if (objRef is UnityEditor.DefaultAsset && UnityEditor.AssetDatabase.IsValidFolder(path))
                        itemList.AddRange(GetDirPrefabs(path));
                }
            }
            return itemList.ToArray();
        }

        public static DroppedItem[] GetFolderItems()
        {
            DroppedItem[] items = null;
            var folder = UnityEditor.EditorUtility.OpenFolderPanel("Add Prefabs in folder:", Application.dataPath, "Assets");
            if (folder.Contains(Application.dataPath))
            {
                folder = folder.Replace(Application.dataPath, "Assets");
                items = GetDirPrefabs(folder);
                if (items.Length == 0)
                    UnityEditor.EditorUtility.DisplayDialog("No Prefabs found", "No prefabs found in folder", "Ok");
            }
            else if (folder != string.Empty)
                UnityEditor.EditorUtility.DisplayDialog("Folder Error", "Folder must be under Assets folder", "Ok");
            return items;
        }
    }
}
