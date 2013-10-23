using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

public class GraphWindow : EditorWindow, IDraw
{
	// Allow user to drag viewport to see graph
	Vector2 ViewPosition;

	//Material lineMaterial;
	
	[MenuItem("Window/Image Graph Editor")]
	public static void Init ()
	{
		GraphWindow window = GetWindow (typeof(GraphWindow)) as GraphWindow;
		window.minSize = new Vector2(600.0f, 400.0f);
		window.wantsMouseMove = true;
		Object.DontDestroyOnLoad( window );
		window.Show();
	}
	
	void OnSelectionChange () { Repaint(); }
	
	// returns currently selected graph
	GraphComponent Graph
	{
		get
		{
			if (UnityEditor.Selection.activeGameObject != null)
			{
				return UnityEditor.Selection.activeGameObject.GetComponent<GraphComponent>();
			}
			return null;
		}
	}
	
	bool m_dragMain;
	Vector2 m_dragStart;
	Vector2 m_nodeStart; // Node.Postion at beginning of drag
	Vector2 m_viewStart; // ViewPostion at beginning of drag
	GNode m_selection;
	GConnector m_hoverConnector;
	GConnector m_selectionConnector;
	Vector2 m_dragPosition;
		
	GNode PickNode(Vector2 pos)
	{
		GraphComponent g = Graph;
		if (g != null)
		{
			return g.Data.PickNode(pos);
		}
		return null;
	}

	private void SelectNode(GNode node)
	{
		if (node != m_selection)
		{
			m_selection = node;
			m_selectionConnector = null;
			Repaint();
		}
	}
	
	private const int BoxHeight = 0;
	private const int TopHeight = BoxHeight+20; // 50 + 25?

	Vector2 MouseToWorld(Vector2 mousePosition)
	{
		return new Vector2(mousePosition.x,mousePosition.y-BoxHeight) + ViewPosition;
	}
	
	// Returns (true) if mouse position is in main graph window.
	bool IsMainPoint(Vector2 p)
	{
		if (p.x > 0 && p.x < (Screen.width-SideWidth) && p.y > BoxHeight)
		{
			return true;
		}
		return false;
	}
	
	private void MainMouseDown(Vector2 pos)
	{
		// Todo: transform to zoom area?
		m_dragMain = true;
		m_dragStart = Event.current.mousePosition;
		m_viewStart = ViewPosition;
		GNode sel = PickNode(pos);
		SelectNode( sel );
		if (m_selection != null)
		{
			m_selectionConnector = m_selection.PickConnector(Graph.Data,pos);
			m_nodeStart = m_selection.Position;
			m_dragPosition = Event.current.mousePosition;
            GUIUtility.keyboardControl = 0;
		}
		Repaint();
	}
	
	private void MainMouseUp(Vector2 pos)
	{
		if (m_selection != null)
		{
			if (m_selectionConnector != null)
			{
				GraphData graph = (Graph != null) ? Graph.Data : null;
				if (graph == null)
				{
					return;
				}
				bool connected = false;
				GNode over = PickNode(pos);
				if (over != null)
				{
					GConnector target = over.PickConnector(graph,pos);
					if (GraphUtil.IsCompatible(m_selectionConnector,target))
					{
						if (!graph.IsConnected(m_selectionConnector,target))
						{
							// Enforce single connection
							//graph.Disconnect(m_selectionConnector);
							//graph.Disconnect(target);
							graph.Connect(m_selectionConnector,target);
							UnityEditor.EditorUtility.SetDirty(Graph.gameObject);
						}
						connected = true;
					}
				}				
			}
			else if (m_nodeStart != m_selection.Position)
			{
				Debug.Log("Node movement saved.");
				UnityEditor.EditorUtility.SetDirty(Graph.gameObject);
			}
		}
		//m_selection = null;
		m_selectionConnector = null;
		Repaint();
	}
	
