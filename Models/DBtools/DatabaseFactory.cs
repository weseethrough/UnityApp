using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sqo;
using UnityEngine;
using Ionic.Zip;

namespace SiaqodbUtils
{
    public class DatabaseFactory
    {
        public static string siaqodbPath;
        private static Siaqodb writableInstance;
        private static Siaqodb staticInstance;

        public static Siaqodb GetStaticInstance()
        {
            if (staticInstance == null)
            {
                char slash = Path.DirectorySeparatorChar;
                siaqodbPath = Path.Combine(Application.streamingAssetsPath, @"database");

                if (siaqodbPath.Contains("://")) {
                    var basePath = Application.persistentDataPath;
                    if (!Directory.Exists(basePath))
                    {
                        Directory.CreateDirectory(basePath);
                    }
                    siaqodbPath = Path.Combine(Application.persistentDataPath, @"assets/database");
                    if (!Directory.Exists(siaqodbPath))
                    {
                        Directory.CreateDirectory(siaqodbPath);
                    }

                    var jarFile = Application.dataPath;
                    var zipFile = new ZipFile(jarFile);
                    foreach (var zipEntry in zipFile)
                    {
                        if (zipEntry.FileName.StartsWith("assets/database/")) {
                            zipEntry.Extract(basePath);
                        }
                    }

                } 

                if (!Directory.Exists(siaqodbPath))
                {
                    Directory.CreateDirectory(siaqodbPath);
                }
                staticInstance = new Siaqodb(siaqodbPath);
            }
            return staticInstance;
        }

        public static Siaqodb GetWritableInstance()
        {
            if (writableInstance == null)
            {
              
                //if ANDROID:
                siaqodbPath = Application.persistentDataPath;
                //if Windows or MAC
                //siaoqodbPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database";
                
                //if iOS (iPhone /iPad)
                //siaoqodbPath =Application.dataPath;
		
                if (!Directory.Exists(siaqodbPath))
                {
                    Directory.CreateDirectory(siaqodbPath);
                }
                writableInstance = new Siaqodb(siaqodbPath);
            }
            return writableInstance;
        }

        public static void CloseDatabase()
        {
            if (writableInstance != null)
            {
                writableInstance.Close();
                writableInstance = null;
            }

            if (staticInstance != null)
            {
                staticInstance.Close();
                staticInstance = null;
            }
        }
    }
}
