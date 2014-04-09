using System;

namespace PositionTracker
{
	public interface ISensorProvider	
	{
		float ForwardAcceleration { get; }
		float TotalAcceleration { get; }
		
	}
}

