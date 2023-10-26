namespace Emotion.Game.World3D
{
	public class LightModel
	{
		public float AmbientLightStrength = 0.5f;
		public float DiffuseStrength = 0.7f;
		public Color AmbientLightColor = new Color(255, 255, 255);
		public Vector3 SunDirection = new Vector3(0.3f, -0.25f, 0.3f);
	}
}