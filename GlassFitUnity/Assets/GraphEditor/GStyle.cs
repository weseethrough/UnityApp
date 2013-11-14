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
    public Color TitleTextIvalidColor;
	public Color HighlightColor;
	public Color HighlightLineColor;
	public Color TextColor;
	
	public GStyle()
	{
		LineColor = new Color(0.20f, 1.0f, 0.0f, 0.5f);
		NodeColor = new Color(0.18f, 0.18f, 0.18f, 1.0f);
		TitleColor = new Color(0.12f, 0.12f, 0.12f, 1.0f);
		TitleTextColor = new Color(0.20f, 1.0f, 0.0f, 1.0f);
        TitleTextIvalidColor = new Color(1.0f, 0.20f, 0.0f, 1.0f);
		HighlightColor = new Color(0.0f, 0.5f, 0.2f, 1.0f);
		HighlightLineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		TextColor = new Color(0.0f, 0.75f, 0.0f, 1.0f);
	}
};
