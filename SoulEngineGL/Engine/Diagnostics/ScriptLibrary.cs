using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.ECS;
using Soul.Engine.Scenography;

namespace Soul.Engine.Diagnostics
{
    public static class ScriptLibrary
    {
        /// <summary>
        /// Returns various statistics about the state of the engine.
        /// </summary>
        /// <returns></returns>
        public static string Statistics()
        {
            List<string> data = new List<string>
            {
                "Ram Usage: " + Core.Process.PrivateMemorySize64 / 1024 / 1024 + "mb",
                "Current Scene: " + SceneManager.CurrentScene,
                "Loaded Scenes: " + SceneManager.LoadedScenes.Count,
                "FPS: " + (1000f / Core.Context.Frametime)
            };

            return string.Join("\n", data);
        }

        /// <summary>
        /// Returns all entities for the current scene and most of their data.
        /// </summary>
        /// <returns></returns>
        public static string GetEntities()
        {
            List<string> data = new List<string> { "Entities: " + SceneManager.CurrentScene?.RegisteredEntities.Count };


            foreach (KeyValuePair<string, Entity> entity in SceneManager.CurrentScene?.RegisteredEntities)
            {
                data.Add(" " + entity.Key);

                data.Add("   Components: " + entity.Value.Components.Count);
                data.AddRange(entity.Value.Components.Select(attachedComponent => "     |- " + attachedComponent));

                data.Add("   Systems: " + entity.Value.LinkedSystems.Length);
                data.AddRange(entity.Value.LinkedSystems.Select(linkedSystem => "     |- " + linkedSystem));
            }

            return string.Join("\n", data);
        }
    }
}
