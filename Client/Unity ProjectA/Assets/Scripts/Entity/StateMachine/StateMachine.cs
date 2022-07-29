using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected State currentState;

    protected virtual void Start()
    {
        currentState = GetInitiateState();

        if (currentState != null)
            currentState.Enter();
    }

    protected virtual void Update()
    {
        if (currentState != null)
            currentState.Update();
    }

    public void ChangeState(State newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    protected virtual State GetInitiateState()
    {
        return null;
    }
}
