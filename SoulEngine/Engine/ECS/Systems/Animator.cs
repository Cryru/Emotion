using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;

namespace Soul.Engine.ECS.Systems
{
    public class Animator : SystemBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(RenderData), typeof(AnimationData) };
        }

        protected internal override void Setup()
        {
            // Should be run somewhat first.
            Priority = 1;
        }

        protected override void Update(Entity link)
        {
            
        }

        #region Functions

        /// <summary>
        /// Switches to the next frame.
        /// </summary>
        public void NextFrame()
        {
            switch (LoopType)
            {
                case LoopType.None:
                    // If the global frame is the last frame.
                    if (CurrentFrame == EndingFrame)
                    {
                        OnFinish?.Invoke(this);
                        // Stop the timer.
                        _timer = -2;
                    }
                    else
                    {
                        // Increment the frame.
                        CurrentFrame++;
                    }
                    break;
                case LoopType.Normal:
                    // If the global frame is the last frame.
                    if (CurrentFrame == EndingFrame)
                    {
                        //Loop to the starting frame.
                        CurrentFrame = StartingFrame;

                        // Call the loop event.
                        OnFinish?.Invoke(this);
                    }
                    else
                    {
                        // Increment the frame.
                        CurrentFrame++;
                    }
                    break;
                case LoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if (CurrentFrame == EndingFrame && _flagReverse == false ||
                        CurrentFrame == StartingFrame && _flagReverse)
                    {
                        // Change the reverse flag.
                        _flagReverse = !_flagReverse;

                        // Call the loop event.
                        OnFinish?.Invoke(this);

                        // Depending on the direction set the frame to be the appropriate one.
                        CurrentFrame = _flagReverse ? EndingFrame - 1 : StartingFrame + 1;
                    }
                    else
                    {
                        // Modify the current frame depending on the direction we are going in.
                        if (_flagReverse)
                            CurrentFrame--;
                        else
                            CurrentFrame++;
                    }
                    break;
                case LoopType.Reverse:
                    // If the global frame is the first frame.
                    if (CurrentFrame == StartingFrame)
                    {
                        // Loop to the ending frame.
                        CurrentFrame = EndingFrame;

                        // Call the loop event.
                        OnFinish?.Invoke(this);
                    }
                    else
                    {
                        // Otherwise decrement the frame, as we are going in reverse.
                        CurrentFrame--;
                    }
                    break;
                case LoopType.NoneReverse:
                    // If the global frame is the first frame.
                    if (CurrentFrame == StartingFrame)
                    {
                        // Call the finish event.
                        OnFinish?.Invoke(this);

                        // Stop the timer.
                        _timer = -2;
                    }
                    else
                    {
                        // Decrement the frame, as we are going in reverse.
                        CurrentFrame--;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

    }
}
