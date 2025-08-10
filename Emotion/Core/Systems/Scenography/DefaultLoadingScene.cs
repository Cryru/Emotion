#nullable enable

namespace Emotion.Core.Systems.Scenography;

/// <inheritdoc />
/// <summary>
/// The default loading scene.
/// </summary>
public class DefaultLoadingScene : Scene
{
    public override IEnumerator LoadSceneRoutineAsync()
    {
        yield break;
    }

    public override void UpdateScene(float dt)
    {

    }

    public override void RenderScene(Renderer c)
    {
        
    }
}