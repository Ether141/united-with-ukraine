using System;
using System.Linq;
using System.Collections.Generic;

public class StateMachine
{
    public IState CurrentState { get; private set; }

    private Dictionary<Type, List<Transition>> transitions = new Dictionary<Type, List<Transition>>();
    private List<Transition> currentTransitions = new List<Transition>();
    private List<Transition> anyTransitions = new List<Transition>();

    private List<IState> allStates = new List<IState>();

    private static List<Transition> EmptyTransitions => new List<Transition>();

    public event Action<IState, IState> OnStateChanged;

    public void Update()
    {
        Transition transition = GetTransition();

        if (transition != null)
            SwitchState(transition.To);

        if (CurrentState != null)
            CurrentState.Tick();
    }

    public void AddTransition(IState from, IState to, Func<bool> condition)
    {
        if (!transitions.TryGetValue(from.GetType(), out List<Transition> transitionsList))
        {
            transitionsList = new List<Transition>();
            transitions[from.GetType()] = transitionsList;
        }

        transitionsList.Add(new Transition(condition, to));

        AddState(from);
        AddState(to);
    }

    public void AddAnyTransition(IState to, Func<bool> condition)
    {
        anyTransitions.Add(new Transition(condition, to));
        AddState(to);
    }

    public void SwitchState(IState state)
    {
        if (CurrentState == state) return;

        IState previousState = CurrentState;
        CurrentState = state;

        transitions.TryGetValue(CurrentState.GetType(), out currentTransitions);

        if (currentTransitions == null)
            currentTransitions = EmptyTransitions;

        OnStateChanged?.Invoke(CurrentState, previousState);

        CurrentState.OnStateEnter();
        previousState?.OnStateExit();
    }

    private void AddState(IState state)
    {
        if (!allStates.Contains(state))
            allStates.Add(state);
    }

    private Transition GetTransition()
    {
        foreach (Transition transition in anyTransitions)
        {
            if (transition.Condition())
                return transition;
        }

        foreach (Transition transition in currentTransitions)
        {
            if (transition.Condition())
                return transition;
        }

        return null;
    }

    public bool IsState<T>() where T : class, IState => (CurrentState as T) != null;

    public T GetState<T>() where T : class, IState => allStates.Where(state => state.GetType() == typeof(T)).FirstOrDefault() as T;

    public bool TryGetState<T>(out T state) where T : class, IState
    {
        T s = GetState<T>();

        if (s != null)
        {
            state = s;
            return true;
        }
        else
        {
            state = null;
            return false;
        }
    }

    private class Transition
    {
        public Func<bool> Condition { get; }
        public IState To { get; }

        public Transition(Func<bool> condition, IState to)
        {
            Condition = condition;
            To = to;
        }
    }
}
