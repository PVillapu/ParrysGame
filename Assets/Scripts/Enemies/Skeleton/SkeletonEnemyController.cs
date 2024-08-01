using Godot;

public partial class SkeletonEnemyController : CharacterBody2D
{
    [Export]
    public float Speed = 100.0f;
    [Export]
    public float MeleeDistance = 100f;
    [Export]
    public float AttackRate = 2.5f;

    public AnimationPlayer CharacterAnimationPlayer = null;
    public Sprite2D CharacterSprite2D = null;
    public Area2D DetectionArea2D = null;
    public PlayerCharacterController PlayerCC = null;

    private SkeletonStateMachine CharacterSM = null;

    private float LastAttackTime = 0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _EnterTree()
    {
        base._EnterTree();

        CharacterAnimationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
        CharacterSprite2D = (Sprite2D)GetNode("Sprite2D");
        DetectionArea2D = (Area2D)GetNode("Area2D");

        if (DetectionArea2D != null)
        {
            DetectionArea2D.BodyShapeEntered += OnShapeEnteredDetectionArea;
        }
    }

    public override void _Ready()
    {
        // Setup SM
        CharacterSM = new SkeletonStateMachine(this);
        CharacterSM.SetupSM();
    }

    private void OnShapeEnteredDetectionArea(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        PlayerCC = body as PlayerCharacterController;
    }

    public override void _PhysicsProcess(double delta)
    {
        CharacterSM.EvaluateSM(delta);
    }

    public override void _ExitTree()
    {
        CharacterSM.CleanSM();

        base._ExitTree();
    }

    private void ManageCharacterBehavior(double delta)
    {
        Vector2 velocity = Velocity;

        // Add the gravity if needed
        if (!IsOnFloor())
            velocity.Y += gravity * (float)delta;

        // Check if has seen the player
        if (PlayerCC == null)
        {
            if (CharacterAnimationPlayer == null) return;
            
            CharacterAnimationPlayer.Play("skeleton_idle");
            MoveAndSlide();

            return;
        }

        if (Time.GetTicksMsec() < LastAttackTime + AttackRate) return;

        if(PlayerCC.Position.DistanceTo(Position) > MeleeDistance)
        {
            float XDirection = PlayerCC.Position.X - Position.X;

            if(XDirection != 0)
            {
                XDirection /= Mathf.Abs(XDirection);
                velocity.X = XDirection * Speed;
                CharacterSprite2D.FlipH = XDirection < 0f;

                if (CharacterAnimationPlayer == null) return;

                CharacterAnimationPlayer.Play("skeleton_walk");
            }
        }
        else
        {
            LastAttackTime = Time.GetTicksMsec();
            velocity.X = 0;
            Attack();
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void Attack()
    {
        if(CharacterAnimationPlayer == null) return;

        CharacterAnimationPlayer.Play("skeleton_attack");
    }

    private void HandleState_Idle()
    {

    }

    private void HandleState_Walking()
    {

    }

    private void HandleState_Attacking()
    {

    }
}
