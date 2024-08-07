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

    [ExportGroup("Health")]
    [Export]
    private HealthComponent EnemyHealthComponent = null;

    public override void _EnterTree()
    {
        if (AttackAreas != null)
        {
            AttackAreas.AreaEntered += OnAttackHitPlayer;
        }

        if(EnemyHealthComponent != null)
        {
            EnemyHealthComponent.OnDamageTaken += OnDamageTaken;
            EnemyHealthComponent.OnDied += OnDied;
        }
    }

    public override void _Process(double delta)
    {
        if(CharacterSprite == null) return;

        float X = CharacterSprite.FlipH ? -1f : 1f;
        AttackAreas.Scale = new Vector2(X, 1);
    }

    protected virtual void OnAttackHitPlayer(Area2D area)
    {
        HealthComponent PlayerHC = area as HealthComponent;
        if(PlayerHC == null) return;

        // Deal damage to player
        PlayerHC.TakeDamage(Damage);
    }

    private void OnDied()
    {
        GD.Print("Skeleton died!");
    }

    private void OnDamageTaken(int damageAmmount)
    {
        GD.Print("Skeleton received " + damageAmmount + " damage! Current Skeleton health: " + EnemyHealthComponent.GetCurrentHealth());
    }
}
