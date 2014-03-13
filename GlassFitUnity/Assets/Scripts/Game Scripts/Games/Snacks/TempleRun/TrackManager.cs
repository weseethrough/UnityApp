using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour {

	protected List<TrackPiece> trackPieces = new List<TrackPiece>();
	protected List<TrackPiece> piecePrototypes;
	public GameObject piecePrototypeRoot;

	const float GRID_SIZE = 10.0f;
	const int NUM_ACTIVE_TRACK_PIECES = 5;

	// Use this for initialization
	void Start () {
		//populate list of prototypes from the root node supplied in editor
		piecePrototypeRoot.SetActive(true);
		TrackPiece[] prototypesArray = piecePrototypeRoot.GetComponentsInChildren<TrackPiece>();
		piecePrototypeRoot.SetActive(false);

		piecePrototypes = new List<TrackPiece>(prototypesArray);

		UnityEngine.Debug.Log("TrackManager: found" + piecePrototypes.Count + " prototype pieces");


		//populate list with some pieces
		for(int i=0; i<NUM_ACTIVE_TRACK_PIECES; i++)
		{
			AddNewTrackPiece();
		}
	}

	protected void AddNewTrackPiece()
	{
		//select random one to add
		TrackPiece prototype = GetNextTrackPiecePrototype();

		//instantiate and initialise
		TrackPiece newPiece = (TrackPiece)Instantiate(prototype);
		float newPiecePos = 0.0f;
		if(trackPieces.Count > 0)
		{
			TrackPiece lastTrackPiece = trackPieces[trackPieces.Count-1];
			newPiecePos = lastTrackPiece.GetDistance() + GRID_SIZE;
		}
		newPiece.SetDistance(newPiecePos);

		//add to end of list
		trackPieces.Add(newPiece);
	}

	protected TrackPiece GetNextTrackPiecePrototype()
	{
		bool useClearPiece = Random.value < 0.8f;
		int pieceIndex = useClearPiece ? 0: Random.Range(1, piecePrototypes.Count-1);
		return piecePrototypes[pieceIndex];
	}

	// Update is called once per frame
	void Update () {
		TrackPiece toRemove = null;

		//find track piece behind the player
		foreach(TrackPiece piece in trackPieces)
		{
			if(Platform.Instance.LocalPlayerPosition.Distance - piece.GetDistance() > GRID_SIZE)
			{
				toRemove = piece;
			}
		}

		//remove if found, and add a new one
		if(toRemove != null)
		{
			trackPieces.Remove(toRemove);
			Destroy(toRemove);
			AddNewTrackPiece();
		}
	}
}
