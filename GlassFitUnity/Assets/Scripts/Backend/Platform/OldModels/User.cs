using System;

public class User
{
	public string username { get; set; }
	public string name { get; set; }
	public int id { get; set; }
		
	public User ()
	{
	}
	public User (int id, string username, string name) 
	{
		this.id = id;
		this.username = username;
		this.name = name;
		UnityEngine.Debug.Log("User: instance set with ID: " + id.ToString() + ", username: " + username + ", name: " + name);
	}
}

