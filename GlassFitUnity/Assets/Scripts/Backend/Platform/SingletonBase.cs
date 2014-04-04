using System;

public class SingletonBase
{
    private static volatile SingletonBase instance;
    private static object syncRoot = new Object();

    public int i =0;

    protected SingletonBase() { }
    
    /// <summary>
    /// T is extension of SingletonBase class and have to have new constructor,
    /// otherwise cant be used as parameter to this instance function... of course
    /// </summary>
    /// <returns></returns>
    protected static SingletonBase GetInstance<T>() where T : SingletonBase, new()
    {        
        if (instance == null)
        {
            lock (syncRoot)
            {
                if (instance == null)
                {
                    instance = new T();
                    instance.Awake();
                }
            }
        }

        return instance;
        
    }

    public int Increase()
    {
        return ++i;
    }

    public virtual void Awake()
    {

    }
}