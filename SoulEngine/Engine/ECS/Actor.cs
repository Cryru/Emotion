// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using Soul.Engine.Internal;
using System.Linq;

#endregion

namespace Soul.Engine.ECS
{
    public abstract class Actor
    {
        #region Properties

        /// <summary>
        /// Actors children of this actor.
        /// </summary>
        private List<object> _children;

        /// <summary>
        /// Name index for named children.
        /// </summary>
        private Dictionary<string, int> _nameIndex;

        /// <summary>
        /// The parent of this actor.
        /// </summary>
        public Actor Parent;

        /// <summary>
        /// The number of children attached to this actor.
        /// </summary>
        public int Count
        {
            get
            {
                return _children?.Count ?? 0;
            }
        }
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
            if (_children != null)
                foreach (Actor a in _children)
                {
                    a.UpdateActor();
                }

            Update();
        }

        /// <summary>
        /// Attaches a child actor to this actor.
        /// </summary>
        /// <param name="actor">The child actor to attach.</param>
        /// <returns>The index of the newly added child.</returns>
        public int AddChild(Actor actor)
        {
            // Add the actor as an object.
            int index = AddChild((object) actor);

            // Run initialization and parenting.
            actor.Parent = this;
            actor.Initialize();

            return index;
        }

        /// <summary>
        /// Attaches a child actor to this actor under an identifier name.
        /// </summary>
        /// <param name="name">The identifier to add the actor under.</param>
        /// <param name="actor">The child actor to attach.</param>
        /// <returns>The index of the newly added child.</returns>
        public int AddChild(string name, Actor actor)
        {
            // Check if this entry is already added.
            if (_nameIndex == null) _nameIndex = new Dictionary<string, int>();
            if (_children == null) _children = new List<object>();
            if (_children.IndexOf(actor) != -1 || _nameIndex.ContainsKey(name))
            {
                Error.Raise(4, "The child or child name - " + name + " is already attached to this parent.");
                return -1;
            }

            // Add the actor under the name index.
            _nameIndex.Add(name, _children.Count);

            // Add through normal function.
            return AddChild(actor);
        }

        /// <summary>
        /// Attaches an object as a child to this actor. [Primary function.]
        /// </summary>
        /// <param name="Object">The child object to attach.</param>
        /// <returns>The index of the newly added child.</returns>
        public int AddChild(object Object)
        {
            if (_children == null) _children = new List<object>();

            if (_children.IndexOf(Object) != -1)
            {
                Error.Raise(4, "The child is already attached to this parent.");
                return -1;
            }

            _children.Add(Object);

            return _children.Count - 1;
        }

        /// <summary>
        /// Attaches a child object to this actor under an identifier name. [Primary function.]
        /// </summary>
        /// <param name="name">The identifier to add the object under.</param>
        /// <param name="Object">The child object to attach.</param>
        /// <returns>The index of the newly added child.</returns>
        public int AddChild(string name, object Object)
        {
            // Check if this entry is already added.
            if (_nameIndex == null) _nameIndex = new Dictionary<string, int>();
            if (_children.IndexOf(Object) != -1 || _nameIndex.ContainsKey(name))
            {
                Error.Raise(4, "The child is already attached to this parent.");
                return -1;
            }

            // Add the actor under the name index.
            _nameIndex.Add(name, _children.Count);

            // Add through normal function.
            AddChild(Object);

            return _children.Count - 1;
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
            foreach (object a in _children)
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
            // Check if the requested name exists in the name index.
            if (!_nameIndex.ContainsKey(name)) return default(T);

            // Get the index from the name index.
            int index = _nameIndex[name];

            // Get the child from the index.
            return GetChild<T>(index);
        }

        /// <summary>
        /// Returns the child under the specified index.
        /// </summary>
        /// <typeparam name="T">The type to return the child in.</typeparam>
        /// <param name="index">The index of the child to return.</param>
        /// <returns>The child itself or the default type value if not found.</returns>
        public T GetChild<T>(int index)
        {
            // Check if any children are attached, and if the index is correct.
            if (_children == null || _children.Count - 1 < index || index < 0) return default(T);

            // Check if right type, and if it is return it.
            if (_children[index] is T) return (T)System.Convert.ChangeType(_children[index], typeof(T));

            // If incorrect type return the default value for the requested type.
            return default(T);
        }

        /// <summary>
        /// Removes the first child of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of child to remove.</typeparam>
        public void RemoveChild<T>()
        {
            // Check if any children are attached.
            if (_children == null) return;

            // Loop through all children until we find one with the requested type.
            foreach (Actor a in _children)
            {
                if (!(a is T)) continue;
                _children.Remove(a);
                return;
            }
        }

        /// <summary>
        /// Removes a child under an identifier name.
        /// </summary>
        /// <param name="name">The child under this name to remove.</param>
        public void RemoveChild(string name)
        {
            // Check if the requested name exists in the name index.
            if (!_nameIndex.ContainsKey(name) || _children == null) return;

            // Get the index from the name index.
            int index = _nameIndex[name];

            // Remove it from both indexes.
            RemoveChild(index);
            _nameIndex.Remove(name);
        }

        /// <summary>
        /// Removes a child under the specified index.
        /// </summary>
        /// <param name="index">The index to remove a child under.</param>
        public void RemoveChild(int index)
        {
            // Check if valid index.
            if (_children == null || _children.Count - 1 < index || index < 0) return;

            // Remove the child.
            _children.RemoveAt(index);

            // Check if the name index contains an entry for this.
            KeyValuePair<string, int> entry = _nameIndex.FirstOrDefault(x => x.Value == index);
            if (entry.Equals(default(KeyValuePair<string, int>)))
            {
                _nameIndex.Remove(entry.Key);
            }
        }

        /// <summary>
        /// Removes a child from actor reference.
        /// </summary>
        /// <param name="actor">The child actor to remove.</param>
        public void RemoveChild(Actor actor)
        {
            // The index of the child within the list.
            int childIndex = _children.IndexOf(actor);

            // Check if valid index.
            if (actor == null || _children.IndexOf(actor) == -1) return;

            // Remove the child.
            _children.RemoveAt(childIndex);

            // Check if the name index contains an entry for this.
            KeyValuePair<string, int> entry = _nameIndex.FirstOrDefault(x => x.Value == childIndex);
            if (entry.Equals(default(KeyValuePair<string, int>)))
            {
                _nameIndex.Remove(entry.Key);
            }      
        }
    }
}