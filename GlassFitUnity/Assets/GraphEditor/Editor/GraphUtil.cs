using UnityEngine;
using System.Collections;

public class GraphUtil
{
	static public void DrawLine(Vector2 a, Vector2 b, Color rgb, float width)
	{
		Vector2 vec = 0.5f*width*(b-a).normalized;
		float dx = vec.y;
		float dy = -vec.x;
		GL.Begin(GL.LINES);
			GL.Color(Color.Lerp(rgb,new Color(0.5f,0.5f,0.5f,0.25f),0.67f));
			GL.Vertex3(a.x-dx,a.y-dy,0);
			GL.Vertex3(b.x-dx,b.y-dy,0);
			GL.Vertex3(a.x+dx,a.y+dy,0);
			GL.Vertex3(b.x+dx,b.y+dy,0);
			GL.Vertex3(a.x-dx,a.y-dy,0);
			GL.Vertex3(a.x+dx,a.y+dy,0);
			GL.Vertex3(b.x-dx,b.y-dy,0);
			GL.Vertex3(b.x+dx,b.y+dy,0);
		GL.End();
		GL.Begin(GL.QUADS);
			GL.Color(rgb);
			GL.Vertex3(a.x-dx,a.y-dy,0);
			GL.Vertex3(a.x+dx,a.y+dy,0);
			GL.Vertex3(b.x+dx,b.y+dy,0);
			GL.Vertex3(b.x-dx,b.y-dy,0);
		GL.End();
	}
		
	static public void DrawPanel(IDraw g, GStyle style, Vector2 pos, Vector2 size, string title, bool selected, bool valid)
	{
		float width = size.x;
		float height = size.y;
			
		float x0 = pos.x;
		float x1 = pos.x+width;
		float y0 = pos.y;
		float y1 = pos.y+height;
		
		float r = 4;
		float xr0 = x0+r;
		float xr1 = x1-r;
		float yr0 = y0+r;
		float yr1 = y1-r;
		
		// Todo: lineMaterial.SetPass(0);
		
		GL.Begin(GL.QUADS);
			GL.Color(selected ? style.NodeColor : style.NodeColor);
			GL.Vertex3(xr0,y0,0);
			GL.Vertex3(xr1,y0,0);
			GL.Vertex3(xr1,y1,0);
			GL.Vertex3(xr0,y1,0);
			//GL.Color(Color.gray);
			GL.Vertex3(x0,yr0,0);
			GL.Vertex3(xr0,y0,0);
			GL.Vertex3(xr0,y1,0);
			GL.Vertex3(x0,yr1,0);
			//GL.Color(Color.gray);
			GL.Vertex3(xr1,y0,0);
			GL.Vertex3(x1,yr0,0);
			GL.Vertex3(x1,yr1,0);
			GL.Vertex3(xr1,y1,0);
			// title bar
			GL.Color(selected ? style.HighlightColor : style.TitleColor);
			GL.Vertex3(xr0,y0,0);
			GL.Vertex3(xr1,y0,0);
			GL.Vertex3(x1,yr0,0);
			GL.Vertex3(x0,yr0,0);
			GL.Vertex3(x0,yr0,0);
			GL.Vertex3(x1,yr0,0);
			GL.Vertex3(x1,yr0+18-r,0);
			GL.Vertex3(x0,yr0+18-r,0);
		GL.End();
		
		GL.Begin(GL.LINES);
            Color c = selected ? style.HighlightLineColor : Color.white;
            c = valid ? c : style.TitleTextIvalidColor;
			GL.Color(c);
			// edges
			GL.Vertex3(xr0,y0,0);
			GL.Vertex3(xr1,y0,0);
			GL.Vertex3(x0,yr0,0);
			GL.Vertex3(x0,yr1,0);
			GL.Vertex3(x1,yr0,0);
			GL.Vertex3(x1,yr1,0);
			GL.Vertex3(xr0,y1,0);
			GL.Vertex3(xr1,y1,0);
			// corners
			GL.Vertex3(x0,yr0,0);
			GL.Vertex3(xr0,y0,0);
			GL.Vertex3(xr1,y0,0);
			GL.Vertex3(x1,yr0,0);
			GL.Vertex3(x0,yr1,0);
			GL.Vertex3(xr0,y1,0);
			GL.Vertex3(x1,yr1,0);
			GL.Vertex3(xr1,y1,0);
		GL.End();

		GUI.color = Color.white;
		GUI.contentColor = Color.white;
		GUIStyle gstyle = new GUIStyle();
		gstyle.normal.textColor = valid? style.TitleTextColor : style.TitleTextIvalidColor; 
		gstyle.fontStyle = FontStyle.Bold;
		g.GuiLabel(new Rect(pos.x+16,pos.y+3,128,128),title,gstyle);
	}
	
