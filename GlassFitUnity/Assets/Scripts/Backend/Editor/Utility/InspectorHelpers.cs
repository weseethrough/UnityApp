using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

/// <summary>
/// collection of static functions used in inspectors for drawing custom types or customized ways of displaying regular types. 
/// Eg split millisecond integer into hour, minutes, seconds and miliseconds for display and edit but store it still only in single integer.
/// </summary>
public class InspectorHelpers
{
    static private int msToHour = 60 * 60 * 1000;
    static private int msToMinute = 60 * 1000;
    static private int msToSecond = 1000;

    /// <summary>
    /// Function to display hours, minutes, seconds and miliseconds of the millisecond integer. It returns modified by the user time converting it back to miliseconds from all 4 components.
    /// </summary>
    /// <param name="label">label to be shown for this component</param>
    /// <param name="miliseconds">number of miliseconds to be converted into time</param>
    /// <returns>number of the miliseconds of the time displayed (it could be altered by typing)</returns>
    static public int DrawTime(string label, double miliseconds)
    {        
        EditorGUILayout.BeginHorizontal();            
            int hours = (int)(miliseconds / msToHour);
            int minutes = (int)(miliseconds / msToMinute) % 60;
            int seconds = (int)(miliseconds / msToSecond) % 60;
            int miliSeconds = (int)miliseconds % 1000;

            EditorGUILayout.LabelField(label);
            EditorGUILayout.LabelField("hr:", GUILayout.MaxWidth(20.0f));
            string hr = EditorGUILayout.TextField(hours.ToString("D2"), GUILayout.MaxWidth(30.0f));
            EditorGUILayout.LabelField("m:", GUILayout.MaxWidth(20.0f));
            string m = EditorGUILayout.TextField(minutes.ToString("D2"), GUILayout.MaxWidth(30.0f));
            EditorGUILayout.LabelField("s:", GUILayout.MaxWidth(20.0f));
            string s = EditorGUILayout.TextField(seconds.ToString("D2"), GUILayout.MaxWidth(30.0f));
            EditorGUILayout.LabelField(".", GUILayout.MaxWidth(10.0f));
            string ms = EditorGUILayout.TextField(miliSeconds.ToString("D3"), GUILayout.MaxWidth(30.0f));
            
            hours       = ConvertStringBack(hr, 2);
            minutes     = ConvertStringBack(m, 2);
            seconds     = ConvertStringBack(s, 2);
            miliSeconds = ConvertStringBack(ms, 3);
            
        EditorGUILayout.EndHorizontal();
        return hours * msToHour + minutes * msToMinute + seconds * msToSecond + miliSeconds;
    }
    
    static public int cmToKm = 100 * 1000;
    static public int cmToM = 100;

    /// <summary>
    /// draws custom distance inspector splitting double distance into cm, meters and kilometers.
    /// </summary>
    /// <param name="label">label of the ispector display for this value</param>
    /// <param name="distance">value to be processed for display</param>
    /// <returns></returns>
    static public int DrawDistance(string label, double distance)
    {
        EditorGUILayout.BeginHorizontal();
            int km = (int)(distance / cmToKm);
            int m = (int)(distance / cmToM) % 1000;
            int cm = (int)distance % 100;

            EditorGUILayout.LabelField(label);            
            EditorGUILayout.LabelField("km:", GUILayout.MaxWidth(30.0f));
            string sKM = EditorGUILayout.TextField(km.ToString(), GUILayout.MaxWidth(30.0f));
            EditorGUILayout.LabelField(" m:", GUILayout.MaxWidth(30.0f));
            string sM = EditorGUILayout.TextField(m.ToString("D3"), GUILayout.MaxWidth(30.0f));
            EditorGUILayout.LabelField(" cm:", GUILayout.MaxWidth(30.0f));
            string sCM = EditorGUILayout.TextField(cm.ToString("D2"), GUILayout.MaxWidth(30.0f));

            km = ConvertStringBack(sKM, 3);
            m = ConvertStringBack(sM, 3);
            cm = ConvertStringBack(sCM, 2);            
        EditorGUILayout.EndHorizontal();
        return km * cmToKm + m * cmToM + cm;
    }

    /// <summary>
    /// converts string into integer accepting certain maximum number of characters.
    /// </summary>
    /// <param name="data">string to be converted</param>
    /// <param name="maxLength">maximum number of the caracters to be converted</param>
    /// <returns>integer represented within selected number of the characters in string</returns>
    static private int ConvertStringBack(string data, int maxLength)
    {
        if (data.Length > maxLength)
        {
            return Convert.ToInt32(data.Substring(0, maxLength));
        }
        else if (data.Length == 0)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(data);
        }
    }

}