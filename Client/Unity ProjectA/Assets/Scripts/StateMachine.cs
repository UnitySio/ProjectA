using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private State currentState;

    private void Start()
    {
        currentState = GetInitState();

        if (currentState != null)
            currentState.Enter();
    }

    private void Update()
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

    protected virtual State GetInitState()
    {
        return null;
    }
}
