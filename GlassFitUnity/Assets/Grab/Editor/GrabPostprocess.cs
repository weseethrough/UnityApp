using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.GrabXCodeEditor;
using System.IO;

namespace UnityEditor.GrabXCodeEditor
{
	public static class XCodePostProcess
	{
		[PostProcessBuild(99)]
		public static void OnPostProcessBuild( BuildTarget target, string path )
		{
			if (target != BuildTarget.iPhone) {
				Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
				return;
			}

			// Create a new project object from build target
			XCProject project = new XCProject( path );

			// Find and run through all projmods files to patch the project.
			string projModPath = System.IO.Path.Combine (Application.dataPath, "Grab/Editor/iOS");
			string[] files = Directory.GetFiles( projModPath, "*.projmods", SearchOption.AllDirectories );
			foreach( string file in files ) {
				project.ApplyMod( file );
			}
			project.Save();
		}
	}
}
