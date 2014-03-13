using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour {

	protected List<TrackPiece> trackPieces = new List<TrackPiece>();
	protected List<TrackPiece> piecePrototypes;
	public GameObject piecePrototypeRoot;
	Log log;
	const float GRID_SIZE = 10.0f;
	const int NUM_ACTIVE_TRACK_PIECES = 5;

	// Use this for initialization
	void Start () {
		if(piecePrototypeRoot == null)
		{
			log.error("Couldn't find prototype root");
		}

		//populate list of prototypes from the root node supplied in editor
		piecePrototypeRoot.SetActive(true);
		TrackPiece[] prototypesArray = piecePrototypeRoot.GetComponentsInChildren<TrackPiece>();
		piecePrototypeRoot.SetActive(false);

		piecePrototypes = new List<TrackPiece>(prototypesArray);

		if(piecePrototypes == null)
		{
			log.error("couldn't create prototype list");
		}
		if(piecePrototypes.Count < 1)
		{
			log.error("don't have any prototypes");
		}

		UnityEngine.Debug.Log("TrackManager: found" + piecePrototypes.Count + " prototype pieces");

		//start with a clear piece
		AddClearTrackPiece();

		//populate list with some pieces
		while(trackPieces.Count < NUM_ACTIVE_TRACK_PIECES)
		{
			AddRandomTrackPiece();
		}

		log = new Log("TrackManager");
	}

	protected void AddClearTrackPiece()
	{
		if(piecePrototypes.Count < 1)
		{
			log.error("no piece prototypes!");
		}
		TrackPiece clearPrototype = piecePrototypes[0];
		AddTrackPieceFromPrototype(clearPrototype);
	}

	protected void AddRandomTrackPiece()
	{
		if(piecePrototypes.Count<1)
		{
			log.error("no piece prototypes!");
		}
		//select random one to add
		bool useClearPiece = Random.value < 0.2f;
		int pieceIndex = useClearPiece ? 0: Random.Range(1, piecePrototypes.Count-1);
	
		AddTrackPieceFromPrototype(piecePrototypes[pieceIndex]);
	}

	protected void AddTrackPieceFromPrototype(TrackPiece prototype)
	{

		//instantiate and initialise
		TrackPiece newPiece = (TrackPiece)Instantiate(prototype);
		float newPiecePos = 0.0f;
		if(trackPieces.Count > 0)
		{
			TrackPiece lastTrackPiece = trackPieces[trackPieces.Count-1];
			newPiecePos = lastTrackPiece.GetDistance() + GRID_SIZE;
			log.info("Created new track piece of type: " + newPiece.name + " at " + newPiecePos);
		}
		newPiece.SetDistance(newPiecePos);

		//add to end of list
		trackPieces.Add(newPiece);
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

		log.info("Removing track piece named: " + toRemove.gameObject.name);

		//remove if found, and add a new one
		if(toRemove != null)
		{
			trackPieces.Remove(toRemove);
			Destroy(toRemove.gameObject);
			AddRandomTrackPiece();
		}
	}
}
