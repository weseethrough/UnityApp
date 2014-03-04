using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sqo;
using UnityEngine;

namespace SiaqodbDemo
{
    public class DatabaseFactory
    {
        public static string siaoqodbPath;
        private static Siaqodb instance;


        public static Siaqodb GetInstance()
        {
            if (instance == null)
            {
               //put here your License Key
                SiaqodbConfigurator.SetLicense(@"Tebhiwc9k0xmxegCMu52/wuW8Vb41BAbvmNC+yyAariYtXIwZ6boPhyIpRH2GPWB");
               

#if UNITY_EDITOR
				siaoqodbPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database";
#else
                siaoqodbPath = Application.persistentDataPath;
#endif
		
                if (!Directory.Exists(siaoqodbPath))
                {
                    Directory.CreateDirectory(siaoqodbPath);
                }
                instance = new Siaqodb(siaoqodbPath);
				Debug.Log("Database: Path: " + siaoqodbPath);
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
