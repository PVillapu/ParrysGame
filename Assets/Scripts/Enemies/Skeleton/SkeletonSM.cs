using utils.StateMachine;
using Godot;

#region STATE MACHINE

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
        Godot.Collections.Dictionary AuxStateMachine = new Godot.Collections.Dictionary();
        Godot.Collections.Dictionary AuxState = new Godot.Collections.Dictionary();

        // Idle
        AuxStateMachine.Add("IsPlayerOnSight", true);
        IdleState.AddNewTransition(new SMConditionalTransition(AuxStateMachine, AuxState), FollowState);

        IdleState.AddNewTransition(new PlayerInAttackRangeTransition(), AttackState);

        // Follow
        AuxState.Clear(); AuxStateMachine.Clear();
        AuxStateMachine.Add("IsPlayerOnSight", false);
        FollowState.AddNewTransition(new SMConditionalTransition(AuxStateMachine, AuxState), IdleState);

        FollowState.AddNewTransition(new PlayerInAttackRangeTransition(), AttackState);

        // Attack
        AuxState.Clear(); AuxStateMachine.Clear();
        AuxStateMachine.Add("IsPlayerOnSight", false);
        AuxStateMachine.Add("IsAttacking", false);
        AttackState.AddNewTransition(new SMConditionalTransition(AuxStateMachine, AuxState), IdleState);

        AuxState.Clear(); AuxStateMachine.Clear();
        AuxStateMachine.Add("IsPlayerOnSight", true);
        AuxStateMachine.Add("IsAttacking", false);
        AttackState.AddNewTransition(new SMConditionalTransition(AuxStateMachine, AuxState), FollowState);

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

#endregion //STATE MACHINE

#region STATES

public class SMSkeletonState : SMState
{
    public SkeletonEnemyController SkeletonController = null;

    public override void OnEnterState()
    {
        SkeletonStateMachine SkeletonSM = StateMachine as SkeletonStateMachine;
        if (SkeletonSM != null)
        {
            SkeletonController = SkeletonSM.SkeletonController;
        }
    }
}

public class SkeletonIdleState : SMSkeletonState
{
    public override void OnEnterState()
    {
        base.OnEnterState();
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

public class SkeletonFollowState : SMSkeletonState
{
    PlayerCharacterController PlayerCC = null;

    public override void OnEnterState()
    {
        base.OnEnterState();

        object PlayerCCValue = StateMachine.GetSMValues()["PlayerCharacterController"].Obj;
        PlayerCC = PlayerCCValue as PlayerCharacterController;
    }

    public override void EvaluateState(double delta)
    {
        Vector2 velocity = SkeletonController.Velocity;

        // Add the gravity if needed
        if (!SkeletonController.IsOnFloor())
            velocity.Y += SkeletonController.gravity * (float)delta;

        float XDirection = PlayerCC.Position.X - SkeletonController.Position.X;

        if (Mathf.Abs(XDirection) > 20f)
        {
            XDirection /= Mathf.Abs(XDirection);
            velocity.X = XDirection * SkeletonController.Speed;
            SkeletonController.CharacterSprite2D.FlipH = XDirection < 0f;

            if (SkeletonController.CharacterAnimationPlayer == null) return;

            SkeletonController.CharacterAnimationPlayer.Play("skeleton_walk");
        }
        else
        {
            velocity.X = 0f;
            SkeletonController.CharacterAnimationPlayer.Play("skeleton_idle");
        }

        SkeletonController.Velocity = velocity;
        SkeletonController.MoveAndSlide();
    }
}

public class SkeletonAttackState : SMSkeletonState
{
    public override void OnEnterState()
    {
        base.OnEnterState();

        StateMachine.GetSMValues()["IsAttacking"] = true;

        SkeletonController.CharacterAnimationPlayer.AnimationFinished += OnAttackAnimationFinished;
        SkeletonController.CharacterAnimationPlayer.Play("skeleton_attack");
    }

    public override void EvaluateState(double delta)
    {
        Vector2 velocity = Vector2.Zero;

        // Add the gravity if needed
        if (!SkeletonController.IsOnFloor())
            velocity.Y += SkeletonController.gravity * (float)delta;

        SkeletonController.Velocity = velocity;
        SkeletonController.MoveAndSlide();
    }

    public override void OnExitState()
    {
        SkeletonController.CharacterAnimationPlayer.AnimationFinished -= OnAttackAnimationFinished;
    }

    private void OnAttackAnimationFinished(StringName animName)
    {
        StateMachine.GetSMValues()["IsAttacking"] = false;
        StateMachine.GetSMValues()["LastAttackTime"] = Time.GetTicksMsec();
    }
}

#endregion //STATES

#region TRANSITIONS

public class PlayerInAttackRangeTransition : SMTransition
{
    PlayerCharacterController PlayerCC = null;
    SkeletonEnemyController SkeletonController = null;

    public override bool EvaluateTriggerCondition()
    {
        if (PlayerCC == null)
        {
            if (!OwnerState.StateMachine.GetSMValues().ContainsKey("PlayerCharacterController")) return false;

            PlayerCC = (PlayerCharacterController)OwnerState.StateMachine.GetSMValues()["PlayerCharacterController"];
        }

        if(SkeletonController == null)
        {
            SMSkeletonState SkeletonState = OwnerState as SMSkeletonState;
            if (SkeletonState == null) return false;

            SkeletonController = SkeletonState.SkeletonController;
        }

        float LastAttackTime = 0f;
        if (OwnerState.StateMachine.GetSMValues().ContainsKey("LastAttackTime"))
        {
            LastAttackTime = (ulong)OwnerState.StateMachine.GetSMValues()["LastAttackTime"];
        }

        return Time.GetTicksMsec() >= LastAttackTime + SkeletonController.AttackRate 
            && PlayerCC.Position.DistanceTo(SkeletonController.Position) < SkeletonController.MeleeDistance;
    }
}

#endregion //TRANSITIONS