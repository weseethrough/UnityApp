using System;
using RaceYourself.Models;

namespace PositionTracker
{
	public interface IPositionListener
	{
		void OnPositionUpdate(Position pos);	
	}
}

