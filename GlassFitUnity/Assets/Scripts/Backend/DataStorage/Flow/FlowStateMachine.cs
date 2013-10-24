using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FlowStateMachine : MonoBehaviour 
{
    private List<FlowState> activeFlow;
    private FlowState targetState;

    void Awake()
    {
        activeFlow = new List<FlowState>();
        targetState = null;
        GraphComponent gc = GetComponent<GraphComponent>();
        if (gc != null)
        {
            foreach(GNode node in gc.Data.Nodes)
            {
                if (node is Start)
                {
                    targetState = node as Start;
                }

            }
        }
    }

    void Update()
    {
        ProgressStateChanges();

        for (int i = 0; i < activeFlow.Count; i++)
        {
            bool specialProgress = UpdateState(activeFlow[i]);

            if (activeFlow[i].state == FlowState.State.Dead)
            {
                activeFlow.RemoveAt(i);
                i--;
                ProgressStateChanges();
            }
            else if (specialProgress)
            {
                i--;
                ProgressStateChanges();
            }
        }
    }

    public bool UpdateState(FlowState fs)
    {
        bool requiresAnotherPass = false;

        switch (fs.state)
        {
            case FlowState.State.Entering:
                if (fs.EnterUpdate())
                {
                    fs.Entered();
                    requiresAnotherPass = true;
                }
                break;
            case FlowState.State.Idle:
                fs.StateUpdate();
                break;
            case FlowState.State.Exiting:
                if (fs.ExitUpdate())
                {
                    fs.Exited();
                    requiresAnotherPass = true;
                }
                break;
        }

        return requiresAnotherPass;
    }

    public bool FollowConnection(GConnector connection)
    {
        if (connection != null && 
            connection.Link != null && 
            connection.Link.Count > 0 &&
            connection.Link[0].Parent != null)
        {
            targetState = connection.Link[0].Parent as FlowState;
        }
        return false;
    }

    /// <summary>
    /// Used to navigate up and down on state hierarchy
    /// </summary>
    /// <param name="fs">state destination</param>
    /// <returns>returns true when operation is finished. until then all calls return false</returns>
    private bool ProgressStateChanges()
    {
        //target state cant be null, machine must be ready and we cant be exactly in target state
        if (targetState != null && 
            IsReady())
        {
            if (activeFlow.Count > 0 && activeFlow[activeFlow.Count - 1] == targetState)
            {
                targetState = null;
                return true;
            }

            FlowState nextStep = targetState;
            FlowState nextStepChild = null;
            while (nextStep != null)
            {
                if (activeFlow.Contains(nextStep))
                {
                    break;
                }
                nextStepChild = nextStep;
                nextStep = nextStep.parent;
            }
            
            //we have any states entered but last one is not within our list of expectations
            if (activeFlow.Count > 0)
            {
                if (activeFlow[activeFlow.Count - 1] != nextStep)
                {
                    activeFlow[activeFlow.Count - 1].ExitStart();
                    return false;
                }
            }
            
            
            //our next step is applied, we will navigate to the child
            if (nextStepChild != null)
            {
                activeFlow.Add(nextStepChild);
                nextStepChild.parentMachine = this;
                nextStepChild.EnterStart();
                return false;
            }
            else
            {
                //shouldn't ever reach this point
                targetState = null;
                return true;
            }                        
        }
        return false;
    }

    public bool IsReady()
    {
        if (activeFlow.Count > 0)
        {
            if (activeFlow[activeFlow.Count - 1].state == FlowState.State.Idle)
            {
                return true;
            }
            return false;
        }
        return true;
    }
}