	const int R=8;
	const int TitleHeight = 24;
	const int LineHeight = 16;

	static Material TxmMat;
	
	static Material GetTextureMaterial()
	{
		if (TxmMat == null)
		{
			Shader diff = Shader.Find("Unlit/Texture");
			TxmMat = new Material( diff );
			TxmMat.hideFlags = HideFlags.HideAndDontSave;
			TxmMat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		return TxmMat;
	}
	
	static Material LineMat;
	
	public static Material GetLineMaterial()
	{
		if (LineMat == null)
		{
			LineMat = new Material(
				"Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass { " +
				//"    Blend Off " +
				"    Blend SrcAlpha OneMinusSrcAlpha " +
				"    ZWrite Off  Cull Off  Fog { Mode Off } " +
				"    BindChannels {" +
				"      Bind \"vertex\", vertex Bind \"color\", color }" +
				"} } }"
			);
			LineMat.hideFlags = HideFlags.HideAndDontSave;
			LineMat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		return LineMat;
	}
	
	static void DrawConnector(GraphData graph, GNode node, GConnector con, float cx, float cy, bool selected, bool highlight)
	{
		if (graph.Style == null)
		{
			Debug.LogWarning("Graph.Style is null?");
			return;
		}
		
		Material mat = GetTextureMaterial();
		
		Texture2D txm = highlight ? graph.Style.HighlightTexture : graph.Style.EmptyTexture;
		if (selected) txm = graph.Style.SelectedTexture;
		
		if (txm != null)
		{
			mat.SetTexture("_MainTex",txm);
			mat.SetPass(0);
			float x0 = cx +   (highlight ? -2 : 0);
			float y0 = cy +   (highlight ? -2 : 0);
			float x1 = cx+R+ (highlight ? +2 : 0);
			float y1 = cy+R+ (highlight ? +2 : 0);
			GL.Begin(GL.QUADS);
				GL.TexCoord(new Vector3(0,0,0));
				GL.Vertex3(x0,y0,0);
				GL.TexCoord(new Vector3(1,0,0));
				GL.Vertex3(x1,y0,0);
				GL.TexCoord(new Vector3(1,1,0));
				GL.Vertex3(x1,y1,0);
				GL.TexCoord(new Vector3(0,1,0));
				GL.Vertex3(x0,y1,0);
			GL.End();
		}
		else // draw colored shapes if style does not have custom textures
		{
			GL.Begin(GL.QUADS);
				GL.Color(Color.white);
				GL.Vertex3(cx,cy,0);
				GL.Vertex3(cx+R,cy,0);
				GL.Vertex3(cx+R,cy+R,0);
				GL.Vertex3(cx,cy+R,0);
				GL.Color(selected ? Color.green : Color.gray);
				GL.Vertex3(cx+1,cy+1,0);
				GL.Vertex3(cx+R-1,cy+1,0);
				GL.Vertex3(cx+R-1,cy+R-1,0);
				GL.Vertex3(cx+1,cy+R-1,0);
			GL.End();
			GL.Begin(GL.LINES);
				GL.Color(Color.gray);
				GL.Vertex3(cx,cy,0);
				GL.Vertex3(cx,cy+R,0);
				GL.Vertex3(cx,cy,0);
				GL.Vertex3(cx+R,cy,0);
			GL.End();
			if (highlight)
			{
				GL.Begin(GL.LINES);
					int Z=4;
					float x0 = cx-Z-1;
					float x1 = cx+R/2;
					float x2 = cx+R+Z+1;
					float y0 = cy-Z-1;
					float y1 = cy+R/2;
					float y2 = cy+R+Z+1;
					GL.Color(Color.magenta);
					GL.Vertex3(x0,y1,0);
					  GL.Vertex3(x1,y0,0);
					GL.Vertex3(x1,y0,0);
					  GL.Vertex3(x2,y1,0);
			  		GL.Vertex3(x2,y1,0);
				      GL.Vertex3(x1,y2,0);
					GL.Vertex3(x1,y2,0);
					  GL.Vertex3(x0,y1,0);
				GL.End();
			}
		}
	}
	
	static void DrawLabel(IDraw g, bool isLeft, Rect r, GraphData graph, string text)
	{
		GUI.color = Color.white;
		GUIStyle style = new GUIStyle();
		style.normal.textColor = graph.Style.TextColor;
		style.fontStyle = FontStyle.Bold;
		if (isLeft)
		{
			g.GuiLabel(new Rect(r.xMax+4,r.y-3,128,128),text,style);
		}
		else // right
		{
			style.alignment = TextAnchor.UpperRight;
			g.GuiLabel(new Rect(r.x-128-4,r.y-3,128,128),text,style);
		}
	}
	
	static void DrawInputConnector(IDraw g, GraphData graph, GNode node, int index, GConnector con, bool selected, bool highlight)
	{
		bool isLeft = (graph.Style.RightToLeft == false);
		
		Rect r = node.GetInputRect(graph,index);
		DrawConnector(graph,node,con,r.x,r.y,selected,highlight);
		
		DrawLabel (g,isLeft,r,graph,con.Name);
	}

	static void DrawOutputConnector(IDraw g, GraphData graph, GNode node, int index, GConnector con, bool selected, bool highlight)
	{
		bool isLeft = (graph.Style.RightToLeft == true);
		
		Rect r = node.GetOutputRect(graph,index);
		DrawConnector(graph,node,con,r.x,r.y,selected,highlight);
	
		DrawLabel (g,isLeft,r,graph,con.Name);
	}
	
	static public bool IsCompatible(GConnector a, GConnector b)
	{
		if (a != null && b != null)
		{
			// Don't allow node to be connected to itself.
			if (a.Parent != b.Parent)
			{
				if (a.IsInput != b.IsInput)  // Input to Output
				{
					return true;
				}
			}
		}
		return false;
	}
	
	static public void DrawPanel(IDraw g, GraphData graph, Material lineMaterial, GNode node, bool nodeSelected, GConnector hover, GConnector connector)
	{
		lineMaterial.SetPass(0);
		
		if (graph == null)
		{
			Debug.LogWarning("DrawPanel graph is null?");
			return;
		}
		if (graph.Style == null)
		{
			Debug.LogWarning("DrawPanel style is null?");
			return;
		}
		if (node == null)
		{
			Debug.LogWarning("DrawPanel node is null?");
			return;
		}
		
		string title = node.GetDisplayName();
		GStyle style = graph.Style;
		Vector3 pos = node.Position;                
		DrawPanel(g, style,pos,node.Size,title,nodeSelected, node.IsValid());
		
		if (node.HasInputs)
		{
			for (int i=0; i<node.Inputs.Count; ++i)
			{
				GConnector c = node.Inputs[i];
				lineMaterial.SetPass(0);
				bool selected = (c == hover) || (c == connector);
				bool highlight = IsCompatible(connector,c);
				DrawInputConnector(g,graph,node,i,c,selected,highlight);
			}
		}

		if (node.Outputs != null)
		{
			for (int i=0; i<node.Outputs.Count; ++i)
			{
				GConnector c = node.Outputs[i];
				lineMaterial.SetPass(0);
				bool selected = (c == hover) || (c == connector);
				bool highlight = IsCompatible(connector,c);
				DrawOutputConnector(g,graph,node,i,c,selected,highlight);
			}
		}
	}
	
	static public void DrawLineStrip(Vector2 [] points, Color rgb, float width)
	{
		if (points.Length > 1)
		{
			GL.Begin(GL.LINES);
				GL.Color(rgb);
				for (int i=0; i<points.Length-1; ++i)
				{
					GL.Vertex3(points[i].x,points[i].y,0);
					GL.Vertex3(points[i+1].x,points[i+1].y,0);
				}
			GL.End();
		}
	}

	static public void DrawLineStrip2(Vector2 [] points, Color rgb)
	{
		if (points.Length > 2)
		{
			GL.Begin(GL.QUADS);
				GL.Color(rgb);
				for (int i=0; (i+3)<points.Length; i+=2)
				{
					GL.Vertex3(points[i+0].x, points[i+0].y, 0);
					GL.Vertex3(points[i+1].x, points[i+1].y, 0);
					GL.Vertex3(points[i+3].x, points[i+3].y, 0);
					GL.Vertex3(points[i+2].x, points[i+2].y, 0);
				}
			GL.End();
		}
	}
}
