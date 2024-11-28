#nullable enable

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Utility;
using Emotion.IO;
using Emotion.ExecTest.TestGame.Abilities;
using Emotion.ExecTest.TestGame.Combat;

namespace Emotion.ExecTest.TestGame.UI;

public static class AbilityIcon
{
    public static void RenderAbility(RenderComposer c, Unit? user, Ability ability, Vector3 position, Vector2 size)
    {
        float cooldownProgress = 0.0f;
        int timeNow = (int) Engine.CurrentGameTime;

        // Ability cooldown
        var meta = user?.GetAbilityMeta(ability);
        if (meta != null)
        {
            int cooldownTimestamp = meta.CooldownTimeStamp;
            if (cooldownTimestamp > timeNow)
            {
                float cooldown = (float) cooldownTimestamp - meta.LastTimeCast;
                float cooldownPassed = (float) cooldownTimestamp - timeNow;
                cooldownProgress = (cooldownPassed / cooldown);
            }
        }

        // Global cooldown
        if (cooldownProgress == 0f &&
            ability.Flags.HasFlag(AbilityFlags.CantBeUsedOnGlobalCooldown) && 
            user != null && user.GlobalCooldownTimestamp > timeNow)
        {
            int cooldownTimestamp = user.GlobalCooldownTimestamp;
            float cooldown = (float)cooldownTimestamp - user.GlobalCooldownLastActivated;
            float cooldownPassed = (float)cooldownTimestamp - timeNow;
            float globalCdProgress = 1.0f - (cooldownPassed / cooldown);
            //cooldownProgress = MathF.Min(cooldownProgress, globalCdProgress);
        }

        // Can't use at all
        if (cooldownProgress == 0 && user != null && ability.CanUse(user, user.Target) != AbilityCanUseResult.CanUse)
        {
            cooldownProgress = 1f;
        }

        Texture? auraIcon = null;
        if (!string.IsNullOrEmpty(ability.Icon))
        {
            var auraIconAsset = Engine.AssetLoader.Get<TextureAsset>(ability.Icon);
            if (auraIconAsset != null)
            {
                if (!auraIconAsset.Texture.Smooth) auraIconAsset.Texture.Smooth = true;
                auraIcon = auraIconAsset.Texture;
            }
        }

        c.RenderSprite(position, size, Color.White, auraIcon);
        RenderProgress(c, position, size, Color.Black * 0.5f, cooldownProgress);
    }

    public static void RenderAura(RenderComposer c, Aura aura, Vector3 position, Vector2 size)
    {
        float progress = 1f;// 1.0f - ((float)aura.TimePassed / aura.Duration);

        Texture? auraIcon = null;
        if (!string.IsNullOrEmpty(aura.Icon))
        {
            var auraIconAsset = Engine.AssetLoader.Get<TextureAsset>(aura.Icon);
            if (auraIconAsset != null)
            {
                if (!auraIconAsset.Texture.Smooth) auraIconAsset.Texture.Smooth = true;
                auraIcon = auraIconAsset.Texture;
            }
        }

        c.SetStencilTest(true);
        c.ToggleRenderColor(false);
        c.StencilStartDraw();

        //c.RenderRoundedRectSdf(position, size, Color.White, 40);
        c.RenderRoundedRect(position, size, Color.White, 4);

        c.ToggleRenderColor(true);
        c.StencilFillIn();

        c.RenderSprite(position, size, Color.White, auraIcon);
        c.RenderSprite(position, size, Color.Black * 0.65f);
        RenderProgress(c, position, size, Color.White, progress, auraIcon);

        c.SetStencilTest(false);
    }

