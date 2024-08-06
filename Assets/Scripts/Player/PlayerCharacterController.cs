using Godot;

[GlobalClass]
public partial class PlayerCharacterController : CharacterBody2D
{
	[ExportGroup("Movement")]
    [Export]
    private float Speed = 300.0f;
    [Export]
    private float JumpVelocity = -600.0f;
    [Export]
    private float JumpInputBufferDelay = 0.1f;
	[Export]
	private float FallingGravityMultiplier = 1.5f;
    [Export]
	private float MaxFallVelocity = 50f;

	[ExportGroup("Jump")]
	[Export]
	private float JumpAirborneFallMultiplier = 1.5f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private AnimationPlayer CharacterAnimationPlayer = null;
    private Sprite2D CharacterSprite2D = null;
	private HealthComponent PlayerHealthComponent = null;

	// Jump
	private ulong LastTimeJumpRequest = 0;
	private bool WantsToJump = false;

	private bool PerformingJump = false;
	private bool HoldingJumpInput = false;
	private bool HasJumpYControl = false;

    public override void _EnterTree()
    {
        base._EnterTree();

        CharacterAnimationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
		CharacterSprite2D = (Sprite2D)GetNode("Sprite2D");
		PlayerHealthComponent = (HealthComponent)GetNode("HealthComponent");

		if(PlayerHealthComponent != null )
		{
            PlayerHealthComponent.OnDamageTaken += OnPlayerDamageTaken;
            PlayerHealthComponent.OnDied += OnPlayerDied;
		}
	}

    private void OnPlayerDamageTaken(int damageAmmount)
    {
		GD.Print("Player received " + damageAmmount + " damage! Current player health: " + PlayerHealthComponent.GetCurrentHealth());
    }

    private void OnPlayerDied()
    {
		GD.Print("Player died!");
    }

    public override void _Process(double delta)
    {
		// Check player jump action pressed
		if (Input.IsActionJustPressed("player_jump")) 
		{
            LastTimeJumpRequest = Time.GetTicksMsec();
			WantsToJump = true;
			HoldingJumpInput = true;
        }

        // Check player jump action released
        if (Input.IsActionJustReleased("player_jump"))
		{
            HoldingJumpInput = false;
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		bool IsGrounded = IsOnFloor();

		PerformingJump = !IsGrounded;

        // Handle Jump
        bool justJumped = WantsToJump && IsGrounded && Time.GetTicksMsec() <= LastTimeJumpRequest + JumpInputBufferDelay * 1000;
		if (justJumped)
		{
			velocity.Y = JumpVelocity;
			WantsToJump = false;
			PerformingJump = true;
			HasJumpYControl = true;
        }

        // See if player is still controlling the airborne Y movement
        HasJumpYControl = PerformingJump && HoldingJumpInput && HasJumpYControl;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("player_moveLeft", "player_moveRight", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

        // Add the gravity.
        if (!IsGrounded)
		{
            // Clamp falling velocity
            if (velocity.Y > 0)
            {
                velocity.Y += gravity * FallingGravityMultiplier * (float)delta;
                velocity.Y = Mathf.Clamp(velocity.Y, 0f, MaxFallVelocity);
            }
            else
            {
				if(PerformingJump && !HasJumpYControl)
				{
					velocity.Y += gravity * JumpAirborneFallMultiplier * (float)delta;
                }
				else
				{
					velocity.Y += gravity * (float)delta;
				}
            }
        }

		Velocity = velocity;
		MoveAndSlide();

		ManageCharacterAnimation(direction, justJumped);
	}

	private void ManageCharacterAnimation(Vector2 direction, bool JustJumped)
	{
		if (CharacterAnimationPlayer == null || CharacterSprite2D == null) return;

		// Sprite flip (right / left)
		if(direction.X != 0)
		{
            CharacterSprite2D.FlipH = direction.X < 0;
        }

		// Jump
        if (JustJumped)
        {
            CharacterAnimationPlayer.Play("player_jump");
			return;
        }

        // Animation state
        if (IsOnFloor())
		{
			if(direction.X == 0)
			{
				CharacterAnimationPlayer.Play("player_idle");
            }
			else
			{
				CharacterAnimationPlayer.Play("player_walk");
			}
		}
		else    // Air
		{
			if(Velocity.Y > 0)
			{
                CharacterAnimationPlayer.Play("player_fall");
            }
		}
	}

	public HealthComponent GetHealthComponent()
	{
		return PlayerHealthComponent;
	}
}
