using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class TextWindow : EditorWindow
{
	// Allow user to drag viewport to see graph
	Vector2 ViewPosition;

	[MenuItem("Window/Text Window")]
	public static void Init ()
	{
		TextWindow window = GetWindow (typeof(TextWindow)) as TextWindow;
		window.minSize = new Vector2(600.0f, 400.0f);
		window.wantsMouseMove = true;
		Object.DontDestroyOnLoad( window );
		window.Show();
	}
	
	void OnSelectionChange () { Repaint(); }
	
	Material lineMaterial;

	void CreateLineMaterial()
	{
		if( ! lineMaterial )
		{
			lineMaterial = new Material(
				"Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass { " +
				//"    Blend Off " +
				"    Blend SrcAlpha OneMinusSrcAlpha " +
				"    ZWrite Off  Cull Off  Fog { Mode Off } " +
				"    BindChannels {" +
				"      Bind \"vertex\", vertex Bind \"color\", color }" +
				"} } }"
			);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
	
	// Todo: work-around for Unity's unexpected GL/GUI behavior?
	void GuiBox(Rect r, string text)
	{
		GL.PushMatrix();
		GL.LoadPixelMatrix(0,Screen.width,Screen.height-20,0);
		GUI.Box(new Rect(r.x-ViewPosition.x,r.y,r.width,r.height),text);
		GL.PopMatrix();
		//float left = ViewPosition.x;
		//float right = left + Screen.width;
		//float top = ViewPosition.y;
		//float bottom = top + Screen.height-20;
		//GL.LoadPixelMatrix(left,right,bottom,top);
	}
	
	void OnGUI ()
	{
		float W = Screen.width;
		float H = Screen.height-20;
		
		ViewPosition = new Vector2(-48,0);

		GL.PushMatrix();

		GL.Viewport(new Rect(0,0,W,H));
		
		// left, right, bottom, top
		float left = ViewPosition.x;
		float right = left + W;
		float top = ViewPosition.y;
		float bottom = top + H;
		GL.LoadPixelMatrix(left,right,bottom,top);

		Rect r1 = new Rect(0,0,64,24);
		Rect r2 = new Rect(-32,32,64,24);

		CreateLineMaterial();
		lineMaterial.SetPass( 0 );
		GL.Begin(GL.QUADS);
		GL.Color(Color.black);
		GL.Vertex3(0,0,0);
		GL.Vertex3(W,0,0);
		GL.Vertex3(W,H,0);
		GL.Vertex3(0,H,0);
		GL.Color(Color.gray);
		GL.Vertex3(-100,0,0);
		GL.Vertex3(0,0,0);
		GL.Vertex3(0,H,0);
		GL.Vertex3(-100,H,0);
		GL.Color(Color.red);
		GL.Vertex3(r1.xMin,r1.yMin,0);
		GL.Vertex3(r1.xMax,r1.yMin,0);
		GL.Vertex3(r1.xMax,r1.yMax,0);
		GL.Vertex3(r1.xMin,r1.yMax,0);
		GL.Color(Color.red);
		GL.Vertex3(r2.xMin,r2.yMin,0);
		GL.Vertex3(r2.xMax,r2.yMin,0);
		GL.Vertex3(r2.xMax,r2.yMax,0);
		GL.Vertex3(r2.xMin,r2.yMax,0);
		GL.End();
		
		GUI.Box(r1,"Box-1");
		//GUI.Box(r2,"Box-2");
		GuiBox(r2,"Box-2");
		
		GL.PopMatrix();
	}

	void GLBox(float x, float y, float w, float h)
	{
		GL.Begin(GL.QUADS);
		GL.Color(Color.white);
		GL.Vertex3(x,y,0);
		GL.Vertex3(x+w,y,0);
		GL.Vertex3(x+w,y+h,0);
		GL.Vertex3(x,y+h,0);
		GL.End();
	}
}	
