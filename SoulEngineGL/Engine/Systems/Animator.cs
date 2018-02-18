using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.Components;
using Soul.Engine.ECS;
using Soul.Engine.Enums;

namespace Soul.Engine.Systems
{
    internal class Animator : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(AnimationData), typeof(RenderData) };
        }

        protected internal override void Setup()
        {
            // Should be run somewhat first.
            Order = 1;
        }

        internal override void Update(Entity link)
        {
           
        }

        internal override void Draw(Entity link)
        {
            // Get components.
            AnimationData animData = link.GetComponent<AnimationData>();
            RenderData renderData = link.GetComponent<RenderData>();

            // Check if there is a texture attached.
            if (renderData.Texture == null) return;

            // Calculate frames if needed.
            if (!animData.FramesCalculated)
            {
                animData.CalculateFrames(
                    new Microsoft.Xna.Framework.Vector2(renderData.Texture.Width, renderData.Texture.Height));
                renderData.TextureArea = animData.CurrentFrameRect;
            }

            // Check if animation has finished.
            if (animData.Finished) return;

            // Add to the timer.
            animData.Timer += Core.Context.Frametime;

            // Check if time has passed for a frame switch.
            if (!(animData.Timer >= animData.FrameTime)) return;

            // Subtract frame time.
            animData.Timer -= animData.FrameTime;

            // Go to the next frame.
            NextFrame(animData);

            // Set the texture to the new frame texture.
            renderData.TextureArea = animData.CurrentFrameRect;
        }

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
                    if (animData.CurrentFrame == animData.EndingFrame)
                        animData.CurrentFrame = animData.StartingFrame;
                    else
                        animData.CurrentFrame++;
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
    }
}
