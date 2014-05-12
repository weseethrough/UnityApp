using System;
using System.Threading;

using RaceYourself.Models;

namespace PositionTracker
{
	public class FakePositionProvider : IPositionProvider
	{
		private PositionTracker positionTracker;
		private ISensorProvider sensorProvider;

		private double[] drift = { 0f, 0f }; // lat, long
		private double lastElapsedDistance;
		private float bearing = -1;

		// Callback class for recurrent sensor polling
		// Sensor poll timer
		Timer fakePositionTimer;


		public FakePositionProvider(PositionTracker positionTracker, ISensorProvider sensorProvider) {
			this.positionTracker = positionTracker;
			this.sensorProvider = sensorProvider;
			lastElapsedDistance = positionTracker.ElapsedDistance;
		}

		// Returns true in case of successful registration, false otherwise
		public bool RegisterPositionListener(IPositionListener posListener) {		
			// Create an inferred delegate that invokes methods for the timer.
			TimerCallback tcb = this.Run;			
			fakePositionTimer = new Timer(tcb, null, 0, 1000); // every 1 second
			return true;
		}

		public void UnregisterPositionListener(IPositionListener posListener) {
			// Stop polling
			if (fakePositionTimer != null) {
				fakePositionTimer.Dispose ();
				fakePositionTimer = null;
			}
		}


		// This method is called by the timer delegate. 
		public void Run(Object stateInfo) {

			// Fake movement in direction device is pointing at a speed
			// determined by how much the user is shaking it (uses same sensor
			// logic / state machine as outdoor mode)
			// TODO:
			if (bearing == -1) {
				// fix bearing at initial device yaw
				bearing = -sensorProvider.Yaw % 360;
			}
			drift[0] += (positionTracker.ElapsedDistance - lastElapsedDistance) * Math.Cos(bearing) / 111229d;
			drift[1] += (positionTracker.ElapsedDistance - lastElapsedDistance) * Math.Sin(bearing) / 111229d;

			// Fake location
			Position pos = new Position((float)drift[0], (float)drift[1]);
			pos.device_ts = Utils.CurrentTimeMillis();
			pos.speed = positionTracker.CurrentSpeed;
			pos.bearing = bearing;

			Console.Out.Write("Fake location: " + pos.latitude + " " + pos.longitude);

			positionTracker.OnPositionUpdate(pos);

			lastElapsedDistance = positionTracker.ElapsedDistance;
		}
	}
}

