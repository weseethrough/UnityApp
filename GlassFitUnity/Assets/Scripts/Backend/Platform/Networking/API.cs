using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Sqo;
using SiaqodbUtils;
using UnityEngine;
using RaceYourself.Models;
using System.Runtime.CompilerServices;

namespace RaceYourself
{
	/// <summary>
	/// Server API
	/// </summary>
	public class API
	{
/* To package an .apk for the app store, we need to use the production auth server. For testing, we have our sandbox.
 * As such, for the app store, define PRODUCTION.
 */
#if PRODUCTION
        private const string SCHEME = "https://";
        private const string AUTH_HOST = "api.raceyourself.com";
        private const string API_HOST = "api.raceyourself.com"; // Note: might be supplied through auth load-balancer in future
        private const string CLIENT_ID = "98f985cd4fca00aefda3f31c10b3d994eaa496d882fdf3db936fad76e4dae236";
        private const string CLIENT_SECRET = "9ca4b4f56b518deca0c2200b92a3435f05cb4e0b3d52b0a5a1608f39d004750e";
#elif LOCALHOST
        private const string SCHEME = "http://";
        private const string AUTH_HOST = "localhost:3000";
        private const string API_HOST = "localhost:3000"; // Note: might be supplied through auth load-balancer in future
        private const string CLIENT_ID = "e4585379c3f6627e5510c21f21af999da38c0bfff82066be1b3bca34efe6092f";
        private const string CLIENT_SECRET = "89ac25d58a314eca793b1597b477f4e38a2c470a5a1f45830043bab912d4bdd9";
#else
        private const string SCHEME = "http://";
        private const string AUTH_HOST = "a.staging.raceyourself.com";
        private const string API_HOST = "a.staging.raceyourself.com"; // Note: might be supplied through auth load-balancer in future
        private const string CLIENT_ID = "c9842247411621e35dbaf21ad0e15c263364778bf9a46b5e93f64ff2b6e0e17c";
        private const string CLIENT_SECRET = "75f3e999c01942219bea1e9c0a1f76fd24c3d55df6b1c351106cc686f7fcd819";
#endif

        private const string AUTH_TOKEN_URL = SCHEME + AUTH_HOST + "/oauth/token";

        private readonly Regex CACHE_REGEXP = new Regex(".*max-age=(?<maxage>[0-9]*).*");
		private readonly string CACHE_PATH = Path.Combine(Application.persistentDataPath, "www-cache");
		
		private Siaqodb db;
		private MonoBehaviour partner;
		
		public OauthToken token = null;
		public User user { get; private set; }
        public PlayerConfig playerConfig { get; private set; }

		private bool syncing = false;
		private bool matching = false;

		protected static Log log = new Log("API");  // for use by subclasses

		public API(Siaqodb database, MonoBehaviour partner) 
		{
			log.info("created");
			this.partner = partner;
            db = database;
			user = db.Query<User>().Where(u => u.self == true).FirstOrDefault();
            token = db.Query<OauthToken>().LastOrDefault();
            if (user != null && token != null) {
				if (user.id != token.userId) {
					log.error("Token in database does not belong to user in database!");
					// TODO: Allow storage of multiple users or delete old data on id mismatch?
					user = null;
					token = null;
				} else {
					if (!token.HasExpired) {
						log.info("Still logged in as " + user.DisplayName + "/" + user.id);
                        Platform.Instance.NetworkMessageListener.authenticated = true;
					}
				}
			} else {
				log.info("No stored account");
			}
			Directory.CreateDirectory(CACHE_PATH);
			Device self = db.Query<Device>().Where(d => d.self == true).FirstOrDefault();
			if (self == null) {
				self = Platform.Instance.DeviceInformation();
				log.info("Unregistered device: " + self.manufacturer + " " + self.model);
				db.StoreObject(self);
			} else {
				if (self.id != 0) log.info("Device registered as " + self.id);
				else log.info("Device waiting to be registered");
			}
		}

		private Coroutine StartCoroutine(IEnumerator routine) {
			return partner.StartCoroutine(routine);
		}

		/// <summary>
		/// Coroutine to retrieve a globally unique device id from the server.
		/// Triggers Platform.OnRegistration upon completion.
		/// </summary>
		public IEnumerator RegisterDevice(Device device) {
			log.info("RegisterDevice()");
			
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
					log.error("RegisterDevice() threw error: " + post.error);
					ret = "Network error";
					yield break;
				}

				var response = JsonConvert.DeserializeObject<SingleResponse<Device>>(post.text);

				var transaction = db.BeginTransaction();
				db.Delete(device, transaction);
				device = response.response;
				device.self = true;
				db.StoreObject(device, transaction);
				transaction.Commit();
				log.info("RegisterDevice(): device registered as " + device.id);
				
