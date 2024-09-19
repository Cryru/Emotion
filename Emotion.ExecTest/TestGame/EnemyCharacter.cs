using Emotion.Game.Time.Routines;
using Emotion.IO;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class EnemyCharacter : ServerAuthorityCharacter
{
    public int AggroRange = 50;
    public int MeleeRange = 20;
    public int MeleeDamage = 5;
    public int MeleeAttackSpeed = 500;

    public CharacterState State = CharacterState.NotInCombat;


    private bool _showSword;
    private Texture _swordAsset = Texture.EmptyWhiteTexture;
    private Vector2 _velocity;

    public EnemyCharacter()
    {
        Name = "Bad Guy";
        Image = "Test/proto/enemy";
        Size = new Vector2(16);
    }

    public override void Init()
    {
        base.Init();
        _swordAsset = Engine.AssetLoader.Get<TextureAsset>("Test/proto/sword.png")?.Texture ?? Texture.EmptyWhiteTexture;
        _swordAsset.Smooth = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        Position2 += _velocity * 0.1f * dt;
    }

    public override IEnumerator UpdateCharacterRoutine()
    {
        while (true)
        {
            _velocity = Vector2.Zero;
            if (IsDead())
            {
                break;
            }

            if (State == CharacterState.NotInCombat)
            {
                foreach (var obj in Map.ForEachObject())
                {
                    if (obj == this) continue;
                    if (obj is EnemyCharacter) continue;

                    Character? ch = obj as Character;
                    if (ch == null) continue;
                    if (ch.IsDead()) continue;

                    var objPos = ch.Position;
                    if (Vector3.Distance(objPos, Position) < AggroRange)
                    {
                        Target = ch;
                        State = CharacterState.InCombat;
                        break;
                    }
                }
            }
            else if (State == CharacterState.InCombat)
            {
                if (Target == null || Target.IsDead())
                {
                    State = CharacterState.NotInCombat;
                    continue;
                }

                if (Vector2.Distance(Target.Position2, Position2) > MeleeRange)
                {
                    _velocity = Vector3.Normalize(Target.Position - Position).ToVec2();
                }
                else
                {
                    yield return PerformMeleeAttack(Target);
                }
            }

            yield return 16;
        }
    }

    private IEnumerator PerformMeleeAttack(Character target)
    {
        if (Vector2.Distance(target.Position2, Position2) > MeleeRange) yield break;
        _showSword = true;

        target.TakeDamage(MeleeDamage);
        yield return MeleeAttackSpeed;
        _showSword = false;
    }

    public override void Render(RenderComposer c)
    {
        base.Render(c);

        float healthPercent = (float) Health / MaxHealth;
        var bar = new Rectangle(X - 20, Y - 15, 40 * healthPercent, 5);
        c.RenderSprite(bar, Color.PrettyRed * 0.5f);

        if (_showSword)
            c.RenderSprite(Position - new Vector3(8, 8, 0), new Vector2(16), _swordAsset);
    }
}
