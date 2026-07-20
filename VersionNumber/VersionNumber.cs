using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace E7.E7Unity
{
    public class VersionNumber : MonoBehaviour {

        public TextMeshProUGUI versionNumber;

    	void Awake() {
            if(versionNumber != null)
            {
                versionNumber.text = "Version " + Application.version;
            }
    	}

    }
}
