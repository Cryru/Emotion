#region Using

using Emotion.Game.Animation;
using Emotion.Graphics.Objects;
using Emotion.IO;

#endregion

#nullable enable

namespace Emotion.Game.Animation2D
{
    /// <summary>
    /// Handles a sprite's animation state.
    /// </summary>
    public class SpriteAnimationController
    {
        public AnimatedSprite Data { get; protected set; }
        public string CurrentAnimation { get; protected set; }

        public SpriteAnimationData CurrentAnimationData
        {
            get => _currentAnimData;
        }

        #region State

        /// <summary>
        /// The number of times the animation has looped.
        /// </summary>
        public int LoopCount { get; protected set; }

        /// <summary>
        /// The index of the current frame within the animation.
        /// </summary>
        public int CurrentFrameIndex { get; protected set; }

        /// <summary>
        /// The loaded asset file that contains all frames of animation.
        /// </summary>
        public TextureAsset? AssetTexture { get; protected set; }

        protected float _animTimer;
        protected bool _inReverse;
        protected SpriteAnimationData _currentAnimData;

        #endregion

        public SpriteAnimationController(AnimatedSprite data)
        {
            Data = data;
            AssetTexture = Engine.AssetLoader.Get<TextureAsset>(Data.AssetFile);

            // Set the first animation as current, by default.
            foreach (KeyValuePair<string, SpriteAnimationData> anim in Data.Animations)
            {
                SetAnimation(anim.Key);
                break;
            }

            Assert(_currentAnimData != null);
            Assert(CurrentAnimation != null);
        }

        /// <summary>
        /// Returns whether the controller contains an animation with the provided name.
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            return Data.Animations.ContainsKey(animationName);
        }

        /// <summary>
        /// Set the current animation to the one with the provided name.
        /// </summary>
        public void SetAnimation(string animationName)
        {
            if (Data.Animations.TryGetValue(animationName, out SpriteAnimationData? animData))
            {
                CurrentAnimation = animationName;
                _currentAnimData = animData;
                Reset();
                return;
            }

            Engine.Log.Warning($"Tried to play invalid animation {animationName} from {Data.AssetFile}.", MessageSource.Anim, true);
        }

        /// <summary>
        /// Advance the animation with the provided time.
        /// </summary>
        public void Update(float deltaTimeMs)
        {
            _animTimer += deltaTimeMs;

            if (_currentAnimData.TimeBetweenFrames == 0) return; // Prevent infinite loop.
            while (_animTimer > _currentAnimData.TimeBetweenFrames)
            {
                _animTimer -= _currentAnimData.TimeBetweenFrames;
                ForceNextFrame();
            }
        }

        /// <summary>
        /// Get the information needed to render the sprite at the current frame of animation.
        /// </summary>
        public void GetRenderData(out Vector3 renderPos, out Texture texture, out Rectangle uv, bool flipX = false)
        {
            int frameIndex = _currentAnimData.FrameIndices.Length == 0 ? 0 : _currentAnimData.FrameIndices[CurrentFrameIndex];
            GetRenderDataForFrame(frameIndex, out renderPos, out texture, out uv, flipX);
        }

        /// <summary>
        /// Gets the information needed to render the sprite at a specific absolute frame index (for the frame source).
        /// </summary>
        public void GetRenderDataForFrame(int absFrameIdx, out Vector3 renderPos, out Texture texture, out Rectangle uv, bool flipX = false)
        {
            texture = AssetTexture?.Texture ?? Texture.EmptyWhiteTexture;

            SpriteAnimationFrameSource? frameSource = Data.FrameSource;
            uv = frameSource.GetFrameUV(absFrameIdx);

            renderPos = Vector3.Zero;
            var origin = OriginPosition.TopLeft;
            Vector2 offset = Vector2.Zero;
            if (frameSource.FrameOrigins != null && frameSource.FrameOrigins.Length > absFrameIdx) origin = frameSource.FrameOrigins[absFrameIdx];
            if (frameSource.FrameOffsets != null && frameSource.FrameOffsets.Length > absFrameIdx) offset = frameSource.FrameOffsets[absFrameIdx];

            if (flipX) offset.X = -offset.X;
            float width = uv.Width;
            if (width % 2 != 0)
            {
                if (flipX)
                    width--;
                else
                    width++;
            }

            float height = uv.Height;
            if (height % 2 != 0) height++;

            switch (origin)
            {
                case OriginPosition.TopLeft:
                    renderPos.X += offset.X;
                    renderPos.Y += offset.Y;
                    break;
                case OriginPosition.TopCenter:
                    renderPos.X -= width / 2 - offset.X;
                    renderPos.Y += offset.Y;
                    break;
                case OriginPosition.TopRight:
                    renderPos.X -= uv.Width - offset.X;
                    renderPos.Y += offset.Y;
                    break;
                case OriginPosition.CenterLeft:
                    renderPos.X += offset.X;
                    renderPos.Y -= height / 2 - offset.Y;
                    break;
                case OriginPosition.CenterCenter:
                    renderPos.X -= width / 2 - offset.X;
                    renderPos.Y -= height / 2 - offset.Y;
                    break;
                case OriginPosition.CenterRight:
                    renderPos.X -= uv.Width - offset.X;
                    renderPos.Y -= height / 2 - offset.Y;
                    break;
                case OriginPosition.BottomLeft:
                    renderPos.X += offset.X;
                    renderPos.Y -= uv.Height - offset.Y;
                    break;
                case OriginPosition.BottomCenter:
                    renderPos.X -= width / 2 - offset.X;
                    renderPos.Y -= uv.Height - offset.Y;
                    break;
                case OriginPosition.BottomRight:
                    renderPos.X -= uv.Width - offset.X;
                    renderPos.Y -= uv.Height - offset.Y;
                    break;
            }
        }

