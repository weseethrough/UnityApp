using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sqo;
using UnityEngine;
using RaceYourself.Models;

namespace RaceYourself
{
	/// <summary>
	/// Server API
	/// </summary>
	public class API
	{
		private const string SCHEME = "http://";
		private const string AUTH_HOST = "auth.raceyourself.com";
		private const string AUTH_TOKEN_URL = SCHEME + AUTH_HOST + "/oauth/token";
		private string apiHost = "api.raceyourself.com"; // Note: might be supplied through auth load-balancer in future
			
		private const string CLIENT_ID = "8c8f56a8f119a2074be04c247c3d35ebed42ab0dcc653eb4387cff97722bb968";
		private const string CLIENT_SECRET = "892977fbc0d31799dfc52e2d59b3cba88b18a8e0080da79a025e1a06f56aa8b2";
		
		private Siaqodb db;
		
		private OauthToken token = null;
		private Account user = null;
		
		public API(Siaqodb database) 
		{
			Debug.Log("API: created");
			db = database;
			user = db.Cast<Account>().SingleOrDefault();
			token = db.Cast<OauthToken>().SingleOrDefault();
			if (user != null && token != null) {
				if (user.id != token.userId) {
					Debug.LogError("API: Token in database does not belong to user in database!");
					// TODO: Allow storage of multiple users or delete old data on id mismatch?
					user = null;
					token = null;
				} else {
					if (!token.HasExpired) {
						Debug.Log("API: Still logged in as " + user.DisplayName + "/" + user.id);
					}
				}
			} else {
				Debug.Log("API: No stored account");
			}
		}
		
		/// <summary>
		/// Coroutine to retrieve a globally unique device id from the server.
		/// Triggers Platform.OnRegistration upon completion.
		/// </summary>
		public IEnumerator RegisterDevice() {
			Debug.Log("API: RegisterDevice()");
			
			string ret = "Failure";
			try {
				Device device = new Device();
				device.manufacturer = "Bob";
				device.model = "test";
				string body = JsonConvert.SerializeObject(device);
				
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				
				var post = new WWW(ApiUrl("devices"), encoding.GetBytes(body), headers);
				yield return post;
							
				if (!String.IsNullOrEmpty(post.error)) {
					Debug.LogError("API: RegisterDevice() threw error: " + post.error);
					ret = "Network error";
					yield break;
				}

				var response = JsonConvert.DeserializeObject<SingleResponse<Device>>(post.text);
				
				device = response.response;
				device.self = true;
				db.StoreObject(device);
				Debug.Log("API: RegisterDevice(): device registered");
				
				ret = "Success";
			} finally {
				Platform.Instance.OnRegistration(ret);
			}
		}
		
		/// <summary>
		/// Coroutine to log in using Resource Owner Password Credentials flow.
		/// Triggers Platform.OnAuthentication upon completion.
		/// </summary>
		public IEnumerator Login(string username, string password) 
		{
			Debug.Log("API: Login(" + username + ",<password>)");
			string ret = "Failure";
			try {
				var form = new WWWForm();
	            form.AddField("grant_type", "password");
	            form.AddField("client_id", CLIENT_ID);
	            form.AddField("client_secret", CLIENT_SECRET);
	            form.AddField("username", username);
	            form.AddField("password", password);
				
				var post = new WWW(AUTH_TOKEN_URL, form);
				yield return post;
				
				if (!String.IsNullOrEmpty(post.error)) {
					Debug.LogError("API: Login(" + username + ",<password>) threw error: " + post.error);
					ret = "Failure";
					yield break;
				}
				
				var response = JsonConvert.DeserializeObject<OauthToken>(post.text);
				if (String.IsNullOrEmpty(response.access_token)) {
					Debug.LogError("API: Login(" + username + ",<password>) received an invalid response: " + post.text);
					ret = "Failure";
					yield break;
				}
				
				Debug.Log("API: Login(" + username + ",<password>) received an access token");
				
				token = response;
				IEnumerator e = UpdateAuthentications();
				while(e.MoveNext()) yield return e.Current;
				
				token.userId = user.id;
				var transaction = db.BeginTransaction();
				try {
					if (!db.UpdateObjectBy("userId", token)) {
						db.StoreObject(token);
					}
					transaction.Commit();
				} catch (Exception ex) {
					transaction.Rollback();
					throw ex;
				}
				
				ret = "Success";				
			} finally {
				Platform.Instance.OnAuthentication(ret);
			}
		}
		
