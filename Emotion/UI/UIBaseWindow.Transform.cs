namespace Emotion.UI
{
    public partial class UIBaseWindow
    {
        public Vector3 Position { get; protected set; }

        public Vector2 Position2 { get => Position.ToVec2(); }

        public float X { get => Position.X; protected set => Position = new Vector3(value, Position.Y, Position.Z); }

        public float Y { get => Position.Y; protected set => Position = new Vector3(Position.X, value, Position.Z); }

        public float Z { get => Position.Z; protected set => Position = new Vector3(Position.X, Position.Y, value); }

        public Vector2 Size { get; protected set; }

        public float Width { get => Size.X; protected set => Size = new Vector2(value, Size.Y); }

        public float Height { get => Size.Y; protected set => Size = new Vector2(Size.X, value); }

        public Rectangle Bounds { get => new Rectangle(Position, Size); }

        public Vector2 Center { get => Bounds.Center; }
    }
}
