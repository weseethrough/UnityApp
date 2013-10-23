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

                }

            }
        }
    }

    void Update()
    {
        for (int i = 0; i < activeFlow.Count; i++)
        {
            switch (activeFlow[i].state)
            {
                case FlowState.State.Entering:
                    if (activeFlow[i].EnterUpdate())
                    {
                        activeFlow[i].Entered();
                    }
                    break;
                case FlowState.State.Idle:
                    activeFlow[i].StateUpdate();
                    break;
                case FlowState.State.Exiting:
                    if (activeFlow[i].ExitUpdate())
                    {
                        activeFlow[i].Exited();
                    }
                    break;
            }
            if (activeFlow[i].state == FlowState.State.Dead)
            {
                activeFlow.RemoveAt(i);
            }
        }
    }

    public bool FollowConnection(GConnector connection)
    {
        if (connection != null && 
            connection.Link != null && 
            connection.Link.Count > 0 &&
            connection.Link[0].Parent != null)
        {
            
        }
        return false;
    }

    /// <summary>
    /// Used to navigate up and down on state hierarchy
    /// </summary>
    /// <param name="fs">state destination</param>
    /// <returns>returns true when operation is finished. until then all calls return false</returns>
    private bool EnterState()
    {
        //target state cant be null, machine must be ready and we cant be exactly in target state
        if (targetState != null && 
            IsReady())
        {
            if (activeFlow[activeFlow.Count - 1] != targetState)
            {
                targetState = null;
                return true;
            }

            FlowState nextStep = targetState;
            while (nextStep != null)
            {
                if (activeFlow.Contains(nextStep))
                {
                    break;
                }
                nextStep = (FlowState)nextStep.Parent;
            }
            
            //we have any states entered but last one is not within our list of expectations
            if (activeFlow.Count > 0 &&
                activeFlow[activeFlow.Count - 1] != nextStep)
            {
                activeFlow[activeFlow.Count - 1].ExitStart();
            }

            //from this point we check if all parents of the state we aim for are in active flow, 
            //if not then we enter them first. you should never have case when you enter state which 
            //parent levels are not entered yet or exit state with children not exited before
            if (activeFlow.Contains(targetState))
            {
                if (activeFlow[activeFlow.Count - 1] != targetState)
                {
                    activeFlow[activeFlow.Count - 1].ExitStart();
                    return false;
                }
                else
                {
                    //entering current state? self entering ignored
                    return true;
                }
            }
            else
            {


             /*   activeFlow.Add(fs);
                fs.EnterStart();
                if (fs.EnterUpdate())
                {
                    fs.Entered();
                }*/
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