        /// <summary>
        /// Reset the state of the animation playing. The animation will remain the same, but
        /// timing information, current frame, and progress will be reset.
        /// </summary>
        public void Reset()
        {
            CurrentFrameIndex = 0;
            _animTimer = 0;
            LoopCount = 0;
            _inReverse = false;
        }

        /// <summary>
        /// Force the controller to switch to the next frame.
        /// </summary>
        public void ForceNextFrame()
        {
            int nextFrameIdx = GetNextFrameIdx(out bool looped, out bool reversed);
            CurrentFrameIndex = nextFrameIdx;
            _inReverse = reversed;
            if (looped) LoopCount++;
        }

        /// <summary>
        /// Force the controller to switch to the previous frame.
        /// </summary>
        public void ForcePrevFrame()
        {
            int prevFrameIdx = GetPrevFrameIdx(out bool looped, out bool reversed);
            CurrentFrameIndex = prevFrameIdx;
            _inReverse = reversed;
            if (looped) LoopCount++;
        }

        /// <summary>
        /// Get the index of the next frame, and how the frame sequence changed.
        /// </summary>
        public virtual int GetNextFrameIdx(out bool looped, out bool reversed)
        {
            looped = false;
            reversed = _inReverse;

            int lastFrameIdx = _currentAnimData.FrameIndices.Length - 1;

            switch (_currentAnimData.LoopType)
            {
                case AnimationLoopType.None:
                    if (LoopCount > 0) return lastFrameIdx;
                    // If the global frame is the last frame.
                    if (CurrentFrameIndex == lastFrameIdx)
                    {
                        looped = true;
                        return lastFrameIdx;
                    }

                    return CurrentFrameIndex + 1;

                case AnimationLoopType.Normal:
                    // If the global frame is the last frame.
                    if (CurrentFrameIndex == lastFrameIdx)
                    {
                        looped = true;
                        return 0;
                    }

                    return CurrentFrameIndex + 1;
                case AnimationLoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if ((CurrentFrameIndex == lastFrameIdx && !_inReverse) ||
                        (CurrentFrameIndex == 0 && _inReverse))
                    {
                        // Change the reverse flag.
                        reversed = !_inReverse;
                        looped = true;

                        // Depending on the direction set the frame to be the appropriate one.
                        return reversed ? lastFrameIdx - 1 : 0 + 1;
                    }

                    // Modify the current frame depending on the direction we are going in.
                    if (_inReverse)
                        return CurrentFrameIndex - 1;
                    return CurrentFrameIndex + 1;
                case AnimationLoopType.Reverse:
                    // If the global frame is the first frame.
                    if (CurrentFrameIndex == 0)
                    {
                        looped = true;
                        return lastFrameIdx;
                    }

                    return CurrentFrameIndex - 1;
                case AnimationLoopType.NoneReverse:
                    if (LoopCount > 0) return 0;
                    // If the global frame is the first frame.
                    if (CurrentFrameIndex == 0)
                    {
                        looped = true;
                        return lastFrameIdx;
                    }

                    return CurrentFrameIndex - 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get the index of the previous frame, and how the frame sequence changed.
        /// </summary>
        public virtual int GetPrevFrameIdx(out bool looped, out bool reversed)
        {
            looped = false;
            reversed = _inReverse;

            int lastFrameIdx = _currentAnimData.FrameIndices.Length - 1;

            switch (_currentAnimData.LoopType)
            {
                case AnimationLoopType.None:
                    if (CurrentFrameIndex == 0)
                    {
                        looped = true;
                        return 0;
                    }

                    return CurrentFrameIndex - 1;

                case AnimationLoopType.Normal:
                    if (CurrentFrameIndex == 0)
                    {
                        looped = true;
                        return lastFrameIdx;
                    }

                    return CurrentFrameIndex - 1;
                case AnimationLoopType.NormalThenReverse:
                    if (CurrentFrameIndex == lastFrameIdx)
                    {
                        reversed = false;
                        return lastFrameIdx - 1;
                    }

                    if (CurrentFrameIndex == 0)
                    {
                        reversed = true;
                        return 0 + 1;
                    }

                    if (_inReverse)
                        return CurrentFrameIndex + 1;
                    return CurrentFrameIndex - 1;
                case AnimationLoopType.Reverse:
                    if (CurrentFrameIndex == lastFrameIdx)
                    {
                        looped = true;
                        return 0;
                    }

                    return CurrentFrameIndex + 1;
                case AnimationLoopType.NoneReverse:
                    if (CurrentFrameIndex == lastFrameIdx)
                    {
                        looped = true;
                        return lastFrameIdx;
                    }

                    return CurrentFrameIndex + 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}