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
        private static Siaqodb writableInstance;
        private static Siaqodb staticInstance;

        public static Siaqodb GetStaticInstance()
        {
            if (staticInstance == null)
            {
                char slash = Path.DirectorySeparatorChar;
                siaoqodbPath = Environment.CurrentDirectory + slash + @"Assets" + slash + @"StreamingAssets" + slash + @"database";

                if (!Directory.Exists(siaoqodbPath))
                {
                    Directory.CreateDirectory(siaoqodbPath);
                }
                staticInstance = new Siaqodb(siaoqodbPath);
            }
            return staticInstance;
        }

        public static Siaqodb GetWritableInstance()
        {
            if (writableInstance == null)
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
                writableInstance = new Siaqodb(siaoqodbPath);
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