	void MainDrag()
	{
		m_dragPosition = Event.current.mousePosition;
		//Debug.Log("MouseDrag "+Event.current.button);
		if (Event.current.button == 0) // && Event.current.modifiers == EventModifiers.Alt) || Event.current.button == 2)
		{
			if (m_selection != null)
			{
				if (m_selectionConnector == null)
				{
					// Drag selected node(s)
					Vector2 delta = Event.current.mousePosition - m_dragStart;
					m_selection.Position = m_nodeStart + delta;
				}
				else
				{
					// Todo: connect lines?
				}
			}
			else // drag world
			{
				Vector2 delta = Event.current.mousePosition - m_dragStart;
				ViewPosition = m_viewStart - delta;
			}
			Repaint();
			return;
		}
	}

	GConnector PickConnector()
	{
		if (IsMainPoint(Event.current.mousePosition))
		{
			Vector2 pos = MouseToWorld(Event.current.mousePosition);
			GNode node = PickNode(pos);
			if (node != null)
			{
				return node.PickConnector(Graph.Data,pos);
			}
		}
		return null;
	}

	void MainMouseMove()
	{
		GConnector h = PickConnector();
		if (h != m_hoverConnector)
		{
			m_hoverConnector = h;
			Repaint();
		}
	}
	
	void ProcessEvents()
	{
		switch (Event.current.type)
		{
		/*case EventType.ScrollWheel:
			// Todo: smooth zoom does not draw text correctly?
			_zoom = (Event.current.delta.y < 0) ? Mathf.Max( 0.3f, _zoom-0.1f) : _zoom;
			_zoom = (Event.current.delta.y > 0) ? Mathf.Min( 1.0f, _zoom+0.1f) : _zoom;
			Event.current.Use();
			break;*/
		case EventType.MouseMove:
			MainMouseMove();
			break;
		case EventType.MouseDrag:
			if (m_dragMain)
			{
				MainDrag();
				MainMouseMove();
			}
			break;
		case EventType.MouseDown:
			{
				if (IsMainPoint(Event.current.mousePosition))
				{
					Vector2 pos = MouseToWorld(Event.current.mousePosition);
					MainMouseDown(pos);
				}
				else
				{
					m_dragMain = false;
				}
			}			
			break;
		case EventType.MouseUp:
			{
				if (IsMainPoint(Event.current.mousePosition))
				{
					Vector2 pos = MouseToWorld(Event.current.mousePosition);
					MainMouseUp(pos);
				}
				m_dragMain = false;
			}			
			break;
		}
	}
	
	public void GuiLabel(Rect r, string text, GUIStyle style)
	{
		if (_zoom == 1.0f) // text is not scaling
		{
			GL.PushMatrix();
			GL.LoadPixelMatrix(0,Screen.width-SideWidth,Screen.height-TopHeight,0);
			if (style != null)
			{
				GUI.Label(new Rect(r.x-ViewPosition.x,r.y-ViewPosition.y,r.width,r.height),text,style);
			}
			else
			{
				GUI.Label(new Rect(r.x-ViewPosition.x,r.y-ViewPosition.y,r.width,r.height),text);
			}
			GL.PopMatrix();
		}
	}
	
	const int GridSize = 512;
	const int GridBlocks = 3;
	
