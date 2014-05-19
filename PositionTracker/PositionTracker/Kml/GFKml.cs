using System;
using System.Collections.Generic;

using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

using RaceYourself.Models;


namespace PositionPredictor
{
	public class GFKml
	{

		private Kml kml = new Kml();
	    // High-level KML document object
	    private Document document = new Document();
	    
		public class PathType
    	{
			
	        public static readonly PathType GPS = new PathType("ffff0000", "Blue", "GPS", 1.0f);
	        public static readonly PathType PREDICTION = new PathType("c0c0c0ff", "Grey", "Prediction", 0.3f);
	        public static readonly PathType EXTRAPOLATED = new PathType("f0f0f0ff", "Silver", "Extrapolated", 0.3f);
	
	
	        public static IEnumerable<PathType> Values
	        {
	            get
	            {
	                yield return GPS;
	                yield return PREDICTION;
	                yield return EXTRAPOLATED;

	            }
	        }
	
			private readonly String color;
        	private readonly String colorName;
        	private readonly String pathName;     
        	private readonly float scale;

	
	        PathType(string color, string colorName, string pathName, float scale)
	        {
	            this.color = color;
	            this.colorName = colorName;
	            this.pathName = pathName;
	            this.scale = scale;
	        }
	
	        public string Color { get { return color; } }
	        public string ColorName { get { return colorName; } }
	        public string PathName { get { return pathName; } }
	
	        public float Scale { get { return scale; } }
	
    	}

	
	    // Container which maps path type to Path object
	    private Dictionary<PathType, Path> pathMap = new Dictionary<PathType, Path>();
	    
	    public GFKml() { 
	        kml.Feature = document;
	    }
	            
	    // Add position to KML as a placemark. The path is created on the first call
	    public bool addPosition(PathType pathType, Position pos) {
	        if (!pathMap.ContainsKey(pathType)) {
	            startPath(pathType);
	        }
	        
	        // Position mark holds all data about a position 
	        PositionMark pm = new PositionMark(pos);
	        // Add placemark to the current path        
	        pathMap[pathType].addPlacemark(pm.getPlacemark());
	        return true;
	
	    }  
	    
	    // Write KML file
		public void write(System.IO.Stream outFile) {
			KmlFile kmlFile = KmlFile.Create(kml, true);
			kmlFile.Save (outFile);
	    } 
	    
	    // Start new position's path. Sequential calls to addPosition will add positions
	    // to this path
	    private void startPath(PathType pathType) {
	        Path path = new Path(document, pathType);
	        pathMap.Add(pathType, path);
	        // TODO: choose style
	    }
	             
	    
	    // The class represents positions path 
	    private class Path {
	    	// KML element encapsulating the path
	        private Folder folder;
	        // Path styles
	        private String styleId;        
	        private String style;
	        // Current position id. Incremented on every new position
	        private int positionId = 0;
	        private PathType pathType;
	        
	        
	        public Path(Document doc, PathType pathType) {
	            folder = new Folder();
				folder.Name = pathType.PathName;
				doc.AddFeature(folder);
				// TODO: initStyles(doc);

	            this.pathType = pathType;
	
	        }
	        // Adds placemark (position) to the path
	        public void addPlacemark(Placemark pm) {
	            // Set style & name
				pm.StyleUrl = new Uri("#" + style, UriKind.Relative);
				pm.Name = ("Point " + (++positionId).ToString());
	            // Add placemark to the current path        
	            folder.AddFeature(pm);
	        }
	
	        // Initialize KML styles for the path 
			public void initStyles(Document doc) {
				Pair normStylePair = initStyle(pathType.Color, "normalPositionStyle" + pathType.ColorName, false);
				Pair hiliStylePair = initStyle(pathType.Color, "hiliPositionStyle" + pathType.ColorName, true);
	            
				StyleMapCollection styleMap = new StyleMapCollection();
				styleId = "generalPositionStyle" + pathType.ColorName;
				styleMap.Id = styleId;
	            // TODO: bug in XML writing of StyleMap, using normal style instead
				style = "normalPositionStyle"  + pathType.ColorName;

				styleMap.Add(normStylePair);
				styleMap.Add(hiliStylePair);
	            
				doc.AddStyle(normStylePair.Selector);
				doc.AddStyle(hiliStylePair.Selector);
	            
	            // FIXME: workaround to avoid duplicated styles
				//normStylePair.setStyle(null);
				//hiliStylePair.setStyle(null);            
	        }
	        
