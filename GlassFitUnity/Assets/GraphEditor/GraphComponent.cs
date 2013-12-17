using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
    [System.NonSerialized]
	public GraphData m_graph;
    public UIAtlas m_defaultHexagonalAtlas;

    private bool initialize = false;

    void Start()
    {       
        MakeAwake();        
    }

    void MakeAwake()
    {
        if (!initialize)
        {
            initialize = true;
            DataStore.LoadStorage(DataStore.BlobNames.flow);
            Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
            StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

            GraphData data = flowDictionary.Get("MainFlow") as GraphData;
            data.Style = m_graph.Style;
            m_graph = data;
        }
    }

	public GraphData Data
	{
		get {
            MakeAwake();
            return m_graph; 
        }
	}
	
	public GraphComponent()
	{
		m_graph = new GraphData();
	}    
}
