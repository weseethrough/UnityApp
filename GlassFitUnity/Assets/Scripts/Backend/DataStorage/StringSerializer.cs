using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;

public class StringSerializer 
{
    private string data;

    //do not modify value below unless you extended string processsing functions and comments to support different string size
    static private int maxLengthSupported = 0x7FFF;
	
	public StringSerializer()
	{		
	}
	
	public StringSerializer(string s)
	{
		SetString(s);	
	}
	
	public StringSerializer(MemoryStream reader)
	{
		ReadFromStream(reader);
	}
	
    public string GetString()
    {
        return data;
    }

    public void SetString(string s)
    {
        if (s.Length >= maxLengthSupported)
        {
            Debug.LogWarning("string serializer doesnt support strings longer than " + maxLengthSupported);
        }

        data = s;
    }

    public void WriteToStream(MemoryStream stream)
    {   

	    StreamWriter writer = new StreamWriter(stream);
	    
        //we allow strings up to 2^15 long
        //and we store length of them in 1 byte unless they are longer than 128.
        //in other case we use first bit to define it is two-bit long length definition and use rest (15 bytes) to define exact length
        if (data.Length > maxLengthSupported)
        {
            Debug.LogError("string serializer can't work at the moment with strings above 2^15 bytes long.");
            return;
        }
        else if (data.Length > 0x7F)
        {
            
            //(1 << 8) marks it is two byte long data 
            //next we are writing only size which would not fit in next byte. 
            //thanks to previous tests we know it would be (1 to 0x7F)
            writer.BaseStream.WriteByte((byte)((1 << 8) | (data.Length >> 8)));
            writer.BaseStream.WriteByte((byte)(data.Length & 0xFF));            
        }        
        else
        {            
            //we know length is smaller than 0x7F so it fits in one byte and leave 0 on first bit.
            writer.BaseStream.WriteByte((byte)(data.Length));            
        }

        writer.Write(data);			
		writer.Flush();
    }

    public void ReadFromStream(MemoryStream stream)
    {   
		if (stream == null || stream.Length < 2)
		{
			Debug.LogError("string doesn't exist in source");
			return;
		}
		
        int length;
        int firstByte = stream.ReadByte(); 
        
        //check if first char is bigger than 0. If it is then we use two bytes long length definition
        if ((firstByte >> 7) > 0)
        {
            int secondByte = (byte)stream.ReadByte();
            length = ((firstByte & 0x7F) << 8) | secondByte;            
        }
        else
        {
            length = firstByte;            
        }

        if (stream.Length >= stream.Position + length)
        {            
			byte[] stringBytes = new byte[length + 1];
			stream.Read(stringBytes, 0, length);
			//Buffer.BlockCopy(streamBuffer, (int)stream.Position, stringBytes, 0, length);
			stringBytes[length] = 0;
			
            data = System.Text.Encoding.UTF8.GetString(stringBytes);
			Debug.Log(data);
        }
        else
        {
            Debug.LogError("string doesnt exist in byte array, expected minimum: " + (int)(stream.Position + length) + " in byte array, found: " + stream.Length);
        }

    }

}
