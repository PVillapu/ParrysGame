#if TOOLS
using Godot;
#endif
using System.Collections.Generic;

namespace utils.StateMachine
{
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
            StateTransitions.Add(NewTransition);
        }

        // Called when the state is loaded as the current state in the SM
        public virtual void OnEnterState() {}

        // Called each frame to evaluate the state
        public virtual void EvaluateState(double delta) {}

        // Called when any transition is triggered and a new state is replacing this one
        public virtual void OnExitState() {}
    }

    public class StateMachine
    {
        private List<SMState> SMStates = new List<SMState>();
        private SMState CurrentState = null;

        protected Godot.Collections.Dictionary StateMachineValues = new Godot.Collections.Dictionary();
        public Godot.Collections.Dictionary GetSMValues() { return StateMachineValues; }

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
    }
}