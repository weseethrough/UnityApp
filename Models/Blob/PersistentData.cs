using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo;

namespace RaceYourself.Models.Blob
{

    public class DataEntry
    {
        public object storedValue = null;        
        public bool persistent;

        public DataEntry()
        {

        }

        public DataEntry(System.Object v, bool p)
        {
            storedValue = v;
            persistent = p;
        }
    }    

    public class PersistentData
    {
        public Dictionary<int>      listInt     = null;
        public Dictionary<bool>     listBool    = null;
        public Dictionary<double>   listDouble  = null;
        public Dictionary<string>   listStr     = null;
        
        public PersistentData()
        {
            listInt     = new Dictionary<int>();
            listBool    = new Dictionary<bool>();
            listDouble  = new Dictionary<double>();
            listStr     = new Dictionary<string>();
        }

        public void AddData(string name, object obj)
        {

            if (obj.GetType() == typeof(int))
            {
                listInt.Set(name, Convert.ToInt32(obj));
            }
            else if (obj.GetType() == typeof(bool))
            {
                listBool.Set(name, Convert.ToBoolean(obj));
            }
            else if (obj.GetType() == typeof(double) || obj.GetType() == typeof(float))
            {
                listDouble.Set(name, Convert.ToDouble(obj));
            }
            else if (obj.GetType() == typeof(string))
            {
                listStr.Set(name, Convert.ToString(obj));
            }
            
        }
    }
}
