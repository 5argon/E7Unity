using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FXTester : MonoBehaviour {

	public FXAnimation effect;
	private Vector3 remember;

	void Start()
	{
		remember = effect.transform.position;
	}

	void Update () {

		if(Input.GetKeyDown(KeyCode.Z))
		{
			effect.Play(remember,false,false,false);
		}
		if(Input.GetKeyDown(KeyCode.X))
		{
			effect.Stop();
		}
	
	}
}