		/// <summary>
		/// Coroutine to update user's third-party authentications list.
		/// </summary>
		public IEnumerator UpdateAuthentications() 
		{
			if (token == null || token.HasExpired) {
				Debug.LogError("API: UpdateAuthentications() called with expired or missing token");
				yield break;
			}
			Debug.Log("API: UpdateAuthentications()");
			
			var headers = new Hashtable();
			headers.Add("Authorization", "Bearer " + token.access_token);
			var request = new WWW(ApiUrl("me"), null, headers);
			yield return request;
			
			if (!String.IsNullOrEmpty(request.error)) {
				Debug.LogError("API: UpdateAuthentications(): threw error: " + request.error);
				yield break;
			}
			
			var account = JsonConvert.DeserializeObject<SingleResponse<Account>>(request.text);
			if (account.response.id <= 0) {
				Debug.LogError("API: UpdateAuthentications(): received an invalid response: " + request.text);
				yield break;
			}

			user = account.response;
			var transaction = db.BeginTransaction();
			try {			
				if (!db.UpdateObjectBy("id", user)) {
					db.StoreObject(user);
				}
				transaction.Commit();
			} catch (Exception ex) {
				transaction.Rollback();
				throw ex;
			}
			
			Debug.Log("API: UpdateAuthentications() fetched " + user.authentications.Count + " authentications");
		}
		
		/// <summary>
		/// Coroutine to sync the database to the server and back.
		/// Triggers Platform.OnSynchronization upon completion.
		/// </summary>
		public IEnumerator Sync() {
			if (token == null || token.HasExpired) {
				Debug.LogError("API: UpdateAuthentications() called with expired or missing token");
				yield break;
			}
			Debug.Log("API: Sync()");
			
			string ret = "Failure";
			try {
				DataWrapper wrapper = new DataWrapper();
				
				wrapper.data.devices = new List<Device>(1);				
				Device self = db.Cast<Device>().Where(d => d.self == true).FirstOrDefault();
				if (self == null) {
					// Register device
					IEnumerator e = RegisterDevice();
					while(e.MoveNext()) yield return e.Current;
					self = db.Cast<Device>().Where(d => d.self == true).First();
				}
				wrapper.data.devices.Add(self);
				
				string body = JsonConvert.SerializeObject(wrapper);
				
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				headers.Add("Authorization", "Bearer " + token.access_token);
				
				var post = new WWW(ApiUrl("sync/-1"), encoding.GetBytes(body), headers);
				yield return post;
							
				if (!String.IsNullOrEmpty(post.error)) {
					Debug.LogError("API: RegisterDevice() threw error: " + post.error);
					ret = "Network error";
					yield break;
				}

//				var response = JsonConvert.DeserializeObject<SingleResponse<Device>>(post.text);
				
				Debug.Log("API: Sync(): " + post.text);
				
				ret = "Failure";
			} finally {
				Platform.Instance.OnSynchronization(ret);
			}
		}
		
		private string ApiUrl(string path) 
		{
			return SCHEME + apiHost + "/api/1/" + path;
		}
				
		private class SingleResponse<T> 
		{
			public T response;
		}
				
		private class DataWrapper
		{
			public Data data = new Data();			
		}
		
		private class Data
		{
			public List<Device> devices;
		}
	}
	
}

