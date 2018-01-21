// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Systems;
using Soul.Engine.ECS.Components;
using Soul.Engine.Enums;
using Soul.Engine.Graphics.Components;

#endregion

namespace Soul.Engine.ECS.Systems
{
    public class Animator : SystemBase
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(RenderData), typeof(AnimationData)};
        }

        protected internal override void Setup()
        {
            // Should be run somewhat first.
            Priority = 1;
        }

        protected override void Update(Entity link)
        {
            // Get components.
            AnimationData animData = link.GetComponent<AnimationData>();
            RenderData renderData = link.GetComponent<RenderData>();

            // Check if calculated.
            if (animData.HasUpdated)
            {
                animData.ResetFrame();
                animData.CalculateFrames(renderData.Texture.Size);

                // Set the texture to the new frame texture.
                renderData.TextureArea = animData.CurrentFrameRect;
            }

            // Check if finished.
            if (animData.Finished) return;

            // Add to the animation data timer.
            animData.Timer += Window.Current.FrameTime * 1000;

            // Check if time has passed for a frame switch.
            if (animData.Timer >= animData.FrameTime)
            {
                // Subtract frame time.
                animData.Timer -= animData.FrameTime;

                // Go to the next frame.
                NextFrame(animData);

                // Set the texture to the new frame texture.
                renderData.TextureArea = animData.CurrentFrameRect;

                Console.WriteLine("Current frame is " + (animData.CurrentFrame + 1) + " and rect is " + renderData.TextureArea);
            }
        }

        #region Functions

        /// <summary>
        /// Switches to the next frame.
        /// </summary>
        /// <param name="animData">The animation data to switch to the next frame.</param>
        public void NextFrame(AnimationData animData)
        {
            switch (animData.LoopType)
            {
                case AnimationLoopType.None:
                    // If the global frame is the last frame.
                    if (animData.CurrentFrame == animData.EndingFrame)
                        animData.Finished = true;
                    else
                        animData.CurrentFrame++;
                    break;
                case AnimationLoopType.Normal:
                    // If the global frame is the last frame.
                    if (animData.CurrentFrameTotal == animData.EndingFrame)
                        animData.CurrentFrameTotal = animData.StartingFrame;
                    else
                        animData.CurrentFrameTotal++;
                    break;
                case AnimationLoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if (animData.CurrentFrame == animData.EndingFrame && animData.InReverse == false ||
                        animData.CurrentFrame == animData.StartingFrame && animData.InReverse)
                    {
                        // Change the reverse flag.
                        animData.InReverse = !animData.InReverse;

                        // Depending on the direction set the frame to be the appropriate one.
                        animData.CurrentFrame =
                            animData.InReverse ? animData.EndingFrame - 1 : animData.StartingFrame + 1;
                    }
                    else
                    {
                        // Modify the current frame depending on the direction we are going in.
                        if (animData.InReverse)
                            animData.CurrentFrame--;
                        else
                            animData.CurrentFrame++;
                    }

                    break;
                case AnimationLoopType.Reverse:
                    // If the global frame is the first frame.
                    if (animData.CurrentFrame == animData.StartingFrame)
                        animData.CurrentFrame = animData.EndingFrame;
                    else
                        animData.CurrentFrame--;
                    break;
                case AnimationLoopType.NoneReverse:
                    // If the global frame is the first frame.
                    if (animData.CurrentFrame == animData.StartingFrame)
                        animData.Finished = true;
                    else
                        animData.CurrentFrame--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}