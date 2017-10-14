// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using System.Linq;
using Soul.Engine.Internal;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.ECS
{
    public abstract class Actor
    {
        #region Properties

        /// <summary>
        /// The parent of this actor.
        /// </summary>
        public Actor Parent;

        /// <summary>
        /// The number of children attached to this actor.
        /// </summary>
        public int ChildrenCount
        {
            get { return _children?.Count ?? 0; }
        }

        /// <summary>
        /// The priority of this actor. Higher priority children will be updated first.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set
            {
                // Set the priority of the child.
                _priority = value;

                // Update the listing of the parent.
                Parent?.UpdatePriority();
            }
        }

        private int _priority;

        #endregion

        #region Internals

        /// <summary>
        /// The actor's children.
        /// </summary>
        private Dictionary<string, Actor> _children;

        #endregion

        /// <summary>
        /// Initializes the actor. Constructor code should be here.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Runs the actor's update code.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Runs the actor's update code and updates the actor's children.
        /// </summary>
        public virtual void UpdateActor()
        {
            // If the children list has been initialized, update all children, then update the parent.
            if (_children != null)
                foreach (Actor a in _children.Values)
                {
                    a.UpdateActor();
                }

            Update();
        }

        /// <summary>
        /// Attaches a child actor to this actor under an identifier name. [Primary]
        /// </summary>
        /// <param name="name">The identifier to add the actor under.</param>
        /// <param name="actor">The child actor to attach.</param>
        /// <returns>The name of the newly added child.</returns>
        public string AddChild(string name, Actor actor)
        {
            // Check if the actor's children list has been initialized.
            if (_children == null) _children = new Dictionary<string, Actor>();
            // Check if that actor has already been added.
            if (_children.ContainsValue(actor) || _children.ContainsKey(name))
            {
                Error.Raise(4, "The child or child name - " + name + " is already attached to this parent.");
                return "";
            }

            // Add the actor under the name index.
            _children.Add(name, actor);

            // Run initialization and parenting.
            actor.Parent = this;
            actor.Initialize();

            // Update the priority list.
            UpdatePriority();

            Debugger.DebugMessage(Enums.DebugMessageSource.Execution, "ECS - Added child " + name);

            return name;
        }

        /// <summary>
        /// Attaches a child actor to this actor.
        /// </summary>
        /// <param name="actor">The child actor to attach.</param>
        /// <returns>The name of the newly added child.</returns>
        public string AddChild(Actor actor)
        {
            return AddChild(actor.GetHashCode().ToString(), actor);
        }

        /// <summary>
        /// Returns the first child of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of child to look for.</typeparam>
        /// <returns>The child itself or the default type value if not found.</returns>
        public T GetChild<T>()
        {
            // Check if any children are attached.
            if (_children == null) return default(T);

            // Loop through all children until we find one with the requested type.
            foreach (Actor a in _children.Values)
            {
                if (a is T) return (T) System.Convert.ChangeType(a, typeof(T));
            }

            return default(T);
        }

        /// <summary>
        /// Returns a child under the specified name.
        /// </summary>
        /// <typeparam name="T">The type to return the child in.</typeparam>
        /// <param name="name">The name identifier.</param>
        /// <returns>The child itself or the default type value if not found.</returns>
        public T GetChild<T>(string name)
        {
            // Check if the actor's children list has been initialized.
            if (_children == null) return default(T);

            // Check if the requested name is assigned.
            if (!_children.ContainsKey(name)) return default(T);

            // Get the child from the index.
            if (_children[name] is T) return (T)System.Convert.ChangeType(_children[name], typeof(T));

            return default(T);
        }

        /// <summary>
        /// Removes a child under an identifier name. [Primary]
        /// </summary>
        /// <param name="name">The child under this name to remove.</param>
        public void RemoveChild(string name)
        {
            // Check if the requested name is assigned.
            if (_children == null || !_children.ContainsKey(name)) return;

            // Remove the child.
            _children.Remove(name);

            Debugger.DebugMessage(Enums.DebugMessageSource.Execution, "ECS - Removed child " + name);
        }

        /// <summary>
        /// Removes the first child of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of child to remove.</typeparam>
        public void RemoveChild<T>()
        {
            // Check if the actor's children list has been initialized.
            if (_children == null) return;

            // Loop through all children until we find one with the requested type.
            foreach (Actor a in _children.Values)
            {
                if (!(a is T)) continue;
                RemoveChild(a);
                return;
            }
        }

        /// <summary>
        /// Removes a child from actor reference.
        /// </summary>
        /// <param name="actor">The child actor to remove.</param>
        public void RemoveChild(Actor actor)
        {
            // Check if the actor's children list has been initialized.
            if (_children == null) return;

            // Get the key of the actor we want to remove.
            KeyValuePair<string, Actor> foundEntry = _children.FirstOrDefault(x => x.Value == actor);

            // Check if valid key was found.
            if (foundEntry.Equals(default(KeyValuePair<string, Actor>))) return;

            // Remove the child.
            RemoveChild(foundEntry.Key);
        }

        #region Private Helpers

        /// <summary>
        /// Updates the children's priority listing.
        /// </summary>
        internal void UpdatePriority()
        {
            _children = _children.OrderBy(x => x.Value.Priority).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        #endregion
    }
}