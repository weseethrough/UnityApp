using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// The actual Graph data class that contains the list of nodes and connections.
// Not a ScriptableObject or it would not be expanded in the Inspector window
[Serializable]
public class GraphData : ISerializable
{
    public enum ConnectorDirection
    {
        In,
        Out,
        Both
    }

	public GStyle Style;    
	
	[HideInInspector]
	public List<GNode> Nodes;
	
	// List of source (output) connectors for the graph.
	[HideInInspector]
	public List<GConnector> Connections;

	[HideInInspector]
	public uint IdNext; // factory for unique node identifiers
	
	public bool Empty
	{
		get { return Nodes.Count == 0; }
	}
	
	public GraphData()
	{
		IdNext = 1;
		Style = new GStyle();
		Nodes = new List<GNode>();
		Connections = new List<GConnector>();
	}
	
	public void Add(GNode node)
	{
		if (node != null)
		{
			//Debug.Log("Add");
			node.Id = IdNext++;
			Nodes.Add(node);
		}
	}
	
	public bool IsConnected(GConnector a, GConnector b)
	{
		if (a != null && b != null)
		{
			if (a.Parent != null && b.Parent != null)
			{
				// Consistent order for connections
				if (a.Parent.Id > b.Parent.Id)
				{
					GConnector swap = a;
					a = b;
					b = swap;
				}
				
				foreach (GConnector c in Connections)
				{
					if (c == a && c.Link.Contains(b))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool Connect(GConnector a, GConnector b)
	{
		if (a != null && b != null)
		{
			if (a.Parent != null && b.Parent != null)
			{				
				// Do not allow (recursive) connections to self!
				if (a.Parent != b.Parent)
				{
					GConnector src,dst;
					src = a.IsInput ? b : a;
					dst = a.IsInput ? a : b;
                    if (!src.Link.Contains(dst)) src.Link.Add(dst);
                    if (!dst.Link.Contains(src)) dst.Link.Add(src);                    
					Connections.Add(src);
				
					return true;
				}				
			}
		}
		Debug.LogWarning("Connect failed.");
		return false;
	}
	
	public GNode PickNode(Vector2 pos)
	{
		foreach(GNode node in Nodes)
		{
			float x0 = node.Position.x;
			float y0 = node.Position.y;
			float x1 = x0 + node.Size.x;
			float y1 = y0 + node.Size.y;
			if (pos.x >= x0 && pos.y >= y0 && pos.x <= x1 && pos.y <= y1)
			{
				return node;
			}
		}
		return null;
	}

    /// <summary>
    /// Allows to find node which is on deepest position under mouse cursor. We select least parental state possible
    /// </summary>
    /// <param name="pos">mouse position</param>
    /// <returns>deepest child state under mouse pointer</returns>
    public FlowState PickDeepStateNode(Vector2 pos, GNode ignore)
    {
        FlowState fs = null;
        int depth = -1;

        foreach (FlowState node in Nodes)
        {
            if (ignore == node) continue;

            float x0 = node.Position.x;
            float y0 = node.Position.y;
            float x1 = x0 + node.Size.x;
            float y1 = y0 + node.Size.y;
            if (pos.x >= x0 && pos.y >= y0 && pos.x <= x1 && pos.y <= y1)
            {
                int nodeDepth = 0;
                FlowState walker = node;
                while (walker.parent != null)
                {
                    walker = walker.parent;
                    nodeDepth ++;
                }
                if (nodeDepth > depth)
                {
                    fs = node;
                    depth = nodeDepth;
                }                
            }
        }
        return fs;
    }

    public bool Disconnect(GConnector source)
    {
        if (source != null && source.Link != null)
        {
            //disconnect self from others
            foreach (GConnector c in source.Link)
            {
                DisconnectSingleDirection(c, source);
                if (Connections.Contains(c))
                {
                    Connections.Remove(c);
                }
            }

            source.Link.Clear();
            return true;
        }
        return false;
    }

    public bool Disconnect(GConnector source, GConnector target)
    {
        return DisconnectSingleDirection(source, target) && DisconnectSingleDirection(target, source);
    }

    private bool DisconnectSingleDirection(GConnector source, GConnector target)
	{
		if (source != null && source.Link != null)
		{
            if (source.Link.Contains(target))
            {
                source.Link.Remove(target);
                if (Connections.Contains(source))
                {
                    Connections.Remove(source);
                }
            }            
            return true;
		}
		return false;
	}

    public void Disconnect(GNode node)
    {
        Disconnect(node, ConnectorDirection.Both);
    }

	public void Disconnect(GNode node, ConnectorDirection direction)
	{
		if (node != null)
		{
			if ((direction == ConnectorDirection.In ||
                direction == ConnectorDirection.Both)
                && node.Inputs != null)
			{
				foreach (GConnector it in node.Inputs)
				{
					Disconnect(it);
				}
			}
            if ((direction == ConnectorDirection.Out ||
                direction == ConnectorDirection.Both)
                && node.Outputs != null)
			{
				foreach (GConnector it in node.Outputs)
				{
					Disconnect(it);
				}
			}
		}
	}
	
	public bool Remove(GNode node)
	{
		if (node != null)
		{
			Disconnect(node);
            FlowState fs = node as FlowState;
            if (fs != null && fs.parent != null)
            {
                fs.parent.RemoveChild(fs);
                fs.parent = null;
            }
			bool ok = this.Nodes.Remove(node);
			if (!ok) Debug.LogWarning("Remove failed?");
			node.Id = 0; // clear the identifier
			return ok;
		}
		return false;
	}

    public void ClearGraphData()
    {
        IdNext = 1;        
        Nodes = new List<GNode>();
        Connections = new List<GConnector>();
    }

    public GraphData(SerializationInfo info, StreamingContext ctxt)
	{        
        this.Nodes          = (List<GNode>)info.GetValue("Nodes", typeof(List<GNode>));
        this.Connections    = (List<GConnector>)info.GetValue("Connections", typeof(List<GConnector>));
        this.IdNext         = (uint)info.GetValue("IdNext", typeof(uint));
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{     
        info.AddValue("Nodes", this.Nodes);
        info.AddValue("Connections", this.Connections);
        info.AddValue("IdNext", this.IdNext);
   	}
}
