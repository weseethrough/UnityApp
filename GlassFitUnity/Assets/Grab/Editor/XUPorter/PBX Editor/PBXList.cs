using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.GrabXCodeEditor
{
	public class PBXList : ArrayList
	{
		public PBXList()
		{
			
		}
		
		public PBXList( object firstValue )
		{
			this.Add( firstValue );
		}
	}
}
