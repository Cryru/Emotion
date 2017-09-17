// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;

#endregion

namespace Soul.Engine.ECS
{
    public abstract class Actor
    {
        #region Properties

        /// <summary>
        /// Actors children of this actor.
        /// </summary>
        public List<object> Children;

        /// <summary>
        /// Name index for named children.
        /// </summary>
        public Dictionary<string, int> NameIndex;

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
                return Children?.Count ?? 0;
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
            if (Children != null)
                foreach (Actor a in Children)
                {
                    a.UpdateActor();
                }

            Update();
        }

        /// <summary>
        /// Attaches a child actor to this actor.
        /// </summary>
        /// <param name="actor">The child actor to attach.</param>
        public void AddChild(Actor actor)
        {
            // Add the actor as an object.
            AddChild((object) actor);

            // Run initialization and parenting.
            actor.Parent = this;
            actor.Initialize();
        }

        /// <summary>
        /// Attaches a child actor to this actor under an identifier name.
        /// </summary>
        /// <param name="name">The identifier to add the actor under.</param>
        /// <param name="actor">The child actor to attach.</param>
        public void AddChild(string name, Actor actor)
        {
            // Check if this entry is already added.
            if (NameIndex == null) NameIndex = new Dictionary<string, int>();
            if (Children == null) Children = new List<object>();
            if (Children.IndexOf(actor) != -1 || NameIndex.ContainsKey(name)) return;

            // Add the actor under the name index.
            NameIndex.Add(name, Children.Count);

            // Add through normal function.
            AddChild(actor);
        }

        /// <summary>
        /// Attaches an object as a child to this actor.
        /// </summary>
        /// <param name="Object">The child object to attach.</param>
        public void AddChild(object Object)
        {
            if (Children == null) Children = new List<object>();

            if (Children.IndexOf(Object) != -1) return;
            Children.Add(Object);
        }

        /// <summary>
        /// Attaches a child object to this actor under an identifier name.
        /// </summary>
        /// <param name="name">The identifier to add the object under.</param>
        /// <param name="Object">The child object to attach.</param>
        public void AddChild(string name, object Object)
        {
            // Check if this entry is already added.
            if (NameIndex == null) NameIndex = new Dictionary<string, int>();
            if (Children.IndexOf(Object) != -1 || NameIndex.ContainsKey(name)) return;

            // Add the actor under the name index.
            NameIndex.Add(name, Children.Count);

            // Add through normal function.
            AddChild(Object);
        }

        /// <summary>
        /// Returns the first child of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of child to look for.</typeparam>
        /// <returns>The child itself or the default type value if not found.</returns>
        public T GetChild<T>()
        {
            // Check if any children are attached.
            if (Children == null) return default(T);

            // Loop through all children until we find one with the requested type.
            foreach (object a in Children)
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
            if (!NameIndex.ContainsKey(name)) return default(T);

            // Get the index from the name index.
            int index = NameIndex[name];

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
            if (Children == null || Children.Count - 1 < index || index < 0) return default(T);

            // Check if right type, and if it is return it.
            if (Children[index] is T) return (T)System.Convert.ChangeType(Children[index], typeof(T));

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
            if (Children == null) return;

            // Loop through all children until we find one with the requested type.
            foreach (Actor a in Children)
            {
                if (!(a is T)) continue;
                Children.Remove(a);
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
            if (!NameIndex.ContainsKey(name) || Children == null) return;

            // Get the index from the name index.
            int index = NameIndex[name];

            // Remove it from both indexes.
            RemoveChild(index);
            NameIndex.Remove(name);
        }

        /// <summary>
        /// Removes a child under the specified index.
        /// </summary>
        /// <param name="index">The index to remove a child under.</param>
        public void RemoveChild(int index)
        {
            // Check if valid index.
            if (Children == null || Children.Count - 1 < index || index < 0) return;

            // Remove the child.
            Children.RemoveAt(index);
        }
    }
}