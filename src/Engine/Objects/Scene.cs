using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects.Components;
using SoulEngine.Events;
using SoulEngine.Enums;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A scene object, different scenes are like different views.
    /// </summary>
    public abstract class Scene : IDisposable
    {
        #region "Declarations"
        /// <summary>
        /// The scene's loaded assets.
        /// </summary>
        public Assets Assets;
        /// <summary>
        /// Objects that belong to this scene.
        /// </summary>
        protected Dictionary<string, GameObject> Objects;
        /// <summary>
        /// Objects that belong to this scene and are generated.
        /// </summary>
        protected Dictionary<string, List<GameObject>> ObjectClusters;
        /// <summary>
        /// The number of objects attached to the scene.
        /// </summary>
        public int ObjectCount
        {
            get
            {
                return Objects.Count + ObjectClusters.Count;
            }
        }
        /// <summary>
        /// Returns the objects dictionary, read only.
        /// </summary>
        public Dictionary<string, GameObject> AttachedObjects
        {
            get
            {
                return Objects;
            }
        }
        #endregion

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public void SetupScene()
        {
            //Check if allowed to setup a new scene.
            if (!Context.Core.__sceneSetupAllowed) throw new Exception("Scene setup must be done by the core, to load a scene use Core.LoadScene(Scene).");

            //Setup assets loader and objects dictionary.
            Assets = new Assets();
            Objects = new Dictionary<string, GameObject>();
            ObjectClusters = new Dictionary<string, List<GameObject>>();

            //Run the start code.
            Start();
        }

        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        public void UpdateHook()
        {
            //Run the scene's update code.
            Update();

            //Update the scene's objects.
            Objects.Select(x => x.Value).ToList().ForEach(x => x.Update());

            //Update the scene's clusters.
            ObjectClusters.Select(x => x.Value).ToList().ForEach(x => x.ForEach(y => y.Update()));
        }
        /// <summary>
        /// Composes component textures on linked objects.
        /// </summary>
        public void Compose()
        {
            //Run texture composition for the scene's objects.
            Objects.Select(x => x.Value).ToList().ForEach(x => x.Compose());

            //Run texture composition for the scene's clusters.
            ObjectClusters.Select(x => x.Value).ToList().ForEach(x => x.ForEach(y => y.Compose()));
        }
        /// <summary>
        /// The core's hook for drawing.
        /// </summary>
        public void DrawHook()
        {
            //Run the draw function on all objects with respect to their selected layer.
            Context.ink.Start(DrawChannel.World);
            Objects.Select(x => x.Value).Where(x => x.Layer == ObjectLayer.World).ToList().ForEach(x => x.Draw());
            ObjectClusters.Select(x => x.Value).ToList().ForEach(x => x.Where(y => y.Layer == ObjectLayer.World).ToList().ForEach(y => y.Draw()));
            Context.ink.End();
            Context.ink.Start(DrawChannel.Screen);
            Objects.Select(x => x.Value).Where(x => x.Layer == ObjectLayer.UI).ToList().ForEach(x => x.Draw());
            ObjectClusters.Select(x => x.Value).ToList().ForEach(x => x.Where(y => y.Layer == ObjectLayer.UI).ToList().ForEach(y => y.Draw()));
            Context.ink.End();
        }
        #endregion

        #region "Object and Cluster Manager"
        /// <summary>
        /// Adds an object to the scene.
        /// </summary>
        /// <param name="Label">A label to access the object with, case insensitive.</param>
        /// <param name="Object">The object to add.</param>
        public void AddObject(string Label, GameObject Object)
        {
            Objects.Add(Label.ToLower(), Object);

            Objects = Objects.OrderBy(x => x.Value.Priority).ToDictionary(x => x.Key, x => x.Value);
        }
        /// <summary>
        /// Removes an object from the scene.
        /// </summary>
        /// <param name="Label">The label of the object you want to remove, case insensitive.</param>
        public void RemoveObject(string Label)
        {
            Objects.Remove(Label.ToLower());
        }
        /// <summary>
        /// Returns the object marked with the specified label.
        /// </summary>
        /// <param name="Label">The label of the object you want, case insensitive.</param>
        public GameObject GetObject(string Label)
        {
            return Objects[Label.ToLower()];
        }
        /// <summary>
        /// Adds an object cluster to the scene.
        /// </summary>
        /// <param name="Label">A label to access the cluster with, case insensitive.</param>
        /// <param name="ObjectCluster">The cluster to add.</param>
        public void AddCluster(string Label, List<GameObject> ObjectCluster)
        {
            ObjectClusters.Add(Label.ToLower(), ObjectCluster);
        }
        /// <summary>
        /// Removes a cluster from the scene.
        /// </summary>
        /// <param name="Label">The label of the object you want to remove, case insensitive.</param>
        public void RemoveCluster(string Label)
        {
            ObjectClusters.Remove(Label.ToLower());
        }
        /// <summary>
        /// Returns the cluster marked with the specified label.
        /// </summary>
        /// <param name="Label">The label of the object you want, case insensitive.</param>
        public List<GameObject> GetCluster(string Label)
        {
            return ObjectClusters[Label.ToLower()];
        }
        #endregion

        /// <summary>
        /// Runs when the scene takes control.
        /// </summary>
        abstract public void Start();
        /// <summary>
        /// Updates objects in the world.
        /// </summary>
        abstract public void Update();

        //Other
        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //Free resources.
                Assets = null;
                foreach (var obj in Objects)
                {
                    obj.Value.Dispose();
                }
                foreach (var cluster in ObjectClusters)
                {
                    for (int i = 0; i < cluster.Value.Count; i++)
                    {
                        cluster.Value[i].Dispose();
                    }
                }

                ObjectClusters = null;
                Objects = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}