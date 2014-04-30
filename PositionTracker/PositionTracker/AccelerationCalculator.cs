using System;
// TODO: using Assimp;
namespace PositionTracker
{
	public class AccelerationCalculator
	{
		private ISensorProvider sensorProvider;
		// TODO:
	    //private Matrix4x4 deviceToWorldTransform = new Matrix4x4(); // rotation matrix to get from device co-ords to world co-ords

		
		public AccelerationCalculator (ISensorProvider sensorProvider)
		{
			this.sensorProvider = sensorProvider;
			ForwardAcceleration = 0.0f;			
		}
		// TODO:
		public float ForwardAcceleration { get; set;}
		
		/**
	     * Computes the magnitude of the device's acceleration vector
     	* @return
     	*/
		public float TotalAcceleration { 
			get {
	        	float[] rawAcceleration3 = sensorProvider.LinearAcceleration;
    	    	return (float)Math.Sqrt(Math.Pow(rawAcceleration3[0],2) + Math.Pow(rawAcceleration3[1],2) + Math.Pow(rawAcceleration3[2],2));
			}
		}

		/**
     	* Computes the component of the device acceleration along a given axis (e.g. forward-backward)
     	* @param float[] (x,y,z) unit vector in real-world space
     	* @return acceleration along the given vector in m/s/s
     	*/
    	private float getAccelerationAlongAxis(float[] axisVector) {
        	float forwardAcceleration = dotProduct(getRealWorldAcceleration(), axisVector);
        	return forwardAcceleration;
    	}
		
	    /**
	     * Get the device's current acceleration in real-world space
	     * @return 3D acceleration vector in real-world co-ordinates
	     */
	    public float[] getRealWorldAcceleration() {
	        float[] rawAcceleration3 = sensorProvider.LinearAcceleration;
	        float[] rawAcceleration4 = {rawAcceleration3[0], rawAcceleration3[1], rawAcceleration3[2], 0};
	        float[] realWorldAcceleration4 = rotateToRealWorld(rawAcceleration4);
	        float[] realWorldAcceleration3 = {realWorldAcceleration4[0], realWorldAcceleration4[1], realWorldAcceleration4[2]};
	        return realWorldAcceleration3;
	    }
		
		
		
	    /**
	     * Compute the dot-product of the two input vectors
	     * @param v1d
	     * @param v2
	     * @return dot-product of v1 and v2
	     */
	    public static float dotProduct(float[] v1, float[] v2) {
	        if (v1.Length != v2.Length) {
	            return -1;
	        }
	        float res = 0;
	        for (int i = 0; i < v1.Length; i++)
	            res += v1[i] * v2[i];
	        return res;
	    }
		
		public float[] rotateToRealWorld(float[] inVec) {
        	float[] resultVec = new float[4];
        	// TODO: Matrix.multiplyMV(resultVec, 0, deviceToWorldTransform, 0, inVec, 0);
        	return resultVec;
    	}


	}
}

