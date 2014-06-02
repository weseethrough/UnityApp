using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sqo;
using UnityEngine;
//using Ionic.Zip;

namespace SiaqodbUtils
{
    public class DatabaseFactory
    {
        public static string siaqodbPath;
        private static Siaqodb instance;
		
        public static Siaqodb GetInstance()
        {
            if (instance == null)
            {
                #if UNITY_ANDROID              
                //if ANDROID:
                siaqodbPath = Application.persistentDataPath;
                #elif UNITY_EDITOR
                //if Windows or MAC
                siaqodbPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database";
                #elif UNITY_IPHONE
                //if iOS (iPhone /iPad)
                siaqodbPath = Application.dataPath;
                #endif

                if (!Directory.Exists(siaqodbPath))
                {
                    Directory.CreateDirectory(siaqodbPath);
                }
                instance = new Siaqodb(siaqodbPath);
				UnityEngine.Debug.Log("Siaqo path: " + siaqodbPath);
            }
            return instance;
        }
        public static void CloseDatabase()
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }
        }

		public static void PopulateFromBundle(string filename) 
		{
			var db = GetInstance(); // Populate siaqodbPath
			lock(db) {
				var bundlePath = Path.Combine(Path.Combine(Application.streamingAssetsPath, "database"), filename);
                if (false && bundlePath.Contains("://")) {
//					var basePath = siaqodbPath;
//
//					var jarFile = Application.dataPath;
//					var zipFile = new ZipFile(jarFile);
//					foreach (var zipEntry in zipFile)
//					{
//						if (zipEntry.FileName == filename) {
//							zipEntry.Extract(basePath);
//						}
//					}
					
				} else {
					File.Copy(bundlePath, Path.Combine(siaqodbPath, filename));
				}

				CloseDatabase();
			}
		}

		public static void ReIndex() {
			var db = GetInstance(); // Populate siaqodbPath
			SiaqodbUtil.ReIndex(siaqodbPath);
		}
    }
}
