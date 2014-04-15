using System;

namespace PositionTracker
{
	public interface IPositionProvider
	{
		// Returns true in case of successful registration, false otherwise
		bool RegisterPositionListener(IPositionListener posListener);
		
		void UnregisterPositionListener(IPositionListener posListener);
	}
}

