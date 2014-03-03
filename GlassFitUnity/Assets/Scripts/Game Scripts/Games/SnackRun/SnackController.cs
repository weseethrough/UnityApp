using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnackController : MonoBehaviour {
	
	//list of available games
	public static List<Game> snackGames {get; private set;}
	
	protected bool isGameInProgress = false;
	protected int lastRotationGameIndex = 0;
	
	protected SnackBase currentSnackGameMainObj = null;
	
	protected Game currentGame = null;
	
	//GestureHelper.OnSwipeRight handleAccept = null;
	//GestureHelper.OnSwipeRight handleBegin = null;
	//GestureHelper.OnBack cancelGame = null;
	
	protected bool acceptedGame = false;
	
	protected bool awaitingAcceptTap = false;
	
	public static List<Game> getSnackGames()
	{
		if(snackGames == null)
		{
			//initialise the list of available games
			snackGames = new List<Game>();
			List<Game> allGames = Platform.Instance.GetGames();
			
			//cherry pick the games which are of type snack, and are unlocked
			foreach( Game game in allGames )
			{
				if( game.type == "Snack")
				{
					UnityEngine.Debug.Log("SnackController: Found a snack: " + game.gameId );
					snackGames.Add( game );	
				}
				UnityEngine.Debug.Log("Game. Type:" + game.type + ". id:" + game.gameId + ". scene:" + game.sceneName);
			}
			
			if(snackGames.Count < 1)
			{
				UnityEngine.Debug.LogError("SnackController: No suitable games found");
			}
		}
		return snackGames;
	}
	
	
	void Start()
	{
		//initialise the list of snack games, if necessary
		getSnackGames();
		
		//create tap handlers
//		handleAccept = new GestureHelper.OnSwipeRight( () => {
//			LoadGame();
//			acceptedGame = true;
//			GestureHelper.onSwipeRight -= handleAccept;
//		});
//		
//		handleBegin = new GestureHelper.OnSwipeRight( () => {
//			BeginGame();
//			GestureHelper.onSwipeRight -= handleBegin;
//		});
//		
//		cancelGame = new GestureHelper.OnBack( () => {
//			CancelGame();
//			GestureHelper.onBack -= cancelGame;
//		});
			
	}
	
	/// <summary>
	/// Offers the user the opportunity to play a game.
	/// </summary>
	public void OfferGameRotation()
	{
		currentGame = getNextGame();
		OfferGame();
	}
	
	
	public void OfferGame()
	{
	
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		//clear current snack offer
		if(awaitingAcceptTap)
		{
			GConnector gc = fs.Outputs.Find( r => r.Name == "Return" );
			if (gc != null)
			{
				fs.parentMachine.FollowConnection(gc);
			}
			
			awaitingAcceptTap = false;
		}
		
		UnityEngine.Debug.Log("SnackController: offering snack");
		//transition flow to panel to offer game
		GConnector gcBegin = fs.Outputs.Find( r => r.Name == "BeginSnack" );
		if (gcBegin != null)
		{
			fs.parentMachine.FollowConnection(gcBegin);
		}
		else
		{
			//couldn't find HUD connector
			UnityEngine.Debug.LogError("SnackController: couldn't find flow connector 'BeginSnack' ");
		}
		
		//set strings in datavault for ui panel
		DataVault.Set("snack_game_title", currentGame.name);
		DataVault.Set("snack_game_desc", currentGame.description);
		//trigger alert flash / chime to get user attention
		
		
		//activate gesture listener
		//GestureHelper.onSwipeRight += handleAccept;
		awaitingAcceptTap = true;
		//stop any existing offers
		StopCoroutine("ScheduleGameOffer");
		//schedule the new offer
		StartCoroutine("ScheduleGameOffer");
	}
	
	public void OfferGame(string gameID)
	{
		Game game = snackGames.Find( r => r.gameId == gameID );
		if(game != null)
		{
			currentGame = game;
			OfferGame();
		}
		else
		{
			UnityEngine.Debug.LogWarning("SnackController: Couldn't find specific game to offer:" + gameID + " Offering based on rotation instead");
			OfferGameRotation();
		}
		
			
	}
	
	IEnumerator ScheduleGameOffer()
	{
		//unload current snack
		CancelCurrentSnack();
		
		yield return new WaitForSeconds(0.5f);
		
		UnityEngine.Debug.Log("SnackController: Started coroutine. Counting out game offer");
		acceptedGame = false;
		
		//wait 5s
		for(int i=10; i>0; i--)
		{
			string s = i>0 ? i.ToString() : "";
			DataVault.Set("snack_prompt", "Tap to play " + s);
			yield return new WaitForSeconds(1.0f);
		}
			
		if(!acceptedGame)
		{
			//dismiss (transition flow back to HUD)
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gc = fs.Outputs.Find( r => r.Name == "Return" );
			if (gc != null)
			{
				fs.parentMachine.FollowConnection(gc);
			}
			
			awaitingAcceptTap = false;
			UnityEngine.Debug.Log("SnackController: Snack declined");
			
			//notify the game that the snack is over
			SnackRun run = (SnackRun)FindObjectOfType(typeof(SnackRun));
			if(run != null)
			{
				run.OnSnackFinished();
			}

			//
		}
		
	}
	
	/// <summary>
	/// Cancels and unloads the current snack
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance cancel current snack; otherwise, <c>false</c>.
	/// </returns>
	public void CancelCurrentSnack()
	{
		//find snack
		SnackBase snack = (SnackBase)FindObjectOfType(typeof(SnackBase));
		
		//call finish
		if(snack != null)
		{
			snack.Finish();
		}
	}
	
	/// <summary>
	/// Gets the next game to play
	/// For now will alternate between games, but eventually could interface with playlist config, or pick randomly etc.
	/// </summary>
	/// <returns>
	/// The next game.
	/// </returns>
	protected Game getNextGame()
	{
		//choose next game
		lastRotationGameIndex ++;
		//wrap back to start if necessary
		if(lastRotationGameIndex >= snackGames.Count)
		{
			lastRotationGameIndex = 0;
		}
		
		Game chosenGame = snackGames[lastRotationGameIndex];
		
		UnityEngine.Debug.Log("SnackController: Chose game: " + chosenGame.gameId);
		
		//store id in datavault
		DataVault.Set("current_snack_game_id", chosenGame.gameId);
		
		return chosenGame;
		
	}
	
	protected void LoadGame()
	{
		UnityEngine.Debug.Log("SnackController: Loading Game");
		SoundManager.PlaySound(SoundManager.Sounds.Tap);
		StartCoroutine( doAsyncLoad(currentGame) );
	}
	
	string getSceneName(Game game)
	{
		return game.sceneName;
	}
	
	IEnumerator doAsyncLoad(Game game)
	{
		UnityEngine.Debug.Log("SnackController: Loading snack: " + game.gameId );
		
		//transition flow to 'loading'
		DataVault.Set("snack_prompt", "Loading...");
		
		AsyncOperation async = Application.LoadLevelAdditiveAsync( getSceneName(game) );
	
		//yield until the load is complete
		yield return async;
	
		//wait a frame or two so that the objects have a chance to initialise.
		//TODO, do this properly.
		yield return new WaitForSeconds(0.2f);
		
		//get the snack game main script
		currentSnackGameMainObj = (SnackBase)FindObjectOfType(typeof(SnackBase));
		if(currentSnackGameMainObj == null)
		{
			UnityEngine.Debug.LogError("Didn't find Snack game controller script");	
		}
		
		//Start the game immediately.
		BeginGame();
	}
	

	
	protected void BeginGame()
	{
		if(currentSnackGameMainObj != null)
		{
			currentSnackGameMainObj.Begin();
			UnityEngine.Debug.Log("SnackController: Beginning Game");
		}
		
		//transition UI back to HUD
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gc = fs.Outputs.Find( r => r.Name == "Return" );
		if (gc != null)
		{
			fs.parentMachine.FollowConnection(gc);
		}
	}
	
	protected void CancelGame()
	{
		//unload game
		Destroy(currentSnackGameMainObj);
		currentSnackGameMainObj = null;
		
		//transition UI back to HUD
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gc = fs.Outputs.Find( r => r.Name == "Return" );
		if (gc != null)
		{
			fs.parentMachine.FollowConnection(gc);
		}
		
		//notify the game that the snack is over
		SnackRun run = (SnackRun)FindObjectOfType(typeof(SnackRun));
		if(run != null)
		{
			run.OnSnackFinished();
		}
	}
	
	/// <summary>
	/// Handles the tap.
	/// </summary>
	/// <returns>
	/// True if we handled it. False if we didn't.
	/// </returns>
	public bool HandleTap()
	{
		if(awaitingAcceptTap)
		{
			LoadGame();
			acceptedGame = true;
			awaitingAcceptTap = false;
			return true;
		}
		else
		{
			return false;
		}
	}
	
}
