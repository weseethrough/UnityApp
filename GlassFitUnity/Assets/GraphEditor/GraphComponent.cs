using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [System.NonSerialized]
	public GraphData m_graph;
    public UIAtlas m_defaultHexagonalAtlas;
    private int selectedFlow = 0;
    private bool initialize = false;

    private string nextStartNavigateTo = "";    

    void Start()
    {       
        MakeAwake();        
    }

    void MakeAwake()
    {
        if (!initialize)
        {
            DataStore.LoadStorage(DataStore.BlobNames.flow);
            initialize = true;
            //below is the example how to initialize game with specific flow
            if (!Platform.Instance.OnGlass() && Platform.Instance.GetIntent().Length > 0 )
            {
                string flowName = Platform.Instance.GetIntent();
                SetSelectedFlowByName("MobileUX");//temporarly hardcode  
            }
            else
            {
                SetSelectedFlowIndex(selectedFlow);
            }            
        }
    }

	public GraphData Data
	{
		get 
        {
            MakeAwake();
            return m_graph; 
        }
	}
	
	public GraphComponent()
	{
		m_graph = new GraphData();
	}    

    public int GetSelectedFlowIndex()
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        if (flowDictionary.Length() <= selectedFlow)
        {
            selectedFlow = 0;
        }

        return selectedFlow;
    }

    public void SetSelectedFlowIndex()
    {
        SetSelectedFlowIndex(selectedFlow);
    }

    public void SetSelectedFlowIndex(int flowIndex)
    {
        //we do not check if index is the same because data could change, flow cold have removed or be modified.

        selectedFlow = flowIndex;

        DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        if (flowDictionary.Length() <= selectedFlow)
        {
            selectedFlow = 0;
        }

        GraphData data = flowDictionary.Get(selectedFlow) as GraphData;
        data.Style = m_graph.Style;
        m_graph = data;
    }

    public void SetSelectedFlowByName(string flowName)
    {
        DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int index = flowDictionary.GetIndex(flowName);
        if (index >= 0)
        {
            selectedFlow = index;
        }
        SetSelectedFlowIndex();
    }

    public void SetSelectedFlowByLast()
    {
        DataStore.LoadStorage(DataStore.BlobNames.flow);
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int index = flowDictionary.Length();
        if (index > 0)
        {
            selectedFlow = index -1;
        }
        SetSelectedFlowIndex();
    }

    public bool GoToFlow(string name)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int nextIndex = flowDictionary.GetIndex(name);
        if (nextIndex == -1 || selectedFlow == nextIndex) return false;
        
        //if flow exists it would be initialized, otherwise initialized would be flow currently defaulted.
        //Make sure statemachine is ready for this type of move
        nextStartNavigateTo = name;

        FlowStateMachine fsm = GetComponent<FlowStateMachine>();
        if (fsm != null)
        {
            fsm.GoToStart();
        }

        return true;
    }

    public bool GoToFlowStage2()
    {        
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        int index = flowDictionary.GetIndex(nextStartNavigateTo);

        if (index >= 0 && index != selectedFlow)
        {
            SetSelectedFlowByName(nextStartNavigateTo);

            FlowStateMachine fsm = GetComponent<FlowStateMachine>();
            if (fsm != null)
            {
                fsm.GoToStart();
            }
            return true;
        }
        return false;
    }
}
