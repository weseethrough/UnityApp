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
			SpeedState speedState = new SpeedState();
			speedState.CurrentState = PositionTracker.State.STOPPED;
			nextState = speedState.NextState( 0.5f, 10.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_ACC);
			
			speedState.CurrentState = PositionTracker.State.STOPPED;
			nextState = speedState.NextState( 0.4f, 10.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STOPPED);
		}
		
		[Test]
		public void SensorAccTest ()
		{
			SpeedState speedState = new SpeedState();

			speedState.CurrentState = PositionTracker.State.SENSOR_ACC;
			nextState = speedState.NextState( 10.0f, 0.1f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
			
			speedState.CurrentState = PositionTracker.State.SENSOR_ACC;
			nextState = speedState.NextState(  0.3f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
			
			speedState.CurrentState = PositionTracker.State.SENSOR_ACC;
			nextState = speedState.NextState( 0.4f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_ACC);
		}
		
		[Test]
		public void SteadyGpsSpeedTest ()
		{
			SpeedState speedState = new SpeedState();

			speedState.CurrentState = PositionTracker.State.STEADY_GPS_SPEED;
			nextState = speedState.NextState( 0.3f, 0.1f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
			
			speedState.CurrentState = PositionTracker.State.STEADY_GPS_SPEED;
			nextState = speedState.NextState(  0.4f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.COAST);
			
			speedState.CurrentState = PositionTracker.State.STEADY_GPS_SPEED;
			nextState = speedState.NextState( 0.4f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
		}
		
		[Test]
		public void CoastTest ()
		{
			SpeedState speedState = new SpeedState();
			
			speedState.CurrentState = PositionTracker.State.COAST;
			nextState = speedState.NextState( 0.3f, 0.1f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
			
			speedState.CurrentState = PositionTracker.State.COAST;
			nextState = speedState.NextState(  0.4f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
			
			speedState.CurrentState = PositionTracker.State.COAST;
			nextState = speedState.NextState( 0.4f, 0.0f); 			
			Assert.AreEqual(nextState, PositionTracker.State.COAST);
		}
		
		[Test]
		public void SensorDecTest ()
		{
			SpeedState speedState = new SpeedState();

			speedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			nextState = speedState.NextState( 0.3f, 0.0f ); 			
			Assert.AreEqual(nextState, PositionTracker.State.STOPPED);
			
			speedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			// TODO: avoid sleeping by overriding GetTimeInState() to mock entry time
			Thread.Sleep(3500);
			nextState = speedState.NextState(0.4f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.STEADY_GPS_SPEED);
			
			speedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			nextState = speedState.NextState( 0.5f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_ACC);
			
			speedState.CurrentState = PositionTracker.State.SENSOR_DEC;
			nextState = speedState.NextState( 0.3f, 0.1f); 			
			Assert.AreEqual(nextState, PositionTracker.State.SENSOR_DEC);
		}		
	}
}

