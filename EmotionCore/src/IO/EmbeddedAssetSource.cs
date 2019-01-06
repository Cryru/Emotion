// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Emotion.Debug;
using Emotion.Engine;

#endregion

namespace Emotion.IO
{
    /// <inheritdoc />
    public sealed class EmbeddedAssetSource : AssetSource
    {
        /// <summary>
        /// The assembly to load embedded assets from.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// The name of the assembly.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The root embedded folder.
        /// </summary>
        public string Folder { get; private set; }

        /// <summary>
        /// Create a new source which loads assets embedded into the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load assets from.</param>
        /// <param name="folder">The root embedded folder.</param>
        public EmbeddedAssetSource(Assembly assembly, string folder)
        {
            Folder = folder.Replace("/", "$").Replace("\\", "$").Replace("$", ".");
            Assembly = assembly;

            string assemblyFullName = assembly.FullName;
            Name = assemblyFullName.Substring(0, assemblyFullName.IndexOf(",", StringComparison.Ordinal));

            // Populate internal manifest.
            Assembly.GetManifestResourceNames().AsParallel().ForAll(x =>
            {
                string enginePath = EmbeddedPathToEnginePath(x);

                if (enginePath != null) InternalManifest.TryAdd(enginePath, x);
            });
        }

        /// <inheritdoc />
        public override byte[] GetAsset(string enginePath)
        {
            // Convert to embedded path.
            bool found = InternalManifest.TryGetValue(enginePath, out string embeddedPath);

            // Check if found.
            if (!found)
            {
                Context.Log.Error($"Couldn't find asset [{enginePath}].", MessageSource.AssetLoader);
                return new byte[0];
            }

            byte[] data;

            // Read the asset from the embedded file.
            using (Stream stream = Assembly.GetManifestResourceStream(embeddedPath))
            {
                // Not found.
                if (stream == null)
                {
                    Context.Log.Error($"Couldn't read asset [{enginePath}] with embedded path [{embeddedPath}].", MessageSource.AssetLoader);
                    return new byte[0];
                }

                // Read from stream.
                data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
            }

            return data;
        }

        /// <summary>
        /// Convert an embedded path to an engine path.
        /// </summary>
        /// <param name="embeddedPath">The embedded file path to convert.</param>
        /// <returns>An engine path corresponding to the embedded path.</returns>
        public string EmbeddedPathToEnginePath(string embeddedPath)
        {
            int rootIndex = embeddedPath.IndexOf(Folder + ".", StringComparison.Ordinal);

            // if not in the root folder then it doesn't concern us.
            if (rootIndex == -1)
            {
                return null;
            }

            // Remove everything before the root folder.
            string noRoot = embeddedPath.Substring(rootIndex).Replace(Folder + ".", "");

            // Extract extension because it will be converted to a slash otherwise.
            string extension = noRoot.Substring(noRoot.LastIndexOf(".", StringComparison.Ordinal));

            // The rest of the name is the actual name.
            string withoutExtension = noRoot.Replace(extension, "");

            return withoutExtension.Replace('.', '/') + extension;
        }
    }
}