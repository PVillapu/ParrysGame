using Godot;
using System.Collections.Generic;

abstract public class SMTransition
{
    protected StateMachine StateMachine;
    public void SetStateMachine(StateMachine stateMachine) { StateMachine = stateMachine; }

    protected SMState OwnerState;
    public void SetOwnerState(SMState State) { OwnerState = State; }

    // Evaluates if the condition to change to a new state is met
    public abstract bool EvaluateTriggerCondition();
}

public class SMState
{
    // Set of the transitions associated with this state
    private List<KeyValuePair<SMTransition, SMState>> StateTransitions = new List<KeyValuePair<SMTransition, SMState>>();
    public List<KeyValuePair<SMTransition, SMState>> GetStateTransitions() { return StateTransitions; }

    protected Godot.Collections.Dictionary StateValues = new Godot.Collections.Dictionary();
    public Godot.Collections.Dictionary GetStateValues() { return StateValues; }

    public StateMachine StateMachine;

    // Adds a transition to the state only if it doesnt exist yet in the current registered transitions
    public void AddNewTransition(SMTransition Transition, SMState TargetState)
    {
        KeyValuePair<SMTransition, SMState> NewTransition = new KeyValuePair<SMTransition, SMState>(Transition, TargetState);

        if (StateTransitions.Contains(NewTransition))
        {
#if TOOLS
            GD.PrintErr("[SM] Trying to add a new transition that already exist on state!");
#endif
            return;
        }

        Transition.SetOwnerState(this);
        Transition.SetStateMachine(StateMachine);
        StateTransitions.Add(NewTransition);
    }

    // Called when the state is loaded as the current state in the SM
    public virtual void OnEnterState() {}

    // Called each frame to evaluate the state
    public virtual void EvaluateState(double delta) {}

    // Called when any transition is triggered and a new state is replacing this one
    public virtual void OnExitState() {}
}

[GlobalClass]
public partial class StateMachine : Node2D
{
    private List<SMState> SMStates = new List<SMState>();
    private SMState CurrentState = null;

    protected Godot.Collections.Dictionary StateMachineValues = new Godot.Collections.Dictionary();
    public Godot.Collections.Dictionary GetSMValues() { return StateMachineValues; }

    [Export]
    private bool ShowStateDebug = false;
    [Export]
    private Vector2 DebugTextOffset = Vector2.Zero;
    
    private Label DebugLabel = null;

    public override void _Ready()
    {
        base._Ready();

#if TOOLS
        if (ShowStateDebug)
        {
            CreateDebugLabel();
        }
#endif

        SetupSM();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

#if TOOLS
        if (ShowStateDebug)
        {
            UpdateDebugLabel();
        }
#endif

        EvaluateSM(delta);
    }

    // Registers a new state only if it is not already registered
    protected virtual void RegisterNewState(SMState NewState, bool SetStartingState = false) 
    {
        if (SMStates.Contains(NewState))
        {
#if TOOLS
            GD.PrintErr("[SM] Trying to add a new state but is already in the state machine!");
#endif
            return;
        }

        NewState.StateMachine = this;
        SMStates.Add(NewState);

        if (SetStartingState) CurrentState = NewState;
    }

    // Initializes the start state of the SM. This is a good place to setup all the states and transitions for a state machine
    public virtual void SetupSM()
    {
        if (CurrentState == null)
        {
#if TOOLS
            GD.PrintErr("[SM] The state machine has no start state!");
#endif
            return;
        }
            
        CurrentState.OnEnterState();
    }

    public virtual void EvaluateSM(double delta)
    {
        if(CurrentState == null) return;

        // Evaluate state transitions
        List<KeyValuePair<SMTransition, SMState>> CurrentStateTransitions = CurrentState.GetStateTransitions();
        foreach(KeyValuePair<SMTransition, SMState> Transition in CurrentStateTransitions)
        {
            if (Transition.Key.EvaluateTriggerCondition())
            {
                CurrentState.OnExitState();
                CurrentState = Transition.Value;
                CurrentState.OnEnterState();
                return;
            }
        }

        // Evaluate current state
        CurrentState.EvaluateState(delta);
    }

    public virtual void CleanSM() { }

#if TOOLS
    private void CreateDebugLabel()
    {
        DebugLabel = new Label();
        AddChild(DebugLabel);

        //DebugLabel.SetAnchorsPreset(Control.LayoutPreset.Center); 
        DebugLabel.HorizontalAlignment = HorizontalAlignment.Center;
        DebugLabel.VerticalAlignment = VerticalAlignment.Center;
    }

    private void UpdateDebugLabel()
    {
        DebugLabel.Position = DebugTextOffset - new Vector2(DebugLabel.Size.X / 2f, 0f);

        if (CurrentState != null)
        {
            DebugLabel.Text = CurrentState.ToString();
        }
        else
        {
            DebugLabel.Text = "-NO STATE-";
        }
    }
#endif //TOOLS
}

#region SM UTILS

public class SMConditionalTransition : SMTransition
{
    private Godot.Collections.Dictionary StateMachineConditionPairs = new Godot.Collections.Dictionary();
    private Godot.Collections.Dictionary StateConditionPairs = new Godot.Collections.Dictionary();

    private SMConditionalTransition() { }

    public SMConditionalTransition(Godot.Collections.Dictionary stateMachineConditionPairs, Godot.Collections.Dictionary stateConditionPairs)
    {
        StateMachineConditionPairs = stateMachineConditionPairs.Duplicate(true);
        StateConditionPairs = stateConditionPairs.Duplicate(true);
    }

    public override bool EvaluateTriggerCondition()
    {
        if(StateMachineConditionPairs != null)
        {
            foreach (var (key, value) in StateMachineConditionPairs)
            {
                if (StateMachine.GetSMValues().ContainsKey(key))
                {
                    if (!StateMachine.GetSMValues()[key].Obj.Equals(value.Obj)) return false;
                }
                else return false;
            }
        }

        if (StateConditionPairs != null)
        {
            foreach (var (key, value) in StateConditionPairs)
            {
                if (OwnerState.GetStateValues().ContainsKey(key))
                {
                    if (!OwnerState.GetStateValues()[key].Obj.Equals(value.Obj)) return false;
                }
                else return false;
            }
        }
            
        return true;
    }
}

#endregion //SM UTILS