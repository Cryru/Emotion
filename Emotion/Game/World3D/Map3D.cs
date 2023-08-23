#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D;

public class Map3D : BaseMap
{
	public override Task InitAsync()
	{
		return Task.CompletedTask;
	}

	public override void Update(float dt)
	{
	}

	public override void Render(RenderComposer c)
	{
	}

	public override void Dispose()
	{
	}
}