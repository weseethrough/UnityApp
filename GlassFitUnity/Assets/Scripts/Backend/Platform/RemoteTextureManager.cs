using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using RaceYourself.Models;
using Sqo;
using System.Text.RegularExpressions;

public class RemoteTextureManager : MonoBehaviour {

	private const int CONCURRENT_REQUESTS = 10;
	private int request = 0;
	private readonly string CACHE_PATH = Path.Combine(Application.persistentDataPath, "texture-cache");

	protected static Log log = new Log("RemoteTextureManager"); 

	public void Awake()
	{
		log.info("created");
		Directory.CreateDirectory(CACHE_PATH);
		DontDestroyOnLoad(gameObject);
	}

	public void LoadImage(string url, object argument, Action<Texture2D, object> callback) {
		if (string.IsNullOrEmpty(url)) return;
		StartCoroutine(LoadImageCoroutine(url, argument, callback));
	}

	private IEnumerator LoadImageCoroutine(string url, object argument, Action<Texture2D, object> callback) {
		var start = DateTime.Now;
		var db = Platform.Instance.db;
		// On-disk cache
		var cache = db.Query<Cache>().Where<Cache>(c => c.id == url).LastOrDefault();
		var originalUrl = url;
		var sanitizedUrl = Regex.Replace(originalUrl, "\\?.*$", "");
		sanitizedUrl = Regex.Replace(sanitizedUrl, "[^a-zA-Z0-9.]", "");
		if (cache != null && !cache.Expired && !string.IsNullOrEmpty(sanitizedUrl)) {
			var path = Path.Combine(CACHE_PATH, sanitizedUrl);
			if (File.Exists(path)) {
				url = "file://" + path;
			} else {
				db.Delete(cache);
				cache = null;
			}
		}
		// Protocol-relative URL
		if (url.StartsWith("//")) {
			originalUrl = url;
			url = "http:" + originalUrl;
		}
		
		bool http = url.StartsWith("http");
		while(http && request >= CONCURRENT_REQUESTS) {
			log.info("queued " + url);
			yield return new WaitForSeconds(1f);
		}
		if (http) request++;
		try {
			WWW webURL = new WWW(url);
			
			yield return webURL;
			
			if (!String.IsNullOrEmpty(webURL.error)) {
				log.info("failed to fetch " + url + ": " + webURL.error);
			} else {
				var texture = webURL.texture;
				//				var texture = new Texture2D(1, 1, TextureFormat.DXT1, false); // Will be overwritten
				//				webURL.LoadImageIntoTexture(texture); // Replace texture contents (incl. dimensions)
				callback(texture, argument);
				
				// Cache to disk if not already a file:// fetch
				if (url == originalUrl) { 
					long maxAge = 60*60; // 1h
					string lastModified = null;
					cache = new Cache(originalUrl, maxAge, lastModified);
					if (maxAge > 0 || lastModified != null) {
						if (!db.UpdateObjectBy("id", cache)) {
							db.StoreObject(cache);
						}
						try {
							File.WriteAllBytes(Path.Combine(CACHE_PATH, sanitizedUrl), webURL.bytes);
							log.info(url + " cached for " + maxAge + "s");
						} catch (Exception ex) {
							log.error("Texture cache threw: " + ex.ToString());
							db.Delete(cache);
						}
					} else {
						db.DeleteObjectBy("id", cache);
					}
				}
			}
		} finally {
			if (http) request--;
			log.info(originalUrl + " completed in " + (DateTime.Now - start).Seconds + "s" );
		}
	}

}
