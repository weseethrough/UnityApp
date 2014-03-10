using System;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Device
	{
		[Index]
		[UniqueConstraint]
		public int _id;
		public string manufacturer;
		public string model;
		public string glassfit_version;
		public string push_id;
		[JsonIgnore]
		public bool self;

		public Device() {}
		public Device(string manufacturer, string model) {
			this._id = 0; // 
			this.self = true;
			this.glassfit_version = "1";
			this.manufacturer = manufacturer;
			this.model = model;
			this.push_id = null;
		}
	}
}

