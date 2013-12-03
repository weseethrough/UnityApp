using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Displays the friends on a texture.
/// </summary>
public class FriendDisplay : MonoBehaviour {
	
	// A texture that says "Loading"
	private Texture2D loading;
	
	// A texture that will download the image
	private Texture wwwFriend;
	
	// The website to obtain the picture
	private WWW displayPic = null;
	
	// Retrieve the list of friends
	private Friend[] friendList;
	
	// Get the start touch 
	private Vector2 startTouch = new Vector2(0, 0);
	
	// Get the current distance between touches
	private float curTouchDist = 0;
	
	// The current friend selected
	private int currentFriend = 0;
	
	// The synchronisation handler
	Platform.OnSync handler = null;
	
	// Boolean for whether or not the image has been fetched
	private bool imageFetched = false;
	
	// Texture to display friend's picture on
	private UITexture tex;
	
	/// <summary>
	/// Start this instance. Sets a loading texture and obtains necessary components
	/// </summary>
	void Start () {		
		// Load the loading texture from resources.
		loading = (Texture2D)Resources.Load("loading", typeof(Texture2D));
		if(loading == null) {
			UnityEngine.Debug.Log("Friend Display: Loading texture not loaded");
		}
		
		// Get the UITexture component and apply the loading texture
		tex = GetComponent<UITexture>();
		tex.material.mainTexture = loading;
		
		// Sync to the server
		Platform.Instance.SyncToServer();
		
		// Create a sync handler that updates the friends list
		handler = new Platform.OnSync(() => {
			UpdateFriendsList();
		});
		Platform.Instance.onSync += handler;	
		
		UpdateFriendsList();
		
		DataVault.Set("screen_name", "Loading Screen Name...");
		UnityEngine.Debug.Log("Friend Display: started");
	}
	
	/// <summary>
	/// Update the friends list with data from the platform.
	/// </summary>
	void UpdateFriendsList() {
		// Get the type of friend
		string type = (string) DataVault.Get("friend_type");		
		// Change the type to lower case.
		type = type.ToLower();
		
		// Get the list of friends
		friendList = Platform.Instance.Friends();
		
		// Filter the friends based on whether they have glass and the type of friend 
		// previously selected
		List<Friend> filtered = new List<Friend>();
		foreach (Friend friend in friendList) {
			if ((friend.userId.HasValue || friend.hasGlass) && friend.provider == type) filtered.Add(friend);
		}
		
		// Set the friend list to the filtered list
		friendList = filtered.ToArray();
	}
	
	/// <summary>
	/// Destroy this instance. Removes its sync handler.
	/// </summary>
	void Destroy () {
		Platform.Instance.onSync -= handler;
	}	
	
	/// <summary>
	/// Update this instance. Updates the friend picture and checks for input
	/// </summary>
	void Update () {
		// If the user has no friends display a message
		if(friendList.Length == 0) {
			DataVault.Set("screen_name", "Ha ha, you have no friends!");
			// TODO: Update list on sync
		} else {
			// Otherwise, if the picture hasn't been fetched
			if(!imageFetched){
				UnityEngine.Debug.Log("URL is: " + friendList[currentFriend].image);
				
				// Create a link to the image
				WWW displayLink = new WWW(friendList[currentFriend].image);
				
				// If the link is not null
				if (displayLink != null){
					// While the link hasn't finished loading
					while(!displayLink.isDone) {
						// If there are errors
						if (displayLink.error != null) {
							// Set the texture to loading and log the error message
							tex.material.mainTexture = loading;
							UnityEngine.Debug.Log(displayLink.error);
							UnityEngine.Debug.Log("Friend Display: error getting display");
						} 
							//UnityEngine.Debug.Log("Friend Display: Loading Image...");
					} 
				
					// Set the texture to the image texture
					tex.mainTexture = displayLink.texture;
					
					// Set the image fetched boolean to true
					imageFetched = true;
					UnityEngine.Debug.Log("Friend Display: image obtained!");
					
				}
			}
			
			// Set the name of the friend
			DataVault.Set("screen_name", friendList[currentFriend].name);
			
			// Set the current friend
			DataVault.Set("current_friend", friendList[currentFriend]);
		}
		
		// If the screen is touched
		if(Input.touchCount == 1) 
		{
			// Save the position of when the screen is pressed
			if(Input.GetTouch(0).phase == TouchPhase.Began) {
				startTouch = Input.touches[0].position;
				UnityEngine.Debug.Log("Friend Display: Touch started");
			}
			
			// Save the current distance between the start and the input
			if(Input.GetTouch(0).phase == TouchPhase.Moved) {
				curTouchDist = Input.GetTouch(0).position.x - startTouch.x;
				UnityEngine.Debug.Log("Friend Display: Touch moved");
			}
			
			// If the touch has ended, change the current friend based on the distance of the finger movement
			if(Input.GetTouch(0).phase == TouchPhase.Ended) {
				UnityEngine.Debug.Log("Friend Display: Touch Ended");
				if((curTouchDist > (float)Screen.width / 3.0f) && currentFriend < friendList.Length - 1) {
					currentFriend ++;
					
					imageFetched = false;
				} else if((curTouchDist < (float)-Screen.width / 3.0f) && currentFriend > 0) {
					currentFriend--;
					imageFetched = false;
				}
				curTouchDist = 0;
			}
		}
	}
}
