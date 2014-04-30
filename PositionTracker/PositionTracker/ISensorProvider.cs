using System;

namespace PositionTracker
{
	public interface ISensorProvider	
	{
		float[] LinearAcceleration { get; }
		
	}
}

