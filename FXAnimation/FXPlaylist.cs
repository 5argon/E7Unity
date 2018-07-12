using UnityEngine;
using System.Collections;

public class FXPlaylist : FXAnimation {

	[Header("Playlist Settings")]
	public FXAnimation[] playlist;
	[Range(1,5)]
	public int repeat = 1;
	[Range(0,2)]
	public float repeatInterval = 0.5f;
	public bool repeatForever;

	public override void Stop()
	{
		foreach(FXAnimation fxa in playlist)
		{
			fxa.Stop();
		}
		base.Stop();
	}

	protected override IEnumerator PlayRoutine()
	{
		for(int i = 0 ; i < repeat ; i++)
		{
			foreach(FXAnimation fss in playlist)
			{
				//All animations got played using remembered position (remembered on Awake()).
				fss.Play();
			}
			yield return new WaitForSeconds(repeatInterval);
			if(repeatForever)
			{
				i--	;
			}
		}
	}

	[ContextMenu("Auto Fill Playlist")]
	private void AutoFill()
	{
		FXAnimation[] allAnimations = GetComponentsInChildren<FXAnimation>();
		FXAnimation[] minusSelf = new FXAnimation[allAnimations.Length-1];
		for(int i = 1 ; i < allAnimations.Length ; i++)
		{
			minusSelf[i-1] = allAnimations[i];
		}
		playlist = minusSelf;
		Debug.Log("Playlist filled with " + minusSelf.Length + " FXAnimations.");
	}
}
