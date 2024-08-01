using utils.StateMachine;
using Godot;

public class SkeletonStateMachine : StateMachine
{
    public SkeletonEnemyController SkeletonController;

    private SkeletonStateMachine() { }

    public SkeletonStateMachine(SkeletonEnemyController skeletonController)
    {
        SkeletonController = skeletonController;
    }

    public override void SetupSM()
    {
        // States
        SMState IdleState = new SkeletonIdleState();
        SMState FollowState = new SkeletonFollowState();
        SMState AttackState = new SkeletonAttackState();

        RegisterNewState(IdleState, true);
        RegisterNewState(FollowState);
        RegisterNewState(AttackState);

        // Transitions
        //IdleState.AddNewTransition();

        // Bind signals
        SkeletonController.DetectionArea2D.BodyEntered += OnBodyEnteredDetectionArea;
        SkeletonController.DetectionArea2D.BodyExited += OnBodyeExitDetectionArea;

        base.SetupSM();
    }

    private void OnBodyEnteredDetectionArea(Node2D body)
    {
        PlayerCharacterController PlayerCC = body as PlayerCharacterController;
        if (PlayerCC == null) return;

        StateMachineValues["LastTimePlayerOnSight"] = Time.GetTicksMsec();
        StateMachineValues["PlayerCharacterController"] = PlayerCC;
        StateMachineValues["IsPlayerOnSight"] = true;
    }

    private void OnBodyeExitDetectionArea(Node2D body)
    {
        PlayerCharacterController PlayerCC = body as PlayerCharacterController;
        if (PlayerCC == null) return;

        StateMachineValues["IsPlayerOnSight"] = false;
    }

    public override void EvaluateSM(double delta)
    {
        base.EvaluateSM(delta);
    }

    public override void CleanSM()
    {
        SkeletonController.DetectionArea2D.BodyEntered -= OnBodyEnteredDetectionArea;
        SkeletonController.DetectionArea2D.BodyExited -= OnBodyeExitDetectionArea;

        base.CleanSM();
    }
}

public class SkeletonIdleState : SMState
{
    SkeletonEnemyController SkeletonController = null;

    public override void OnEnterState()
    {
        SkeletonStateMachine SkeletonSM = StateMachine as SkeletonStateMachine;
        if (SkeletonSM != null)
        {
            SkeletonController = SkeletonSM.SkeletonController;
        }
    }

    public override void EvaluateState(double delta)
    {
        Vector2 velocity = SkeletonController.Velocity;

        // Add the gravity if needed
        if (!SkeletonController.IsOnFloor())
            velocity.Y += SkeletonController.gravity * (float)delta;

        SkeletonController.Velocity = velocity;
        SkeletonController.MoveAndSlide();

        SkeletonController.CharacterAnimationPlayer.Play("skeleton_idle");
    }
}

public class SkeletonFollowState : SMState
{
    PlayerCharacterController PlayerCC = null;
    SkeletonEnemyController SkeletonController = null;

    public override void OnEnterState()
    {
        object PlayerCCValue = StateMachine.GetSMValues()["PlayerCharacterController"];
        PlayerCC = PlayerCCValue as PlayerCharacterController;

        SkeletonStateMachine SkeletonSM = StateMachine as SkeletonStateMachine;
        if (SkeletonSM != null)
        {
            SkeletonController = SkeletonSM.SkeletonController;
        }
    }

    public override void EvaluateState(double delta)
    {
        Vector2 velocity = SkeletonController.Velocity;

        // Add the gravity if needed
        if (!SkeletonController.IsOnFloor())
            velocity.Y += SkeletonController.gravity * (float)delta;

        float XDirection = PlayerCC.Position.X - SkeletonController.Position.X;

        if (Mathf.Abs(XDirection) < 20f && XDirection != 0)
        {
            XDirection /= Mathf.Abs(XDirection);
            velocity.X = XDirection * SkeletonController.Speed;
            SkeletonController.CharacterSprite2D.FlipH = XDirection < 0f;

            if (SkeletonController.CharacterAnimationPlayer == null) return;

            SkeletonController.CharacterAnimationPlayer.Play("skeleton_walk");
        }

        SkeletonController.Velocity = velocity;
        SkeletonController.MoveAndSlide();
    }
}

public class SkeletonAttackState : SMState
{
    public override void EvaluateState(double delta)
    {

    }
}
