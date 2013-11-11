using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class ScreenRecorder : MonoBehaviour {
        public int width = 512;
        public int height = 256;
        
        private int fps = 0;
        private Texture2D tex;
        private RenderTexture rtex;
        private FileStream fs;
        private FileStream afs;
        private string path;
        private string filename;
        private string afilename;
        private byte[] buffer = null;
        private string status = "N/A";
        private WWW client = null;
        private long mb = 0;
        private long amb = 0;
        
        private int ch = -1;
        private long floats = 0;
        
        private byte[] audiobuffer = new byte[1024*1024*16];
                
        void OnAudioFilterRead(float[] data, int channels) {
                ch = channels;
        int rescaleFactor = 32767; //to convert float to Int16
        byte[] byteArr; 

                for (int i=0; i<data.Length; i++) {
            Int16 integer = (Int16)(data[i]*rescaleFactor);
            byteArr = BitConverter.GetBytes(integer);    
            byteArr.CopyTo(audiobuffer,floats + i*2);  
                }
                floats += data.Length*2;
        }
                
        void Start() {                
        tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                rtex = new RenderTexture(width, height, 16);
                rtex.isPowerOfTwo = true;
                rtex.Create();        
                
                path = Path.Combine(Application.persistentDataPath, "video");
                Directory.CreateDirectory(path);
                Debug.LogWarning("Video path: " + path);
                filename = Path.Combine(path, DateTime.Now.ToString("dd-MM-yyyy") + "video.raw");
                afilename = Path.Combine(path, "audio.raw");
                Init();
        }
        
        void Init() {
                File.Delete(filename);
                File.Delete(afilename);
                fs = new FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
                afs = new FileStream(afilename, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
                status = "Start!";
        }
        
        void Update() {                
                DateTime start = DateTime.Now;
                foreach (Camera camera in Camera.allCameras) camera.targetTexture = rtex;
                RenderTexture.active = rtex;                
                
                foreach (Camera camera in Camera.allCameras) camera.Render();
                tex.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0);
                tex.Apply();
                Color[] pixels = tex.GetPixels();
                
                if (status == "Recording") {
                        if (buffer == null || buffer.Length != pixels.Length*3) buffer = new byte[pixels.Length*3];
                        for (int i=0; i<pixels.Length; i++) {
                                buffer[i*3+0] = (byte)(pixels[i].r*255);
                                buffer[i*3+1] = (byte)(pixels[i].g*255);
                                buffer[i*3+2] = (byte)(pixels[i].b*255);
                        }
                        fs.Write(buffer, 0, buffer.Length);
                        // Potential race condition
                        afs.Write(audiobuffer, 0, (int)floats);
                } 
                floats = 0;
                
                TimeSpan diff = DateTime.Now - start;                        
                if (diff.Milliseconds > 0) fps = 1000/diff.Milliseconds;
                RenderTexture.active = null;
                foreach (Camera camera in Camera.allCameras) camera.targetTexture = null;
        }
        
        void OnGUI() {
                if (client != null && status == "Uploading") {
                        if (client.isDone) Init();
                }
                
                if (fs != null) mb = fs.Length/1000/1000;
                if (afs != null) amb = afs.Length/1000/1000;
                GUI.Box(new Rect(Screen.width/2-50, 100, 100, 100), "" + fps + "fps " + mb + "MB");
                GUI.Box(new Rect(Screen.width/2-50-150, 100, 100, 100), "" + AudioSettings.outputSampleRate/1000 + "kHz " + ch + "ch " + amb + "MB");
//                GUI.DrawTexture(new Rect(Screen.width/2-256, 200, 512, 256), tex);
                if (GUI.Button(new Rect(Screen.width/2-150, 200, 300, 300), status)) {
                        if (status == "Start!") {
                                status = "Recording";
                        } else if (status == "Recording") {
                                fs.Flush();
                                fs.Close();
                                afs.Flush();
                                afs.Close();
                                
                                fs = null;
                                afs = null;
                                status = "Saved!";
//                                Platform.Instance.Encode(tex.GetNativeTextureID());
				}
//                        } else {
//                                WWWForm post = new WWWForm();
//                                post.AddBinaryData("raw", File.ReadAllBytes(filename), "video.raw", "video/raw");
//                                client = new WWW("http://glassfit.dannyhawkins.co.uk/upload", post);
//                                status = "Uploading";
//                        }
                }
        }
        
        void OnDestroy() {
                if (fs != null) {
                        fs.Close();
                        fs = null;
                }
                if (afs != null) {
                        afs.Close();
                        afs = null;
                }
        }        
}