	        private Pair initStyle(String color, String id, bool isHiLi) {
				string href;
	            float scale;
				string pairKey;
	            if (isHiLi) {
	                href = "http://maps.google.com/mapfiles/kml/shapes/arrow.png";
	                scale = (float)0.0;
	                pairKey = "highlight";
	                
	            } else {
	                href = "http://maps.google.com/mapfiles/kml/shapes/shaded_dot.png";
					scale = pathType.Scale;
	                pairKey = "normal";
	            }
	            
	            // Icon style
				IconStyle ics = new IconStyle();
				ics.Color = Color32.Parse(color);
				IconStyle.IconLink ic = new IconStyle.IconLink(new Uri(href, UriKind.Absolute));        
				ics.Icon = ic;

				// Label style
	            LabelStyle ls = new LabelStyle();
				ls.Scale = scale;
	            // Line style
	            LineStyle lis = new LineStyle();
				lis.Color = Color32.Parse(color);
	            // Construct style itself
				Style st = new Style();
				st.Id = id;
				st.Icon = ics;
				st.Label = ls;
				st.Line = lis;
	            
	            Pair stylePair = new Pair();
				stylePair.Id = pairKey;
				stylePair.StyleUrl = new Uri(id, UriKind.Relative);
				stylePair.Selector = st;
	            return stylePair;
	            
	        }
	    }
	    
	    // The class represents single placemark initialized from position
	    private class PositionMark {
	        private Placemark placemark = new Placemark();
	        private Position position;
	        
	        //private SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
	        
			public PositionMark(Position pos) {
	            this.position = pos;
	            // Fill placemark with position's details
	            addTime();
	            addGeometry(); 
	            addDisplayData();
	        }
	        
	        public Placemark getPlacemark() { return placemark; }
	        
	        private void addTime() {
	            // Add timestamp. TODO: choose between Gps and Device timestamp according to path type
				Timestamp ts = new Timestamp();
				ts.When = Date.FromUnixTime(position.device_ts);
				placemark.Time = ts;
	        }
	        
	        // Define geometry of the placemark
	        private void addGeometry() {
	            // Multigeometry will hold point and heading line
				MultipleGeometry mg = new MultipleGeometry();

	            // Add point
				Vector vec = new Vector();
				vec.Latitude = position.latitude;
				vec.Longitude = position.longitude;

				Point pt = new Point();
				pt.Coordinate = vec;
				mg.AddGeometry(pt);

				// Add heading as a line from given to predicted position
	            LineString heading = addHeading();
	            if (heading != null) {
					mg.AddGeometry(heading);
	            }
				placemark.Geometry = mg;
	        }
	        
	        // Draw line from current towards predicted position
	        private LineString addHeading() {
				Position predictedPos = PositionUtils.predictPosition(position, 300); // milliseconds
	            if (predictedPos == null) {
	                return null;
	            }

	            LineString ls = new LineString();  
				ls.Coordinates = new CoordinateCollection ();
				ls.Coordinates.Add(positionToCoordinate(position));
				ls.Coordinates.Add(positionToCoordinate(predictedPos));
	            return ls;
	        }
	        
	        // Add data displayed in the popup when clicking on the placemark
	        private void addDisplayData() {
				ExtendedData ed = new ExtendedData();
	            
	            Data d = new Data();
				d.DisplayName = "DeviceTs";
				d.Value = DateTime.FromFileTime(position.device_ts).ToShortDateString();
				ed.AddData(d);

				d = new Data ();
				d.DisplayName = "Speed";
				d.Value = position.speed.ToString();
				ed.AddData(d);

				d = new Data ();
				d.DisplayName = "Accuracy";
				d.Value = position.epe.ToString();
				ed.AddData(d);

				d = new Data ();
				d.DisplayName = "Bearing";
				d.Value = position.bearing.ToString();
				ed.AddData(d);

				placemark.ExtendedData = ed;
	            
	        }
	        
			private Vector positionToCoordinate(Position pos) {
				Vector p = new Vector();
				p.Latitude = pos.latitude;
				p.Longitude = pos.longitude;
				p.Altitude = pos.alt;
				return p;
	        }
	    
	        private String positionToString(Position pos) {
				return positionToCoordinate(pos).ToString();
	        }
	
	    }
	    

	}
}

