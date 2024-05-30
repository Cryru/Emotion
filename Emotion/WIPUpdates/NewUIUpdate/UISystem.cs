using Emotion.Scenography;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.NewUIUpdate;

// todo: add to Scenes
// todo: scenes - make current scene current even while loading, add loaded bool
// todo: investigate loading exceptions for the 100th time

public class UISystem : UIController
{
}

public abstract class Scene_UIUpdate : Scene
{
    public UISystem UI = new()
    {
        UseNewLayoutSystem = true
    };

    public override void Update()
    {
        UpdateScene(Engine.DeltaTime);
        UI.Update();
    }

    public override void Draw(RenderComposer composer)
    {
        RenderScene(composer);

        composer.SetUseViewMatrix(false);
        composer.SetDepthTest(true);
        composer.ClearDepth();
        UI.Render(composer);
    }

    protected abstract void UpdateScene(float dt);

    protected abstract void RenderScene(RenderComposer c);
}