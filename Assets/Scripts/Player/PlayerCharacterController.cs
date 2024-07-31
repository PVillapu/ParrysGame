using Godot;

public partial class PlayerCharacterController : CharacterBody2D
{
	private const float Speed = 300.0f;
	private const float JumpVelocity = -600.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private AnimationPlayer AnimationPlayer = null;
    private Sprite2D Sprite2D = null;

    public override void _EnterTree()
    {
        base._EnterTree();

        AnimationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
		Sprite2D = (Sprite2D)GetNode("Sprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		bool justJumped = Input.IsActionJustPressed("ui_accept") && IsOnFloor();
		if (justJumped)
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
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
		if (AnimationPlayer == null || Sprite2D == null) return;

		// Sprite flip (right / left)
		if(direction.X != 0)
		{
            Sprite2D.FlipH = direction.X < 0;
        }

		// Jump
        if (JustJumped)
        {
            AnimationPlayer.Play("player_jump");
			return;
        }

        // Animation state
        if (IsOnFloor())
		{
			if(direction.X == 0)
			{
				AnimationPlayer.Play("player_idle");
            }
			else
			{
				AnimationPlayer.Play("player_walk");
			}
		}
		else    // Air
		{
			if(Velocity.Y > 0)
			{
                AnimationPlayer.Play("player_fall");
            }
		}
	}
}
