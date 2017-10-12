using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine;
using Soul;
using Raya.Graphics;
using Raya.Graphics.Primitives;
using Soul.Engine;

namespace Examples.ESScripts
{
    public static class Tree
    {
        private static string mapsPath;
        private static Context SEContext;
        private static List<ScriptNode> Nodes = new List<ScriptNode>();
        private static List<Basic> Circles = new List<CircleShape>();
        private static Text ToolTip;
        private static Text fpsMeter;
        private static RectangleShape ToolTipBG;
        private static int selectedNode = -1;
        private static bool moveMode = false;

        public static void Main()
        {
            // Initiate SoulEngine.
            SEContext = new Context(null);

            // Read all scripts. (Try debug dropbox path, and if running on another pc try relative tools path.)
            string scriptsPath;
            if(System.IO.Directory.Exists(@"C:\Users\Vlad\Dropbox\Project RobotTextAdventure\Demo\Windows\Demo\Content\Scripts"))
            {
                scriptsPath = @"C:\Users\Vlad\Dropbox\Project RobotTextAdventure\Demo\Windows\Demo\Content\Scripts\";
                mapsPath = @"C:\Users\Vlad\Dropbox\Project RobotTextAdventure\Demo\Windows\Demo\Content\Maps\";
            }
            else
            {
                scriptsPath = @"..\..\Content\Scripts\";
                mapsPath = @"..\..\Content\Maps\";
            }

            string[] files = System.IO.Directory.GetFiles(scriptsPath, "*", System.IO.SearchOption.AllDirectories);

            // Read through file and extract the script node.
            for (int i = 0; i < files.Length; i++)
            {
                //ScriptNode currentFileNode = ExtractNode(files[i]);
                //if (currentFileNode != null) Nodes.Add(currentFileNode);
            }

            // Sort by emotion.
            Nodes.OrderBy(x => x.Emotion).ThenBy(x => x.ScriptName);

            // Generate drawing objects from every node.
            for (int i = 0; i < Nodes.Count; i++)
            {
                CircleShape nodeCircle = new CircleShape(5);
                nodeCircle.FillColor = ScriptNode.ColorFromEmotion(Nodes[i].Emotion);
                nodeCircle.OutlineColor = Color.Black;
                nodeCircle.OutlineThickness = 1;

                Circles.Add(nodeCircle);
            }

            // Calculate node positions.
            CalculatePosition();

            // Define a font for text.
            Font font = new Font("Data/arial.ttf");

            // Setup tooltip.
            ToolTip = new Text("", font);
            ToolTip.FillColor = Color.White;
            ToolTip.CharacterSize = 12;

            ToolTipBG = new RectangleShape();
            ToolTipBG.FillColor = new Color(0, 0, 0, 200);
            ToolTipBG.OutlineColor = Color.White;
            ToolTipBG.OutlineThickness = 1;

            // Hook up events.
            SEContext.MouseMoved += MouseMoved;
            SEContext.MouseButtonPressed += MouseClicked;
            SEContext.MouseButtonReleased += MouseOverClick;

            fpsMeter = new Text(SEContext.FPS.ToString(), font);
            fpsMeter.FillColor = Color.Yellow;
            fpsMeter.CharacterSize = 10;
            fpsMeter.Position = new Vector2f(5, 5);

            // Start engine loop.
            while (SEContext.Running)
            {
                SEContext.Tick();
                Update();
                SEContext.StartDraw();
                Draw();
                SEContext.EndDraw();
            }
        }


      
        private static void CalculatePosition()
        {
            int xOffset = 0;
            int yOffset = 0;
            int X = 0;
            int Y = 0;

            for (int i = 0; i < Circles.Count; i++)
            {
                if (i != 0 && Nodes[i].Emotion != Nodes[i - 1].Emotion)
                {
                    X = 0;
                    Y = 0;
                }

                if (Nodes[i].Emotion == Emotion.Neutral)
                {
                    xOffset = 30;
                    yOffset = 30;
                }
                else if (Nodes[i].Emotion == Emotion.Anger)
                {
                    xOffset = 300;
                    yOffset = 30;
                }
                else if (Nodes[i].Emotion == Emotion.Fear)
                {
                    xOffset = 600;
                    yOffset = 30;
                }
                else if (Nodes[i].Emotion == Emotion.Joy)
                {
                    xOffset = 30;
                    yOffset = 300;
                }
                else if (Nodes[i].Emotion == Emotion.Sadness)
                {
                    xOffset = 300;
                    yOffset = 300;
                }

                Circles[i].Position = new Raya.Graphics.Primitives.Vector2f(xOffset + X, yOffset + Y);
                X += 50;

                if (X > 200)
                {
                    Y += 50;
                    X = 0;
                }
            }
        }
        private static int FindMouseInObject()
        {
            Vector2 MousePosition = Raya.Input.Mouse.GetPosition(SEContext.Window);
            MousePosition = (Vector2) SEContext.Window.MapPixelToCoords(MousePosition);

            for (int i = 0; i < Circles.Count; i++)
            {
                Rectanglef CircleBounds = Circles[i].GetGlobalBounds();

                if (CircleBounds.Contains((Vector2f)MousePosition))
                {
                    return i;
                }
            }

            return -1;
        }
        private static List<ScriptNode> GetConnectionsToTarget(ScriptNode Node)
        {
            List<ScriptNode> Matches = new List<ScriptNode>();

            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int l = 0; l < Nodes[i].ScriptLinks.Count; l++)
                {
                    if(Nodes[i].ScriptLinks[l].destinationScript == Node.ScriptName && Nodes[i].ScriptLinks[l].destinationEmotion == Node.Emotion)
                    {
                        Matches.Add(Nodes[i]);
                        break;
                    }
                }
            }

            

