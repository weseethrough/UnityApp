using System;
using System.IO;

namespace PositionTracker
{
	public class Utils
	{
		public static long CurrentTimeMillis() {
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}
		
		public static void Log(TextWriter w, string logMessage)
		{
			bool debug = false;
			if (!debug) return; // TODO: only in debug mode

			w.Write("\r\nLog Entry : ");
			w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
				DateTime.Now.ToLongDateString());
			w.WriteLine("  :");
			w.WriteLine("  :{0}", logMessage);
			w.WriteLine ("-------------------------------");
		}

	}
}

