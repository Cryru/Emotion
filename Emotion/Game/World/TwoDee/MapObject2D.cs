#nullable enable

namespace Emotion.Game.World.TwoDee;

public class MapObject2D : GameObject
{
    #region Transform

    public float Width => ScaleX;

    public float Height => ScaleY;

    /// <summary>
    /// The center of the object's bounds.
    /// </summary>
    [DontSerialize]
    public Vector2 BoundingRectCenter
    {
        get => GetBoundingRect().Center;
        set
        {
            _x = value.X - _scaleX / 2;
            _y = value.Y - _scaleY / 2;

            Moved();
        }
    }

    public override Rectangle GetBoundingRect()
    {
        return new Rectangle(_x, _y, _scaleX, _scaleY);
    }

    public override Cube GetBoundingCube()
    {
        return new Cube(GetBoundingRect().Center.ToVec3(Z), new Vector3(_scaleX / 2f, _scaleY / 2f, 0.5f));
    }

    public void SetBoundingRect(Rectangle rect)
    {
        _x = rect.X;
        _y = rect.Y;
        _scaleX = rect.Width;
        _scaleY = rect.Height;

        Moved();
        Resized();
    }

    #endregion

}
