using UnityEngine;

public abstract class BaseAI : MonoBehaviour
{
    public StateMachine StateMachine { get; private set; }

    protected virtual void Start() => InitializeStateMachine();

    protected virtual void Update() => StateMachine.Update();

    protected virtual void InitializeStateMachine() => StateMachine = new StateMachine();
}
