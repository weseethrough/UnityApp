using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnackController : MonoBehaviour {
	
	//list of available games
	protected List<Game> snackGames = null;
	
	protected bool isGameInProgress = false;
	protected int lastChosenGameIndex = -1;
	
	protected GameObject currentSnackGameMainObj = null;
	
	protected Game currentGame = null;
	
	GestureHelper.OnTap handleTapAccept = null;
	GestureHelper.OnTap handleTapBegin = null;
	GestureHelper.DownSwipe cancelGame = null;
	
	void Start()
	{
		//initialise the list of available games
		snackGames = new List<Game>();
		List<Game> allGames = Platform.Instance.GetGames();
		
		//cherry pick the games which are of type snack, and are unlocked
		foreach( Game game in allGames )
		{
			if( game.type == "Snack" && game.state == "Unlocked")
			{
				snackGames.Add( game );	
			}
			UnityEngine.Debug.Log("Game. Type:" + game.type + ". id:" + game.gameId + ". scene:" + game.sceneName);
		}
		
		if(snackGames.Count < 1)
		{
			UnityEngine.Debug.LogError("SnackController: No suitable games found");
		}
		
		//create tap handlers
		handleTapAccept = new GestureHelper.OnTap( () => {
			LoadGame();
			GestureHelper.onTap -= handleTapAccept;
		});
		
		handleTapBegin = new GestureHelper.OnTap( () => {
			BeginGame();
			GestureHelper.onTap -= handleTapBegin;
		});
		
		cancelGame = new GestureHelper.DownSwipe( () => {
			CancelGame();
			GestureHelper.onSwipeDown -= cancelGame;
		});
			
	}
	
	/// <summary>
	/// Offers the user the opportunity to play a game.
	/// </summary>
	public void OfferGame()
	{
		currentGame = getNextGame();
		StartCoroutine(ScheduleGameOffer());
	}
	
	IEnumerator ScheduleGameOffer()
	{
		
		//transition flow to panel to offer game
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gc = fs.Outputs.Find( r => r.Name == "BeginSnack" );
		if (gc != null)
		{
			fs.parentMachine.FollowConnection(gc);
		}
		
		//set strings in datavault for ui panel
		DataVault.Set("snack_game_title", currentGame.name);
		DataVault.Set("snack_game_desc", currentGame.description);
		
		//trigger alert flash / chime to get user attention
		
		//activate gesture listener
		GestureHelper.onTap += handleTapAccept;
		
		//wait 5s
		for(int i=5; i>0; i++)
		{
			yield return new WaitForSeconds(1.0f);
			string s = i>0 ? i.ToString() : "";
			DataVault.Set("snack_prompt", "tap to play" + s);
		}
			
		//dismiss (transition flow back to HUD)
		gc = fs.Outputs.Find( r => r.Name == "Return" );
		if (gc != null)
		{
			fs.parentMachine.FollowConnection(gc);
		}		
		//unregister gesture listener
		GestureHelper.onTap -= handleTapAccept;
		
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
		lastChosenGameIndex ++;
		//wrap back to start if necessary
		if(lastChosenGameIndex >= snackGames.Count)
		{
			lastChosenGameIndex = 0;
		}
		
		Game chosenGame = snackGames[lastChosenGameIndex];
		
		//store id in datavault
		DataVault.Set("current_snack_game_id", chosenGame.gameId);
		
		return chosenGame;
		
	}
	
	protected void LoadGame()
	{
		UnityEngine.Debug.Log("SnackController: Loading Game");
		
		StartCoroutine( doAsyncLoad(currentGame) );
	}
	
	string getSceneName(Game game)
	{
		
		return game.sceneName;
		
		string sceneName = "";
		
		//get the scene name
		if(game.gameId == "activity_boulder")
		{
			sceneName = "Snack_Boulder";
		}
		else if(game.gameId == "activity_bolt_level1")
		{
			sceneName = "Snack_Bolt";
		}
		else
		{
			UnityEngine.Debug.LogError("SnackController: Unable to get scene for game " + game.gameId);
		}
		
		return sceneName;
		
	}
	
	IEnumerator doAsyncLoad( Game game )
	{
		//transition flow to 'loading'
		DataVault.Set("snack_prompt", "Loading...");
		
		AsyncOperation async = Application.LoadLevelAdditiveAsync( getSceneName(game) );
	
		//yield until the load is complete
		yield return async;
	
		//get the snack game main script
		//snackGame = GetComponent<SnackBase>();
		if(currentSnackGameMainObj == null)
		{
			UnityEngine.Debug.LogError("Didn't find Snack game controller script");	
		}
		
		//offer the user the chance to start the game. show 'tap to start' UI
		DataVault.Set("snack_prompt", "Tap to Begin Game");
		
		//register gesture listener to start the game.
		GestureHelper.onTap += handleTapAccept;
		//register gesture listener to cancel the game.
		GestureHelper.onSwipeDown += cancelGame;
	}
	
	protected void BeginGame()
	{
		if(currentSnackGameMainObj != null)
		{
			//snackGame.Begin();
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
	}
	
}
