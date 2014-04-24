using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sqo;
using UnityEngine;

namespace SiaqodbUtils
{
    public class DatabaseFactory
    {
        public static string siaoqodbPath;
        private static Siaqodb instance;


        public static Siaqodb GetInstance()
        {
            if (instance == null)
            {
              
                //if ANDROID:
                siaoqodbPath = Application.persistentDataPath;
                //if Windows or MAC
                //siaoqodbPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database";
                
                //if iOS (iPhone /iPad)
                //siaoqodbPath =Application.dataPath;
		
                if (!Directory.Exists(siaoqodbPath))
                {
                    Directory.CreateDirectory(siaoqodbPath);
                }
                instance = new Siaqodb(siaoqodbPath);
				UnityEngine.Debug.Log("Siaqo path: " + siaoqodbPath);
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
    }
}
