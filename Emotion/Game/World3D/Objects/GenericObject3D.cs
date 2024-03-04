#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Editor;
using Emotion.Game.World;
using Emotion.IO;

#endregion

namespace Emotion.Game.World3D.Objects;

public class GenericObject3D : GameObject3D
{
    [AssetFileName<MeshAsset>]
    public string? EntityPath;

    // Used for serializing the current animation,
    // and for when the animation is set before the object is init
    // (in which case the entity isn't loaded and we don't know what
    // animations they have at all)
    [DontShowInEditor]
    public string? SetAnimationOnInit { get; set; }

    public override async Task LoadAssetsAsync()
    {
        await base.LoadAssetsAsync();
        if (string.IsNullOrEmpty(EntityPath)) return;

        var asset = await Engine.AssetLoader.GetAsync<MeshAsset>(EntityPath);
        Entity = asset?.Entity;
    }

    public override void Init()
    {
        base.Init();

        if (SetAnimationOnInit != CurrentAnimation)
            SetAnimation(SetAnimationOnInit);
    }

    public new void SetAnimation(string? name)
    {
        SetAnimationOnInit = name;
        if (ObjectState == ObjectState.None)
            return;

        base.SetAnimation(name);
    }
}