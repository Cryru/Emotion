namespace Emotion.Game.World3D
{
	public class LightModel
	{
		public float AmbientLightStrength = 1f;
		public float DiffuseStrength = 1f;
		public Color AmbientLightColor = new Color(255, 255, 255);
		public Vector3 SunDirection = new Vector3(0.3f, -0.25f, 0.3f);
	}
}