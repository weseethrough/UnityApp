using UnityEngine;
using System.Collections;

public class ChallengeMaster  {

	public string name = string.Empty;
	public string description = string.Empty;
	public string imageId = string.Empty;
	public string type = string.Empty;

	public ChallengeMaster() {}

	public ChallengeMaster(string name, string description, string imageId, string type) {
		this.name = name;
		this.description = description;
		this.imageId = imageId;
		this.type = type;
	}
}
