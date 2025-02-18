#region Using

using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World2D;
using Emotion.UI;
using System.Threading.Tasks;
using Emotion.WIPUpdates.TimeUpdate;
using System.Reflection.Emit;

#endregion

#nullable enable

namespace Emotion.WIPUpdates.TextUpdate;

public class TimerDemoScene : World2DBaseScene<Map2D>
{
    private Timer _linearTimer = new Timer(5000, FactorMethod.Linear, FactorType.InOut);
    private Timer _cubicTimer = new Timer(5000, FactorMethod.Cubic, FactorType.InOut);
    private Timer _exponentialTimer = new Timer(5000, FactorMethod.Exponential, FactorType.InOut);
    private Timer _bounceTimer = new Timer(5000, FactorMethod.Bounce, FactorType.In);

    private UIText _label;

    public override Task LoadAsync()
    {
        _editor.EnterEditor();

        UIText label = new()
        {
            AnchorAndParentAnchor = UIAnchor.CenterCenter
        };
        _editor.UIController!.AddChild(label);
        _label = label;

        return Task.CompletedTask;
    }

    public override void Update()
    {
        base.Update();

        _label.Text = $"{_linearTimer.GetFactorClamped():0.0}\n{_cubicTimer.GetFactorClamped():0.0}\n{_exponentialTimer.GetFactorClamped():0.0}\n{_bounceTimer.GetFactorClamped():0.0}";

        _linearTimer.Update(Engine.DeltaTime);
        if (_linearTimer.Finished) _linearTimer.Reset();

        _cubicTimer.Update(Engine.DeltaTime);
        if (_cubicTimer.Finished) _cubicTimer.Reset();

        _exponentialTimer.Update(Engine.DeltaTime);
        if (_exponentialTimer.Finished) _exponentialTimer.Reset();

        _bounceTimer.Update(Engine.DeltaTime);
        if (_bounceTimer.Finished) _bounceTimer.Reset();
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);

        composer.RenderCircle(new Vector3(100, 100, 0) + new Vector3(200, 0, 0) * _linearTimer.GetFactorClamped(), 20, Color.PrettyYellow, true);
        composer.RenderCircle(new Vector3(100, 150, 0) + new Vector3(200, 0, 0) * _cubicTimer.GetFactorClamped(), 20, Color.PrettyYellow, true);
        composer.RenderCircle(new Vector3(100, 200, 0) + new Vector3(200, 0, 0) * _exponentialTimer.GetFactorClamped(), 20, Color.PrettyYellow, true);
        composer.RenderCircle(new Vector3(100, 250, 0) + new Vector3(200, 0, 0) * _bounceTimer.GetFactorClamped(), 20, Color.PrettyYellow, true);

        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);
    }
}