				ret = "Success";
			} finally {
                Platform.Instance.NetworkMessageListener.OnRegistration(ret);
			}
		}
		
		/// <summary>
		/// Coroutine to log in using Resource Owner Password Credentials flow.
		/// Triggers Platform.OnAuthentication upon completion.
		/// </summary>
		public IEnumerator Login(string username, string password)
        {
			log.info("Login(" + username + ",<password>)");
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
                    // info, because on mobile, the user may mistype their password - so potentially user error not app/network error.
                    log.info("Login(" + username + ",<password>) has errors: " + post.error);

                    if (post.error.StartsWith("401") || post.error.StartsWith("400"))
                    {
                        ret = "Failure";
                    }
                    else
                    {
                        ret = "CommsFailure";
                    }
					yield break;
				}
				
				var response = JsonConvert.DeserializeObject<OauthToken>(post.text);
				if (String.IsNullOrEmpty(response.access_token)) {
					log.error("Login(" + username + ",<password>) received an invalid response: " + post.text);
					ret = "Failure";
					yield break;
				}
				
				log.info("Login(" + username + ",<password>) received an access token");
				
				token = response;
				yield return StartCoroutine(UpdateAuthentications());

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
                Platform.Instance.NetworkMessageListener.OnAuthentication(ret);
			}
		}
		
		/// <summary>
		/// Coroutine to update user's third-party authentications list.
		/// </summary>
		public IEnumerator UpdateAuthentications() 
		{
			if (token == null || token.HasExpired) {
                log.info("UpdateAuthentications() called with expired or missing token (legit pre-sign in on mobile)");
				yield break;
			}
            log.info("UpdateAuthentications() " + token.access_token);
			
			var headers = new Hashtable();
			headers.Add("Authorization", "Bearer " + token.access_token);
            var request = new WWW(ApiUrl("me"), null, headers);

			yield return request;
			
			if (!request.isDone) {}
			
			if (!String.IsNullOrEmpty(request.error)) {
				log.error("UpdateAuthentications(): threw error: " + request.error);
				yield break;
			}
			
			var account = JsonConvert.DeserializeObject<SingleResponse<User>>(request.text);
			if (account.response.id <= 0) {
				log.error("UpdateAuthentications(): received an invalid response: " + request.text);
				yield break;
			}

			user = account.response;
			user.self = true;
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
			
			log.info("UpdateAuthentications() fetched " + user.authentications.Count + " authentications");
		}

		/// <summary>
		/// Coroutine to link a third-party service to the logged in user.
		/// </summary>
        public IEnumerator LinkProvider(ProviderToken ptoken, Action<string> callback) {
			// TODO: Update POST schema to use JSON path authentications/{provider, uid, access_token}
			log.info(string.Format("LinkProvider({0})", ptoken.provider));

			string ret = "Failed";
			try {
				string body = JsonConvert.SerializeObject(ptoken);
				
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				headers.Add("Authorization", "Bearer " + token.access_token);				

				var post = new WWW(ApiUrl("credentials"), encoding.GetBytes(body), headers);
				yield return post;
				
				if (!post.isDone) {}
				
				if (!String.IsNullOrEmpty(post.error)) {
					log.error(string.Format("LinkProvider({0}) threw error: {1}", ptoken.provider, post.error));
					ret = "Network error";
					yield break;
				}

				var account = JsonConvert.DeserializeObject<SingleResponse<User>>(post.text);
				if (account.response.id <= 0) {
					log.error(string.Format("LinkProvider({0}): received an invalid response: {1}", ptoken.provider, post.text));
					yield break;
				}
				
				user = account.response;
				user.self = true;

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

                ret = "Failed";
                foreach (Authentication auth in user.authentications)
                {
                    if (ptoken.provider == auth.provider)
                    {
                        ret = "Success";
                        break;
                    }
                }

                log.info(string.Format("LinkProvider({0}): {1}", ptoken.provider, ret));
            } finally
            {
                if (callback != null)
                    callback(ret);
            }
        }
        
		/// <summary>
		/// Coroutine to update a user's details.
		/// 
		/// Null values in explicit arguments are not sent.
		/// Null values inside profile object cause that key to be deleted.
		/// </summary>
		public IEnumerator UpdateUser(string username, string name, string image, char? gender, int? timezone, Hashtable profile) {
			log.info(string.Format("UpdateUser({0})", user.email));
			
			string ret = "Failed";
			try {
				var data = new Hashtable();
				if (username != null) data.Add("username", username);
				if (name != null) data.Add("name", name);
				if (image != null) data.Add("image", image);
				if (gender != null) data.Add("gender", gender);
				if (timezone.HasValue) data.Add("timezone", timezone.Value);
				if (profile != null) data.Add("profile", profile);
				string body = JsonConvert.SerializeObject(data);
				
				var encoding = new System.Text.UTF8Encoding();			
				var headers = new Hashtable();
				headers.Add("Content-Type", "application/json");
				headers.Add("Authorization", "Bearer " + token.access_token);				
				
				var post = new WWW(ApiUrl("credentials"), encoding.GetBytes(body), headers);
				yield return post;
				
				if (!post.isDone) {}
				
				if (!String.IsNullOrEmpty(post.error)) {
					log.error(string.Format("UpdateUser({0}) threw error: {1}", user.email, post.error));
					ret = "Network error";
					yield break;
				}
				
				var account = JsonConvert.DeserializeObject<SingleResponse<User>>(post.text);
				if (account.response.id <= 0) {
					log.error(string.Format("UpdateUser({0}): received an invalid response: {1}", user.email, post.text));
					yield break;
				}
				
				user = account.response;
				user.self = true;
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
				
				log.info(string.Format("UpdateUser({0}): succeeded", user.email));
				
				ret = "Success";
			} finally {
			}
		}
		
		/// <summary>
		/// Coroutine to sign up.
		/// </summary>
		public IEnumerator SignUp(string email, string password, string inviteCode, Profile profile, ProviderToken authentication,
                                  Action<bool, Dictionary<string, IList<string>>> callback)
		{
			log.info("SignUp()");
			var encoding = new System.Text.UTF8Encoding();			
			var headers = new Hashtable();
			headers.Add("Content-Type", "application/json");
			headers.Add("Accept-Charset", "utf-8");
			headers.Add("Accept-Encoding", "gzip");

            SignUpRequest wrapper = new SignUpRequest(email, password, inviteCode, profile, authentication);

			byte[] body = encoding.GetBytes(JsonConvert.SerializeObject(wrapper));
			
			var post = new WWW(ApiUrl("sign_up"), body, headers);
			yield return post;
			
			if (!post.isDone) {}
			
			if (!String.IsNullOrEmpty(post.error)) {
				log.error("SignUp() threw error: " + post.error);
                callback(false, new Dictionary<string, IList<string>>() {{"network", new List<string> {post.error}}});
				yield break;
			}

			string responseEncoding = "uncompressed";
			foreach (string key in post.responseHeaders.Keys) {
				if (key.ToLower().Equals("content-encoding") && post.responseHeaders[key] != null) {
					responseEncoding = post.responseHeaders[key];
					break;
				}
				string headerString = post.responseHeaders[key].ToString();
				//log.info("Sync response header: " + key + ":" + headerString);
            }
            
			string responseBody = null;
            if (responseEncoding.ToLower().Equals("gzip")) responseBody = encoding.GetString(DecompressGZipBuffer(post.bytes));
			else responseBody = encoding.GetString(post.bytes);
            
			var response = JsonConvert.DeserializeObject<SingleResponse<SignUpResponse>>(responseBody);
            if (!response.response.success) {
                var errors = new System.Text.StringBuilder();
                foreach (KeyValuePair<string, IList<string>> entry in response.response.errors)
                {
                    foreach (string v in entry.Value)
                    {
                        errors.Append(entry.Key);
                        errors.Append(" ");
                        errors.AppendLine(v);
                    }
                }

				log.error("SignUp() failed. Errors: " + errors);
				callback(false, response.response.errors);
				yield break;
			}

			if (response.response.user != null) {
				user = response.response.user;
				user.self = true;
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
			}

			log.info("SignUp() was successful");
			callback(true, null);
		}
		
		/// <summary>
		/// Coroutine to upload a profile image to the server.
		/// </summary>
		public IEnumerator UploadProfileImage(byte[] image, Action<string> callback) {
			log.info("UploadProfileImage");
			
			string ret = "Failed";
			try {
				WWWForm form = new WWWForm();
				form.AddBinaryData("image", image, "ignored.jpg", "image/jpeg");

				var data = form.data;
				var headers = form.headers;
				headers.Add("Authorization", "Bearer " + token.access_token);				
				
				var post = new WWW(ApiUrl("credentials"), form.data, headers);
				yield return post;
				
				if (!post.isDone) {}
				
				if (!String.IsNullOrEmpty(post.error)) {
					log.error(string.Format("UploadProfileImage threw error: {0}", post.error));
					ret = "Network error";
					yield break;
				}
				
				ret = "Success";
				log.info(string.Format("UploadProfileImage: {0}", ret));
			} finally
			{
				if (callback != null)
					callback(ret);
			}
		}
		
		/// <summary>
		/// Coroutine to sync the database to the server and back.
		/// Triggers Platform.OnSynchronization upon completion.
		/// </summary>
        public IEnumerator Sync() {
            if (token == null || token.HasExpired) {
                log.info("UpdateAuthentications() called with expired or missing token (legit pre-sign in on mobile)");
                Platform.Instance.NetworkMessageListener.OnSynchronization("Failure");
                yield break;
            }
            log.info("Sync()");
            var start = DateTime.Now;
            
            if (syncing) {
                log.info("Sync() already syncing");
                yield break;
            }
            
            string ret = "Failure";
            try {
                syncing = true;
                
                var encoding = new System.Text.UTF8Encoding();          
                SyncState state = db.Query<SyncState>().LastOrDefault();
                if (state == null) state = new SyncState(0);
                Debug.Log ("API: Sync() " + "head: " + state.sync_timestamp
                           + " tail: " + state.tail_timestamp + "#" + state.tail_skip);
                
                // Register device if it doesn't have an id
                Device self = db.Query<Device>().Where(d => d.self == true).First();
                if (self.id <= 0) {
                    yield return StartCoroutine(RegisterDevice(self));
                    self = db.Query<Device>().Where(d => d.self == true).First();
                }
                DataWrapper wrapper = null;
                byte[] body = null;
                Thread backgroundThread = new Thread(() => {
                    var gatherStart = DateTime.Now;
                    wrapper = new DataWrapper(db, self);
                    log.info("Sync() gathered " + wrapper.data.ToString() + " in " + (DateTime.Now - gatherStart));
                    
                    var encodeStart = DateTime.Now;
                    body = encoding.GetBytes(JsonConvert.SerializeObject(wrapper));
                    log.info("Sync() encoded push data in " + (DateTime.Now - encodeStart));
                });
                backgroundThread.Priority = System.Threading.ThreadPriority.Lowest;
                backgroundThread.Start();
                while (backgroundThread.IsAlive) yield return new WaitForSeconds(0.1f);
                
                var headers = new Hashtable();
                headers.Add("Content-Type", "application/json");
                headers.Add("Accept-Charset", "utf-8");
                headers.Add("Accept-Encoding", "gzip");
                headers.Add("Authorization", "Bearer " + token.access_token);               
                
                log.info("Sync() pushing " + (body.Length/1000) + "kB");
                
                var post = new WWW(ApiUrl("sync/" + state.sync_timestamp), body, headers);
                yield return post;
                
                if (!post.isDone) {}
                
                if (!String.IsNullOrEmpty(post.error)) {
                    log.error("Sync() threw error: " + post.error);
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
                    if (key.ToLower().Equals("content-encoding") && post.responseHeaders[key] != null) {
                        responseEncoding = post.responseHeaders[key];
                        break;
                    }
                    string headerString = post.responseHeaders[key].ToString();
                    //log.info("Sync response header: " + key + ":" + headerString);
                }
                log.info("Sync() received " + (post.bytesDownloaded/1000) + "kB " + responseEncoding);
                
                // Run slow ops in a background thread
                var bytes = post.bytes;             
                backgroundThread = new Thread(() => {
                    ResponseWrapper response = null;
                    var parseStart = DateTime.Now;
                    try {
                        string responseBody = null;
                        if(bytes == null) {
                            throw new Exception("Sync() bytes is null");
                        }
                        
                        //log.info("Sync() response body is " + encoding.GetString(bytes));
                        
                        if (responseEncoding.ToLower().Equals("gzip")) responseBody = encoding.GetString(DecompressGZipBuffer(bytes));
                        else responseBody = encoding.GetString(bytes);
                        
                        if(responseBody == null) {
                            throw new Exception("Sync() responsebody is null");
                        }
                        
                        
                        response = JsonConvert.DeserializeObject<ResponseWrapper>(responseBody);
                    } catch (Exception ex) {
                        ret = "Failure";
                        log.error("Sync() threw exception " + ex.ToString());
                        log.error(ex, "Sync() exception stack");
                        throw ex;
                    }
                    log.info("Sync() parsed " + response.response.ToString() + " in " + (DateTime.Now - parseStart));
                    if (response.response.errors.Count > 0) {
                        log.error("Sync(): server reported " + response.response.errors.Count + " errors: " + string.Join("\n", response.response.errors.ToArray()));
                    }
                    
                    //                    PlayerConfig cfg = null;
                    //                    IEnumerator e = api.get("configurations/unity", (body) => {
                    //                        cfg = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.PlayerConfig>>(body).response;
                    //                        var payload = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.ConfigurationPayload>>(cfg.configuration).response;
                    //                        cfg.payload = payload;
                    //                    });
                    //                    while(e.MoveNext()) {}; // block until finished
                    //                    playerConfig = cfg;
                    
                    var transaction = db.BeginTransaction();
                    try {
                        wrapper.data.flush(db);
                        response.response.persist(db);
                        transaction.Commit();
                    } catch (Exception ex) {
                        ret = "Failure";
                        log.error("Sync() threw exception " + ex.ToString());
                        transaction.Rollback();
                        throw ex;
                    }
                    
                    if (response.response.tail_timestamp.HasValue && response.response.tail_timestamp.Value > 0) ret = "partial";
                    else ret = "full";
                });
                backgroundThread.Priority = System.Threading.ThreadPriority.Lowest;
                backgroundThread.Start();
                while (backgroundThread.IsAlive) yield return new WaitForSeconds(0.25f);
                
                if (db.Query<Parameter>().Where(p => p.key == "matches" && p.value == "fetched").FirstOrDefault() == null) {
                    StartCoroutine(PopulateAutoMatchMatrix());
                }
            } finally {             
                if (ret.Equals("full") || ret.Equals("partial")) {
                    log.info("Sync() completed successfully in " + (DateTime.Now - start));
                } else {
                    log.error("Sync() failed with " + ret + " after " + (DateTime.Now - start));
                }
                syncing = false;
                Platform.Instance.NetworkMessageListener.OnSynchronization(ret);
            }
        }
		/// <summary>
		/// Populates the auto-match matrix from the app bundle.
		/// </summary>
        public void PopulateAutoMatchMatrixFromBundle() {
            if (db.Query<TrackBucket>().FirstOrDefault() != null) return; // Do not overwrite existing values
			log.warning("PopulateAutoMatchMatrixFromBundle");
            
// TODO: Remove old logic and default_matches.json when stable
// TODO: Fix new logic: populate db on load or make sure we can trigger a reload of all db handles
#if true
			// Populate from json
			var body = Platform.Instance.ReadAssetsString("default_matches.json");
			Dictionary<string,Dictionary<string,List<Track>>> matches = JsonConvert.DeserializeObject<
				RaceYourself.API.SingleResponse<Dictionary<string,Dictionary<string,List<Track>>>>>(body).response;

			Type[] types = new Type[] {typeof(Track), typeof(Position), typeof(TrackBucket)};
			db.StartBulkInsert(types);
			foreach (var bucket in db.LoadAll<TrackBucket>()) db.Delete(bucket);
			foreach (var match in db.LoadAll<MatchedTrack>()) db.Delete(match);

			foreach (var fitnessLevel in matches.Keys) {
				var buckets = matches[fitnessLevel];
				foreach (var duration in buckets.Keys) {
					if (buckets[duration].Count == 0) {
						log.error("Track bucket[" + fitnessLevel + "," + duration + "] is empty!");
					}
					var tracks = new List<Track>();
					foreach (var track in buckets[duration]) {
						track.GenerateCompositeId();
						if (!db.UpdateObjectBy("id", track)) db.StoreObject(track);
						foreach (var position in track.positions) {
							position.GenerateCompositeId();
							if (!db.UpdateObjectBy("id", position)) db.StoreObject(position);
                        }
                        tracks.Add(track);
					}
					TrackBucket bucket = new TrackBucket(fitnessLevel, int.Parse(duration), tracks);
					db.StoreObject(bucket);
				}
			}
			db.EndBulkInsert(types);
			log.warning("Populated " + db.Count<TrackBucket>() + " buckets from json");
			log.warning("Populated " + db.Count<Track>() + " tracks from json");
			log.warning("Populated " + db.Count<Position>() + " positions from json");
            #else
			// Populate from read-only database
			var tracks = new List<Track>(db.LoadAll<Track>()); // Merge
			lock (db) {
				db.DropType(typeof(MatchedTrack)); // Clear
				db.DropType(typeof(Track));
				db.DropType(typeof(TrackBucket)); // debug, should always be inexistant
	            DatabaseFactory.PopulateFromBundle("RaceYourself.Models.TrackBucket. Models.sqo"); // TrackBucket
				DatabaseFactory.PopulateFromBundle("RaceYourself.Models.Track. Models.sqo"); // Tracks
				// TODO: Track positions
				DatabaseFactory.ReIndex();
				db = DatabaseFactory.GetInstance();
				Platform.Instance.db = db; // TODO: Something less icky!
				// TODO: Everywhere else needs to refresh the connection too
				// NOTE: THIS DOES NOT WORK ATM
			}
			log.warning("Populated " + db.Count<TrackBucket>() + " buckets from bundle");
            log.warning("Populated " + db.Count<Track>() + " tracks from bundle");
            foreach (var track in tracks) {
				try {
					db.StoreObject(track);

				} catch (Exception e) { 
					// Ignore
				}
			}
#endif
			var p = new Parameter("matches", "bundled");
			if (!db.UpdateObjectBy("key", p)) db.StoreObject(p);
        }

		/// <summary>
		/// Populates the auto match matrix from the server.
		/// </summary>
		public IEnumerator PopulateAutoMatchMatrix() {
            if (matching) yield break;
			matching = true;
			yield return StartCoroutine(get("matches", body => {
				if (body == null) {
					matching = false;
					return;
				}

                Thread backgroundThread = new Thread(() => {
                    Dictionary<string,Dictionary<string,List<Track>>> matches = JsonConvert.DeserializeObject<
						RaceYourself.API.SingleResponse<Dictionary<string,Dictionary<string,List<Track>>>>>(body).response;

					Type[] types = new Type[] {typeof(Track), typeof(Position), typeof(TrackBucket)};
					db.StartBulkInsert(types);
					foreach (var bucket in db.LoadAll<TrackBucket>()) db.Delete(bucket);
					foreach (var match in db.LoadAll<MatchedTrack>()) db.Delete(match);
					
					foreach (var fitnessLevel in matches.Keys) {
						var buckets = matches[fitnessLevel];
						foreach (var duration in buckets.Keys) {
							if (buckets[duration].Count == 0) {
								log.error("Track bucket[" + fitnessLevel + "," + duration + "] is empty!");
                            }
							var tracks = new List<Track>();
							foreach (var track in buckets[duration]) {
								track.GenerateCompositeId();
								if (!db.UpdateObjectBy("id", track)) db.StoreObject(track);
								foreach (var position in track.positions) {
									position.GenerateCompositeId();
									if (!db.UpdateObjectBy("id", position)) db.StoreObject(position);
                                }
                                tracks.Add(track);
                            }
                            TrackBucket bucket = new TrackBucket(fitnessLevel, int.Parse(duration), tracks);
                            db.StoreObject(bucket);
                        }
                    }
                    db.EndBulkInsert(types);
					log.warning("Populated " + db.Count<TrackBucket>() + " buckets from network");
                    log.warning("Populated " + db.Count<Track>() + " tracks from network");
					log.warning("Populated " + db.Count<Position>() + " positions from network");

                    matching = false;
					var p = new Parameter("matches", "fetched");
					if (!db.UpdateObjectBy("key", p)) db.StoreObject(p);
				});
				backgroundThread.Priority = System.Threading.ThreadPriority.Lowest;
				backgroundThread.Start();
            }));
        }
            
        /// <summary>
        /// Return a bucket of tracks for auto-matching
		/// 
		/// Will block if db buckets haven't been populated.
		/// </summary>
		/// <returns>The track bucket</returns>
		/// <param name="fitnessLevel">Fitness level.</param>
		/// <param name="duration">Duration.</param>
		public TrackBucket AutoMatch(string fitnessLevel, int duration) {
			var start = DateTime.Now;
			TrackBucket bucket = db.Query<TrackBucket>().Where(tb => tb.fitnessLevel == fitnessLevel && tb.duration == duration).FirstOrDefault();
			if (bucket == null) {
				log.error("Auto-match matrix is empty! Populating from bundle");
				PopulateAutoMatchMatrixFromBundle();
				return AutoMatch(fitnessLevel, duration);
			}

			// Populate unmatched track list
			bucket.tracks = new List<Track>(bucket.all.Count);
			foreach (var track in bucket.all) {
                                int deviceId = track.deviceId;
                int trackId = track.trackId;

                if (db.Query<MatchedTrack>().Where(mt => mt.deviceId == deviceId && mt.trackId == trackId).FirstOrDefault() == null) {
					track.positions = new List<Position>(db.Query<Position>().Where(p => p.deviceId == deviceId && p.trackId == trackId));
					bucket.tracks.Add(track);
				}
			}

			log.info(string.Format("AutoMatch({0},{1}): gathered in " + (DateTime.Now - start), fitnessLevel, duration));
			return bucket;
		}

		/// <summary>
		/// Marks an auto-matched track as matched so that it isn't matched again.
		/// </summary>
		/// <returns><c>true</c>, if track was marked as matched, <c>false</c> otherwise.</returns>
		/// <param name="track">Track</param>
		public bool MarkTrackAsMatched(Track track) {
			return MarkTrackAsMatched(track.deviceId, track.trackId);
		}

		/// <summary>
		/// Marks an auto-matched track as matched so that it isn't matched again.
		/// </summary>
		/// <returns><c>true</c>, if track was marked as matched, <c>false</c> otherwise.</returns>
		/// <param name="deviceId">Track device identifier.</param>
		/// <param name="trackId">Track track identifier.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool MarkTrackAsMatched(int deviceId, int trackId) {
			// Always queue action as server db might have been cleared
			Platform.Instance.QueueAction(string.Format(@"{
                                                          'action' : 'matched_track',
                                                          'device_id' : '{0}',
                                                          'track_id' : '{1}'
                                                        }".Replace("'", "\""), deviceId, trackId));

			if (db.Query<MatchedTrack>().Where(mt => mt.deviceId == deviceId && mt.trackId == trackId).FirstOrDefault() != null) return false;

			db.StoreObject(new MatchedTrack(deviceId, trackId));

			return true;
		}
		
		/// <summary>
		/// Fetch cached data from API
		/// Returns cached data or null if missing
		/// </summary>
		public string getCached(string route) {
			return getCached(route, true);
		}

		public string getCached(string route, bool checkExpiry) {
			var cache = db.Query<Models.Cache>().Where<Models.Cache>(c => c.id == route).LastOrDefault();
			if (cache == null || (checkExpiry && cache.Expired)) return null;
			try {
				return File.ReadAllText(Path.Combine(CACHE_PATH, Regex.Replace(route, "[^a-zA-Z0-9._-]", "_")));
			} catch (Exception ex) {
				log.error("Cache get threw: " + ex.ToString());
				db.Delete(cache);
				return null;
			}
		}

		public IEnumerator get(string route, Action<string> callback) {
			return get (route, callback, true);
		}
		

		/// <summary>
		/// Coroutine to fetch data from API.
		/// Response string or null if failed returned through callback.
		/// </summary>
		public IEnumerator get(string route, Action<string> callback, bool checkCache) {
			Models.Cache cache = null;
			if (checkCache) {
				cache = db.Query<Models.Cache>().Where<Models.Cache>(c => c.id == route).FirstOrDefault();
				if (cache != null && !cache.Expired) {
					try {
						var cached = File.ReadAllText(Path.Combine(CACHE_PATH, Regex.Replace(route, "[^a-zA-Z0-9._-]", "_")));
						callback(cached);
						yield break;
					} catch (Exception ex) {
						log.error("Cache get threw: " + ex.ToString());
						db.Delete(cache);
						cache = null;
					}
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
			if (cache != null && cache.lastModified != null) {
				headers["If-Modified-Since"] = cache.lastModified;
			}

            var www = new WWW(ApiUrl(route), null, headers);

#if !UNITY_IPHONE
			yield return www;
#endif

			//this way to wait works better on iOS
            while (!www.isDone && String.IsNullOrEmpty(www.error)) 
			{ 
				yield return 0; 
			}
				
			if (!String.IsNullOrEmpty(www.error)) {
				log.error("get(" + route + ") threw error: " + www.error);
				if (www.error.ToLower().Contains("401 unauthorized")) {
					db.Delete(token);
					token = null;
				}
				callback(null);
				yield break;
			}			

			if (cache != null && www.bytesDownloaded <= 0) {
				log.error("304?");
				// 304 Not Modified (hopefully)
				var cached = File.ReadAllText(Path.Combine(CACHE_PATH, Regex.Replace(route, "[^a-zA-Z0-9._-]", "_")));
				callback(cached);
				yield break;
			}

			string responseEncoding = "uncompressed";
			string cacheControl = "no-cache";
			long maxAge = 0;
			string lastModified = null;
			string date = null;
			foreach (string key in www.responseHeaders.Keys) {
				if (key.ToLower().Equals("content-encoding")) {
					responseEncoding = www.responseHeaders[key];
				}
				if (key.ToLower().Equals("cache-control")) {
					cacheControl = www.responseHeaders[key];
					Match matches = CACHE_REGEXP.Match(cacheControl);
					if (matches.Success) {
						maxAge = long.Parse(matches.Groups["maxage"].Value);
						if (maxAge < 300) maxAge = 300; // TODO: Fix server cache-control responses
					}
				}
				if (key.ToLower().Equals("last-modified")) {
					lastModified = www.responseHeaders[key];
				}
				if (key.ToLower().Equals("date")) {
					date = www.responseHeaders[key];
				}
				//log.info("Get Response Header:" + key + ":" + www.responseHeaders[key]);
			}
			if (lastModified == null && date != null) lastModified = date;
			if (lastModified == null && maxAge == 0) maxAge = 300;

			var encoding = new System.Text.UTF8Encoding();
			string body = null;

			if (responseEncoding.ToLower().Equals("gzip")) body = encoding.GetString(DecompressGZipBuffer(www.bytes));
			else body = encoding.GetString(www.bytes);

			//log.info("Get response body: " + body);

			cache = new Models.Cache(route, maxAge, lastModified);
			if (maxAge > 0 || lastModified != null) {
				if (!db.UpdateObjectBy("id", cache)) {
					db.StoreObject(cache);
				}
				try {
					File.WriteAllText(Path.Combine(CACHE_PATH, Regex.Replace(route, "[^a-zA-Z0-9._-]", "_")), body);
					log.info("get(" + route + ") cached for " + maxAge + "s");
				} catch (Exception ex) {
					log.error("Cache store threw: " + ex.ToString());
					db.Delete(cache);
				}
			} else {
				db.DeleteObjectBy("id", cache);
			}
			
			log.info("get(" + route + ") returned in " + (DateTime.Now - start));
			callback(body);
		}
		
		private string ApiUrl(string path) 
		{
			return SCHEME + API_HOST + "/api/1/" + path;
		}
			
		/// <summary>
		/// Decompresses the Gzip buffer.
		/// Use this method rather than calling the Ionic libs directly, since data is automatically unzipped on iOS
		/// </summary>
		/// <returns>The Gzip buffer.</returns>
		/// <param name="bytes">Bytes.</param>
		public static byte[] DecompressGZipBuffer(byte[] bytes)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			return bytes;
#else
			return Ionic.Zlib.GZipStream.UncompressBuffer(bytes);
#endif
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
			public List<Models.Notification> notifications;
			public List<Models.Transaction> transactions;
			public List<Models.Action> actions;
            public List<Models.Event> events;
    		
			public Data(Siaqodb db, Device self) {
				devices = new List<Models.Device>();
				devices.Add(self);
				tracks = new List<Models.Track>(db.Query<Models.Track>().Where<Models.Track>(t => t.dirty == true));
				positions = new List<Models.Position>(db.Query<Models.Position>().Where<Models.Position>(p => p.dirty == true));
				notifications = new List<Models.Notification>(db.Query<Models.Notification>().Where<Models.Notification>(n => n.dirty == true));
				transactions = new List<Models.Transaction>(db.Query<Models.Transaction>().Where<Models.Transaction>(t => t.dirty == true));
				actions = new List<Models.Action>(db.LoadAll<Models.Action>());
                events = new List<Models.Event>(db.LoadAll<Models.Event>());

				// Populate device_id
				foreach (Models.Track track in tracks) {
					track.deviceId = self.id;
				}
				foreach (Models.Position pos in positions) {
					pos.deviceId = self.id;
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
                foreach (Models.Notification n in notifications) {
					if (n.deleted_at.HasValue) {
						db.Delete(n);
						deletes++;
						continue;
					}
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
				log.info("Sync: flushed old data: " + updates + " updates, " + deletes + " deletes in " + (DateTime.Now - start));				
			}
			
			public override string ToString()
			{
				return (LengthOrNull(devices) + " devices, "
                        + LengthOrNull(tracks) + " tracks, "
            			+ LengthOrNull(positions) + " positions, "
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
		
        public void persist(Siaqodb db, PlayerConfig playerConfig) {
            if (!db.UpdateObjectBy("id", playerConfig)) {
                db.StoreObject(playerConfig);
            }
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
			public List<Models.Notification> notifications;
            public List<Models.Transaction> transactions;
            //public List<Models.PlayerConfig> playerConfig;
            public List<Models.Game> games;
			
			public List<string> errors = new List<string>();
			
			public override string ToString()
			{
				return ("head: " + sync_timestamp
						+ " tail: " + tail_timestamp + "#" + tail_skip + "; "
					    + LengthOrNull(devices) + " devices, "
                        + LengthOrNull(friends) + " friends, "
                        + LengthOrNull(games) + " games, "
						+ LengthOrNull(challenges) + " challenges, "
						+ LengthOrNull(tracks) + " tracks, "
						+ LengthOrNull(positions) + " positions, "
						+ LengthOrNull(notifications) + " notifications, "
						+ LengthOrNull(transactions) + " transactions, "
						+ LengthOrNull(errors) + " errors");
			}

			public void persist(Siaqodb db) {
				var start = DateTime.Now;
				uint inserts, updates, deletes;
				inserts = updates = deletes = 0;

				Device self = db.Query<Device>().Where(d => d.self == true).First();
				
				SyncState state = db.Query<SyncState>().LastOrDefault();
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
						if (device.id == self.id) device.self = true;
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
						friendship.friend.GenerateCompositeId();

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

                if (games != null) {
                    db.StartBulkInsert(typeof(Models.Game));
                    foreach (Models.Game game in games) {
                        // TODO: Move to <model>.save(db)?
                        if (!db.UpdateObjectBy("gameId", game)) {
                            db.StoreObject(game);
                            inserts++;
                        } else updates++;
                    }
                    db.EndBulkInsert(typeof(Models.Game));
                }

                if (tracks != null) {
					db.StartBulkInsert(typeof(Models.Device));
					foreach (Models.Track track in tracks) {
						track.GenerateCompositeId();
						if (track.deleted_at != null) {
							if (db.DeleteObjectBy("id", track))
                                deletes++;
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
				
				if (notifications != null) {
					db.StartBulkInsert(typeof(Models.Notification));
					foreach (Models.Notification notification in notifications) {
						if(notification.deleted_at != null) {
							if(db.DeleteObjectBy("id", notification)) deletes++;
								continue;
						}
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
				log.info("Sync: persisted new data: " + inserts + " inserts, " + updates + " updates, " + deletes + " deletes in " + (DateTime.Now - start));
			}
		}

		private class SignUpRequest 
		{
			public string email;
			public string password;
			public string invite_code;
			public Profile profile;
            public ProviderToken authentication;

			public SignUpRequest(string email, string password, string inviteCode, Profile profile, ProviderToken authentication) {
				this.email = email;
				this.password = password;
				this.invite_code = inviteCode;
				this.profile = profile;
                this.authentication = authentication;
			}
		}

		private class SignUpResponse
		{
			public bool success = false;
			public User user;
			public Dictionary<string, IList<string>> errors;
		}

		private static string LengthOrNull(IList list) {
			if (list == null) return "null";
			return list.Count.ToString();
		}
	}

}

