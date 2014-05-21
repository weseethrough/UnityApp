using System;
using System.IO;

namespace PositionTracker
{
	public class Utils
	{
		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long CurrentTimeMillis() {
			return (long) (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
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

