using Godot;

public partial class SkeletonEnemyController : CharacterBody2D
{
    [Export]
    private float Speed = 100.0f;
    [Export]
    private float MeleeDistance = 100f;
    [Export]
    private float AttackRate = 2.5f;

    private AnimationPlayer CharacterAnimationPlayer = null;
    private Sprite2D CharacterSprite2D = null;
    private Area2D DetectionArea2D = null;
    private PlayerCharacterController PlayerCC = null;
    private AnimationTree CharacterAnimationTree = null;

    private float LastAttackTime = 0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _EnterTree()
    {
        base._EnterTree();

        CharacterAnimationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
        CharacterSprite2D = (Sprite2D)GetNode("Sprite2D");
        DetectionArea2D = (Area2D)GetNode("Area2D");
        CharacterAnimationTree = (AnimationTree)GetNode("AnimationTree");

        if(CharacterAnimationTree != null)
        {
            CharacterAnimationTree.AnimationStarted += OnAnimationStarted;
        }

        if (DetectionArea2D != null)
        {
            DetectionArea2D.BodyShapeEntered += OnShapeEnteredDetectionArea;
        }
    }

    private void OnAnimationStarted(StringName animName)
    {
        GD.Print(animName);
    }

    private void OnShapeEnteredDetectionArea(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        PlayerCC = body as PlayerCharacterController;
    }

    public override void _PhysicsProcess(double delta)
    {
        ManageCharacterBehavior(delta);
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
