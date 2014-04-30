using System;
using System.Threading;
using NUnit.Framework;

namespace PositionTracker
{
	
	public class PositionProviderStub : IPositionProvider
	{
		// Returns true in case of successful registration, false otherwise
		public bool RegisterPositionListener(IPositionListener posListener) { return true; }
		
		public void UnregisterPositionListener(IPositionListener posListener) { }
	}
	
	public class SensorProviderStub :  ISensorProvider	
	{
		public SensorProviderStub() { 
			float[] linAcc =  {0.0f, 0.0f, 0.0f}; 
			LinearAcceleration = linAcc;
		}
		
		public float[] LinearAcceleration { get; set; }
		
	}
	
	[TestFixture]
	public class PositionTrackerTest
	{
		[Test]
		public void PositionTrackerTest1 ()
		{
			PositionTracker positionTracker = new PositionTracker(new PositionProviderStub(), new SensorProviderStub());
			positionTracker.OnPause();
			Assert.IsTrue(true);
		}
	}
}

