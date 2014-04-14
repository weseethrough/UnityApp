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

    public GStyleColor TextColor;
    public GStyleColor TitleTextColor;
    public GStyleColor TitleTextIvalidColor;

    public GStyleColor TitleColor;
    public GStyleColor HighlightColor;
    public GStyleColor UnrelatedColor;

    public GStyleColor NodeColor;
    public GStyleColor HighlightNodeColor;
    public GStyleColor UnrelatedNodeColor;

    public GStyleColor LineColor;
    public GStyleColor HighlightLineColor;
    public GStyleColor UnrelatedLineColor;

    public GStyleColor ConnectionLineColor;
    public GStyleColor HighlightConnectionLineColor;
    public GStyleColor UnrelatedConnectionLineColor;
	
	public GStyle()
	{
        TextColor = new GStyleColor(0.0f, 0.75f, 0.0f, 1.0f);
        TitleTextColor = new GStyleColor(0.20f, 1.0f, 0.0f, 1.0f);
        TitleTextIvalidColor = new GStyleColor(1.0f, 0.20f, 0.0f, 1.0f);

        TitleColor = new GStyleColor(0.12f, 0.12f, 0.12f, 1.0f);
        HighlightColor = new GStyleColor(0.09f, 0.09f, 0.09f, 1.0f);
        UnrelatedColor = new GStyleColor(0.12f, 0.12f, 0.12f, 1.0f);

        NodeColor = new GStyleColor(0.18f, 0.18f, 0.25f, 1.0f);
        HighlightNodeColor = new GStyleColor(0.18f, 0.18f, 0.18f, 1.0f);
        UnrelatedNodeColor = new GStyleColor(0.13f, 0.13f, 0.13f, 1.0f);

        LineColor = new GStyleColor(0.20f, 1.0f, 0.0f, 0.5f);
        HighlightLineColor = new GStyleColor(0.2f, 1.0f, 0.0f, 1.0f);
        UnrelatedLineColor = new GStyleColor(0.2f, 0.4f, 0.2f, 1.0f);

        ConnectionLineColor = new GStyleColor(0.2f, 1.0f, 0.0f, 0.5f);
        HighlightConnectionLineColor = new GStyleColor(0.4f, 1.0f, 0.0f, 1.0f);
        UnrelatedConnectionLineColor = new GStyleColor(0.2f, 0.2f, 0.2f, 0.2f);

	}
};
