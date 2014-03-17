using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
		private const string SCHEME = "https://";
		private const string AUTH_HOST = "auth.raceyourself.com";
		private const string AUTH_TOKEN_URL = SCHEME + AUTH_HOST + "/oauth/token";
		private string apiHost = "auth.raceyourself.com"; // Note: might be supplied through auth load-balancer in future
			
		private const string CLIENT_ID = "8c8f56a8f119a2074be04c247c3d35ebed42ab0dcc653eb4387cff97722bb968";
		private const string CLIENT_SECRET = "892977fbc0d31799dfc52e2d59b3cba88b18a8e0080da79a025e1a06f56aa8b2";
		
		private readonly Regex CACHE_REGEXP = new Regex(".*max-age=(?<maxage>[0-9]*).*");
		private readonly string CACHE_PATH = Path.Combine(Application.persistentDataPath, "www-cache");
		
		private Siaqodb db;
		
		private OauthToken token = null;
		public User user { get; private set; }
		
		private bool syncing = false;
		
		public API(Siaqodb database) 
		{
			Debug.Log("API: created");
			db = database;
			user = db.Cast<User>().SingleOrDefault();
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
			Directory.CreateDirectory(CACHE_PATH);
			Device self = db.Query<Device>().Where(d => d.self == true).FirstOrDefault();
			if (self == null) {
				self = Platform.Instance.DeviceInformation();
				db.StoreObject(self);
				Debug.Log("API: Unregistered device");
			} else {
				if (self.id != 0) Debug.Log("API: Device registered as " + self.id);
				else Debug.Log("API: Device waiting to be registered");
			}
		}
		
		/// <summary>
		/// Coroutine to retrieve a globally unique device id from the server.
		/// Triggers Platform.OnRegistration upon completion.
		/// </summary>
		public IEnumerator RegisterDevice(Device device) {
			Debug.Log("API: RegisterDevice()");
			
			string ret = "Failure";
			try {
				string body = JsonConvert.SerializeObject(device);
				
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				// TODO: Shortcircuit linkage to user by adding Authorization if available
				
				var post = new WWW(ApiUrl("devices"), encoding.GetBytes(body), headers);
				yield return post;
							
				if (!post.isDone) {}
				
				if (!String.IsNullOrEmpty(post.error)) {
					Debug.LogError("API: RegisterDevice() threw error: " + post.error);
					ret = "Network error";
					yield break;
				}

				var response = JsonConvert.DeserializeObject<SingleResponse<Device>>(post.text);
				
				db.Delete(device);
				device = response.response;
				device.self = true;
				db.StoreObject(device);
				Debug.Log("API: RegisterDevice(): device registered as " + device.id);
				
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
						
				if (!post.isDone) {}
				
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
				db.Flush();
				
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
					
			if (!request.isDone) {}
			
			if (!String.IsNullOrEmpty(request.error)) {
				Debug.LogError("API: UpdateAuthentications(): threw error: " + request.error);
				yield break;
			}
			
			var account = JsonConvert.DeserializeObject<SingleResponse<User>>(request.text);
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
				Platform.Instance.OnSynchronization("Failure");
				yield break;
			}
			Debug.Log("API: Sync()");
			var start = DateTime.Now;
			
			string ret = "Failure";
			try {
				if (syncing) {
					Debug.Log("API: Sync() already syncing");
					yield break;
				}
				syncing = true;
				
				SyncState state = db.Cast<SyncState>().FirstOrDefault();
				if (state == null) state = new SyncState(0);
				Debug.Log ("API: Sync() " + "head: " + state.sync_timestamp
						+ " tail: " + state.tail_timestamp + "#" + state.tail_skip);
				
				// Register device if it doesn't have an id
				Device self = db.Cast<Device>().Where(d => d.self == true).First();
				if (self.id <= 0) {
					IEnumerator e = RegisterDevice(self);
					while(e.MoveNext()) yield return e.Current;
					self = db.Cast<Device>().Where(d => d.self == true).First();
				}
				var gatherStart = DateTime.Now;
				DataWrapper wrapper = new DataWrapper(db, self);
				Debug.Log("API: Sync() gathered " + wrapper.data.ToString() + " in " + (DateTime.Now - gatherStart));
				
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				headers.Add("Accept-Charset", "utf-8");
				headers.Add("Accept-Encoding", "gzip");
				headers.Add("Authorization", "Bearer " + token.access_token);				
				
				byte[] body = encoding.GetBytes(JsonConvert.SerializeObject(wrapper));
				Debug.Log("API: Sync() pushing " + (body.Length/1000) + "kB");
				
				var post = new WWW(ApiUrl("sync/" + state.sync_timestamp), body, headers);
				yield return post;
							
				if (!post.isDone) {}
						
				if (!String.IsNullOrEmpty(post.error)) {
					Debug.LogError("API: RegisterDevice() threw error: " + post.error);
					if (post.error.ToLower().Contains("401 unauthorized")) {
						db.Delete(token);
						token = null;
						ret = "Unauthorized";
					} else {
						ret = "Network error";
					}
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
				
				// Run slow ops in a background thread
				var bytes = post.bytes;				
				Thread backgroundThread = new Thread(() => {				
					ResponseWrapper response = null;
					var parseStart = DateTime.Now;
					try {
						string responseBody = null;
						if (responseEncoding.ToLower().Equals("gzip")) responseBody = encoding.GetString(Ionic.Zlib.GZipStream.UncompressBuffer(bytes));
						else responseBody = encoding.GetString(bytes);
						response = JsonConvert.DeserializeObject<ResponseWrapper>(responseBody);
					} catch (Exception ex) {
						ret = "Failure";
						Debug.LogError("API: Sync() threw exception " + ex.ToString());
						throw ex;
					}
					Debug.Log("API: Sync() parsed " + response.response.ToString() + " in " + (DateTime.Now - parseStart));
					if (response.response.errors.Count > 0) {
						Debug.LogError("API: Sync(): server reported " + response.response.errors.Count + " errors: " + string.Join("\n", response.response.errors.ToArray()));
					}
					
					var transaction = db.BeginTransaction();
					try {
						wrapper.data.flush(db);
						response.response.persist(db);
						transaction.Commit();
					} catch (Exception ex) {
						ret = "Failure";
						Debug.LogError("API: Sync() threw exception " + ex.ToString());
						transaction.Rollback();
						throw ex;
					}
					
					if (response.response.tail_timestamp.HasValue && response.response.tail_timestamp.Value > 0) ret = "partial";
					else ret = "full";
				});
				backgroundThread.Start();
				while (backgroundThread.IsAlive) yield return null;
				
				if (ret.Equals("full") || ret.Equals("partial")) Debug.Log("API: Sync() completed successfully in " + (DateTime.Now - start));
				
			} finally {
				syncing = false;
				Platform.Instance.OnSynchronization(ret);
			}
		}

		/// <summary>
		/// Fetch cached data from API
		/// Returns cached data or null if missing
		/// </summary>
		public string getCached(string route) {
			var cache = db.Cast<Models.Cache>().Where<Models.Cache>(c => c.id.Equals(route)).FirstOrDefault();
			if (cache == null || cache.Expired) return null;
			try {
				return File.ReadAllText(Path.Combine(CACHE_PATH, Regex.Replace(route, "[^a-zA-Z0-9._-]", "_")));
			} catch (Exception ex) {
				Debug.LogError("API: Cache get threw: " + ex.ToString());
				db.Delete(cache);
				return null;
			}
		}
		
		/// <summary>
		/// Coroutine to fetch data from API.
		/// Response string or null if failed returned through callback.
		/// </summary>
		public IEnumerator get(string route, Action<string> callback, bool checkCache=true) {
			if (checkCache) {
				var cached = getCached(route);
				if (cached != null) {
					callback(cached);
					yield break;
				}
			}
			
			if (token == null || token.HasExpired) {
				callback(null);
				yield break;
			}
			var start = DateTime.Now;
			var headers = new Hashtable();
			headers.Add("Accept-Charset", "utf-8");
			headers.Add("Accept-Encoding", "gzip");
			headers.Add("Authorization", "Bearer " + token.access_token);
			
			var www = new WWW(ApiUrl(route), null, headers);
			yield return www;
			
			while (!www.isDone) {}
				
			if (!String.IsNullOrEmpty(www.error)) {
				Debug.LogError("API: get(" + route + ") threw error: " + www.error);
				if (www.error.ToLower().Contains("401 unauthorized")) {
					db.Delete(token);
					token = null;
				}
				callback(null);
				yield break;
			}			
			
			string responseEncoding = "uncompressed";
			string cacheControl = "no-cache";
			long maxAge = 60;
			foreach (string key in www.responseHeaders.Keys) {
				if (key.ToLower().Equals("content-encoding")) {
					responseEncoding = www.responseHeaders[key];
				}
				if (key.ToLower().Equals("cache-control")) {
					cacheControl = www.responseHeaders[key];
					Match matches = CACHE_REGEXP.Match(cacheControl);
					if (matches.Success) {
						maxAge = long.Parse(matches.Groups["maxage"].Value);
						if (maxAge < 60) maxAge = 60; // TODO: Fix server cache-control responses
					}
				}
			}
			
			var encoding = new System.Text.UTF8Encoding();
			string body = null;			
			if (responseEncoding.ToLower().Equals("gzip")) body = encoding.GetString(Ionic.Zlib.GZipStream.UncompressBuffer(www.bytes));
			else body = encoding.GetString(www.bytes);
			
			Models.Cache cache = new Models.Cache(route, maxAge);
			if (maxAge > 0) {
				if (!db.UpdateObjectBy("id", cache)) {
					db.StoreObject(cache);
				}
				try {
					File.WriteAllText(Path.Combine(CACHE_PATH, Regex.Replace(route, "[^a-zA-Z0-9._-]", "_")), body);
					Debug.Log("API: get(" + route + ") cached for " + maxAge + "s");
				} catch (Exception ex) {
					Debug.LogError("API: Cache store threw: " + ex.ToString());
					db.Delete(cache);
				}
			} else {
				db.DeleteObjectBy("id", cache);
			}
			
			Debug.Log("API: get(" + route + ") returned in " + (DateTime.Now - start));
			callback(body);
		}
		
		private string ApiUrl(string path) 
		{
			return SCHEME + apiHost + "/api/1/" + path;
		}
				
		public class SingleResponse<T> 
		{
			public T response;
		}
		
		public class ListResponse<T>
		{
			public List<T> response;
		}
				
		private class DataWrapper
		{
			public Data data;
			
			public DataWrapper(Siaqodb db, Device self) {
				data = new Data(db, self);	
			}
		}
		
		private class Data
		{
			public List<Models.Device> devices;
			public List<Models.Track> tracks;
			public List<Models.Position> positions;
			public List<Models.Orientation> orientations;
			public List<Models.Notification> notifications;
			public List<Models.Transaction> transactions;
			public List<Models.Action> actions;
			public List<Models.Event> events;
			
			public Data(Siaqodb db, Device self) {
				devices = new List<Models.Device>(db.LoadAll<Models.Device>());
				tracks = new List<Models.Track>(db.Cast<Models.Track>().Where<Models.Track>(t => t.dirty == true));
				positions = new List<Models.Position>(db.Cast<Models.Position>().Where<Models.Position>(p => p.dirty == true));
				orientations = new List<Models.Orientation>(db.Cast<Models.Orientation>().Where<Models.Orientation>(o => o.dirty == true));
				notifications = new List<Models.Notification>(db.Cast<Models.Notification>().Where<Models.Notification>(n => n.dirty == true));
				transactions = new List<Models.Transaction>(db.Cast<Models.Transaction>().Where<Models.Transaction>(t => t.dirty == true));
				actions = new List<Models.Action>(db.LoadAll<Models.Action>());
				events = new List<Models.Event>(db.LoadAll<Models.Event>());
				
				// Populate device_id
				foreach (Models.Track track in tracks) {
					track.deviceId = self.id;
				}
				foreach (Models.Position pos in positions) {
					pos.deviceId = self.id;
				}
				foreach (Models.Orientation o in orientations) {
					o.deviceId = self.id;
				}
				foreach (Models.Event e in events) {
					e.deviceId = self.id;
				}
				// Objects are stored by OID in flush
			}
			
			public void flush(Siaqodb db) {
				var start = DateTime.Now;
				uint updates, deletes;
				updates = deletes = 0;
				// Reset dirty flag and remove soft-deleted models
				foreach (Models.Track track in tracks) {
					if (track.deleted_at.HasValue) {
						db.Delete(track);
						deletes++;
						continue;
					}
					track.dirty = false;
					track.GenerateCompositeId(); 
					db.StoreObject(track); // Store non-transient object by OID
					updates++;
				}
				foreach (Models.Position p in positions) {
					if (p.deleted_at.HasValue) {
						db.Delete(p);
						deletes++;
						continue;
					}
					p.dirty = false;
					p.GenerateCompositeId(); 
					db.StoreObject(p); // Store non-transient object by OID
					updates++;
				}
				foreach (Models.Orientation o in orientations) {
					if (o.deleted_at.HasValue) {
						db.Delete(o);
						deletes++;
						continue;
					}
					o.dirty = false;
					o.GenerateCompositeId(); 
					db.StoreObject(o); // Store non-transient object by OID
					updates++;
				}
				foreach (Models.Notification n in notifications) {
					n.dirty = false;
					db.StoreObject(n); // Store non-transient object by OID
					updates++;
				}
				foreach (Models.Transaction t in transactions) {
					if (t.deleted_at.HasValue) {
						db.Delete(t);
						deletes++;
						continue;
					}
					t.dirty = false;
					t.GenerateCompositeId(); 
					db.StoreObject(t); // Store non-transient object by OID
					updates++;
				}
				
				// Remove fire and forget model
				foreach (Models.Action action in actions) {
					db.Delete(action);
					deletes++;
				}
				foreach (Models.Event e in events) {
					db.Delete(e);
					deletes++;
				}
				Debug.Log("API: Sync: flushed old data: " + updates + " updates, " + deletes + " deletes in " + (DateTime.Now - start));				
			}
			
			public override string ToString()
			{
				return (LengthOrNull(devices) + " devices, "
						+ LengthOrNull(tracks) + " tracks, "
						+ LengthOrNull(positions) + " positions, "
						+ LengthOrNull(orientations) + " orientations, "
						+ LengthOrNull(notifications) + " notifications, "
						+ LengthOrNull(transactions) + " transactions, "
						+ LengthOrNull(actions) + " actions, "
						+ LengthOrNull(events) + " events");
			}
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
			
			public List<string> errors = new List<string>();
			
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
				var start = DateTime.Now;
				uint inserts, updates, deletes;
				inserts = updates = deletes = 0;
				
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
						// TODO: Move to <model>.save(db)?
						if (!db.UpdateObjectBy("id", device)) {
							db.StoreObject(device);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Device));
				}
				
				if (friends != null) {
					db.StartBulkInsert(typeof(Models.Friend));
					foreach (Models.Friendship friendship in friends) {
						if (friendship.deleted_at != null) {
							if (db.DeleteObjectBy("guid", friendship.friend)) deletes++;
							continue;
						}
						if (!db.UpdateObjectBy("guid", friendship.friend)) {
							db.StoreObject(friendship.friend);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Friend));
				}
				
				if (challenges != null) {
					db.StartBulkInsert(typeof(Models.Challenge));
					foreach (Models.Challenge challenge in challenges) {
						if (!db.UpdateObjectBy("id", challenge)) {
							db.StoreObject(challenge);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Challenge));
				}
				
				if (tracks != null) {
					db.StartBulkInsert(typeof(Models.Device));
					foreach (Models.Track track in tracks) {
						track.GenerateCompositeId();
						if (track.deleted_at != null) {
							db.DeleteObjectBy("id", track);
							continue;
						}
						if (!db.UpdateObjectBy("id", track)) {
							db.StoreObject(track);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Device));
				}
				
				if (positions != null) {
					db.StartBulkInsert(typeof(Models.Position));
					foreach (Models.Position position in positions) {
						position.GenerateCompositeId();
						if (position.deleted_at != null) {
							if (db.DeleteObjectBy("id", position)) deletes++;
							continue;
						}
						if (!db.UpdateObjectBy("id", position)) {
							db.StoreObject(position);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Position));
				}
				
				if (orientations != null) {
					db.StartBulkInsert(typeof(Models.Orientation));
					foreach (Models.Orientation orientation in orientations) {
						orientation.GenerateCompositeId();
						if (orientation.deleted_at != null) {
							if (db.DeleteObjectBy("id", orientation)) deletes++;
							continue;
						}							
						if (!db.UpdateObjectBy("id", orientation)) {
							db.StoreObject(orientation);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Orientation));
				}
				
				if (notifications != null) {
					db.StartBulkInsert(typeof(Models.Notification));
					foreach (Models.Notification notification in notifications) {
						if (!db.UpdateObjectBy("id", notification)) {
							db.StoreObject(notification);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Notification));
				}
				
				if (transactions != null) {
					db.StartBulkInsert(typeof(Models.Transaction));
					foreach (Models.Transaction gtransaction in transactions) {
						gtransaction.GenerateCompositeId();
						if (gtransaction.deleted_at != null) {
							if (db.DeleteObjectBy("id", gtransaction)) deletes++;
							continue;
						}
						if (!db.UpdateObjectBy("id", gtransaction)) {
							db.StoreObject(gtransaction);
							inserts++;
						} else updates++;
					}
					db.EndBulkInsert(typeof(Models.Transaction));
				}
				
				db.StoreObject(state);
				Debug.Log("API: Sync: persisted new data: " + inserts + " inserts, " + updates + " updates, " + deletes + " deletes in " + (DateTime.Now - start));
			}
		}
		
		private static string LengthOrNull(IList list) {
			if (list == null) return "null";
			return list.Count.ToString();
		}
	}
	
}

