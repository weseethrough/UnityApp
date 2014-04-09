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
	        document.setFeatureList( new Vector<Feature>());
	        document.setStyleSelector(new ArrayList<StyleSelector>());
	
	        kml.setFeature(document);
	    }
	            
	    // Add position to KML as a placemark. The path is created on the first call
	    public bool addPosition(PathType pathType, Position pos) {
	        if (pathMap.get(pathType) == null) {
	            startPath(pathType);
	        }
	        
	        // Position mark holds all data about a position 
	        PositionMark pm = new PositionMark(pos);
	        // Add placemark to the current path        
	        pathMap.Item[pathType].addPlacemark(pm.getPlacemark());
	        return true;
	
	    }  
	    
	    // Write KML file
	  /*  public void write(java.io.OutputStream out) {
	        Serializer serializer = new Serializer();
	        serializer.write(kml, out);
	    } */
	    
	    // Start new position's path. Sequential calls to addPosition will add positions
	    // to this path
	    private void startPath(PathType pathType) {
	        Path path = new Path(document, pathType);
	        pathMap.Add(pathType, path);
	        path.initStyles(document.getStyleSelector());
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
	            folder.setName(pathType.pathName());
	            folder.setFeatureList(new Vector<Feature>());
	            doc.getFeatureList().add(folder);
	            // Init styles
	            this.pathType = pathType;
	
	        }
	        // Adds placemark (position) to the path
	        public void addPlacemark(Placemark pm) {
	            // Set style & name
	            pm.setStyleUrl("#" + style);
	            pm.setName("Point " + Integer.toString(++positionId));
	            // Add placemark to the current path        
	            folder.getFeatureList().add(pm);
	        }
	
	        // Initialize KML styles for the path 
	        public void initStyles(List<StyleSelector> styleSelector) {
	            Pair normStylePair = initStyle(pathType.color(), "normalPositionStyle" + pathType.colorName(), false);
	            Pair hiliStylePair = initStyle(pathType.color(), "hiliPositionStyle" + pathType.colorName(), true);
	            
	            StyleMap styleMap = new StyleMap();
	            styleId = "generalPositionStyle" + pathType.colorName();
	            styleMap.setId(style);
	            // TODO: bug in XML writing of StyleMap, using normal style instead
	            style = "normalPositionStyle"  + pathType.colorName();
	            List<Pair> lp = new ArrayList<Pair>();
	            lp.add(normStylePair);
	            lp.add(hiliStylePair);
	            styleMap.setPairList(lp);
	            
	            styleSelector.add(normStylePair.getStyle());
	            styleSelector.add(hiliStylePair.getStyle());
	            styleSelector.add(styleMap);
	            
	            // FIXME: workaround to avoid duplicated styles
	            normStylePair.setStyle(null);
	            hiliStylePair.setStyle(null);            
	        }
	        
	        private Pair initStyle(String color, String id, bool isHiLi) {
	            String href;
	            float scale;
	            String pairKey;
	            if (isHiLi) {
	                href = "http://maps.google.com/mapfiles/kml/shapes/arrow.png";
	                scale = (float)0.0;
	                pairKey = "highlight";
	                
	            } else {
	                href = "http://maps.google.com/mapfiles/kml/shapes/shaded_dot.png";
	                scale = pathType.scale();
	                pairKey = "normal";
	            }
	            
	            // Icon style
	            IconStyle is = new IconStyle();
	            is.setColor(color);
	            Icon ic = new Icon();        
	            ic.setHref(href);
	            is.setIcon(ic);
	            // Label style
	            LabelStyle ls = new LabelStyle();
	            ls.setScale(scale);
	            // Line style
	            LineStyle lis = new LineStyle();
	            is.setColor(color);
	            // Construct style itself
	            Style st = new Style();
	            st.setId(id);
	            st.setIconStyle(is);
	            st.setLabelStyle(ls);
	            st.setLineStyle(lis);
	            
	            Pair stylePair = new Pair();
	            stylePair.setKey(pairKey);
	            stylePair.setStyleUrl(id);
	            stylePair.setStyle(st);
	            return stylePair;
	            
	        }
	    }
	    
	    // The class represents single placemark initialized from position
	    private class PositionMark {
	        private Placemark placemark = new Placemark();
	        private Position position;
	        
	        //private SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
	        
	        PositionMark(Position pos) {
	            this.position = pos;
	            // Fill placemark with position's details
	            addTime();
	            addGeometry(); 
	            addDisplayData();
	        }
	        
	        public Placemark getPlacemark() { return placemark; }
	        
	        private void addTime() {
	            // Add timestamp. TODO: choose between Gps and Device timestamp according to path type
	            Date date = new Date();
	            date.setTime(position.getDeviceTimestamp());
	            TimeStamp ts = new TimeStamp();
	            //ts.setWhen(dateFormat.format(date));
	            placemark.setTimePrimitive(ts);
	        }
	        
	        // Define geometry of the placemark
	        private void addGeometry() {
	            // Geometry list of the placemark
	            List<Geometry> lg = new Vector<Geometry>();
	            placemark.setGeometryList(lg);
	            // Multigeometry will hold point and heading line
	            MultiGeometry mg = new MultiGeometry();
	            mg.setGeometryList(new Vector<Geometry>());
	            lg.add(mg);
	            // Add point
	            Point pt = new Point();
	            pt.setCoordinates(positionToCoordinate(position));
	            mg.getGeometryList().add(pt);
	            // Add heading as a line from given to predicted position
	            LineString heading = addHeading();
	            if (heading != null) {
	                mg.getGeometryList().add(heading);
	            }
	        }
	        
	        // Draw line from current towards predicted position
	        private LineString addHeading() {
	            Position predictedPos = Position.predictPosition(position, 300); // milliseconds
	            if (predictedPos == null) {
	                return null;
	            }
	            ArrayList<Coordinate> coordList = new ArrayList<Coordinate>();
	            coordList.add(positionToCoordinate(position));
	            coordList.add(positionToCoordinate(predictedPos));
	
	            Coordinates coords = new Coordinates(positionToString(position));
	            coords.setList(coordList);
	            
	            LineString ls = new LineString();        
	            ls.setCoordinates(coords);
	            return ls;
	        }
	        
	        // Add data displayed in the popup when clicking on the placemark
	        private void addDisplayData() {
	            List<Data> ld = new ArrayList<Data>();
	            
	            Data d = new Data();
	
	            d.setDisplayName("DeviceTs");
	            //d.setValue(dateFormat.format(position.getDeviceTimestamp()));
	            ld.add(d);
	            
	            d = new Data();
	            d.setDisplayName("Speed");
	            d.setValue(Float.toString(position.getSpeed()));
	            ld.add(d);
	
	            if (position.getEpe() != null) {
	                d = new Data();
	                d.setDisplayName("Accuracy");
	                d.setValue(Float.toString(position.getEpe()));
	                ld.add(d);
	            }
	            
	            if (position.getBearing() != null) {
	            	d = new Data();
	                d.setDisplayName("Bearing");
	                d.setValue(Float.toString(position.getBearing()));
	                ld.add(d);	
	            }
	             
	            ExtendedData ed = new ExtendedData();
	            ed.setDataList(ld);
	            placemark.setExtendedData(ed);
	            
	        }
	        
	        private Point positionToCoordinate(Position pos) {
	            Point p = new Point();
				p.Coordinate = new Vector(pos.getLngx(), pos.getLatx(), pos.getAltitude());
				return p;
	        }
	    
	        private String positionToString(Position pos) {
	            return positionToCoordinate(pos).toString();
	        }
	
	    }
	    

	}
}

