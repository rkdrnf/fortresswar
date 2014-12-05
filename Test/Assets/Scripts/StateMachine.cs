using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class StateMachine<T> : MonoBehaviour {

    private T curState;
    private float stateStartTime;
    private Queue<T> stateChangingQueue;
    private bool isInitialized = false;

    public StateMachine()
    {
        stateChangingQueue = new Queue<T>();
        isInitialized = false;
    }

    public void Initialize(T initialState)
    {
        SetState(initialState);
        isInitialized = true;
    }

    public T GetState()
    {
        return curState;
    }

    public float GetStateStartTime()
    {
        return stateStartTime;
    }

    public float GetElapsedTimeInState()
    {
        return Time.time - GetStateStartTime();
    }

    public void SetState(T state)
    {
        stateChangingQueue.Enqueue(state);
    }

    protected abstract void OnEnter(T state);
    protected abstract void OnUpdate();
    protected abstract void OnExit(T state);

    void Update()
    {
        if (!isInitialized)
            return;

        while (stateChangingQueue.Count > 0)
        {
            T nextState = stateChangingQueue.Dequeue();

            OnExit(curState);
            curState = nextState;
            stateStartTime = Time.time;
            OnEnter(curState);
        }

        OnUpdate();
    }
}
