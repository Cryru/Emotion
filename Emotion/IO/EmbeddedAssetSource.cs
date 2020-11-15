#region Using

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Emotion.Common;
using Emotion.Standard.Logging;

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
        /// The internal manifest.
        /// </summary>
        public ConcurrentDictionary<string, string> InternalManifest { get; private set; }

        /// <summary>
        /// Create a new source which loads assets embedded into the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load assets from.</param>
        /// <param name="folder">The root embedded folder.</param>
        public EmbeddedAssetSource(Assembly assembly, string folder)
        {
            InternalManifest = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Folder = folder.Replace("/", "$").Replace("\\", "$").Replace("$", ".");
            Assembly = assembly;

            string assemblyFullName = assembly.FullName;
            Name = assemblyFullName?.Substring(0, assemblyFullName.IndexOf(",", StringComparison.Ordinal)) ?? "";

            // Populate internal manifest.
            string[] resources = Assembly.GetManifestResourceNames();
            for (var i = 0; i < resources.Length; i++)
            {
                string resource = resources[i];
                string enginePath = EmbeddedPathToEnginePath(resource);
                if (enginePath != null) InternalManifest.TryAdd(enginePath, resource);
            }
        }

        /// <inheritdoc />
        public override ReadOnlyMemory<byte> GetAsset(string enginePath)
        {
            // Convert to embedded path.
            bool found = InternalManifest.TryGetValue(enginePath, out string embeddedPath);

            // Check if found.
            if (!found) return ReadOnlyMemory<byte>.Empty;

            // Read the asset from the embedded file.
            using Stream stream = Assembly.GetManifestResourceStream(embeddedPath);
            // Not found.
            if (stream == null)
            {
                Engine.Log.Error($"Couldn't read asset [{enginePath}] with embedded path [{embeddedPath}].", MessageSource.AssetLoader);
                return new byte[0];
            }

            // Read from stream.
            var data = new byte[stream.Length];
            stream.Read(data, 0, (int) stream.Length);

            return data;
        }

        /// <summary>
        /// The list of files this source can load.
        /// </summary>
        public override string[] GetManifest()
        {
            return InternalManifest.Keys.ToArray();
        }

        /// <summary>
        /// Convert an embedded path to an engine path.
        /// </summary>
        /// <param name="embeddedPath">The embedded file path to convert.</param>
        /// <returns>An engine path corresponding to the embedded path.</returns>
        public string EmbeddedPathToEnginePath(string embeddedPath)
        {
            var folder = $".{Folder}.";
            int rootIndex = embeddedPath.IndexOf(folder, StringComparison.Ordinal);

            // if not in the root folder then it doesn't concern us.
            if (rootIndex == -1) return null;

            // Remove everything before the root folder.
            string noRoot = embeddedPath.Substring(rootIndex).Replace(folder, "");

            // Extract extension because it will be converted to a slash otherwise.
            string extension = noRoot.Substring(noRoot.LastIndexOf(".", StringComparison.Ordinal));

            // The rest of the name is the actual name.
            string withoutExtension = noRoot.Replace(extension, "");

            return withoutExtension.Replace('.', '/') + extension;
        }

        public override string ToString()
        {
            return $"EmbeddedSource [{Assembly.GetName().Name}] @ ./{Folder}";
        }
    }
}