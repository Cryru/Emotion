﻿namespace Emotion.Game.World3D
{
    public class LightModel
    {
        public bool Shadows = false;

        public float AmbientLightStrength = 1f;
        public Color AmbientLightColor = new Color(255, 255, 255);

        public float DiffuseStrength = 0.7f;
        public float ShadowOpacity = 0.25f;
        public Vector3 SunDirection = Vector3.Normalize(new Vector3(0.3f, -0.25f, 0.3f));

        public static LightModel DefaultLightModel = new LightModel();
    }
}