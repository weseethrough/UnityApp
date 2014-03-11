using System;

namespace RaceYourself.Models
{
	public class SyncState
	{
		public long sync_timestamp;
		public long? tail_timestamp;
		public long? tail_skip;

		public SyncState() 
		{
		}
		public SyncState(long sync_timestamp) 
		{
			this.sync_timestamp = sync_timestamp;
		}
		public SyncState(long sync_timestamp, long? tail_timestamp, long? tail_skip)
		{
			this.sync_timestamp = sync_timestamp;
			this.tail_timestamp = tail_timestamp;
			this.tail_skip = tail_skip;
		}
	}
}

