// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Objects.Bases;
using Emotion.Primitives;

#endregion

namespace Emotion.Objects.Game
{
    public class Camera : Transform
    {
        #region Properties

        /// <summary>
        /// The transform the camera should follow.
        /// </summary>
        public Transform Target { get; set; }

        private Vector2 _targetLastPosition;

        public Rectangle InnerBounds
        {
            get
            {
                // Generate rectangle from the bounds size, and center it on the camera bounds.
                Rectangle temp = new Rectangle(0, 0, (int) _innerBoundSize.X, (int) _innerBoundSize.Y) {Center = Bounds.Center};

                return temp;
            }
            set => _innerBoundSize = new Vector2(value.Width, value.Height);
        }

        private Vector2 _innerBoundSize;

        #endregion

        public Camera(Rectangle bounds) : base(bounds)
        {
            _innerBoundSize = new Vector2((int) (bounds.Width - bounds.Width * 0.60), (int) (bounds.Height - bounds.Height * 0.60));
        }

        public void SnapToTarget()
        {
            Bounds.Center = Target.Bounds.Center;
        }

        public void Update(Input input)
        {
            // Check if no target.
            if (Target == null) return;

            //Point mouseLocation = input.GetMousePosition(this);

            //int cursorDistX = mouseLocation.X - Target.Bounds.X;
            //int cursorDistY = mouseLocation.Y - Target.Bounds.Y;

            //float pullX = 0;
            //float pullY = 0;

            //int pullMaxDist = 100;

            //if (cursorDistX < -100)
            //{
            //    pullX = -50;
            //}
            //else if(cursorDistX < 0)
            //{
            //    pullX = cursorDistX / 100;
            //}

            //if (cursorDistX > 100)
            //{
            //    pullX = 50;
            //}
            //else if(cursorDistX > 0)
            //{
            //    pullX = cursorDistX / 100;
            //}

            //pullX = cursorDistX < 0 ? -1 : 1 * GameMath.SmoothStep(0f, pullMaxDist, Math.Abs(pullX));


            //if (_targetLastPosition != Point.Empty)
            //{
            //    if (Target.Bounds.X < InnerBounds.X || Target.Bounds.X + Target.Bounds.Width > InnerBounds.X + InnerBounds.Width)
            //    {
            //        float diffX = Target.Bounds.X - _targetLastPosition.X;

            //        xPull = (int) (diffX * 1.5f);
            //    }


            //}
            //_targetLastPosition = Target.Bounds.Location;
            ////Console.WriteLine(GameMath.SmoothStep(Center.X, Target.Center.X, 0.1f));

            //float testWayOne = GameMath.Lerp(1575, 1584, 0.1f);
            //float testLeft = GameMath.Lerp(1575, 1574, 0.1f);
            //float diff = testWayOne - testLeft;
            //float testWayOne_Reverse = GameMath.Lerp(1584, 1575, 0.1f);


            //float velX = _targetLastPosition.X - Target.Bounds.Center.X;
            //float velY = _targetLastPosition.Y - Target.Bounds.Center.Y;

            ////Console.WriteLine(velX);

            //float iX = GameMath.Lerp(Bounds.Center.X, Target.Bounds.Center.X + velX, 0.1f);
            //float iY = GameMath.Lerp(Bounds.Center.Y, Target.Bounds.Center.Y + velY, 0.1f);

            //Bounds.Center = new Vector2(Target.Bounds.Center.X - velX * 2, Target.Bounds.Center.Y + velY * 2);
            //_targetLastPosition = Target.Bounds.Center;

            //Console.WriteLine(pullX);

            //Center = new Point(Target.Center.X + (int) pullX, Target.Center.Y + (int) pullY);

            //Center = new Point(Target.Center.X, Target.Center.Y);

            SnapToTarget();
        }
    }
}