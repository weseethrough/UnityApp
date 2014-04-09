using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GraphComponentBase : MonoBehaviour
{    
	protected GraphData m_graph;    
    protected int selectedFlow = 0;
    protected bool initialize = false;

    protected string nextStartNavigateTo = "";

    public GraphData Data
    {
        get
        {
            MakeAwake();
            return m_graph;
        }
    }

    public abstract void MakeAwake();

    public abstract bool GoToFlowStage2();
}
