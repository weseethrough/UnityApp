using UnityEngine;
using System.Collections;

[System.Serializable]
public class GStyle
{
	// Which side of node should input/output ports appear?
	//   false => Left=Input, Right=Output
	public bool RightToLeft;
	public Texture2D EmptyTexture;
	public Texture2D SelectedTexture;
	public Texture2D HighlightTexture;
	public Color LineColor;
	public Color NodeColor;
	public Color TitleColor;
	public Color TitleTextColor;
	public Color HighlightColor;
	public Color HighlightLineColor;
	public Color TextColor;
	
	public GStyle()
	{
		LineColor = new Color(1,1,0,0.5f);
		NodeColor = new Color(0,0,0,1);
		TitleColor = new Color(0,0.25f,1,1);
		TitleTextColor = new Color(1,1,0,1);
		HighlightColor = new Color(0,0.5f,1,1);
		HighlightLineColor = new Color(1,1,1,1);
		TextColor = new Color(0,0.75f,0,1);
	}
};
