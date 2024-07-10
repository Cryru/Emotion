using Emotion.AutomationMechanism;
using Emotion.IO;
using Emotion.Standard.Image.ImgBin;
using Emotion.Standard.Image.PNG;
using System.IO;
using System.Collections.Generic;
using Emotion.Utility;
using Emotion.Standard.XML;
using System.Threading.Tasks;
using System.Linq;

Dictionary<string, string> fileMap = new Dictionary<string, string>();

if (!EmaSystem.GetGameProjectFolder(out string projectFolder)) return 0;

bool deleteConvertedFiles = false;
if (CommandLineParser.FindArgument(EmaSystem.Args, "deleteConverted", out string _)) deleteConvertedFiles = true;

string assetsFolder = Path.Join(projectFolder, "Assets");
Engine.Log.Info($"Identified assets folder: {assetsFolder}", "EMA");

string[] files = Directory.GetFiles(assetsFolder, "*", SearchOption.AllDirectories);

// Build textures
List<Task> tasks = new List<Task>();
for (int i = 0; i < files.Length; i++)
{
    string fileName = files[i];
    if (fileName.EndsWith(".png")) // Detect based on file?
    {
        var newTask = Task.Run(() =>
        {
            byte[] fileContent = File.ReadAllBytes(fileName);
            byte[] pixels = PngFormat.Decode(fileContent, out PngFileHeader header);
            byte[] convertedFile = ImgBinFormat.Encode(pixels, header.Size, header.PixelFormat);

            string newName = fileName[..^(".png").Length] + ".imgbin";
            File.WriteAllBytes(newName, convertedFile);
            if (deleteConvertedFiles) File.Delete(fileName);

            string fileNameRelativeToAssets = fileName.Replace(assetsFolder, "")[1..];
            string newNameRelativeToAssets = newName.Replace(assetsFolder, "")[1..];

            string assetName = AssetLoader.NameToEngineName(fileNameRelativeToAssets);
            string assetNameNew = AssetLoader.NameToEngineName(newNameRelativeToAssets);

            lock(fileMap)
            {
                fileMap.Add(assetName, assetNameNew);
            }

            Engine.Log.Info($"Converted file: {fileName.Replace(assetsFolder, "")}", "EMA");
        });
        tasks.Add(newTask);
    }
}
Task.WaitAll(tasks.ToArray());

string fileMapAsXML = XMLFormat.To(fileMap);
string remapFilePath = Path.Join(assetsFolder, "AssetRemap.xml");
File.WriteAllText(remapFilePath, fileMapAsXML);

return 1;