            return Matches.OrderBy(x => x.Emotion).ThenBy(x => x.ScriptName).ToList();
        }

        private static void MouseMoved(object sender, Raya.Events.MouseMoveEventArgs e)
        {
            if (moveMode) return;

            // Find the node the mouse is over.
            selectedNode = FindMouseInObject();

            if (selectedNode != -1)
            {
                // Move the tooltip to this node.
                ToolTip.DisplayedString = Nodes[selectedNode].ScriptName + "\n Out:\n";

                // Show connections in the tooltip.
                for (int i = 0; i < Nodes[selectedNode].ScriptLinks.Count; i++)
                {
                    ToolTip.DisplayedString += " |- " + Nodes[selectedNode].ScriptLinks[i].destinationEmotion + "/" + Nodes[selectedNode].ScriptLinks[i].destinationScript + "\n";
                }

                // Get nodes connected to this one.
                List<ScriptNode> ConnectedNodes = GetConnectionsToTarget(Nodes[selectedNode]);
                ToolTip.DisplayedString += " In:\n";

                for (int i = 0; i < ConnectedNodes.Count; i++)
                {
                    ToolTip.DisplayedString += " |- " + ConnectedNodes[i].Emotion + "/" + ConnectedNodes[i].ScriptName + "\n";
                }


                // Position the tooltipBG.
                ToolTipBG.Position = Circles[selectedNode].GetGlobalBounds().Position;
                ToolTipBG.Position = new Vector2f(ToolTipBG.Position.X + 20, ToolTipBG.Position.Y);
                ToolTipBG.Size = new Vector2f(ToolTip.GetGlobalBounds().Size.X + 10, ToolTip.GetGlobalBounds().Size.Y + 10);

                ToolTip.Position = new Vector2f(ToolTipBG.Position.X + 5, ToolTipBG.Position.Y + 5);
            }
            else
            {
                // Hide tooltip if not mouse overed.
                ToolTip.DisplayedString = "";
            }
        }
        private static void MouseOverClick(object sender, Raya.Events.MouseButtonEventArgs e)
        {
            moveMode = false;
        }
        private static void MouseClicked(object sender, Raya.Events.MouseButtonEventArgs e)
        {
            moveMode = true;
        }

        private static void Update()
        {
            fpsMeter.DisplayedString = SEContext.FPS.ToString();

            if(moveMode)
            {
                if(selectedNode != -1)
                {
                    Circles[selectedNode].Position = (Vector2f) Raya.Input.Mouse.GetPosition(SEContext.Window);
                }
            }
        }
        private static void Draw()
        {
            // Draw connection lines.
            DrawLines();

            for (int i = 0; i < Circles.Count; i++)
            {
                // Draw the circle
                SEContext.Window.Draw(Circles[i]);
            }

            // Draw the tooltip.
            if(!moveMode && selectedNode != -1)
            {
                SEContext.Window.Draw(ToolTipBG);
                SEContext.Window.Draw(ToolTip);
            }

            // Draw the fps meter.
            SEContext.Window.Draw(fpsMeter);
        }
        private static void DrawLines()
        {
            for (int i = 0; i < Circles.Count; i++)
            {
                Arrow Arrow = new Arrow();
                Arrow.Position = Circles[i].GetGlobalBounds().Center;

                // Go through all links.
                for (int l = 0; l < Nodes[i].ScriptLinks.Count; l++)
                {
                    // Find the link we are connecting to.
                    int linkId = Nodes.FindIndex(x => x.ScriptName == Nodes[i].ScriptLinks[l].destinationScript &&
                    x.Emotion == Nodes[i].ScriptLinks[l].destinationEmotion);

                    // Check any node is selected show only connections to it.
                    if (selectedNode != -1 && (selectedNode != linkId && selectedNode != i)) continue;

                    // Check if the linked node exists.
                    if (linkId == -1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        // Report to the console.
                        Console.WriteLine("Missing referenced script: " + Nodes[i].ScriptLinks[l].destinationEmotion + "/" + Nodes[i].ScriptLinks[l].destinationScript);
                        Console.ResetColor();
                        continue;
                    }

                    if (Nodes[linkId].Emotion == Nodes[i].Emotion)
                    {
                        Arrow.Color = ScriptNode.ColorFromEmotion(Nodes[i].Emotion);
                    }

                    // Set the arrow direction be outside the radius of the circle.
                    Arrow.Direction = (Vector2f) Raya.System.Helpers.FindPointWithin((Vector2) Arrow.Position, (Vector2) Circles[linkId].GetGlobalBounds().Center, (int) Circles[linkId].Radius * 2);
                    SEContext.Window.Draw(Arrow);
                }
            }
        }
    }
}
