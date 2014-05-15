using System;
using System.Threading;
using NUnit.Framework;

using RaceYourself.Models;
using PositionPredictor;

namespace PositionTracker
{
	[TestFixture]
	public class PositionPredictorTest
	{
		[Test]
		public void PredictorTest ()
		{
			PositionPredictor.PositionPredictor predictor = new PositionPredictor.PositionPredictor();
			
			
			long timeStamp = 1000000;

			for (long x = 100; x < 1000; x += 100) {				
				Position pos = new Position(x, x-100);
				pos.device_ts = timeStamp;
				pos.gps_ts = timeStamp;
				pos.speed = 3; // m/s
				Console.Out.WriteLine("GPS position: " + PositionUtils.ToString(pos));
				predictor.updatePosition(pos);

				// Predict position every 30 ms
				for (long predictedTimeStamp = timeStamp; predictedTimeStamp < timeStamp + 1000; predictedTimeStamp += 30) {					
					Position predictedPosition = predictor.predictPosition(predictedTimeStamp);
					Console.Out.WriteLine("Predicted position: " + PositionUtils.ToString(pos));

				}
				
				
				timeStamp += 1000;
			}
			
		}

	}
}