	void DrawGraph(int x, int y, int w, int h)
	{
		float zx = ViewPosition.x;
		float zy = ViewPosition.y;
		_zoomArea = new Rect(zx, zy+TopHeight, w, h);

		Material lineMaterial = GraphUtil.GetLineMaterial();
			
		lineMaterial.SetPass( 0 );

		// Within the zoom area all coordinates are relative to the top left corner of the zoom area
        // with the width and height being scaled versions of the original/unzoomed area's width and height.
        EditorZoomArea.Begin(_zoom, _zoomArea);
		
		GL.PushMatrix();
		
		float vleft = ViewPosition.x;
		float vright = vleft+w/_zoom;
		float vtop = ViewPosition.y;
		float vbottom = vtop+h/_zoom;
		GL.LoadPixelMatrix(vleft,vright,vbottom,vtop);
		
		GL.Viewport(new Rect(0,0,w,h));

		// Draw Grid
		Color grid = new Color(0.85f,0.85f,0.85f,1);
		lineMaterial.SetPass( 0 );
		int full = GridSize*(GridBlocks);
		for (int i=0; i<=GridBlocks; ++i)
		{
			GraphUtil.DrawLine(new Vector2(i*GridSize,0), new Vector2(i*GridSize,full),grid,1);
			GraphUtil.DrawLine(new Vector2(0,i*GridSize), new Vector2(full,i*GridSize),grid,1);
		}

		GraphData data = (Graph != null) ? Graph.Data : null;
		if (data != null)
		{
			foreach(GNode node in data.Nodes)
			{
				if (node != null)
				{
					node.ClearEvaluated();
				}
			}
			
			foreach(GNode node in data.Nodes)
			{
				if (node != null)
				{
					// Reset material or Label() makes subsequent GL commands invisible!
					lineMaterial.SetPass( 0 );
					bool selected = (m_selection==node) && (m_selectionConnector == null);
					GraphUtil.DrawPanel(this, data, lineMaterial,node,selected,m_hoverConnector,m_selectionConnector);
					
					lineMaterial.SetPass( 0 );
					//node.TryEvaluate();
					node.OnDraw(new Rect(0,0,0,0));
				}
			}
			
			lineMaterial.SetPass( 0 );
			foreach (GConnector c in data.Connections)
			{                
				if (c.Link != null)
				{
                    foreach (GConnector dest in c.Link)
                    {
                        Vector2 p1 = c.GetPosition(data);
                        Vector2 p2 = dest.GetPosition(data);
                        bool swap = c.IsInput ? (data.Style.RightToLeft == false) : (data.Style.RightToLeft == true);
                        if (swap)
                        {
                            Vector2 tmp = p1;
                            p1 = p2;
                            p2 = tmp;
                        }

                        Vector2[] strip;
                        float length = 16;
                        float angle = 30;
                        Vector2 vec = (p2 - p1 + new Vector2(-length * 2, 0)).normalized;
                        float rr = 90 - Mathf.Atan2(vec.x, vec.y) * 180 / 3.1415f;
                        rr = (rr > 180) ? (rr - 360) : rr;
                        angle = Mathf.Clamp(rr, -180, 180);
                        strip = Calculate(p1, p2, 4, 12.0f, angle, length);
                        lineMaterial.SetPass(0);
                        GraphUtil.DrawLineStrip2(strip, data.Style.LineColor);
                    }
				}
			}
		}
		
		if (m_selectionConnector != null)
		{
			Vector2 p1 = m_selectionConnector.GetPosition(data);
			lineMaterial.SetPass( 0 );
			Vector2 p2 = MouseToWorld(m_dragPosition);
			GraphUtil.DrawLine(p1, p2+new Vector2(2,0), Color.black, 2);
		}
		
		GL.PopMatrix();
		
        EditorZoomArea.End();		
	}
	
