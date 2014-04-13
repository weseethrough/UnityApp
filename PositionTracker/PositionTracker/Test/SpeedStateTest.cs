using System;
using System.Threading;
using NUnit.Framework;

namespace PositionTracker
{
	[TestFixture]
	public class SpeedStateTest
	{
		PositionTracker.State nextState;
		
		[Test]
		public void StoppedTest ()
		{
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.STOPPED;
			nextState = PositionTracker.SpeedState.NextState( 0.5f, 10.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_ACC);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.STOPPED;
			nextState = PositionTracker.SpeedState.NextState( 0.4f, 10.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STOPPED);
		}
		
		[Test]
		public void SensorAccTest ()
		{
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_ACC;
			nextState = PositionTracker.SpeedState.NextState( 10.0f, 0.1f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_ACC;
			nextState = PositionTracker.SpeedState.NextState(  0.3f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_ACC;
			nextState = PositionTracker.SpeedState.NextState( 0.4f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_ACC);
		}
		
		[Test]
		public void SteadyGpsSpeedTest ()
		{
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.STEADY_GPS_SPEED;
			nextState = PositionTracker.SpeedState.NextState( 0.3f, 0.1f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.STEADY_GPS_SPEED;
			nextState = PositionTracker.SpeedState.NextState(  0.4f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.COAST);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.STEADY_GPS_SPEED;
			nextState = PositionTracker.SpeedState.NextState( 0.4f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
		}
		
		[Test]
		public void CoastTest ()
		{
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.COAST;
			nextState = PositionTracker.SpeedState.NextState( 0.3f, 0.1f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.COAST;
			nextState = PositionTracker.SpeedState.NextState(  0.4f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.COAST;
			nextState = PositionTracker.SpeedState.NextState( 0.4f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.COAST);
		}
		
		[Test]
		public void SensorDecTest ()
		{
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			nextState = PositionTracker.SpeedState.NextState( 0.3f, 0.0f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.STOPPED);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			// TODO: avoid sleeping by overriding GetTimeInState() to mock entry time
			Thread.Sleep(3500);
			nextState = PositionTracker.SpeedState.NextState(0.4f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			nextState = PositionTracker.SpeedState.NextState( 0.5f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_ACC);
			
			PositionTracker.SpeedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			nextState = PositionTracker.SpeedState.NextState( 0.3f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
		}		
	}
}

