using System;

public class Device
{
	public int id { get; protected set; }
	public string manufacturer { get; protected set; }
	public string model { get; protected set; }
	public int glassfit_version { get; protected set; }
		
	public Device ()
	{
	}
	public Device (int id, string manufacturer, string model, int glassfit_version) 
	{
		this.id = id;
		this.manufacturer = manufacturer;
		this.model = model;
		this.glassfit_version = glassfit_version;
	}
}

