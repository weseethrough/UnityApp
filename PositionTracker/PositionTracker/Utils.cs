using System;

namespace PositionTracker
{
	public class Utils
	{
		public static long CurrentTimeMillis() {
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}
		

	}
}

