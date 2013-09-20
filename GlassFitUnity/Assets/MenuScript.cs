using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {
	
	// logo box and texture
	Texture2D logoTex;
	private Rect logoBox;
	
	//login text
	private Rect userTextbox;
	private Rect passTextbox;
	private string userText = "Enter username: ";
	private string passText = "Enter password: ";
	private string login = "Login";
	
	//login boxes
	private Rect user;
	private Rect pass;
	private Rect start; 
	
	//facebook symbol
	private Rect face;	
	Texture2D faceTex;
	
	private const int MARGIN = 20;
	private const int originalWidth = 800;
	private const int originalHeight = 500;
	private Vector3 scale;
	
	// Use this for initialization
	void Start () {
		logoTex = Resources.Load("GlassfitLogo") as Texture2D;
		logoBox = new Rect(MARGIN, MARGIN, 392, 66);
		
		userTextbox = new Rect(MARGIN, originalHeight/2, 150, 40);
		passTextbox = new Rect(MARGIN, originalHeight/2+80, 150, 40);
		
		user = new Rect(userTextbox.x + 140, userTextbox.y+5, 300, 30);
		pass = new Rect(user.x, passTextbox.y+5, 300, 30);
		start = new Rect(user.x + 350, passTextbox.y-40, 100, 50);
		
		faceTex = Resources.Load("facebook") as Texture2D;
		face = new Rect(Screen.width - 100, Screen.height - 100, 56, 56);
		
	}
	
	void OnGUI() {
		scale.x = Screen.width/originalWidth;
		scale.y = Screen.height/originalHeight;
		scale.z = 1;
		
		var svMat = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		GUI.DrawTexture(logoBox, logoTex);
		
		//GUI.contentColor = Color.white;
		GUI.skin.box.fontSize = 15;
		
		GUI.Label(userTextbox, userText);
		GUI.Label(passTextbox, passText);
		
		GUI.color = Color.white;
		
		GUI.TextField(user, "");
		GUI.TextField(pass, "");
		
		if(GUI.Button(start, login)) {
			Application.LoadLevel("testGameSelect");	
		}
		
		GUI.Button (face, "");
		GUI.DrawTexture(face, faceTex);		
		GUI.matrix = svMat;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
