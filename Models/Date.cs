using System;

namespace RaceYourself.Models
{
	public class Date
	{
		public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static TimeSpan UnixTime {
			get {
				return (DateTime.UtcNow - epoch);
			}
		}

        public static DateTime FromUnixTime(long millis) {
            var clone = epoch; 
            DateTime time = new DateTime(millis);
            TimeSpan offset = time - time.ToUniversalTime(); 
            clone = clone.AddMilliseconds(millis);
            clone = clone.Add(offset);
            return clone;
        }
	}
}

