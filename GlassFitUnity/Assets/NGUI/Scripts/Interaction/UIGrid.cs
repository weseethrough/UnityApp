//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// All children added to the game object with this script will be repositioned to be on a grid of specified dimensions.
/// If you want the cells to automatically set their scale based on the dimensions of their content, take a look at UITable.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Grid")]
public class UIGrid : UIWidgetContainer
{
	public enum Arrangement
	{
		Horizontal,
		Vertical,
	}

	/// <summary>
	/// Type of arrangement -- vertical or horizontal.
	/// </summary>

	public Arrangement arrangement = Arrangement.Horizontal;

	/// <summary>
	/// Maximum children per line.
	/// If the arrangement is horizontal, this denotes the number of columns.
	/// If the arrangement is vertical, this stands for the number of rows.
	/// </summary>

	public int maxPerLine = 0;

	/// <summary>
	/// The width of each of the cells.
	/// </summary>

	public float cellWidth = 200f;

	/// <summary>
	/// The height of each of the cells.
	/// </summary>

	public float cellHeight = 200f;

	/// <summary>
	/// Reposition the children on the next Update().
	/// </summary>

	public bool repositionNow = false;

	/// <summary>
	/// Whether the children will be sorted alphabetically prior to repositioning.
	/// </summary>

	public bool sorted = false;

	/// <summary>
	/// Whether to ignore the disabled children or to treat them as being present.
	/// </summary>

	public bool hideInactive = true;

	bool mStarted = false;

	void Start ()
	{
		mStarted = true;
#if UNITY_EDITOR
		if (Application.isPlaying)
#endif
		Reposition();
	}

	void Update ()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}

	static public int SortByName (Transform a, Transform b) { return string.Compare(a.name, b.name); }

	/// <summary>
	/// Recalculate the position of all elements within the grid, sorting them alphabetically if necessary.
	/// </summary>

	public void Reposition ()
	{
		if (!mStarted)
		{
			repositionNow = true;
			return;
		}

		Transform myTrans = transform;

		int x = 0;
		int y = 0;

		UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		if (drag != null) drag.ResetPosition(); 

		if (sorted)
		{
			List<Transform> list = new List<Transform>();

			for (int i = 0; i < myTrans.childCount; ++i)
			{
				Transform t = myTrans.GetChild(i);
				if (t && (!hideInactive || NGUITools.GetActive(t.gameObject))) list.Add(t);
			}
			list.Sort(SortByName);

			for (int i = 0, imax = list.Count; i < imax; ++i)
			{
				Transform t = list[i];

				if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

				float depth = t.localPosition.z;
				t.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, depth) :
					new Vector3(cellWidth * y, -cellHeight * x, depth);

				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
			}
		}
		else
		{
			for (int i = 0; i < myTrans.childCount; ++i)
			{
				Transform t = myTrans.GetChild(i);

				if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;

				float depth = t.localPosition.z;
				t.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, depth) :
					new Vector3(cellWidth * y, -cellHeight * x, depth);

				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
			}
		}
		if (drag != null) drag.ResetPosition(); 


	}

	public void RemoveButtons() {
		for(int i=transform.childCount - 1; i>=0; i--) {
			if(transform.GetChild(i).gameObject.activeSelf) {
				Destroy(transform.GetChild(i).gameObject);
				transform.GetChild(i).parent = null;
			}
		}
//		UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
//		if (drag != null) drag.ResetPosition();
	}
}
