#nullable enable

using Emotion.Common.Serialization;

namespace Emotion.WIPUpdates.One.Work;

public class MapObject2D : MapObject
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
        get => BoundingRect.Center;
        set
        {
            _x = value.X - _scaleX / 2;
            _y = value.Y - _scaleY / 2;

            Moved();
        }
    }

    [DontSerialize]
    public override Rectangle BoundingRect
    {
        get => new Rectangle(_x, _y, _scaleX, _scaleY);
    }

    public override Cube BoundingCube
    {
        get => new Cube(BoundingRect.Center.ToVec3(Z), new Vector3(_scaleX / 2f, _scaleY / 2f, 0.5f));
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
