#region Using

using Emotion.Audio;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.Audio
{
	public class AudioSetVolumeModModal : ImGuiModal
	{
		private AudioLayer _layer;
		private float _volume;
		private int _time;

		public AudioSetVolumeModModal(AudioLayer layer) : base("Audio Set Volume")
		{
			_volume = 1f;
			_layer = layer;
		}

		public override void Update()
		{
		}

		protected override void RenderContent(RenderComposer composer)
		{
			ImGui.DragFloat("Volume", ref _volume, 0.1f, 0f, 1f);
			ImGui.DragInt("Milliseconds", ref _time);

			if (ImGui.Button("Set"))
			{
				_layer.SetVolume(_volume, _time);
				Open = false;
			}
			ImGui.SameLine();

			if (ImGui.Button("Close")) Open = false;
		}
	}
}