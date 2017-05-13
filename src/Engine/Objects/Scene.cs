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
        /// <summary>
        /// Returns the object cluster dictionary, read only.
        /// </summary>
        public Dictionary<string, List<GameObject>> AttachedClusters
        {
            get
            {
                return ObjectClusters;
            }
        }
        /// <summary>
        /// Queue of actions to do on the next update.
        /// </summary>
        private List<string> queue = new List<string>();
        /// <summary>
        /// Whether we are expecting UI objects in clusters.
        /// </summary>
        public bool UIClusters = false;
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when the scene is clicked and not a UI object.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> OnClicked;
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

            //Attach the mouse down event.
            Input.OnMouseButtonDown += Input_OnMouseButtonDown;

            //Run the start code.
            Start();
        }

        /// <summary>
        /// Check if not clicking on an object.
        /// </summary>
        private void Input_OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(Functions.inObject(Input.getMousePos(), new Microsoft.Xna.Framework.Rectangle(0, 0, Settings.Width, Settings.Height), -1))
            {
                OnClicked?.Invoke(this, e);
            }
        }

        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        public void UpdateHook()
        {
            //Execute queued actions.
            for (int i = 0; i < queue.Count; i++)
            {
                string[] actions = queue[i].Split((char)007);
                
                //Determine action
                switch(actions[0])
                {
                    case "remove-obj":
                        Objects[actions[1]].Dispose();
                        Objects.Remove(actions[1]);
                        break;
                    case "remove-cluster":
                        for (int o = 0; o < ObjectClusters[actions[1]].Count; o++)
                        {
                            ObjectClusters[actions[1]][o].Dispose();
                        }
                        ObjectClusters.Remove(actions[1]);
                        break;
                    case "clean-cluster":
                        for (int o = 0; o < ObjectClusters[actions[1]].Count; o++)
                        {
                            ObjectClusters[actions[1]][o].Dispose();
                        }
                        ObjectClusters[actions[1]].Clear();
                        break;
                }
            }
            queue.Clear();

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
            //Draw all objects.
            ObjectLayer Current = ObjectLayer.World;
            Context.ink.Start(DrawChannel.World);
            foreach (var obj in Objects)
            {
                //Check if the next object is on the same layer.
                if(obj.Value.Layer == Current)
                {
                    obj.Value.Draw();
                }
                else
                {
                    //If it isn't switch layers.
                    Context.ink.End();
                    Current = obj.Value.Layer;
                    if(Current == ObjectLayer.UI) Context.ink.Start(DrawChannel.Screen); else Context.ink.Start(DrawChannel.World);
                    obj.Value.Draw();
                }
            }
            //Make sure to close the drawing channel.
            Context.ink.End();

            //Draw all clusters.
            Current = ObjectLayer.World;
            Context.ink.Start(DrawChannel.World);
            foreach (var obj in ObjectClusters)
            {
                //Loop through all objects in the cluster.
                for (int i = 0; i < obj.Value.Count; i++)
                {
                    //Check if the next object is on the same layer.
                    if (obj.Value[i].Layer == Current)
                    {
                        obj.Value[i].Draw();
                    }
                    else
                    {
                        //If it isn't switch layers.
                        Context.ink.End();
                        Current = obj.Value[i].Layer;
                        if (Current == ObjectLayer.UI) Context.ink.Start(DrawChannel.Screen); else Context.ink.Start(DrawChannel.World);
                        obj.Value[i].Draw();
                    }
                }
            }
            //Make sure to close the drawing channel.
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
            //Check if an object with that name exists.
            if (Objects.ContainsKey(Label.ToLower()))
            {
                //If it does overwrite it.
                Objects[Label.ToLower()] = Object;
            }
            else
            {
                //If it doesn't then add it.
                Objects.Add(Label.ToLower(), Object);
            }

            Object.Name = Label.ToLower();

            //Order objects based on priority.
            OrderObjects();
        }

        /// <summary>
        /// Orders attached objects by priority.
        /// </summary>
        public void OrderObjects()
        {
            Objects = Objects.OrderBy(x => x.Value.Priority).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Removes an object from the scene.
        /// </summary>
        /// <param name="Label">The label of the object you want to remove, case insensitive.</param>
        public void RemoveObject(string Label)
        {
            if(Objects.ContainsKey(Label.ToLower())) queue.Add("remove-obj" + (char)007 + Label.ToLower());
        }
        /// <summary>
        /// Returns the object marked with the specified label.
        /// </summary>
        /// <param name="Label">The label of the object you want, case insensitive.</param>
        public GameObject GetObject(string Label)
        {
            try
            {
                return Objects[Label.ToLower()];
            }
            catch
            {
                throw new Exception("No object with the name " + Label.ToLower() + " is attached to " + GetType().ToString().Replace("SoulEngine.", ""));
            }         
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
            if (ObjectClusters.ContainsKey(Label)) queue.Add("remove-cluster" + (char)007 + Label.ToLower());
        }
        /// <summary>
        /// Clears a cluster.
        /// </summary>
        /// <param name="Label">The label of the object you want to clear, case insensitive.</param>
        public void CleanCluster(string Label)
        {
            if (ObjectClusters.ContainsKey(Label)) queue.Add("clean-cluster" + (char)007 + Label.ToLower());
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
                Assets.Dispose();

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