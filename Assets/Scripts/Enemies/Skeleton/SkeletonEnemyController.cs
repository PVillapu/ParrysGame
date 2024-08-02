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

    private SkeletonStateMachine CharacterSM = null;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _EnterTree()
    {
        base._EnterTree();

        CharacterAnimationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
        CharacterSprite2D = (Sprite2D)GetNode("Sprite2D");
        DetectionArea2D = (Area2D)GetNode("Area2D");
    }

    public override void _Ready()
    {
        // Setup SM
        CharacterSM = new SkeletonStateMachine(this);
        CharacterSM.SetupSM();
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
}
