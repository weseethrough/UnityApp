using System;
using System.Diagnostics;

public class Log
{
	private String tag = "";
	private long lastLogTimeMillis = 0;
	private static Stopwatch timer = new Stopwatch();
	
	static Log() {
		timer.Start();
	}
	
	public Log (String tag)
	{
		this.tag = tag;
	}

	public void info(String message)
	{
		UnityEngine.Debug.Log(tic() + "ms: " + tag + ": " + message);
	}

	public void warning(String message)
	{
		UnityEngine.Debug.LogWarning(tic() + "ms: " + tag + ": " + message);
	}

	public void error(Exception e, String message)
	{
		UnityEngine.Debug.LogError(tic() + "ms: " + tag + ": " + message);
        UnityEngine.Debug.LogError(e.Message);
        UnityEngine.Debug.LogError(e.StackTrace);
	}

	public void exception(Exception e)
	{
		UnityEngine.Debug.LogException(e);
	}


	public void profile(String message)
	{
		UnityEngine.Debug.Log(tic() + "ms, delta=" + (tic() - lastLogTimeMillis) + "ms: "+ tag + ": " + message);
		lastLogTimeMillis = tic();
	}

	private long tic() {
		return timer.ElapsedMilliseconds;
	}
}

