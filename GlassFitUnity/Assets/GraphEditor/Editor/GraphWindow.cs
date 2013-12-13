using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System;

public class GraphWindow : EditorWindow, IDraw
{
	// Allow user to drag viewport to see graph
	Vector2 ViewPosition;

    bool m_dirtySave = false;
    public bool dirtySave
    {
        get 
        { 
            return m_dirtySave; 
        }
        set 
        { 
            m_dirtySave = value; 
        }
    }
	
	[MenuItem("Window/Image Graph Editor")]
	public static void Init ()
	{       
		GraphWindow window = GetWindow (typeof(GraphWindow)) as GraphWindow;
		window.minSize = new Vector2(600.0f, 400.0f);
		window.wantsMouseMove = true;
		UnityEngine.Object.DontDestroyOnLoad( window );
		window.Show();
	}
	
	void OnSelectionChange () { Repaint(); }
    
    void Update()
    {
        if (dirtySave)
        {
            SaveGraph();
            dirtySave = false;
        }
    }

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
    bool m_drawInfo;
    bool m_dragConnector;
    bool m_selectionChanges;
    bool m_showExitManager;

    string m_newExitName;
    Vector2 m_exitManagerScroll;

	Vector2 m_dragStart;
	Vector2 m_nodeStart; // Node.Postion at beginning of drag
	Vector2 m_viewStart; // ViewPostion at beginning of drag
	GNode m_selection;

    Vector2 m_scrolableMenuPosition;

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

    FlowState PickDeepestChildNode(Vector2 pos, GNode ignore)
    {
        GraphComponent g = Graph;
        if (g != null)
        {
            return g.Data.PickDeepStateNode(pos, ignore);
        }
        return null;
    }

