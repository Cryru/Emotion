using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raya.Graphics;
using Raya.Graphics.Primitives;

namespace Examples.ESScripts
{
    public class ScriptNode
    {
        public string ScriptName;
        public List<NodeLink> ScriptLinks = new List<NodeLink>();
        public Emotion Emotion;

        public static Emotion EmotionFromString(string EmotionString)
        {
            switch (EmotionString)
            {
                case "neutral":
                    return Emotion.Neutral;
                case "anger":
                    return Emotion.Anger;
                case "fear":
                    return Emotion.Fear;
                case "joy":
                    return Emotion.Joy;
                case "sadness":
                    return Emotion.Sadness;
            }

            return Emotion.Unknown;
        }

        public static Color ColorFromEmotion(Emotion Emotion)
        {
            switch (Emotion)
            {
                case Emotion.Neutral:
                    return Color.Black;
                case Emotion.Anger:
                    return Color.Red;
                case Emotion.Fear:
                    return Color.Magenta;
                case Emotion.Joy:
                    return Color.Yellow;
                case Emotion.Sadness:
                    return Color.Blue;
            }

            // If unknown.
            return Color.Cyan;
        }
    }

    public enum Emotion
    {
        Neutral,
        Anger,
        Fear,
        Joy,
        Sadness,
        Unknown
    }

}
