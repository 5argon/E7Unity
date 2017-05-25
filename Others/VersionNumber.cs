using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VersionNumber : MonoBehaviour {

    public Text versionNumber;

	void Awake() {
        if(versionNumber != null)
        {
            versionNumber.text = "Version " + Application.version;
        }
	
	}
	
}