	// a = point on the right side of a box
	// b = point on the left side of a box
	Vector2[] Calculate(Vector2 a, Vector2 b, float width, float radius, float angle, float length)
	{
		List<Vector2> list = new List<Vector2>();
		float w = 0.5f*width;
		float step = 17; // degrees
		Vector2 vw = new Vector2(0,w);
		list.Add(a+vw); // bottom
		list.Add(a-vw); // top
		//Vector2 a1 = a;
		Vector2 a2 = a + new Vector2(length,0);
		list.Add(a2+vw); // bottom
		list.Add(a2-vw); // top
		//Debug.Log("A1 = "+a);
		//Debug.Log("A2 = "+a2);
		if (Mathf.Abs (a.y-b.y) > 8)
		{
			if (a.y < b.y)
			{
				Vector2 center = a2 + new Vector2(0,+radius);
				float ia = 0;
				while (ia < angle)
				{
					ia = Mathf.Min(ia+step,angle);
					float radians = ia*3.14f/180;
					float vx = Mathf.Sin(radians);
					float vy = -Mathf.Cos(radians);
					Vector2 ray = new Vector2(vx,vy);
					Vector2 p1 = center + ray*(radius-w);
					Vector2 p2 = center + ray*(radius+w);
					list.Add(p1); // bottom
					list.Add(p2); // top
				}
				Vector2 b1 = b;
				Vector2 b2 = b - new Vector2(length,0);
				center = b2 + new Vector2(0,-radius);
				ia = angle;
				while (ia > 0)
				{
					float radians = (ia+180)*3.14f/180;
					float vx = Mathf.Sin(radians);
					float vy = -Mathf.Cos(radians);
					Vector2 ray = new Vector2(vx,vy);
					Vector2 p1 = center + ray*(radius+w);
					Vector2 p2 = center + ray*(radius-w);
					list.Add(p1); // bottom
					list.Add(p2); // top
					ia = Mathf.Min(ia-step,angle);
				}
				list.Add(b2+vw);
				list.Add(b2-vw);
				list.Add(b1+vw);
				list.Add(b1-vw);
			}
			else
			{
				if (angle < 0)
				{
					angle = -angle;
				}
				//Debug.Log("Up angle = "+angle);
				Vector2 center = a2 + new Vector2(0,-radius);
				float ia = 0;
				while (ia < angle)
				{
					ia = Mathf.Min(ia+step,angle);
					float radians = ia*3.14f/180;
					float vx = Mathf.Sin(radians);
					float vy = Mathf.Cos(radians);
					Vector2 ray = new Vector2(vx,vy);
					Vector2 p1 = center + ray*(radius+w);
					Vector2 p2 = center + ray*(radius-w);
					list.Add(p1); // bottom
					list.Add(p2); // top
				}
				Vector2 b1 = b;
				Vector2 b2 = b - new Vector2(length,0);
				center = b2 + new Vector2(0,+radius);
				ia = angle;
				while (ia > 0)
				{
					float radians = -(ia)*3.14f/180;
					float vx = Mathf.Sin(radians);
					float vy = -Mathf.Cos(radians);
					Vector2 ray = new Vector2(vx,vy);
					Vector2 p1 = center + ray*(radius-w);
					Vector2 p2 = center + ray*(radius+w);
					list.Add(p1); // bottom
					list.Add(p2); // top
					ia = Mathf.Min(ia-step,angle);
				}
				list.Add(b2+vw);
				list.Add(b2-vw);
				list.Add(b1+vw);
				list.Add(b1-vw);
			}
		}
		else
		{
			// simple line
			Vector2 b1 = b;
			Vector2 b2 = b - new Vector2(length,0);
			list.Add(b2+vw);
			list.Add(b2-vw);
			list.Add(b1+vw);
			list.Add(b1-vw);
		}
		return list.ToArray();
	}
	
    private const float kZoomMin = 0.1f;
    private const float kZoomMax = 10.0f;
 
    private Rect _zoomArea = new Rect(0.0f, TopHeight, 600.0f, 400.0f);
    private float _zoom = 1.0f;
    //private Vector2 _zoomCoordsOrigin = Vector2.zero;
 	
	// Size of right-side column
	int SideWidth = 256;
	
	void DrawDivider(int x, int y, int w, int h)
	{
		GraphUtil.GetLineMaterial().SetPass( 0 );
		
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		
		GL.Begin(GL.LINES);
		GL.Color(Color.black);
		GL.Vertex3(x,0,0);
		GL.Vertex3(x,h,0);
		//GL.Color(Color.white);
		//GL.Vertex3(x+1,0,0);
		//GL.Vertex3(x+1,h,0);
		GL.Color(Color.white);
		GL.Vertex3(x+w-1,1,0);
		GL.Vertex3(x+w-1,h,0);
		GL.End();
		
		GL.PopMatrix();
	}
	
	// Cached list of types that descend from GNode class
	List<System.Type> m_types;
	
