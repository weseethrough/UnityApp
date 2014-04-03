using System;
using RaceYourself.Models;

namespace PositionPredictor
{
	public class PositionUtils
	{
		private static double R = 6371000; // average earth's radius (meters)
		private static double INV_R = 0.0000001569612306; // 1/earth's radius (meters)
		
		public static double distanceBetween(Position pos1, Position pos2) {
            double lat1R = ToRadians(pos1.latitude);
            double lat2R = ToRadians(pos2.latitude);
            double dLatR = Math.Abs(lat2R - lat1R);
            double dLngR = Math.Abs(ToRadians(pos2.longitude - pos1.longitude));
            double a = Math.Sin(dLatR / 2) * Math.Sin(dLatR / 2) + Math.Cos(lat1R) * Math.Cos(lat2R)
                            * Math.Sin(dLngR / 2) * Math.Sin(dLngR / 2);
            double distanceInRadians =  2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			return distanceInRadians * R;

		}

		
		// Precise position prediction based on the last
	    // position, bearing and speed
	    public static Position predictPosition(Position aLastPosition, long milliseconds) {
	       if (aLastPosition.speed < 0.01) {
	           return aLastPosition;
	       }
	       
			// TODO
			/*if (aLastPosition.bearing == null) {
	         return null;
	       }*/
	
	       Position next = new Position();
	       double d = aLastPosition.speed * milliseconds / 1000.0f; // distance = speed(m/s) * time (s)
	
	       double dR = d*INV_R;
	       // Convert bearing to radians
	       double brng = ToRadians(aLastPosition.bearing);
	       double lat1 = ToRadians(aLastPosition.latitude);
	       double lon1 = ToRadians(aLastPosition.longitude);
	       //System.out.printf("d: %f, dR: %f; brng: %f\n", d, dR, brng);
	       // Predict lat/lon
	       double lat2 = Math.Asin(Math.Sin(lat1)*Math.Cos(dR) + 
	                    Math.Cos(lat1)*Math.Sin(dR)*Math.Cos(brng) );
	       double lon2 = lon1 + Math.Atan2(Math.Sin(brng)*Math.Sin(dR)*Math.Cos(lat1), 
	                     Math.Cos(dR)-Math.Sin(lat1)*Math.Sin(lat2));
	       // Convert back to degrees
	       next.latitude = (float)ToDegrees(lat2);
	       next.longitude = (float)ToDegrees(lon2);
	       next.gps_ts = (aLastPosition.gps_ts + milliseconds);
	       next.device_ts = (aLastPosition.device_ts + milliseconds);
	       next.bearing = aLastPosition.bearing;
	       next.speed = aLastPosition.speed;
	       
	       return next;
	    }
		
		public static float calcBearing(Position start, Position end) {
            return normalizeBearing((float)ToDegrees(calcBearingInRadians(start, end)));
	    }
	
	    public static float calcBearingInRadians(Position from, Position to) {
	        double lat1R = ToRadians(from.latitude);
	        double lat2R = ToRadians(to.latitude);
	        double dLngR = ToRadians(to.longitude - from.longitude);
	        double a = Math.Sin(dLngR) * Math.Cos(lat2R);
	        double b = Math.Cos(lat1R) * Math.Sin(lat2R) - Math.Sin(lat1R) * Math.Cos(lat2R)
	                                * Math.Cos(dLngR);
	        return (float)Math.Atan2(a, b);
	     }
	    
	    public static float normalizeBearing(float bearing) {
            if (Double.IsNaN(bearing) || Double.IsPositiveInfinity(bearing) || Double.IsNegativeInfinity(bearing))
                    return (float)Double.NaN;
            float bearingResult = bearing % 360;
            if (bearingResult < 0)
                    bearingResult += 360;
            return bearingResult;
	    }

		// Calculate minimal angle difference (in degrees) between two 
	    public static float bearingDiffDegrees(float bearing1, float bearing2) {
	    	float diff = bearing1 - bearing2;
	    	diff  += (diff>180) ? -360 : (diff<-180) ? 360 : 0;
	    	return diff;
	    }
	    // Returns bearing which is in between bearing1 and bearing2 at percentile position.
	    // E.g. for b1 = 120, b2 = 60, p = 0.4 will return 120 - (120 - 60)*0.4 = 96
	    // for b1 = 300, b2 = 10, p = 0.8 will return 300 + (360+10 - 300) * 0.8 = 356
	    public static float bearingDiffPercentile(float bearing1, float bearing2, float percentile) {
	    	float diff = bearingDiffDegrees(bearing1, bearing2);
	    	int sign = 1;
	    	if (normalizeBearing(bearing1 - diff) == bearing2) {
	    		sign = -1;
	    	}
			return normalizeBearing(bearing1 + sign*percentile*diff);
	    }

		
		public static double ToRadians(double val)
    	{
        	return (Math.PI / 180) * val;
    	}
		
		public static double ToDegrees(double val)
    	{
        	return (180 / Math.PI) * val;
    	}
	}
}

