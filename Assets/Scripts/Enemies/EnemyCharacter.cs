using Godot;

[GlobalClass]
public partial class EnemyCharacter : CharacterBody2D
{
    [ExportGroup("Damage")]
    [Export]
    private int Damage = 0;

    [Export]
    private Area2D[] AttackAreas = new Area2D[0];

    public override void _EnterTree()
    {
        foreach (Area2D area in AttackAreas)
        {
            if(area == null) continue;
            area.AreaEntered += OnAttackHitPlayer;
        }
    }

    protected virtual void OnAttackHitPlayer(Area2D area)
    {
        PlayerCharacterController Player = area.GetParent() as PlayerCharacterController;
        if(Player == null) return;

        // Deal damage to player
        Player.GetHealthComponent().TakeDamage(Damage);
    }
}
