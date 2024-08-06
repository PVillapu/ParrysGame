using Godot;

[GlobalClass]
public partial class EnemyCharacter : CharacterBody2D
{
    [ExportGroup("Damage")]
    [Export]
    private int Damage = 0;
    [Export]
    private Area2D AttackAreas = null;
    [Export]
    private Sprite2D CharacterSprite = null;

    public override void _EnterTree()
    {
        if (AttackAreas == null) return;
        AttackAreas.AreaEntered += OnAttackHitPlayer;
    }

    public override void _Process(double delta)
    {
        if(CharacterSprite == null) return;

        float X = CharacterSprite.FlipH ? -1f : 1f;
        AttackAreas.Scale = new Vector2(X, 1);
    }

    protected virtual void OnAttackHitPlayer(Area2D area)
    {
        PlayerCharacterController Player = area.GetParent() as PlayerCharacterController;
        if(Player == null) return;

        // Deal damage to player
        Player.GetHealthComponent().TakeDamage(Damage);
    }
}
