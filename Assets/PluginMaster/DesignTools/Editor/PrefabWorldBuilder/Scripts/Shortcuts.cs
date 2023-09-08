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
#if UNITY_2019_1_OR_NEWER
using UnityEngine;

namespace PluginMaster
{
    public static partial class Shortcuts
    {
        #region TOGGLE TOOLS
        public const string PWB_TOGGLE_PIN_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Pin Tool";
         [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_PIN_SHORTCUT_ID,
             KeyCode.Alpha1, UnityEditor.ShortcutManagement.ShortcutModifiers.Shift
             | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
         private static void TogglePin() => PWBIO.ToogleTool(ToolManager.PaintTool.PIN);

        public const string PWB_TOGGLE_BRUSH_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Brush Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_BRUSH_SHORTCUT_ID, KeyCode.Alpha2,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleBrush() => PWBIO.ToogleTool(ToolManager.PaintTool.BRUSH);

        public const string PWB_TOGGLE_GRAVITY_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Gravity Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_GRAVITY_SHORTCUT_ID, KeyCode.Alpha3,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleGravity() => PWBIO.ToogleTool(ToolManager.PaintTool.GRAVITY);

        public const string PWB_TOGGLE_LINE_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Line Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_LINE_SHORTCUT_ID, KeyCode.Alpha4,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleLine() => PWBIO.ToogleTool(ToolManager.PaintTool.LINE);

        public const string PWB_TOGGLE_SHAPE_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Shape Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_SHAPE_SHORTCUT_ID, KeyCode.Alpha5,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleShape() => PWBIO.ToogleTool(ToolManager.PaintTool.SHAPE);

        public const string PWB_TOGGLE_TILING_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Tiling Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_TILING_SHORTCUT_ID, KeyCode.Alpha6,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleTiling() => PWBIO.ToogleTool(ToolManager.PaintTool.TILING);

        public const string PWB_TOGGLE_REPLACER_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Replacer Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_REPLACER_SHORTCUT_ID, KeyCode.Alpha7,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleReplacer() => PWBIO.ToogleTool(ToolManager.PaintTool.REPLACER);

        public const string PWB_TOGGLE_ERASER_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Eraser Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_ERASER_SHORTCUT_ID, KeyCode.Alpha8,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleEraser() => PWBIO.ToogleTool(ToolManager.PaintTool.ERASER);

        public const string PWB_TOGGLE_SELECTION_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Selection Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_SELECTION_SHORTCUT_ID, KeyCode.Alpha9,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleSelection() => PWBIO.ToogleTool(ToolManager.PaintTool.SELECTION);

        public const string PWB_TOGGLE_EXTRUDE_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Extrude Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_EXTRUDE_SHORTCUT_ID, KeyCode.X,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleExtrude() => PWBIO.ToogleTool(ToolManager.PaintTool.EXTRUDE);

        public const string PWB_TOGGLE_MIRROR_SHORTCUT_ID = "Prefab World Builder/Tools - Toggle Mirror Tool";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_TOGGLE_MIRROR_SHORTCUT_ID, KeyCode.M,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void ToggleMirror() => PWBIO.ToogleTool(ToolManager.PaintTool.MIRROR);
        #endregion
        #region WINDOWS
        public const string PWB_CLOSE_ALL_WINDOWS_ID = "Prefab World Builder/Close All Windows";
        [UnityEditor.ShortcutManagement.Shortcut(PWB_CLOSE_ALL_WINDOWS_ID, KeyCode.End,
            UnityEditor.ShortcutManagement.ShortcutModifiers.Shift | UnityEditor.ShortcutManagement.ShortcutModifiers.Alt)]
        private static void PWBCloseAllWindows()
        {
            ToolManager.DeselectTool();
            PWBIO.CloseAllWindows();
        }
        #endregion

    }
}
#endif