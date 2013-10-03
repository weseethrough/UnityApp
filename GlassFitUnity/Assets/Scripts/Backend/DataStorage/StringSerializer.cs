using UnityEngine;
using System.Collections;
using System;
using System.Text;

public struct StringSerializer 
{
    private string data;

    //do not modify value below unless you extended string processsing functions and comments to support different string size
    static private int maxLengthSupported = 0x7FFF;

    public string GetString()
    {
        return data;
    }

    public void SetString(string s)
    {
        if (s.Length > maxLengthSupported)
        {
            Debug.LogWarning("string serializer doesnt support strings longer than " + maxLengthSupported);
        }

        data = s;
    }

    public byte[] GetSource()
    {
        byte[] destination;
        int startingByte = 0;

        //we allow strings up to 2^15 long
        //and we store lenght of them in 1 byte unless they are longer than 128.
        //in other case we use first bit to define it is two-bit long length definition and use rest (15 bytes) to define exact length
        if (data.Length > maxLengthSupported)
        {
            Debug.LogError("string derializer can't work at the moment with strings above 2^15 bytes long.");
            return null;
        }
        else if (data.Length > 0x7F)
        {
            destination = new byte[data.Length + 2];
            //(1 << 8) marks it is two byte long data 
            //next we are writing only size which would not fit in next byte. 
            //thanks to previous tests we know it would be (1 to 0x7F)
            destination[0] = (byte)((1 << 8) | (data.Length >> 8));
            destination[1] = (byte)(data.Length & 0xFF);
            startingByte = 2;
        }        
        else
        {
            destination = new byte[data.Length + 1];
            //we know lenght is smaller than 0x7F so it fits in one byte and leave 0 on first bit.
            destination[0] = (byte)(data.Length);
            startingByte = 1;
        }

        System.Buffer.BlockCopy(data.ToCharArray(), 0, destination, startingByte, data.Length);

        return destination;
    }

    public void SetSource(byte[] source, int position)
    {
        if (source.Length < position + 2)
        {
            Debug.LogWarning("Warning!! Reading string from source at position "+position+" of "+source.Length);
            return;
        }

        int length;
        //check if first char is bigger than 0. If it is then we use two bytes long lenght definition
        if ((source[position] >> 7) > 0)
        {
            length = ((source[position] & 0x7F) << 8) | source[position + 1];
            position += 2;
        }
        else
        {
            length = source[position];
            position += 1;
        }

        if (source.Length > position + length)
        {
            data = System.Text.Encoding.UTF8.GetString(source, position, length);
        }
        else
        {
            Debug.LogError("string doesnt exist in byte array, expected minimum: " + (int)(position + length) + " in byte array, found: " + source.Length);
        }

    }

}
