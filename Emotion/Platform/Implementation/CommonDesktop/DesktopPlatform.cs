#region Using

using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.IO;
using WinApi.User32;

#nullable enable

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    public abstract class DesktopPlatform : PlatformBase
    {
        protected string _platformIdentifier = "";
        protected string _platformExtension = "";

        protected override void Setup(Configurator config)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _platformIdentifier = "win";
                if (RuntimeInformation.OSArchitecture == Architecture.X64 && RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    _platformIdentifier += "64";
                else
                    _platformIdentifier += "32";
                _platformExtension = ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _platformIdentifier = "linux";
                _platformExtension = ".so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _platformIdentifier = "macos";
                _platformExtension = ".dylib";
            }

            ForceMainGPU();
            base.Setup(config);

            if (Engine.AssetLoader == null) return;

            if (!Engine.Configuration.DebugMode || !DeveloperMode_InitializeAssetsFromProject())
            {
                Engine.Log.Trace("Adding default desktop asset sources.", MessageSource.Platform);
                Engine.AssetLoader.AddSource(new FileAssetSource("Assets"));
            }

            Engine.AssetLoader.AddStore(new FileAssetStore("Player"));
        }

        #region File IO

        protected string _devModeAssetFolder = "";
        protected string _devModeProjectFolder = "";

        public string DeveloperMode_GetProjectFolder()
        {
            return _devModeProjectFolder;
        }

        public string DeveloperMode_GetAssetFolder()
        {
            return _devModeAssetFolder;
        }

        public void DeveloperMode_SelectFileNative<T>(Action<T?> onLoaded) where T : Asset, IAssetWithFileExtensionSupport, new()
        {
            string selectedPath = DeveloperMode_OSSelectFileToImport<T>();
            if (string.IsNullOrEmpty(selectedPath)) return;

            string assetPath = _devModeAssetFolder;
            string fullAssetPath = Path.GetFullPath(assetPath, AssetLoader.GameDirectory);

            bool existingAsset = selectedPath.StartsWith(fullAssetPath);
            if (existingAsset)
            {
                string relativePath = Path.GetRelativePath(fullAssetPath, selectedPath);
                T? asset = Engine.AssetLoader.ONE_Get<T>(relativePath);
                onLoaded.Invoke(asset);
            }
            else
            {
                // todo: import asset
            }
        }

        protected virtual string DeveloperMode_OSSelectFileToImport<T>() where T : Asset, IAssetWithFileExtensionSupport
        {
            // nop
            return string.Empty;
        }

        private bool DeveloperMode_InitializeAssetsFromProject()
        {
            Engine.Log.Trace("Attempting to add developer mode desktop asset sources.", MessageSource.Platform);

            string? projectFolder = null;
            try
            {
                projectFolder = DetermineDeveloperModeProjectFolder();
                Engine.Log.Info($"Found project folder: {projectFolder}", MessageSource.Platform);
            }
            catch (Exception)
            {

            }

            if (projectFolder == null) return false;

            _devModeProjectFolder = projectFolder;

            string assetFolder = Path.Join(projectFolder, "Assets");
            _devModeAssetFolder = assetFolder;
            Engine.AssetLoader.ONE_AddAssetSource(new DevModeProjectAssetSource(assetFolder));

            // todo: add store

            return true;
        }

        private static string? DetermineDeveloperModeProjectFolder()
        {
            string currentDirectory = AssetLoader.GameDirectory;
            DirectoryInfo? parentDir = Directory.GetParent(currentDirectory);
            int levelsBack = 1;
            while (parentDir != null)
            {
                bool found = false;
                foreach (DirectoryInfo dir in parentDir.GetDirectories())
                {
                    if (dir.Name == "Assets")
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    found = false;
                    foreach (FileInfo file in parentDir.EnumerateFiles())
                    {
                        if (file.Extension == ".csproj" || file.Extension == ".sln" || file.Extension == ".slnx")
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                {
                    // Convert to relative path.
                    StringBuilder s = new StringBuilder();
                    for (int i = 0; i < levelsBack; i++)
                    {
                        s.Append("..");
                        if (i != levelsBack - 1) s.Append("\\");
                    }

                    return s.ToString();
                }

                parentDir = Directory.GetParent(parentDir.FullName);
                levelsBack++;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// List of connected monitors.
        /// </summary>
        public List<Monitor> Monitors = new List<Monitor>();

        /// <summary>
        /// Returns the monitor the window is on, or the primary monitor if undetermined.
        /// </summary>
        /// <returns>The monitor the window is on.</returns>
        internal virtual Monitor GetMonitorOfWindow()
        {
            if (Monitors.Count == 0) return null;

            var rect = new Rectangle(Position, Size);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Monitor monitor in Monitors)
            {
                var monitorRect = new Rectangle(monitor.Position.X, monitor.Position.Y, monitor.Width, monitor.Height);
                if (monitorRect.Contains(rect) || monitorRect.Intersects(rect)) return monitor;
            }

            return Monitors[0];
        }

        protected void UpdateMonitor(Monitor monitor, bool connected, bool first)
        {
            if (connected)
            {
                Engine.Log.Info($"Detected monitor - {monitor.Name} ({monitor.Width}x{monitor.Height}){(first ? " Primary" : "")}", MessageSource.Platform);

                if (first)
                {
                    Monitors.Insert(0, monitor);

                    // Re-initiate fullscreen mode.
                    if (DisplayMode != DisplayMode.Fullscreen) return;
                    DisplayMode = DisplayMode.Windowed;
                    DisplayMode = DisplayMode.Fullscreen;
                }
                else
                {
                    Monitors.Add(monitor);
                }
            }
            else
            {
                Engine.Log.Info($"Disconnected monitor - {monitor.Name} ({monitor.Width}x{monitor.Height}){(first ? " Primary" : "")}", MessageSource.Platform);

                Monitors.Remove(monitor);
                UpdateDisplayMode();
            }
        }

        /// <summary>
        /// The folder in which native libraries are stored by the desktop platform.
        /// </summary>
        public const string DESKTOP_NATIVE_LIB_FOLDER = "AssetsNativeLibs";

        /// <inheritdoc />
        public override void AssociateAssemblyWithNativeLibrary(Assembly ass, string libraryFolder, string importName)
        {
            string libraryPath = AppendPlatformIdentifierAndExtension(libraryFolder, importName);
            NativeLibrary.SetDllImportResolver(ass, (_, _, _) => Engine.Host.LoadLibrary(libraryPath));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string AppendPlatformIdentifierAndExtension(string libFolder, string libName)
        {
            return Path.Join(DESKTOP_NATIVE_LIB_FOLDER, libFolder, _platformIdentifier, $"{libName}{_platformExtension}");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IntPtr LoadLibrary(string path)
        {
            // Since these libraries are referenced from the base Emotion assembly
            // they cannot be registered via AssociateAssemblyWithNativeLibrary
            // todo: maybe have them as plugins?
            switch (path)
            {
                case "glfw":
                    path = AppendPlatformIdentifierAndExtension("GLFW", "glfw");
                    break;
                case "libEGL":
                    path = AppendPlatformIdentifierAndExtension("ANGLE", "libEGL");
                    break;
                case "libGLESv2":
                    path = AppendPlatformIdentifierAndExtension("ANGLE", "libGLESv2");
                    break;
                case "mesa":
                    path = AppendPlatformIdentifierAndExtension("Mesa", "opengl32");
                    break;
                case "OpenAL":
                {
                    path = Path.Join(DESKTOP_NATIVE_LIB_FOLDER, "OpenAL", _platformIdentifier);

                    // Linux only dependency, located in the same folder.
                    if (_platformIdentifier == "linux") LoadLibrary(Path.Join(path, "libsndio.so.6.1"));

                    path = Path.Join(path, $"openal32{_platformExtension}");
                    break;
                }
            }

            bool loaded = NativeLibrary.TryLoad(path, out IntPtr ptr);
            if (loaded) return ptr;

            Engine.Log.Info($"Couldn't load library {path}", MessageSource.Engine);
            if (!File.Exists(path)) Engine.Log.Info($"File doesn't exist at {path}", MessageSource.Engine);
            return IntPtr.Zero;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return !NativeLibrary.TryGetExport(library, symbolName, out IntPtr ptr) ? IntPtr.Zero : ptr;
        }

        /// <inheritdoc />
        public override void DisplayMessageBox(string message)
        {
            // todo: do this with inheritance in GLFW platform and Win32 platform
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    User32Methods.MessageBox(IntPtr.Zero, message, "Something went wrong!", (uint) MessageBoxFlags.MB_ICONERROR);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    UnixNative.ExecuteBashCommand($"osascript -e 'tell app \"System Events\" to display dialog \"{message}\" buttons {{\"OK\"}} with icon caution'");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    try
                    {
                        // Display a message box using Zenity.
                        UnixNative.ExecuteBashCommand($"zenity --error --text=\"{message}\" --title=\"Something went wrong!\" 2>/dev/null");
                    }
                    catch (Exception)
                    {
                        // Fallback to xmessage.
                        UnixNative.ExecuteBashCommand($"xmessage \"{message}\"");
                    }
            }
            catch (Exception e)
            {
                Engine.Log.Error($"Couldn't display error message box - {message}. {e}", MessageSource.Platform);
            }
        }

        public void OpenUrl(string url)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        // Used to prevent integrated GPU from creating the GL context.
        private static void ForceMainGPU()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                    NativeLibrary.TryLoad("nvapi64.dll", out nint _);
                else
                    NativeLibrary.TryLoad("nvapi.dll", out nint _);

                // todo: figure out how to do AMD
            }
        }
    }
}