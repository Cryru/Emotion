// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examples.ESScripts;
using Raya.Graphics;
using Raya.Primitives;
using Soul.Engine;
using Soul.Engine.Enums;
using Soul.Engine.Objects;
using Soul.IO;

#endregion

namespace Soul.Examples.ESScript
{
    public class ElectricSleepViewer : Scene
    {
        private static string mapsPath;
        private static List<ScriptNode> Nodes = new List<ScriptNode>();
        private static Raya.Graphics.Text ToolTip;
        private static Raya.Graphics.Text fpsMeter;
        private static int selectedNode = -1;
        private static bool moveMode = false;

        public static void Main()
        {
            // Start the engine.
            Core.Start(new ElectricSleepViewer());
        }

        public override void Initialize()
        {
            // Read all scripts. (Try to debug with the dropbox path, if it doesn't work try using a relative tools path.)
            string scriptsPath;
            if (Directory.Exists(@"C:\Users\Vlad\Dropbox\Project RobotTextAdventure\Demo\Windows\Demo\Content\Scripts"))
            {
                scriptsPath = @"C:\Users\Vlad\Dropbox\Project RobotTextAdventure\Demo\Windows\Demo\Content\Scripts\";
                mapsPath = @"C:\Users\Vlad\Dropbox\Project RobotTextAdventure\Demo\Windows\Demo\Content\Maps\";
            }
            else
            {
                scriptsPath = @"..\..\Content\Scripts\";
                mapsPath = @"..\..\Content\Maps\";
            }
            string[] files = Directory.GetFiles(scriptsPath, "*", SearchOption.AllDirectories);

            // Read through file and extract the script node.
            foreach (string file in files)
            {
                ScriptNode currentFileNode = ExtractNode(file);
                if (currentFileNode != null) Nodes.Add(currentFileNode);
            }

            // Sort by emotion.
            Nodes.OrderBy(x => x.Emotion).ThenBy(x => x.ScriptName);

            // Generate drawing objects from every node.
            foreach (ScriptNode node in Nodes)
            {
                GameObject tempObj = new GameObject
                {
                    Size = new Vector2(5, 5)
                };
                tempObj.AddChild(new BasicShape(ShapeType.Circle)
                {
                    Color = ScriptNode.ColorFromEmotion(node.Emotion),
                    OutlineColor = Color.Black,
                    OutlineThickness = 1
                });
                AddChild(tempObj);
            }
        }

        public override void Update()
        {
        }

        #region Node Processing

        /// <summary>
        /// Extracts a script node from a script.
        /// </summary>
        /// <param name="filePath">The path to the file containing the script.</param>
        /// <param name="choice">Whether we are parsing a choice script.</param>
        /// <returns>The extracted script node from the script.</returns>
        private static ScriptNode ExtractNode(string filePath, bool choice = false)
        {
            if (filePath.Contains("_choice") && !choice) return null;

            // Create temp node.
            ScriptNode temp = new ScriptNode();

            // Get file name and emotion.
            string[] pathSplit = filePath.Split('\\');
            string emotion = pathSplit[pathSplit.Length - 2];
            string fileName = pathSplit[pathSplit.Length - 1].Replace(".script", "");
            temp.ScriptName = fileName;
            temp.Emotion = ScriptNode.EmotionFromString(emotion);

            // Check if invalid emotion.
            if (temp.Emotion == Emotion.Unknown) return null;

            // Read file.
            string[] lines = Read.File(filePath, true);

            // Tracks choices in the current script.
            int choiceCount = 0;

            foreach (string line in lines)
            {
                // Look for a map file.
                if (line.Contains("map(\""))
                {
                    string linkedMap = line.Replace("map(\"", "").Replace("\")", "");

                    // Check if that map file exists.
                    if (!File.Exists(mapsPath + linkedMap + ".tmx"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        // Report to the console.
                        Console.WriteLine("Missing map File: " + linkedMap);
                        Console.ResetColor();
                    }
                }

                // Look for a script link in this line, if found add it as a link.
                if (line.Contains("script(\""))
                {
                    temp.ScriptLinks.Add(new NodeLink(line.Replace("script(\"", "").Replace("\")", ""), temp.Emotion));
                }
                // Look for choices.
                else if (line.Contains("choice(\""))
                {
                    choiceCount++;
                    ScriptNode choiceNode =
                        ExtractNode(filePath.Replace(".script", "_choice" + choiceCount + ".script"), true);
                    // Add choice node links to the parent node.
                    temp.ScriptLinks.AddRange(choiceNode.ScriptLinks);
                }
                // Look for transitioning to another emotion.
                else if (line.Contains("emotion(\""))
                {
                    temp.ScriptLinks.Add(new NodeLink("begin",
                        ScriptNode.EmotionFromString(line.Replace("emotion(\"", "").Replace("\")", ""))));
                }
            }

            return temp;
        }

        #endregion
    }
}