    private static void RenderProgress(RenderComposer composer, Vector3 position, Vector2 size, Color color, float progress, Texture? t = null)
    {
        float progressAsAngle = Maths.Map(progress, 1f, 0f, 0, 359);

        Span<VertexData> vertices = composer.RenderStream.GetStreamMemory(8 * 3, BatchMode.SequentialTriangles, t);
        Debug.Assert(vertices != null);

        uint c = color.ToUint();
        //for (var i = 0; i < vertices.Length; i++)
        //{
        //    vertices[i].Color = c;
        //    vertices[i].UV = Vector2.Zero;
        //    vertices[i].Vertex = Vector3.Zero;
        //}

        Vector3[] rectPoints = new Vector3[]
        {
                position + new Vector3(size.X / 2f, 0, 0),
                position + new Vector3(size.X, 0, 0),
                position + new Vector3(size.X, size.Y / 2f, 0),

                position + new Vector3(size.X, size.Y, 0),
                position + new Vector3(size.X / 2f, size.Y, 0),
                position + new Vector3(0, size.Y, 0),
                position + new Vector3(0, size.Y / 2f, 0),
                position + new Vector3(0, 0, 0),
                position + new Vector3(size.X / 2f, 0, 0),
        };

        Vector2[] rectUvs = new Vector2[]
        {
                new Vector2(0.5f, 0),
                new Vector2(1f, 0),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0, 1f),
                new Vector2(0, 0.5f),
                new Vector2(0, 0),
                new Vector2(0.5f, 0),
        };

        int vertId = 0;
        for (int i = 0; i < 8; i++)
        {
            ref var v0 = ref vertices[vertId];
            ref var v1 = ref vertices[vertId + 1];
            ref var v2 = ref vertices[vertId + 2];

            v0.Color = c;
            v0.UV = rectUvs[i];
            v0.Vertex = rectPoints[i];

            v1.Color = c;
            v1.UV = rectUvs[i + 1];
            v1.Vertex = rectPoints[i + 1];

            v2.Color = c;
            v2.UV = new Vector2(0.5f);
            v2.Vertex = position + new Vector3(size.X / 2f, size.Y / 2f, 0);

            float startAngle = i * 45;
            float endAngle = (i * 45) + 45;

            float percent = Maths.Map(progressAsAngle, startAngle, endAngle, 0, 1f);
            percent = Maths.Clamp01(percent);
            v0.Vertex = Vector3.Lerp(v0.Vertex, v1.Vertex, percent);
            v0.UV = Vector2.Lerp(v0.UV, v1.UV, percent);

            vertId += 3;
        }

        if (progress != 1f)
        {
            composer.SetClipRect(new Rectangle(position, size));

            uint headerColor = Color.PrettyYellow.ToUint();
            float headerEndAngle = progressAsAngle + 15;
            if (headerEndAngle > 360) headerEndAngle = 360;

            float pAsAngleRad = Maths.DegreesToRadians(progressAsAngle - 90f);
            float headerEndRad = Maths.DegreesToRadians(headerEndAngle - 90f);

            float radius = (MathF.Max(size.X, size.Y) / 2f) * 1.2f;
            float x0 = MathF.Cos(pAsAngleRad) * radius;
            float y0 = MathF.Sin(pAsAngleRad) * radius;

            float x1 = MathF.Cos(headerEndRad) * radius;
            float y1 = MathF.Sin(headerEndRad) * radius;

            Vector3 center = position + new Vector3(size.X / 2f, size.Y / 2f, 0);

            vertices = composer.RenderStream.GetStreamMemory(3, BatchMode.SequentialTriangles);
            ref var vS0 = ref vertices[0];
            ref var vS1 = ref vertices[0 + 1];
            ref var vS2 = ref vertices[0 + 2];

            vS0.Color = headerColor;
            vS0.UV = Vector2.Zero;
            vS0.Vertex = center + new Vector3(x0, y0, 0);

            vS1.Color = headerColor;
            vS1.UV = Vector2.Zero;
            vS1.Vertex = center + new Vector3(x1, y1, 0f);

            vS2.Color = headerColor;
            vS2.UV = Vector2.Zero;
            vS2.Vertex = center;

            composer.SetClipRect(null);
        }

    }
}