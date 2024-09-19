using Emotion.Common.Serialization;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.WIPUpdates.One.Work;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public enum CharacterState
{
    NotInCombat,
    InCombat
}

public class Character : MapObject
{
    public string Image = string.Empty;
    private Texture _image = Texture.EmptyWhiteTexture;

    public string Name = "???";
    public int Health;
    public int MaxHealth = 100;

    [DontSerialize]
    public Character? Target;

    private Coroutine _updateCoroutine = Coroutine.CompletedRoutine;

    public Character()
    {
        Size = new Vector2(32);
    }

    public override void Init()
    {
        base.Init();
        Health = MaxHealth;
        UpdateRenderMode();
    }

    public void UpdateCharacter(CoroutineManager manager)
    {
        if (!_updateCoroutine.Finished) return;
        _updateCoroutine = manager.StartCoroutine(UpdateCharacterRoutine());
    }

    public virtual IEnumerator UpdateCharacterRoutine()
    {
        yield break;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead()) return;
        Health -= damage;
        if (Health < 0) Health = 0;
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    public virtual void SetTarget(Character? ch)
    {
        Target = ch;
    }

    #region Render

    private bool _mouseOver = false;
    private bool _targetMode = false;

    public void SetRenderMouseover(bool mouseover)
    {
        _mouseOver = mouseover;
        UpdateRenderMode();
    }

    public void SetRenderTargetMode(bool targetMode)
    {
        _targetMode = targetMode;
        UpdateRenderMode();
    }

    private void UpdateRenderMode()
    {
        if (_mouseOver)
        {
            var modeImage = Engine.AssetLoader.Get<TextureAsset>(Image + "_Target.png");
            if (modeImage != null)
            {
                _image = modeImage.Texture;
                return;
            }
        }

        if (_targetMode)
        {
            var modeImage = Engine.AssetLoader.Get<TextureAsset>(Image + "_Target_Current.png");
            if (modeImage != null)
            {
                _image = modeImage.Texture;
                return;
            }
        }

        var normalImage = Engine.AssetLoader.Get<TextureAsset>(Image + ".png");
        _image = normalImage?.Texture ?? Texture.EmptyWhiteTexture;
    }

    public override void Render(RenderComposer c)
    {
        base.Render(c);
        c.RenderSprite((Position2 - Size / 2f).ToVec3(), Size, Color.White, _image);
    }

    #endregion
}
