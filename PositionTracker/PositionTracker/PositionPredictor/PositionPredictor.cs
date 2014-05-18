using System;
using RaceYourself.Models;
using System.Collections.Generic;

namespace PositionPredictor
{
	public class PositionPredictor
	{
		private bool LOG_KML = true;
		private GFKml kml = new GFKml();
	    // Constant used to optimize calculations
	    private double INV_DELTA_TIME_MS = CardinalSpline.getNumberPoints() / 1000.0; // delta time between predictions
	    // Positions below this speed threshold will be discarded in bearing computation
	    private float SPEED_THRESHOLD_MS = 1.25f;
	    // Maximal number of predicted positions used for spline interpolation
	    private int MAX_PREDICTED_POSITIONS = 3;
	    private int MAX_EXT_PREDICTED_POSITIONS = 5;
    
	    // Last predicted positions - used for spline
	    private List<Position> recentPredictedPositions = new List<Position>();
	
	    private List<Position> recentGpsPositions = new List<Position>();
	    
	    private BearingCalculator bearingCalculator;
	    // Interpolated positions between recent predicted position
	    private Position[] interpPath;
	    // Last GPS position
	    private Position lastGpsPosition = new Position();
	    private Position lastPredictedPos = new Position();
	    // Accumulated GPS distance
	    private double gpsTraveledDistance = 0;
	    // Accumulated predicted distance
	    private double predictedTraveledDistance = 0;
	    
	    private int numStaticPos = 0;
	    
	    private int MAX_NUM_STATIC_POS = 2;
	        
	    public PositionPredictor() {
			bearingCalculator = new BearingCalculator(recentGpsPositions);
			interpPath = new Position[MAX_PREDICTED_POSITIONS * CardinalSpline.getNumberPoints()];
	    }
	    
	    public BearingCalculator getBearingCalculator() {
	    	return bearingCalculator; 
	    }
	    // Update prediction with new GPS position. 
	    // Input: recent GPS positon, output: correspondent predicted position 
	    public Position updatePosition(Position aLastGpsPos) {
	    	//System.out.printf("\n------ %d ------\n", ++i);
	        if (aLastGpsPos == null || aLastGpsPos.bearing == null) {
	            return null;
	        }
	        if(LOG_KML) kml.addPosition(GFKml.PathType.GPS, aLastGpsPos);
	        // Need at least 3 positions
	        if (recentPredictedPositions.Count < 2) {
	            recentPredictedPositions.Add(aLastGpsPos);
	            recentGpsPositions.Add(aLastGpsPos);
	            return aLastGpsPos;
	        } else if (recentPredictedPositions.Count == 2) {
	            recentPredictedPositions.Add(extrapolatePosition(recentPredictedPositions[recentPredictedPositions.Count-1], 1));
	        }
	        // Update traveled distance
	        updateDistance(aLastGpsPos);
	        
	        bearingCalculator.updatePosition(recentPredictedPositions[recentPredictedPositions.Count-1]);
	        
	        // correct last (predicted) position with last GPS position
	        correctLastPredictedPosition(aLastGpsPos);
	        
	        // predict next user position (in 1 sec) based on current speed and bearing
	        Position nextPos = extrapolatePosition(recentPredictedPositions[recentPredictedPositions.Count-1], 1);
	        if(LOG_KML) kml.addPosition(GFKml.PathType.EXTRAPOLATED, nextPos);
	
	        // Update number static positions
	        numStaticPos = (aLastGpsPos.speed < SPEED_THRESHOLD_MS) ? numStaticPos+1 : 0;
	        // Throw away static positions and flush predicted path/traveled distance
	        if (nextPos == null || numStaticPos > MAX_NUM_STATIC_POS 
	        		|| aLastGpsPos.speed == 0.0) { // standing still
	        	recentPredictedPositions.Clear();
	        	recentGpsPositions.Clear();
	        	predictedTraveledDistance = gpsTraveledDistance;
	        	numStaticPos = 0;
	            return null;
	        }
	        recentGpsPositions.Add(aLastGpsPos);
	        if (recentGpsPositions.Count > MAX_EXT_PREDICTED_POSITIONS) {
	        	recentGpsPositions.RemoveAt(0);
	        }
	        
	        
	        // Add predicted position for the next round
	        recentPredictedPositions.Add(nextPos);
	        // Keep queue within maximal size limit
	        Position firstToRemove = null;
	        if (recentPredictedPositions.Count > MAX_PREDICTED_POSITIONS) {
	        	firstToRemove = recentPredictedPositions[0]; 
	        	recentPredictedPositions.RemoveAt(0); 
	        }
	        // Fill input for interpolation
	        Position[] points = new Position[recentPredictedPositions.Count];
	        
	        int i = 0;
	        foreach (Position p in recentPredictedPositions) {
				Console.Out.WriteLine("PositionPredictor: ctl  position: " + PositionUtils.ToString(p));

	            points[i++] = p;
	            //System.out.printf("CTL POINTS: points[%d], %.15f,,%.15f, bearing: %f\n",	i, p.getLngx(), p.getLatx(), p.getBearing());
	        }
	        // interpolate points using spline
	        interpPath = interpolatePositions(points);
	        
	        lastGpsPosition = aLastGpsPos;
	        return recentPredictedPositions[recentPredictedPositions.Count-1];
	    }
	    
