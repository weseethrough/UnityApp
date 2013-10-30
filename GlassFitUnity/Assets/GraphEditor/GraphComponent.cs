using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GraphComponent : MonoBehaviour
{
	[SerializeField]
	public GraphData m_graph;

    void Awake()
    {                
        DataStorage.LoadStorage(DataStorage.BlobNames.flow);
        Storage s = DataStorage.GetStorage(DataStorage.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;

        GraphData data = flowDictionary.Get("MainFlow") as GraphData;
        data.Style = m_graph.Style;
        m_graph = data;
    }

	public GraphData Data
	{
		get { return m_graph; }
	}
	
	public GraphComponent()
	{
		m_graph = new GraphData();
	}
}
