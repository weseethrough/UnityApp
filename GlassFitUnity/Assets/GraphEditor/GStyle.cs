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
	    		
    public Color TextColor;
	public Color TitleTextColor;
    public Color TitleTextIvalidColor;

    public Color TitleColor;
    public Color HighlightColor;
    public Color UnrelatedColor;

    public Color NodeColor;
    public Color HighlightNodeColor;
    public Color UnrelatedNodeColor;
	
    public Color LineColor;
    public Color HighlightLineColor;
    public Color UnrelatedLineColor;

    public Color ConnectionLineColor;
    public Color HighlightConnectionLineColor;
    public Color UnrelatedConnectionLineColor;
	
	public GStyle()
	{
        TextColor = new Color(0.0f, 0.75f, 0.0f, 1.0f);
		TitleTextColor = new Color(0.20f, 1.0f, 0.0f, 1.0f);
        TitleTextIvalidColor = new Color(1.0f, 0.20f, 0.0f, 1.0f);						

        TitleColor = new Color(0.12f, 0.12f, 0.12f, 1.0f);
        HighlightColor = new Color(0.09f, 0.09f, 0.09f, 1.0f);
        UnrelatedColor = new Color(0.12f, 0.12f, 0.12f, 1.0f);

        NodeColor = new Color(0.18f, 0.18f, 0.25f, 1.0f);
        HighlightNodeColor = new Color(0.18f, 0.18f, 0.18f, 1.0f);
        UnrelatedNodeColor = new Color(0.13f, 0.13f, 0.13f, 1.0f);

        LineColor = new Color(0.20f, 1.0f, 0.0f, 0.5f);
        HighlightLineColor = new Color(0.2f, 1.0f, 0.0f, 1.0f);
        UnrelatedLineColor = new Color(0.2f, 0.4f, 0.2f, 1.0f);

        ConnectionLineColor = new Color(0.2f, 1.0f, 0.0f, 0.5f);
        HighlightConnectionLineColor = new Color(0.4f, 1.0f, 0.0f, 1.0f);
        UnrelatedConnectionLineColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);

	}
};