	    // Returns predicted position at a given timestamp
	    public Position predictPosition(long aDeviceTimestampMilliseconds) {
	        if (interpPath == null || recentPredictedPositions.Count < 3)
	        {
	            return null;
	        }
	        // Find closest point (according to device timestamp) in interpolated path
	        long firstPredictedPositionTs = recentPredictedPositions[0].device_ts;
	        int index = (int) ((aDeviceTimestampMilliseconds - firstPredictedPositionTs) * INV_DELTA_TIME_MS);
	        // Predicting only within current path
	        //System.out.printf("BearingAlgo::predictPosition: ts: %d, index: %d, path length: %d\n", aDeviceTimestampMilliseconds
	        //                   ,index, interpPath.length);   
	        if (index < 0 || index >= interpPath.Length) {
	            return null;
	        }
	        if(LOG_KML) kml.addPosition(GFKml.PathType.PREDICTION, interpPath[index]);
	
	        return interpPath[index];
	    }
	
	    public void stopTracking() {
			/* Dump KML */
	        if(LOG_KML) {
				string fileName = /*Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)*/ "/sdcard/Downloads/" + "track_" + 
					".kml";
	            //System.out.println("Dumping KML: " + fileName); 
				System.IO.StreamWriter fileStream = new System.IO.StreamWriter(fileName);
				kml.write (fileStream.BaseStream);
	        } 
	        bearingCalculator.reset();
	    }
	    
	    // Returns bearing of the predicted position at given time
	    public float? predictBearing(long aDeviceTimestampMilliseconds) {
	        Position pos = predictPosition(aDeviceTimestampMilliseconds);
	        if (pos == null)
	        {
	            return null;
	        }
	        return pos.bearing;
	    }
	    
	    // Extrapolate (predict) position based on last positions given time ahead
	    private Position extrapolatePosition(Position aLastPos, long timeSec) {
	    	// Simple method - calculate based on speed and bearing of last position
	    	// TODO: return new Position(PositionUtils.predictPosition(aLastPos, timeSec*1000));
			return PositionUtils.predictPosition(aLastPos, timeSec*1000);
	    }
	    
	    private Position[] interpolatePositions(Position[] ctrlPoints) {
	    	if (!constrainControlPoints(ctrlPoints)) {
	            // TODO: avoid conversion to array
	    		return CardinalSpline.create(ctrlPoints).ToArray();
	    	} else {
	    		// TODO: return ConstrainedCubicSpline.create(ctrlPoints);
	    		return CardinalSpline.create(ctrlPoints).ToArray();
	    	}
	    }
	    
	    private bool constrainControlPoints(Position[] pts) {
	    	float prevDistance = (float) PositionUtils.distanceBetween(pts[0], pts[1]);
	    	if (prevDistance == 0) {
	    		return false;
	    	}
	    	for (int i = 1; i < pts.Length; ++i) {
	    		float distance = (float) PositionUtils.distanceBetween(pts[i], pts[i-1]);
	    		float ratio = distance/prevDistance;
	    		//System.out.printf("constrainControlPoints i = %d, ratio: %f\n", i, ratio);
	    		if (ratio >= 8.0 || ratio <= 0.125) {
	    			return true;
	    		}
	    		prevDistance = distance;
	    	}
	    	return false;
	    }
	
	    
	    // Update calculations for predicted and real traveled distances
	    // TODO: unify distance calculations with GpsTracker distance calculations
	    private void updateDistance(Position aLastPos) {

	        Position prevPredictedPos = recentPredictedPositions[recentPredictedPositions.Count-2];        
	        double distancePredicted = PositionUtils.distanceBetween(prevPredictedPos, 
				recentPredictedPositions[recentPredictedPositions.Count-1]);
	        predictedTraveledDistance += distancePredicted;	
	        
	        double distanceReal = PositionUtils.distanceBetween(lastGpsPosition, aLastPos);
	        gpsTraveledDistance += distanceReal;
	    }
	    
	    private void correctLastPredictedPosition(Position aLastGpsPos) {
	        // correct last (predicted) position with last GPS position
	        lastPredictedPos = recentPredictedPositions[recentPredictedPositions.Count-1];
	        lastPredictedPos.bearing = bearingCalculator.calcBearing(aLastGpsPos);
	        lastPredictedPos.device_ts = aLastGpsPos.device_ts;
	        lastPredictedPos.gps_ts = aLastGpsPos.gps_ts;        
	        lastPredictedPos.speed = calcCorrectedSpeed(aLastGpsPos);
	    }
	    
	
	    private float calcCorrectedSpeed(Position aLastPos) {
	    	// Do not correct position below threshold
	    	if (aLastPos.speed < SPEED_THRESHOLD_MS) {
	    		return aLastPos.speed;
	    	}
	    	double offset = (gpsTraveledDistance - predictedTraveledDistance);
	    	/*System.out.printf("GPS DIST: %f, EST DIST: %f, OFFSET: %f\n" , 
	    			gpsTraveledDistance,predictedTraveledDistance, offset);
			*/
	        double coeff = (offset > 0 ) ? 0.3 : -0.3;        
	        coeff = Math.Abs(offset) <= aLastPos.speed ? offset/aLastPos.speed : coeff;
	
	        double correctedSpeed = aLastPos.speed*(1 + coeff);
	        
	        // System.out.printf("SPEED: %f, CORRECTED SPEED: %f, DISTANCE COEFF: %f\n",aLastPos.getSpeed(), correctedSpeed, coeff);
	        return (float) correctedSpeed;
	    	
	    }
	   
	
	}
}