	// Returns top-right corner of screen for creating new un-attached nodes.
	Vector2 GetNewPosition(Vector2 size)
	{
		float x = Screen.width - SideWidth;
		float y = 0;
		return new Vector2(x,y) + ViewPosition + new Vector2(-size.x-8,+8);
	}
	
	float ToFloat(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			float result;
			if (float.TryParse(text,out result))
			{
				return result;
			}
		}
		return 0;
	}
	
	static Rect[] PartitionRect(float x, float y, float width, float height, int count, float gap)
	{
		Rect [] result = new Rect[count];
		int dw = Mathf.FloorToInt((width-(count-1)*gap)/count);
		for (int i=0; i<count; ++i)
		{
			float xx = x + i*(dw+gap);
			result[i] = new Rect(xx,y,dw,height);
			//Debug.Log("Rect#"+i+" = "+result[i].ToString());
		}
		return result;
	}
	
	const int ParmNameWidth = 80;
	const int ParmHeight = 16;
	const int ParmGap = 2;
	const int ButtonHeight = 24;
	
	void DrawParameter(Rect r, GParameter parm)
	{
		bool changed = false;
		float nameWidth = ParmNameWidth;
		GUI.BeginGroup(r);
		switch (parm.Type)
		{
		case GraphValueType.Float:
			GUI.Label(new Rect(0,0,nameWidth,ParmHeight),parm.Key);
			{
				float old = ToFloat(parm.Value);
				float newValue = old;
				if (parm.HasRange())
				{
					newValue = EditorGUI.Slider(new Rect(nameWidth,0,r.width-nameWidth,ParmHeight),"",old,parm.FloatMin,parm.FloatMax);
				}
				else
				{
					string result = EditorGUI.TextField(new Rect(nameWidth,0,r.width-nameWidth,ParmHeight),parm.Value);
					float.TryParse(result,out newValue);
				}
				if (old != newValue)
				{
					parm.Value = string.Format("{0}",newValue);
					changed = true;
				}
			}
			break;
		case GraphValueType.Vector3:
			GUI.Label(new Rect(0,0,nameWidth,ParmHeight),parm.Key);
			{
				string [] parts = parm.Value.Split(',');
				//int dw = Mathf.FloorToInt((r.width-nameWidth-2)/3);
				float gap = 1;
				Rect [] rects = PartitionRect(nameWidth,0,r.width-nameWidth-2,ParmHeight,3,gap);
				for (int i=0; i<3; ++i)
				{
					string v = (i < parts.Length) ? parts[i] : "0";
					// The EditorGUI version is required for proper handling of cursor and copy/paste.
					string result = EditorGUI.TextField(rects[i],v);
					float old = ToFloat(v);
					float newValue = ToFloat(result);
					if (old != newValue)
					{
						parts[i] = newValue.ToString();
						changed = true;
					}
				}
				if (changed)
				{
					parm.Value = string.Format("{0},{1},{2}",parts[0],parts[1],parts[2]);
				}
			}
			break;
        case GraphValueType.String:
            GUI.Label(new Rect(0, 0, nameWidth, ParmHeight), parm.Key);
            parm.Value = EditorGUI.TextField(new Rect(nameWidth, 0, r.width - nameWidth - 2, ParmHeight), parm.Value);
            break;
        case GraphValueType.UIPrefab:
            GUI.Label(new Rect(0, 0, nameWidth, ParmHeight), parm.Key);
            int index;
            string[] names = GetUIComponentNames(parm.Value, out index);
            if (names == null) names = new string[1];

            
            index = EditorGUI.Popup(new Rect(nameWidth, 0, r.width - nameWidth - 2, ParmHeight), index, names);
            string newTypeValue = ( index > -1 && names != null && names.Length > index) ? names[index] : "Null";

            if (parm.Value != newTypeValue && m_selection)
            {
                parm.Value = newTypeValue;
                Graph.Data.Disconnect(m_selection, GraphData.ConnectorDirection.Out);
                m_selection.RebuildConnections();
            }
            break;
		default:
			GUI.Label(new Rect(0,0,nameWidth,ParmHeight),parm.Key);
			EditorGUI.TextField(new Rect(nameWidth,0,r.width-nameWidth-2,ParmHeight),parm.Value);
			break;
		}
		GUI.EndGroup();
		if (changed)
		{
			UnityEditor.EditorUtility.SetDirty(Graph.gameObject);
		}
	}
    private int counterT = 0;
	bool DrawNode(GNode node, float w)// Rect r)
	{
        int offset = 0;
        GUI.Box(new Rect(0, 0, w, 20), "Selected: " + node.GetDisplayName());
        offset += 20;

		GUI.color = Color.white;
		if (node.NumParameters > 0)
		{
			for (int i=0; i<node.Parameters.Count; ++i)
			{
				GParameter p = node.Parameters[i];
                int yy = offset + i * (ParmHeight + ParmGap);
				DrawParameter(new Rect(0,yy,w,ParmHeight),p);
			}
		}
		else
		{
			GUI.Label(new Rect(0,20,w,20),"No parameters");
		}
		
		int pcount = Mathf.Max(1,node.NumParameters);

        float by = offset + pcount * (ParmHeight + ParmGap);
		float bw = Mathf.Min(160,w);
		
		if (GUI.Button(new Rect((w - bw) / 2, by, bw, ButtonHeight),"Disconnect"))
		{
			Graph.Data.Disconnect(node);
		}
		
		GUI.color = Color.red;
		by += ButtonHeight;
        if (GUI.Button(new Rect((w - bw) / 2, by, bw, ButtonHeight), "Delete Node"))
        {
            bool ok = Graph.Data.Remove(node);
            if (ok)
            {
                return true;
            }
        }
        by += ButtonHeight;
        if (GUI.Button(new Rect((w - bw) / 2, by, bw, ButtonHeight), "Cleanup Unconnected"))
        {
            GConnector c;
            for (int i = 0; i < Graph.Data.Connections.Count; i++ )
            {
                c = Graph.Data.Connections[i];
                if (/*c.Link == null ||*/ c.Link.Count == 0)
                {
                    Graph.Data.Connections.Remove(c);
                    Debug.Log("Removed unused Connection : "+c.Name+" from "+c.Parent.GetDisplayName());
                    i--;
                }
            }

            GNode gNode;
            for (int i=0; i< Graph.Data.Nodes.Count; i++)
            {
                gNode = Graph.Data.Nodes[i];
                if (!gNode.IsConnected())
                {
                    Graph.Data.Nodes.Remove(gNode);
                    Debug.Log("Removed unused Node:"+gNode.GetDisplayName());
                    i--;
                }
            }
        }

		return false;
	}

	void FindNodeTypes()
	{
		m_types = new List<System.Type>();
		foreach (System.Type t in typeof(GNode).Assembly.GetTypes())
		{
			if (t.IsSubclassOf(typeof(GNode)))
			{
				m_types.Add(t);
			}
		}
	}
	
	void DrawSideArea(int x, int y, int w, int h)
	{
		if (m_types == null)
		{
			FindNodeTypes();
		}
		
		GUILayout.BeginArea(new Rect(x,y,w,h));
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Click buttons to add Node to graph",EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		for (int i=0; i<m_types.Count; ++i)
		{
			System.Type it = m_types[i];
			//Debug.Log(it.Name);
			if (it.IsAbstract)
			{
				continue;
			}
			string displayName = it.Name;						

			// Note: GUILayout works for static buttons
			// GUI.Button is required if button visibility changes
        	if (GUILayout.Button( displayName, GUILayout.Height(24) ))
			{
				GraphComponent gc = Graph;
				if (gc != null)
				{
					GNode newNode = (GNode)ScriptableObject.CreateInstance(it.Name);
					newNode.Id = gc.Data.IdNext++;
					newNode.Position = GetNewPosition(newNode.Size);
					SelectNode(newNode);
					//Debug.Log("Pos = "+newNode.Position.ToString());
					Graph.Data.Add(newNode);
					UnityEditor.EditorUtility.SetDirty(gc.gameObject);
				}
				else
				{
					Debug.LogWarning("Object does not contain GraphComponent!");
				}
			}
		}

		GUILayout.EndArea();
		
		if (m_selection != null)
		{
			if (Graph != null)
			{
				GUI.color = (Graph.Data.Style != null) ? Graph.Data.Style.HighlightColor : Color.cyan;
				int pcount = Mathf.Max (1, m_selection.NumParameters);
				int blockH = 20 + pcount*(ParmHeight+ParmGap) + 3 * 24;
				int yy = h-20-blockH-4;
				GUI.BeginGroup(new Rect(x,yy,w,blockH));
				if (	DrawNode(m_selection,w))
				{
					UnityEditor.EditorUtility.SetDirty(Graph.gameObject);
					SelectNode(null);
				}
				GUI.EndGroup();
				GUI.color = Color.white;
			}
		}
	}

	void OnGUI ()
	{
		if (!wantsMouseMove)
		{
			// Not sure why the original initialization fails.
			// Required to show hover state changes
			wantsMouseMove = true;
		}
		
		ProcessEvents();
		
		if (Graph == null)
		{
			m_selection = null;
			m_hoverConnector = null;
			m_selectionConnector = null;
		}
		
		// divider line between areas
		int divx = Screen.width - SideWidth;
		int divy = 0;
		int divw = 4;
		
        //_zoom = EditorGUI.Slider(new Rect(0.0f, 50.0f, 600.0f, 25.0f), _zoom, kZoomMin, kZoomMax);

		DrawGraph(0,0,divx,Screen.height-TopHeight);
		
		DrawDivider (divx,divy,divw,Screen.height);
		
		//EditorZoomArea.Begin(1.0f, _zoomArea);
		DrawSideArea(divx+divw,divy,SideWidth-divw,Screen.height);
		//EditorZoomArea.End();
	}

    /// <summary>
    /// Gets screens designed and saved in storage
    /// </summary>
    /// <param name="selectedName"></param>
    /// <param name="selected"></param>
    /// <returns></returns>
    public string[] GetUIComponentNames(string selectedName, out int selected)
    {
        Storage s                           = DataStorage.GetStorage(DataStorage.BlobNames.core);
        if (s == null || s.dictionary == null)
        {
            selected = -1;
            return null;
        }

        StorageDictionary screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);
        int count                           = screensDictionary == null ? 0 : screensDictionary.Length();
        string[]  screens                   = count > 0 ? new string[count] : null;

        selected = -1;

        for (int i = 0; i < count; i++)
        {
            string screenName;
            ISerializable screen;
            screensDictionary.Get(i, out screenName, out screen);
            screens[i] = screenName;
            if (selectedName == screens[i])
            {
                selected = i;
            }
        }

        return screens;
    }
}

// Helper Rect extension methods
public static class RectExtensions
{
    public static Vector2 TopLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }
}
 
public class EditorZoomArea
{
    private const float kEditorWindowTabHeight = 21.0f;
    private static Matrix4x4 _prevGuiMatrix;
 
	// Area = screenCoords
    public static Rect Begin(float zoomScale, Rect area)
    {
        GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.
 
        Rect clippedArea = new Rect(area.x, area.y, area.width/zoomScale, area.height/zoomScale);
        clippedArea.y += kEditorWindowTabHeight;
		
        GUI.BeginGroup(clippedArea);
		//GUI.BeginGroup(new Rect(0,0,256,256));
 
        _prevGuiMatrix = GUI.matrix;
        Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
        GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
		
        return clippedArea;
    }
 
    public static void End()
    {
        GUI.matrix = _prevGuiMatrix;
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, Screen.width, Screen.height));
    }
}

public interface IDraw
{
	void GuiLabel(Rect r, string text, GUIStyle style);
}

