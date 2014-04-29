using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo;

namespace RaceYourself.Models.Blob
{
    public struct DictionaryEntry<T>
    {
        public T data;
        public string name;
        public int index;
    }

    [Serializable]
    public class Dictionary<T> : System.Collections.IEnumerable
    {
        public List<T> data = null;
        public List<String> names = null;
        public string name = "";

        public Dictionary()
	    {
            this.data = new List<T>();
		    this.names = new List<string>();
	    }

        /// <summary>
        /// Enumerator for "foreach" loops, also returns bundled data of name and data
        /// </summary>
        /// <returns></returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            for (int i = 0; i < data.Count; i++)
            {
                DictionaryEntry<T> entry;
                entry.data = data[i];
                entry.name = names[i];
                entry.index = i;
                yield return entry;
            }
        }

        #region UPDATE
        /// <summary>
        /// This metod adds object only if one doesnt exists
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Add(string name, T obj)
        {            
            int index = this.names.FindIndex(x => x == name);
            if (index >= 0)
            {
                //we have this object in our list                
                return false;
            }

            this.names.Add(name);
            this.data.Add(obj);
            return true;
        }

        /// <summary>
        /// This method allows to set value (add new or update old one)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Set(string name, T obj)
        {           

            int index = this.names.FindIndex(x => x == name);
            if (index >= 0)
            {
                this.data[index] = obj;
                return true;
            }

            this.names.Add(name);
            this.data.Add(obj);
            return true;
        }

        /// <summary>
        /// changes name identifier of the element in the dictionary
        /// </summary>
        /// <param name="name">name identifier of the object to change</param>
        /// <param name="newName">new name under whch object would exist from now on</param>
        /// <returns>true if succesfuly changed</returns>
        public bool Rename(string name, string newName)
        {            
            int index = this.names.FindIndex(x => x == name);
            if (index >= 0)
            {
                this.names[index] = newName;

                return true;
            }

            return false;
        }
        #endregion 

        #region GET

        /// <summary>
        /// returns length of dictionary
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            return data.Count();
        }

        /// <summary>
        /// Get object by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get(string name)
        {            
            int index = this.names.FindIndex(x => x == name);
            if (index >= 0)
            {
                return data[index];
            }

            return default(T);
        }

        /// <summary>
        /// Get element by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Get(int index)
        {            
            if (index >= 0 && data.Count > index)
            {
                return data[index];
            }

            return default(T);
        }

        /// <summary>
        /// get name by object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetName(object obj)
        {
            int index = this.data.FindIndex(x => Equals(x,  obj));
            if (index >= 0)
            {
                return names[index];
            }

            return default(string);
        }

        /// <summary>
        /// get name by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetName(int index)
        {
            if (index >= 0 && names.Count > index)
            {
                return names[index];
            }

            return default(string);
        }

        /// <summary>
        /// Get object index in dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetIndex(object obj)
        {
            int index = this.data.FindIndex(x => Equals(x, obj));
            if (index >= 0)
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Get index by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetIndex(string name)
        {
            int index = this.names.FindIndex(x => x == name);
            if (index >= 0)
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Gets single entry as bundled data
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DictionaryEntry<T> GetEntry(int index)
        {
            if (index >= 0 && names.Count > index)
            {
                DictionaryEntry<T> entry;
                entry.data = data[index];
                entry.name = names[index];
                entry.index = index;
                return entry;
            }

            DictionaryEntry<T> defaultEntry = new DictionaryEntry<T>();
            return defaultEntry;
        }

        /// <summary>
        /// Makes copy of dictionary and returns pointer to it
        /// </summary>
        /// <returns></returns>
        public Dictionary<T> Clone()
        {
            Dictionary<T> copy = new Dictionary<T>();
            for(int i=0; i<data.Count; i++)
            {
                copy.Add(names[i], data[i]);
            }
            return copy;
        }
        #endregion

        #region REMOVE
        /// <summary>
        /// Removes element by name if exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {           
            int index = this.names.FindIndex(x => x == name);
            if (index >= 0)
            {
                this.names.RemoveAt(index);
                this.data.RemoveAt(index);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes element from dictionary if its there
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Remove(T obj)
        {
            int index = this.data.FindIndex(x => Equals( x, obj));
            if (index >= 0)
            {
                this.names.RemoveAt(index);
                this.data.RemoveAt(index);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes element by index if possible
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveAt(int index)
        {            
            if (index >= 0 && this.data.Count > index)
            {
                this.names.RemoveAt(index);
                this.data.RemoveAt(index);

                return true;
            }

            return false;
        }

        #endregion 
    
    }
}
