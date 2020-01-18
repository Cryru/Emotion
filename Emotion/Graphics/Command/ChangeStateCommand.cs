#region Using

using System;
using Emotion.Common;
using Emotion.Graphics.Shading;

#endregion

namespace Emotion.Graphics.Command
{
    public class ChangeStateCommand : RecyclableCommand
    {
        public bool Force;
        public RenderState State;
        public Action<ShaderProgram> ShaderOnSet;

        public override void Recycle()
        {
            ShaderOnSet = null;
            Force = false;
        }

        public override void Execute(RenderComposer _)
        {
            // Invalid command.
            if (State == null) return;

            // todo: Merge state changing commands one after another into one.

            // Check which state changes should apply, by checking which were set and which differ from the current.
            if (State.Shader != null && (Force || State.Shader != Engine.Renderer.CurrentState.Shader))
            {
                ShaderProgram.EnsureBound(State.Shader.Pointer);
                Engine.Renderer.CurrentState.Shader = State.Shader;
                Engine.Renderer.SyncShader();

                ShaderOnSet?.Invoke(State.Shader);
            }

            if (State.DepthTest != null && (Force || State.DepthTest != Engine.Renderer.CurrentState.DepthTest)) Engine.Renderer.SetDepth((bool) State.DepthTest);
            if (State.StencilTest != null && (Force || State.StencilTest != Engine.Renderer.CurrentState.StencilTest)) Engine.Renderer.SetStencil((bool) State.StencilTest);
            if (State.AlphaBlending != null && (Force || State.AlphaBlending != Engine.Renderer.CurrentState.AlphaBlending)) Engine.Renderer.SetBlending((bool) State.AlphaBlending);
            if (State.ViewMatrix != null && (Force || State.ViewMatrix != Engine.Renderer.CurrentState.ViewMatrix)) Engine.Renderer.SetViewMatrix((bool) State.ViewMatrix);
            if (Force || State.ClipRect != Engine.Renderer.CurrentState.ClipRect) Engine.Renderer.SetClip(State.ClipRect);
        }
    }
}