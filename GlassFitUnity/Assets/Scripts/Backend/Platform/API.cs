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
		
		private bool syncing = false;
		
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
				if (syncing) {
					Debug.Log("API: Sync() already syncing");
					yield break;
				}
				syncing = true;
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
								
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				headers.Add("Accept-Encoding", "gzip");
				headers.Add("Authorization", "Bearer " + token.access_token);				
				
				byte[] body = encoding.GetBytes(JsonConvert.SerializeObject(wrapper));
				Debug.Log("API: Sync() pushing " + (body.Length/1000) + "kB");
				
				var post = new WWW(ApiUrl("sync/0"), body, headers);
				yield return post;
							
				if (!String.IsNullOrEmpty(post.error)) {
					Debug.LogError("API: RegisterDevice() threw error: " + post.error);
					ret = "Network error";
					yield break;
				}
				string responseEncoding = "uncompressed";
				foreach (string key in post.responseHeaders.Keys) {
					if (key.ToLower().Equals("content-encoding")) {
						responseEncoding = post.responseHeaders[key];
						break;
					}
				}
				Debug.Log("API: Sync() received " + (post.size/1000) + "kB " + responseEncoding);
				
				ResponseWrapper response = null;
				try {
					string responseBody = null;
					if (responseEncoding.ToLower().Equals("gzip")) responseBody = encoding.GetString(Ionic.Zlib.GZipStream.UncompressBuffer(post.bytes));
					else responseBody = post.text;
					Debug.Log (responseBody);
					response = JsonConvert.DeserializeObject<ResponseWrapper>(responseBody);
				} catch (Exception ex) {
					ret = "Failure";
					Debug.Log("API: Sync() threw exception " + ex.ToString());
					throw ex;
				}
				Debug.Log("API: Sync() parsed " + response.response.ToString());
				
				response.response.persist(db);
				Debug.Log("API: Sync() persisted " + response.response.ToString());
				
				ret = "stuff";
			} finally {
				syncing = false;
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
		
		private class ResponseWrapper
		{
			public Response response;
		}
		
		private class Response
		{
			public long sync_timestamp;
			public long? tail_timestamp;
			public long? tail_skip;
			
			public List<Models.Device> devices;
			public List<Models.Friendship> friends;
			public List<Models.Challenge> challenges;
			public List<Models.Track> tracks;
			public List<Models.Position> positions;
			public List<Models.Orientation> orientations;
			public List<Models.Notification> notifications;
			public List<Models.Transaction> transactions;
			
			public List<string> errors;	
			
			public override string ToString()
			{
				return ("head: " + sync_timestamp
						+ " tail: " + tail_timestamp + "#" + tail_skip + "; "
					    + LengthOrNull(devices) + " devices, "
						+ LengthOrNull(friends) + " friends, "
						+ LengthOrNull(challenges) + " challenges, "
						+ LengthOrNull(tracks) + " tracks, "
						+ LengthOrNull(positions) + " positions, "
						+ LengthOrNull(orientations) + " orientations, "
						+ LengthOrNull(notifications) + " notifications, "
						+ LengthOrNull(transactions) + " transactions, "
						+ LengthOrNull(errors) + " errors");
			}
			
			public void persist(Siaqodb db) {
				var transaction = db.BeginTransaction();
				try {
					SyncState state = db.Cast<SyncState>().FirstOrDefault();
					if (state == null) {
						state = new SyncState(sync_timestamp, tail_timestamp, tail_skip);
					} else {
						state.sync_timestamp = sync_timestamp;
						state.tail_timestamp = tail_timestamp;
						state.tail_skip = tail_skip;
					}
										
					if (devices != null) {
						db.StartBulkInsert(typeof(Models.Device));
						foreach (Models.Device device in devices) {							
							if (!db.UpdateObjectBy("id", device)) {
								db.StoreObject(device);
							}
						}
						db.EndBulkInsert(typeof(Models.Device));
					}
					
					if (friends != null) {
						db.StartBulkInsert(typeof(Models.Friend));
						foreach (Models.Friendship friendship in friends) {
							if (friendship.deleted_at != null) {
								db.DeleteObjectBy("_id", friendship.friend);
								continue;
							}
							if (!db.UpdateObjectBy("_id", friendship.friend)) {
								db.StoreObject(friendship.friend);
							}
						}
						db.EndBulkInsert(typeof(Models.Friend));
					}
					
					if (challenges != null) {
						db.StartBulkInsert(typeof(Models.Challenge));
						foreach (Models.Challenge challenge in challenges) {
							if (!db.UpdateObjectBy("_id", challenge)) {
								db.StoreObject(challenge);
							}
						}
						db.EndBulkInsert(typeof(Models.Challenge));
					}
					
					if (tracks != null) {
						db.StartBulkInsert(typeof(Models.Device));
						foreach (Models.Track track in tracks) {
							if (track.deleted_at != null) {
								db.DeleteObjectBy("_id", track);
								continue;
							}
							// TODO: Generate composite keys before upsert
							if (!db.UpdateObjectBy("_id", track)) {
								db.StoreObject(track);
							}
						}
						db.EndBulkInsert(typeof(Models.Device));
					}
					
					if (positions != null) {
						db.StartBulkInsert(typeof(Models.Position));
						foreach (Models.Position position in positions) {
							if (position.deleted_at != null) {
								db.DeleteObjectBy("_id", position);
								continue;
							}
							// TODO: Generate composite keys before upsert
							if (!db.UpdateObjectBy("_id", position)) {
								db.StoreObject(position);
							}
						}
						db.EndBulkInsert(typeof(Models.Position));
					}
					
					if (orientations != null) {
						db.StartBulkInsert(typeof(Models.Orientation));
						foreach (Models.Orientation orientation in orientations) {
							if (orientation.deleted_at != null) {
								db.DeleteObjectBy("_id", orientation);
								continue;
							}
							// TODO: Generate composite keys before upsert
							if (!db.UpdateObjectBy("_id", orientation)) {
								db.StoreObject(orientation);
							}
						}
						db.EndBulkInsert(typeof(Models.Orientation));
					}
					
					if (notifications != null) {
						db.StartBulkInsert(typeof(Models.Notification));
						foreach (Models.Notification notification in notifications) {
							if (!db.UpdateObjectBy("_id", notification)) {
								db.StoreObject(notification);
							}
						}
						db.EndBulkInsert(typeof(Models.Notification));
					}
					
					if (transactions != null) {
						db.StartBulkInsert(typeof(Models.Transaction));
						foreach (Models.Transaction gtransaction in transactions) {
							if (gtransaction.deleted_at != null) {
								db.DeleteObjectBy("_id", gtransaction);
								continue;
							}
							if (!db.UpdateObjectBy("_id", gtransaction)) {
								db.StoreObject(gtransaction);
							}
						}
						db.EndBulkInsert(typeof(Models.Transaction));
					}
					
					db.StoreObject(state);
					transaction.Commit();
					db.Flush();
				} catch (Exception ex) {
					transaction.Rollback();
					throw ex;
				}
			}
		}
		
		private static string LengthOrNull(IList list) {
			if (list == null) return "null";
			return list.Count.ToString();
		}
	}
	
}

