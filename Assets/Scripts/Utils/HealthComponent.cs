using Godot;

[GlobalClass]
public partial class HealthComponent : Area2D
{
	[Export]
	private int MaxHealth = 1;

	private int CurrentHealth = 0;

	[Signal]
	public delegate void OnDiedEventHandler();
    [Signal]
    public delegate void OnDamageTakenEventHandler(int damageAmmount);

    public override void _Ready()
	{
		CurrentHealth = MaxHealth;
	}

    public void TakeDamage(int damage)
	{
		CurrentHealth -= damage;
		EmitSignal(SignalName.OnDamageTaken, damage);

		if(CurrentHealth <= 0)
		{
			EmitSignal(SignalName.OnDied);
		}
	}

	public void RestoreHealth(int healthToRestore)
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth + healthToRestore, healthToRestore, 0);
	}

	public void RestoreFullHealth()
	{
		CurrentHealth = MaxHealth;
	}

	public void SetMaxHealth(int newMaxHealt)
	{
		MaxHealth = newMaxHealt;
	}

	public int GetMaxHealth()
	{
		return MaxHealth;
	}

	public int GetCurrentHealth()
	{
		return CurrentHealth;
	}
}
