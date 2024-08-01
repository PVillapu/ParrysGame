#if TOOLS
using Godot;
#endif
using System.Collections.Generic;

namespace utils.StateMachine
{
    abstract public class SMTransition
    {
        // Reference to the state machine
        protected StateMachine StateMachine;
        public void SetStateMachine(StateMachine stateMachine) { this.StateMachine = stateMachine; }

        // Evaluates if the condition to change to a new state is met
        public abstract bool EvaluateTriggerCondition();
    }

    public class SMState
    {
        // Reference to the state machine
        protected StateMachine StateMachine;
        public void SetStateMachine(StateMachine stateMachine) { this.StateMachine = stateMachine; }

        // Set of the transitions associated with this state
        private List<KeyValuePair<SMTransition, SMState>> StateTransitions = new List<KeyValuePair<SMTransition, SMState>>();
        public List<KeyValuePair<SMTransition, SMState>> GetStateTransitions() { return StateTransitions; }

        // Adds a transition to the state only if it doesnt exist yet in the current registered transitions
        public void AddNewTransition(KeyValuePair<SMTransition, SMState> NewTransition)
        {
            if (StateTransitions.Contains(NewTransition))
            {
#if TOOLS
                GD.PrintErr("[SM] Trying to add a new transition that already exist on state!");
#endif
                return;
            }

            StateTransitions.Add(NewTransition);
        }

        // Called when the state is loaded as the current state in the SM
        public virtual void OnEnterState() {}

        // Called each frame to evaluate the state
        public virtual void EvaluateState() {}

        // Called when any transition is triggered and a new state is replacing this one
        public virtual void OnExitState() {}
    }

    public class StateMachine
    {
        private List<SMState> SMStates = new List<SMState>();
        private SMState CurrentState = null;

        // Registers a new state only if it is not already registered
        public void RegisterNewState(SMState NewState, bool SetStartingState = false) 
        {
            if (SMStates.Contains(NewState))
            {
#if TOOLS
                GD.PrintErr("[SM] Trying to add a new state but is already in the state machine!");
#endif
                return;
            }

            SMStates.Add(NewState);

            if (SetStartingState) CurrentState = NewState;
        }

        // Initializes the start state of the SM
        public void StartSM()
        {
            if (CurrentState == null) return;
            
            CurrentState.OnEnterState();
        }

        public void EvaluateSM()
        {
            if(CurrentState == null) return;

            // Evaluate state transitions
            List<KeyValuePair<SMTransition, SMState>> CurrentStateTransitions = CurrentState.GetStateTransitions();
            for(int i = 0; i < CurrentStateTransitions.Count; ++i)
            {
                if (CurrentStateTransitions[i].Key.EvaluateTriggerCondition())
                {
                    CurrentState.OnExitState();
                    CurrentState = CurrentStateTransitions[i].Value;
                    CurrentState.OnEnterState();
                    return;
                }
            }

            // Evaluate current state
            CurrentState.EvaluateState();
        }
    }
}