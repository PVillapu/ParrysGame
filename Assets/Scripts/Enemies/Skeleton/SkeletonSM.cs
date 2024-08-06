using Godot;

[GlobalClass]
public partial class SkeletonSM : StateMachine
{
    [Export]
    public float Speed = 100.0f;
    [Export]
    public float MeleeDistance = 100f;
    [Export]
    public float AttackRate = 2.5f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public CharacterBody2D CharacterBody = null;
    public AnimationPlayer CharacterAnimationPlayer = null;
    public Sprite2D CharacterSprite2D = null;
    public Area2D DetectionArea2D = null;

    public override void _EnterTree()
    {
        base._EnterTree();

        CharacterBody = (CharacterBody2D)GetNode("../");
        CharacterAnimationPlayer = (AnimationPlayer)GetNode("../AnimationPlayer");
        CharacterSprite2D = (Sprite2D)GetNode("../Sprite2D");
        DetectionArea2D = (Area2D)GetNode("../DetectionArea");
    }

    public override void _ExitTree()
    {
        CleanSM();

        base._ExitTree();
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
        DetectionArea2D.BodyEntered += OnBodyEnteredDetectionArea;
        DetectionArea2D.BodyExited += OnBodyeExitDetectionArea;

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
        DetectionArea2D.BodyEntered -= OnBodyEnteredDetectionArea;
        DetectionArea2D.BodyExited -= OnBodyeExitDetectionArea;

        base.CleanSM();
    }
}

#region STATES

public class SMSkeletonState : SMState
{
    public SkeletonSM SkeletonStateMachine = null;

    public override void OnEnterState()
    {
        SkeletonStateMachine = StateMachine as SkeletonSM;
    }
}

public class SkeletonIdleState : SMSkeletonState
{
    public override void EvaluateState(double delta)
    {
        CharacterBody2D CharacterBody = SkeletonStateMachine.CharacterBody;

        Vector2 velocity = CharacterBody.Velocity;

        velocity.X = 0f;

        // Add the gravity if needed
        if (!CharacterBody.IsOnFloor())
            velocity.Y += SkeletonStateMachine.gravity * (float)delta;

        CharacterBody.Velocity = velocity;
        CharacterBody.MoveAndSlide();

        SkeletonStateMachine.CharacterAnimationPlayer.Play("skeleton_idle");
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
        CharacterBody2D CharacterBody = SkeletonStateMachine.CharacterBody;

        Vector2 velocity = CharacterBody.Velocity;

        // Add the gravity if needed
        if (!CharacterBody.IsOnFloor())
            velocity.Y += SkeletonStateMachine.gravity * (float)delta;

        float XDirection = PlayerCC.Position.X - CharacterBody.Position.X;

        if (Mathf.Abs(XDirection) > 20f)
        {
            XDirection /= Mathf.Abs(XDirection);
            velocity.X = XDirection * SkeletonStateMachine.Speed;
            SkeletonStateMachine.CharacterSprite2D.FlipH = XDirection < 0f;

            if (SkeletonStateMachine.CharacterAnimationPlayer == null) return;

            SkeletonStateMachine.CharacterAnimationPlayer.Play("skeleton_walk");
        }
        else
        {
            velocity.X = 0f;
            SkeletonStateMachine.CharacterAnimationPlayer.Play("skeleton_idle");
        }

        CharacterBody.Velocity = velocity;
        CharacterBody.MoveAndSlide();
    }
}

public class SkeletonAttackState : SMSkeletonState
{
    public override void OnEnterState()
    {
        base.OnEnterState();

        StateMachine.GetSMValues()["IsAttacking"] = true;

        SkeletonStateMachine.CharacterAnimationPlayer.AnimationFinished += OnAttackAnimationFinished;
        SkeletonStateMachine.CharacterAnimationPlayer.Play("skeleton_attack");
    }

    public override void EvaluateState(double delta)
    {
        CharacterBody2D CharacterBody = SkeletonStateMachine.CharacterBody;

        Vector2 velocity = Vector2.Zero;

        // Add the gravity if needed
        if (!CharacterBody.IsOnFloor())
            velocity.Y += SkeletonStateMachine.gravity * (float)delta;

        CharacterBody.Velocity = velocity;
        CharacterBody.MoveAndSlide();
    }

    public override void OnExitState()
    {
        SkeletonStateMachine.CharacterAnimationPlayer.AnimationFinished -= OnAttackAnimationFinished;
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
    SkeletonSM SkeletonSM = null;
    CharacterBody2D SkeletonCharacterBody = null;

    public override bool EvaluateTriggerCondition()
    {
        if (PlayerCC == null)
        {
            if (!StateMachine.GetSMValues().ContainsKey("PlayerCharacterController")) return false;

            PlayerCC = (PlayerCharacterController)StateMachine.GetSMValues()["PlayerCharacterController"];
        }

        if(SkeletonSM == null)
        {
            SkeletonSM = StateMachine as SkeletonSM;
        }

        if(SkeletonCharacterBody == null)
        {
            if (SkeletonSM == null) return false;

            SkeletonCharacterBody = SkeletonSM.CharacterBody;
        }

        float LastAttackTime = 0f;
        if (OwnerState.StateMachine.GetSMValues().ContainsKey("LastAttackTime"))
        {
            LastAttackTime = (ulong)OwnerState.StateMachine.GetSMValues()["LastAttackTime"];
        }

        return Time.GetTicksMsec() >= LastAttackTime + SkeletonSM.AttackRate 
            && PlayerCC.Position.DistanceTo(SkeletonCharacterBody.Position) < SkeletonSM.MeleeDistance;
    }
}

#endregion //TRANSITIONS