#region Using

#endregion

namespace Emotion.Game.QuadTree
{
	public class QuadTreeQuery<T>
	{
		public IShape SearchArea;
		public List<T> Results;
	}
}