using System;
using RaceYourself.Models;
using System.Collections.Generic;

namespace PositionPredictor
{
	public class CardinalSpline {  /**
		   * Increment NPOINTS for better resolution (lower performance).
		   */
		  private static int NPOINTS = 30;
		  private static int DELTA_MS = 1000 / NPOINTS;
		  
		  
		  private static double[] B0;
		  private static double[] B1;
		  private static double[] B2;
		  private static double[] B3;
		
		  private static  void initialize()
		  {
		    if ( B0 == null )
		    {
		      B0 = new double[ NPOINTS ];
		      B1 = new double[ NPOINTS ];
		      B2 = new double[ NPOINTS ];
		      B3 = new double[ NPOINTS ];
		      double deltat = 1.0 / (NPOINTS-1);
		      double t = 0.0;
		      double t1, t12, t2 = 0.0;
		      for( int i=0; i<NPOINTS; i++ )
		      {
		        t1 = 1-t;
		        t12 = t1*t1;
		        t2 = t*t;
		        B0[i] = t1*t12;
		        B1[i] = 3*t*t12;
		        B2[i] = 3*t2*t1;
		        B3[i] = t*t2;
		        t+=deltat;
		      }
		    }
		  }
		
		  /**
		   * Creates a GeneralPath representing a curve connecting different
		   * points.
		   * @param points the points to connect (at least 3 points are required).
		   * @return a GeneralPath that connects the points with curves.
		   */
		  public static List<Position> create( Position[] points)
		  {
		    initialize();
		    if ( points.Length <= 2 )
		    {
		      throw new System.ArgumentException("At least 3 points are required to build a CardinalSpline");
		    }
		    // TODO: avoid new array allocation
		    Position [] p = new Position[ points.Length + 2 ];
		    List<Position> path = new List<Position>();
			path.AddRange(points);
		    //System.arraycopy( points, 0, p, 1, points.Length );
		    calcBoundaryPositions(p);
		
		    //System.out.printf("\nPNT[0]: %f,%f\n" ,p[0].latitude, p[0].longitude);
		    //System.out.printf("PNT[N+1]: %f,%f\n" ,p[points.length+1].latitude, p[points.length+1].longitude);
		
		    path.Add( p[1]);
		    Position prevToLast = p[0];
		    for( int i=1; i<p.Length-2; i++ )
		    {
		      for( int j=0; j<NPOINTS; j++ )
		      {
		        double x = p[i].longitude * B0[j]
		                 + (p[i].longitude+(p[i+1].longitude-p[i-1].longitude)*0.1666667)*B1[j]
		                 + (p[i+1].longitude-(p[i+2].longitude-p[i].longitude)*0.1666667)*B2[j]
		                 + (p[i+1].longitude*B3[j]);
		        double y = p[i].latitude * B0[j]
		                 + (p[i].latitude+(p[i+1].latitude-p[i-1].latitude)*0.1666667)*B1[j]
		                 + (p[i+1].latitude-(p[i+2].latitude-p[i].latitude)*0.1666667)*B2[j]
		                 + (p[i+1].latitude*B3[j]);
		        Position pos = new Position();
		        pos.longitude = (float)x;
		        pos.latitude = (float)y;
		        // Interpolate timestamps
		        int deltaTimeMilliseconds = j*DELTA_MS;
		        pos.gps_ts = (p[i].gps_ts + deltaTimeMilliseconds);
		        pos.device_ts = (p[i].device_ts + deltaTimeMilliseconds);
		        // TODO: interpolate speed
		        pos.speed = p[i].speed;
		        // Calculate bearing of last position in path
		        float bearing = calcBearing(prevToLast, pos);
		        //System.out.printf("SPLINE BEARING: %f\n" ,bearing);
		        path[path.Count-1].bearing = bearing;        
		        prevToLast = path[path.Count-1];
		        path.Add(pos);
		      }
		    }
		    // Calculate bearing for last position in path
		    path[path.Count-1].bearing = calcBearing(prevToLast, p[p.Length-1]);
		    
		    return path;
		  }
		  // Calculate bearing for p1 based on previous (p0) and next (p2) points
		  private static float calcBearing(Position p0, Position p2) {
			  // TODO: use tightness for more precise calculation
			  //return Bearing.calcBearing(p0, p2);	  
		      // Interpolate bearing. TODO: check if tightness is required here 
		      float bearing = (float)PositionUtils.ToDegrees(/*TIGHTNESS * */PositionUtils.calcBearingInRadians(p0, p2)) % 360;
		      //System.out.printf("SPLINE BEARING BEFORE NORM: %f\n" ,bearing);
		
		      return PositionUtils.normalizeBearing(bearing);
		      
		  }
		 
		  private static void calcBoundaryPositions(Position[] p) {
			    p[0] = new Position();
			    p[p.Length-1] = new Position();
			    
			    p[0] = p[1]; 
			    p[p.Length-1] = p[p.Length-2];
			    
		  }
		  
		  // Returns number of interpolated points in between control points
		  public static int getNumberPoints() {
		      return NPOINTS;
		  }

	}
}

