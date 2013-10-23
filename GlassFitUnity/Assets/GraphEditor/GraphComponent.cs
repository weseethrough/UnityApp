using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraphComponent : MonoBehaviour
{
	[SerializeField]
	public GraphData m_graph;

	public GraphData Data
	{
		get { return m_graph; }
	}
	
	public GraphComponent()
	{
		m_graph = new GraphData();
	}
}
