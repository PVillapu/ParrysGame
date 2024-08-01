using Godot;

public partial class PlayerCharacterController : CharacterBody2D
{
	[Export]
	private float Speed = 300.0f;
	[Export]
	private float JumpVelocity = -600.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private AnimationPlayer CharacterAnimationPlayer = null;
    private Sprite2D CharacterSprite2D = null;

    public override void _EnterTree()
    {
        base._EnterTree();

        CharacterAnimationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
		CharacterSprite2D = (Sprite2D)GetNode("Sprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		bool justJumped = Input.IsActionJustPressed("player_jump") && IsOnFloor();
		if (justJumped)
			velocity.Y = JumpVelocity;

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
}