	private void SelectNode(GNode node)
	{
		if (node != m_selection)
		{
			m_selection = node;
            m_selectionChanges = true;
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
		GNode sel = PickDeepestChildNode(pos, null);
		SelectNode( sel );
		if (m_selection != null)
		{
			m_selectionConnector = m_selection.PickConnector(Graph.Data, pos);
            if (m_selectionConnector != null)
            {
                m_dragConnector = true;                
            }

            m_selectionChanges = true;

			m_nodeStart = m_selection.Position;
			m_dragPosition = Event.current.mousePosition;
            GUIUtility.keyboardControl = 0;            
		}
		//Repaint();
	}
	
	private void MainMouseUp(Vector2 pos)
	{
		if (m_selection != null)
		{
            if (m_dragConnector && m_selectionConnector != null)
			{
				GraphData graph = (Graph != null) ? Graph.Data : null;
				if (graph == null)
				{
					return;
				}
				
				GNode over = PickDeepestChildNode(pos, null);
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
					}                    
				}
                dirtySave = true;
			}
			else if (m_nodeStart != m_selection.Position)
			{
                FlowState currentSelection = m_selection as FlowState;
                if (currentSelection != null)
                {
                    FlowState over = PickDeepestChildNode(pos, currentSelection);

                    if (over != null )
                    {
                        if ( !currentSelection.InChildSubtree(over))
                        {
                            over.AddChild(currentSelection);
                        }                        
                    }
                    else if (currentSelection.parent != null)
                    {                        
                        if (Event.current.shift)
                        {
                            currentSelection.parent.UpdateSize();
                        }
                        else
                        {
                            currentSelection.parent.RemoveChild(currentSelection);
                        }
                        
                    }
                }
				Debug.Log("Node movement saved.");
				UnityEditor.EditorUtility.SetDirty(Graph.gameObject);
                dirtySave = true;
			}
            
		}
        m_dragConnector = false;
		//m_selection = null;
		//m_selectionConnector = null;
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
                if (!m_dragConnector || m_selectionConnector == null)
				{
					// Drag selected node(s)
					Vector2 delta = Event.current.mousePosition - m_dragStart;
					m_selection.Position = m_nodeStart + delta;
                    if (m_selection is FlowState)
                    {                        
                        (m_selection as FlowState).UpdatePosition(false);
                    }
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

    void DrawNode(IDraw g, GraphData graph, Material lineMaterial, GNode node, bool drawSelection)
    {
        if (!drawSelection && node == m_selection) return;
        
        // Reset material or Label() makes subsequent GL commands invisible!
        lineMaterial.SetPass(0);
        bool selected = (m_selection == node) && (m_selectionConnector == null);
        GraphUtil.DrawPanel(this, graph, lineMaterial, node, selected, m_hoverConnector, m_selectionConnector);

        lineMaterial.SetPass(0);
        //node.TryEvaluate();
        node.OnDraw(new Rect(0, 0, 0, 0));

        FlowState fs = node as FlowState;
        if (fs != null)
        {
            foreach (FlowState child in fs.children)
            {
                DrawNode(g, graph, lineMaterial, child, drawSelection);
            }
        }
    }

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
                    FlowState fs = node as FlowState;

                    if (fs != null)
                    {
                        if (fs.parent == null)
                        {
                            DrawNode(this, data, lineMaterial, node, false);
                        }
                    }
                    else
                    {
                        DrawNode(this, data, lineMaterial, node, false);
                    }					
				}
			}

            //this is section where we draw selected node to ensure its always on top of the other nodes on graph
            if (m_selection != null)
            {
                DrawNode(this, data, lineMaterial, m_selection, true);
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
	
	//const int ParmNameWidth = 80;
	//const int ParmHeight = 16;
	//const int ParmGap = 2;
	//const int ButtonHeight = 24;
	
	void DrawParameter(GParameter parm, float width)
	{
		bool changed = false;     
		switch (parm.Type)
		{
		case GraphValueType.Float:
            EditorGUILayout.LabelField(parm.Key);
			{
				float old = ToFloat(parm.Value);
				float newValue = old;
				if (parm.HasRange())
				{
					newValue = EditorGUILayout.Slider("",old,parm.FloatMin,parm.FloatMax);
				}
				else
				{
                    string result = EditorGUILayout.TextField(parm.Value);
					float.TryParse(result,out newValue);
				}
				if (old != newValue)
				{
					parm.Value = string.Format("{0}",newValue);
					changed = true;
                    dirtySave = true;
				}
			}
			break;		

        case GraphValueType.String:
            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
            EditorGUILayout.LabelField(parm.Key, GUILayout.Width(width /2));
            string value = EditorGUILayout.TextField(parm.Value);
            if (value != parm.Value)
            {
                parm.Value = value;
                dirtySave = true;
            }
            EditorGUILayout.EndHorizontal();
            break;

        case GraphValueType.UIPrefab:
            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
            EditorGUILayout.LabelField(parm.Key, GUILayout.Width(width / 2));
            int index;
            string[] names = GetUIComponentNames(parm.Value, out index);
            if (names == null) names = new string[1];


            index = EditorGUILayout.Popup(index, names);
            string newTypeValue = ( index > -1 && names != null && names.Length > index) ? names[index] : "Null";

            if (parm.Value != newTypeValue && m_selection != null)
            {
                parm.Value = newTypeValue;
                Graph.Data.Disconnect(m_selection, GraphData.ConnectorDirection.Out);
                m_selection.RebuildConnections();
                dirtySave = true;
            }
            EditorGUILayout.EndHorizontal();
            break;

        case GraphValueType.Settings:
            if (m_selection is Panel)
            {
                FlowPanelComponent components = (m_selection as Panel).panelNodeData;
                if (components != null)
                {
                    if (components.OnInspectorGUI(width))
                    {
                        dirtySave = true;
                    }
                }
            }
            break;
        case GraphValueType.HexButtonManager:
            if (m_selection is HexPanel)
            {
                HexPanel hexPanel = m_selection as HexPanel;
                if (hexPanel.buttonData == null)
                {
                    hexPanel.buttonData = new List<HexButtonData>();
                }

                //String[] spriteNames = Graph.m_defaultHexagonalAtlas.GetListOfSprites().ToArray();
                BetterList<string> spriteNames = Graph.m_defaultHexagonalAtlas.GetListOfSprites();    
                string[] spriteNamesArray = spriteNames.ToArray();                
                bool isScrolled = false;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("name:", GUILayout.Width(width * 0.3f));
                EditorGUILayout.LabelField("exit:", GUILayout.Width(width * 0.2f));
                EditorGUILayout.LabelField("graphic:", GUILayout.Width(width * 0.4f));
                EditorGUILayout.EndHorizontal();

                //Scrolling start point
              
                if (hexPanel.buttonData.Count > 0)
                {
                    if (m_scrolableMenuPosition == null)
                    {
                        m_scrolableMenuPosition = new Vector2();
                    }
                    m_scrolableMenuPosition = EditorGUILayout.BeginScrollView(m_scrolableMenuPosition);
                    isScrolled = true;
                }

                for (int i = 0; i < hexPanel.buttonData.Count; i++)
                {
                    HexButtonData hexData = hexPanel.buttonData[i];

                    int spriteIndex = spriteNamesArray.Length -1;
                    for (int arrayIndex = 0; arrayIndex < spriteNamesArray.Length; arrayIndex++)
                    {
                        if (spriteNamesArray[arrayIndex] == hexData.imageName)
                        {
                            spriteIndex = arrayIndex;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();

                    List<HexButtonData> hexNameSanityCheck = hexPanel.buttonData.FindAll(r => r.buttonName == hexData.buttonName);
                    if (hexNameSanityCheck.Count > 1)
                    {
                        GUI.color = Color.red;
                    }

                    string name = EditorGUILayout.TextField(hexData.buttonName, GUILayout.Width(width * 0.3f));
                    GUI.color = Color.white;
                    if (GUILayout.Button("+", GUILayout.Width(width * 0.2f)))
                    {
                        GConnector connector = hexPanel.Outputs.Find(r => r.Name == hexData.buttonName);
                        if (connector == null)
                        {
                            hexPanel.NewOutput(hexData.buttonName, "Flow");
                            hexPanel.UpdateSize();
                            dirtySave = true;
                        }
                    }

                    int newIndex = EditorGUILayout.Popup(spriteIndex, spriteNamesArray);

                    if (name != hexData.buttonName)
                    {                        
                            GConnector connector = hexPanel.Outputs.Find(r => r.Name == hexData.buttonName);
                            if (connector != null)
                            {                                
                                connector.Name = name;
                            }
                        
                        hexData.buttonName = name;
                        dirtySave = true;
                    }

                    if (spriteIndex != newIndex)
                    {
                        hexData.imageName = spriteNamesArray[newIndex] == "--" ? "" : spriteNamesArray[newIndex];
                        dirtySave = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                  
                    List<HexButtonData> hexPositionSanityCheck = hexPanel.buttonData.FindAll(r => (r.column == hexData.column && r.row == hexData.row));

                    if (hexPositionSanityCheck.Count > 1)
                    {
                        GUI.color = Color.red;
                    }

                    EditorGUILayout.LabelField("Column:", GUILayout.Width(width * 0.30f));
                    int column = EditorGUILayout.IntField(hexData.column, GUILayout.Width(width * 0.1f));
                    EditorGUILayout.LabelField(",  Row:", GUILayout.Width(width * 0.30f));
                    int row = EditorGUILayout.IntField(hexData.row, GUILayout.Width(width * 0.1f));
                    EditorGUILayout.EndHorizontal();

                    if (column != hexData.column || row != hexData.row)
                    {
                        hexData.column = column;
                        hexData.row = row;
                        dirtySave = true;
                    }

                    GUI.color = Color.white;

                    //allow to make button locked
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Locked visually button :");
                        bool locked =  EditorGUILayout.Toggle(hexData.locked);
                        if (locked != hexData.locked)
                        {                            
                            hexData.locked = locked;
                            dirtySave = true;
                        }
                    EditorGUILayout.EndHorizontal();

                    //add simple divider
                    if (i < hexPanel.buttonData.Count - 1)
                    {
                        EditorGUILayout.LabelField("------------------------");
                    }
                }

                if (isScrolled)
                {
                    EditorGUILayout.EndScrollView();
                }                

                if (GUILayout.Button("Add new button"))
                {
                    HexButtonData buttonData = new HexButtonData();
                    buttonData.buttonName = "Set Name " + hexPanel.buttonData.Count;
                    hexPanel.buttonData.Add(buttonData);
                    dirtySave = true;
                }
            }
            break;
		default:
            EditorGUILayout.LabelField(parm.Key);
            EditorGUILayout.TextField(parm.Value);
			break;
		}
		if (changed)
		{
			UnityEditor.EditorUtility.SetDirty(Graph.gameObject);
		}
	}

    void DrawConnectorConfiguration(GNode node, GConnector connector, float width)
	{
        EditorGUILayout.LabelField("Selected Connector: " + connector.Name);
        if (GUILayout.Button("Disconnect Connector"))
        {           
            Graph.Data.Disconnect(connector);
            m_selectionConnector = null;
            dirtySave = true;
        }
        
        EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
            EditorGUILayout.LabelField("Function event to call", GUILayout.Width(width / 2));
        
            int index;
            string eventFunction = connector.EventFunction != null ? connector.EventFunction : "";
            string[] names = GetFunctionEventNames(connector.EventFunction, out index);
                    
            index = EditorGUILayout.Popup(index, names);

            if (index < 0 || index >= names.Length)
            {
                connector.EventFunction = "";
            }
            else
            {
                connector.EventFunction = names[index];                
            }

        EditorGUILayout.EndHorizontal();
    }
    
	void DrawNodeConfiguration(GNode node, float width)
	{        
        EditorGUILayout.LabelField("Selected: " + node.GetDisplayName());     

		GUI.color = Color.white;
		if (node.NumParameters > 0)
		{                       
            for (int i = 0; i < node.Parameters.Count; ++i)
            {
                GParameter p = node.Parameters[i];
                DrawParameter(p, width);
            }                       
		}
		else
		{
            EditorGUILayout.LabelField("No parameters");
		}

        ExitManager();

        if (GUILayout.Button("Disconnect"))
		{
			Graph.Data.Disconnect(node);
            dirtySave = true;
		}
		
		GUI.color = Color.red;
        if (GUILayout.Button("Delete Node"))
        {
            bool ok = Graph.Data.Remove(node);
            if (ok)
            {
                SelectNode(null);
                dirtySave = true;
                return;
            }
        }
        if (GUILayout.Button("Cleanup Unconnected"))
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
            dirtySave = true;
        }
	}

    void ExitManager()
    {
        EditorGUILayout.LabelField("------------");
        m_showExitManager = EditorGUILayout.Foldout(m_showExitManager, "Exit Manager");
        if (m_showExitManager && m_selection != null)
        {

            m_exitManagerScroll = EditorGUILayout.BeginScrollView(m_exitManagerScroll);
            

            for (int i=0; i< m_selection.Outputs.Count; i++)
            {
                GConnector output = m_selection.Outputs[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(output.Name);
                if (GUILayout.Button("X", GUILayout.MaxWidth(30)))
                {
                    m_selection.Outputs.RemoveAt(i);                    
                    if (m_selection is Panel)
                    {
                        (m_selection as Panel).UpdateSize();
                    }

                    i--;
                    m_dirtySave = true;
                }
                EditorGUILayout.EndHorizontal();
            }
           
            EditorGUILayout.LabelField("New output connector:");
            EditorGUILayout.BeginHorizontal();
            if (m_newExitName == null)
            {
                m_newExitName = "";
            }
            m_newExitName = EditorGUILayout.TextField(m_newExitName);
            if (GUILayout.Button("Add", GUILayout.MaxWidth(40)) && m_newExitName.Length > 0)
            {
                m_selection.NewOutput(m_newExitName, "Flow");
                if (m_selection is Panel)
                {
                    (m_selection as Panel).UpdateSize();
                }

                m_dirtySave = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        
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
        EditorGUILayout.BeginHorizontal(GUILayout.Width(x + w));
        GUILayout.FlexibleSpace();     
        EditorGUILayout.BeginVertical(GUILayout.Width(w));
		
		EditorGUILayout.BeginHorizontal();
		//GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField("Click buttons to add Node to graph",EditorStyles.boldLabel);
		//GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		for (int i=0; i<m_types.Count; ++i)
		{
			System.Type it = m_types[i];
			//Debug.Log(it.Name);
			if (it.IsAbstract)
			{
				continue;
			}
			string displayName = it.Name;						

			// Note: EditorGUILayout works for static buttons
			// GUI.Button is required if button visibility changes
        	if (GUILayout.Button( displayName, GUILayout.Height(24) ))
			{
				GraphComponent gc = Graph;
				if (gc != null)
				{
                    System.Object o = Activator.CreateInstance(it);
                    GNode newNode = (GNode)o;// ScriptableObject.CreateInstance(it.Name);
					newNode.Id = gc.Data.IdNext++;
					newNode.Position = GetNewPosition(newNode.Size);
					SelectNode(newNode);
					//Debug.Log("Pos = "+newNode.Position.ToString());
					Graph.Data.Add(newNode);
					UnityEditor.EditorUtility.SetDirty(gc.gameObject);

                    dirtySave = true;
				}
				else
				{
					Debug.LogWarning("Object does not contain GraphComponent!");
				}
			}
		}

        //change layout only during proper event. We don't want to change it during repaint as it will error
        if (m_drawInfo == false && Event.current.type == EventType.Layout)
        {
            if (m_selection != null)
            {
                m_drawInfo = true;
            }
        }
        else if (m_drawInfo == true && (m_selection == null || m_selectionChanges))
        {
            //we keep true state only as long as we really need it. Any changes to structure require layout event first
            m_drawInfo = false;
            m_selectionChanges = false;
        }

        if (m_drawInfo)
        {
            if (Graph != null)
            {
                GUILayout.FlexibleSpace();
                if (m_selectionConnector != null)
                {
                    DrawConnectorConfiguration(m_selection, m_selectionConnector, w);
                }
                else
                {
                    DrawNodeConfiguration(m_selection, w);
                }
                
            }
        }    
        
		            
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
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
            m_selectionChanges = true;
			m_hoverConnector = null;
			m_selectionConnector = null;
		}
		
		// divider line between areas
		int divx = Screen.width - SideWidth;
		int divy = 0;
		int divw = 4;
		
        //_zoom = EditorGUI.Slider(new Rect(0.0f, 50.0f, 600.0f, 25.0f), _zoom, kZoomMin, kZoomMax);

		DrawGraph(0,0,divx,Screen.height-TopHeight);
		
		DrawDivider (divx-5,divy,divw,Screen.height);
		
		//EditorZoomArea.Begin(1.0f, _zoomArea);
		DrawSideArea(divx+divw,divy,SideWidth-divw,Screen.height-25);
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
        Storage s                           = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
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

    private string[] GetFunctionEventNames(string currentName, out int index)
    {
        var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.ExactBinding | BindingFlags.Static;


        var methods = typeof(ButtonFunctionCollection).GetMethods(bindingFlags);        
        List<String> names = new List<String>();

        index = - 1;
        

        for (int i=0; i< methods.Length; i++)
        {
            //if (methods[i].IsStatic && methods[i].IsPublic)
            {
                names.Add(methods[i].Name);
                if (methods[i].Name == currentName)
                {
                    index = i;
                }
            }            
        }

        names.Add("None");

        if (index == -1)
        {
            index = names.Count - 1;
        }
                
        return names.ToArray();
    }

    public void SaveGraph()
    {


        GraphComponent gc = Graph;            
        Storage s = DataStore.GetStorage(DataStore.BlobNames.flow);
        StorageDictionary flowDictionary = (StorageDictionary)s.dictionary;
        if (gc != null && gc.Data != null)
        {
            if (!flowDictionary.Contains("MainFlow"))
            {
                flowDictionary.Add("MainFlow", gc.m_graph);
            }
            else
            {
                flowDictionary.Set(gc.m_graph, "MainFlow");
            }
        }    

        DataStore.SaveStorage(DataStore.BlobNames.flow);
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

