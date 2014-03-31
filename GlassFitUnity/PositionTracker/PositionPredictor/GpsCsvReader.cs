using System;
using System.Collections.Generic;
using RaceYourself.Models;
using CSVFile;

namespace PositionPredictor
{
	public class GpsCsvReader
	{
		private CSVReader csvReader = null;
		// TODO: move to CSV reader class when it is moved out to a separate file
		private enum CsvField {
			LATX,
			LNGX,
			SPEED,
			BEARING,
			GPS_TS,
			DEVICE_TS,
			EPE,
			TRACK_ID
		};
		// Maps predefined fields into actual file indices
	    private Dictionary<CsvField, Int32> csvFieldMap = new Dictionary<CsvField, Int32>();
	    
	    // Previously read line
	    private String[] prevLine = null;
	    
	    
	    public GpsCsvReader(CSVReader csvReader) {
	    	this.csvReader = csvReader;
	    	// Read and parse CSV header
	    	String[] header = csvReader.NextLine();
	        trimLine(header);
	        parseCsvHeader(header);
	    }
	  
	    public Position readNextPosition() { 
	    	Position pos = new Position();
	        try {
				String[] line = csvReader.NextLine();
	            // Fill position with parsed line
				if (parsePositionLineRaceYourself(line, pos)) {
				    return pos;
				}
			} catch (Exception e) {
				// TODO Auto-generated catch block
			}
	        
	        return null;
	    }
	    
		private void parseCsvHeader(String[] aLine) {
			int i = 0;
			CsvField csvField;
			//System.out.println("HEADER: " + aLine.toString());
			foreach (String field in aLine) {
				//System.out.println("FIELD: " + field);
				++i;
				if (field.Equals("bearing"))
					csvField = CsvField.BEARING;
				else if (field.Equals("latx"))
					csvField = CsvField.LATX;
				else if (field.Equals("lngx"))
					csvField = CsvField.LNGX;
				else if (field.Equals("speed"))
					csvField = CsvField.SPEED;
				else if (field.Equals("gps_ts"))
					csvField = CsvField.GPS_TS;
				else if (field.Equals("device_ts"))
					csvField = CsvField.DEVICE_TS;
				else if (field.Equals("epe"))
					csvField = CsvField.EPE;
				else if (field.Equals("id"))
					csvField = CsvField.TRACK_ID;
				else {
					// Skipping other fields for now
					continue;
				}
				//System.out.println("FIELD: " + field + " --> " + i);
	
				csvFieldMap.Add(csvField, i-1);
	        }
	 	
		}

	    private bool isEqualPosition(String[] aLine1, String[] aLine2) {
	    	return aLine1[mapIndex(CsvField.LATX)].Equals(aLine2[mapIndex(CsvField.LATX)])
	    			&& aLine1[mapIndex(CsvField.LNGX)].Equals(aLine2[mapIndex(CsvField.LNGX)]);
	    }
	    
	    private bool parsePositionLineRaceYourself(String[] aLine, Position aPos) {
	        
	        if (aLine[mapIndex(CsvField.LATX)].Equals("") || aLine[mapIndex(CsvField.LNGX)].Equals("")) {
	            return false;
	        }
	        // Skip the same positions (indoor CSV contains not only GPS points)
	        if (prevLine != null && isEqualPosition(aLine, prevLine)) {
	        	return false;
	        }
	        // Parse line with lon/lat and speed  
	        aPos.longitude = (float)(Double.Parse(aLine[mapIndex(CsvField.LNGX)]));
	        aPos.latitude = (float)(Double.Parse(aLine[mapIndex(CsvField.LATX)]));
	
	        if (!aLine[mapIndex(CsvField.SPEED)].Equals("null")) {
	        	//System.out.println("SPEED: " + aLine[mapIndex(CsvField.SPEED)]);
	        	aPos.speed = (float)(Double.Parse(aLine[mapIndex(CsvField.SPEED)])); 
	        }
	        
	        if (!aLine[mapIndex(CsvField.BEARING)].Equals("")) {
	            aPos.bearing = (float)(Double.Parse(aLine[mapIndex(CsvField.BEARING)])); 
	        }
	        if (!aLine[mapIndex(CsvField.GPS_TS)].Equals("")) {
	        	aPos.gps_ts = (Int64.Parse(aLine[mapIndex(CsvField.GPS_TS)]));
	        }
	        if (!aLine[mapIndex(CsvField.DEVICE_TS)].Equals("")) {
	        	aPos.device_ts = (Int64.Parse(aLine[mapIndex(CsvField.DEVICE_TS)]));
	        }
	        if (!aLine[mapIndex(CsvField.EPE)].Equals("")) {
	        	aPos.epe = (float)(Double.Parse(aLine[mapIndex(CsvField.EPE)]));
	        }
	        prevLine = aLine;
	        return true;
	        
	    }
	
	    private void trimLine(String[] aLine) {
	        for (int i = 0; i < aLine.Length; ++i) {
	            aLine[i] = aLine[i].Trim();
	        }
	    }
		
		private Int32 mapIndex(CsvField field) {
			Int32 index;
			if(csvFieldMap.TryGetValue(field, out index))
				return index;
			return 0;
		}

	}
}

