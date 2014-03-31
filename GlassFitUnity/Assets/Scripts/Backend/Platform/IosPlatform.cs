using System;

#if UNITY_IPHONE
/// <summary>
/// Ios platform. Overrides platform functionality with iOS-specific functionality where necessary. Usually this means native iOS calls.
/// </summary>
public class IosPlatform : Platform
{

	public override Device DeviceInformation() 
	{
		return new Device("Apple", "iUnknown");
	}
}

#endif

