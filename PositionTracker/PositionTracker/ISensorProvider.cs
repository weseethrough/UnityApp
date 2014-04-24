using System;
using Assimp;

namespace PositionTracker
{
	public interface ISensorProvider	
	{
		float[] LinearAcceleration { get; }
		
	}
}

