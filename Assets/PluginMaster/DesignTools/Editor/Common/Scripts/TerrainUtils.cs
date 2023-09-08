/*
Copyright (c) 2022 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2022.

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
    public static class TerrainUtils
    {
        public static Vector3[] GetCorners(Terrain terrain, Space space)
        {
            var terrainData = terrain.terrainData;
            var origin = space == Space.Self ? Vector3.zero : terrain.transform.position;
            var max = terrainData.heightmapResolution - 1;
            var scale = terrainData.heightmapScale;
            var corners = new Vector3[]
            {
                origin + new Vector3(0, terrainData.GetHeight(0, 0), 0),
                origin + new Vector3(max * scale.x, terrainData.GetHeight(max, 0), 0),
                origin + new Vector3(max * scale.x, terrainData.GetHeight(max, max), max * scale.z),
                origin + new Vector3(0, terrainData.GetHeight(0, max), max * scale.z)
            };
            return corners;
        }
    }
}
