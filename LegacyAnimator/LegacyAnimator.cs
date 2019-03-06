using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx.Async;
using System.Threading;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

public class LegacyAnimator : MonoBehaviour {

#pragma warning disable 0649

#if ODIN_INSPECTOR
	[ValueDropdown("LimitToTriggers")]
#endif
	[Tooltip("Sample the first frame of this trigger on Start()")]
	[SerializeField]
	private string waitTrigger;

#if ODIN_INSPECTOR
	[ValueDropdown("LimitToTriggers")]
#endif
	[Tooltip("Immediately trigger this on Start()")]
	[SerializeField]
	private string autoplayTrigger;

	[Space]
	[SerializeField]
	private LegacyAnimatorNode[] nodes;
	
#pragma warning restore 0649

	private Dictionary<string,LegacyAnimatorNode> nodeSearch;

	private Dictionary<string,bool> variableBool;
	private Animation animationComponent;

	//When chaining, this will accumulates and can be used to automatically disable the component
	private float cumulativePlayTime;
	private string firstLayerCurrentlyPlaying;
	private string secondLayerCurrentlyPlaying;
	private const string waitClipName = "WAIT_CLIP";

    public List<string> LimitToTriggers()
    {
        if (nodes != null)
        {
            List<string> list = nodes.Select(n => n.Trigger).ToList();
            list.Add("");
            return list;
        }
        else
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Kills all animations and play a new one.
    /// </summary>
    public LegacyAnimator SetTrigger(string triggerName)
    {
		Prepare();
		CheckTrigger(triggerName);

		if(animationComponent.enabled == false)
		{
			animationComponent.enabled = true;
			cumulativePlayTime = 0;
		}

		//If it is layered, the play time needs to accommodate the longest clip.
		float playTime = SpeedAdjust(triggerName);

		//If already higher, there might be a longer clip running in other layer.
		if(cumulativePlayTime < playTime)
		{
			cumulativePlayTime = playTime;
		}

        StopBeforePlayLogic(animationComponent[triggerName]);
		//Stop same layer does not work lol...
        animationComponent.Play(triggerName, PlayMode.StopSameLayer);

		// WrapMode wrapMode = animationComponent[triggerName].wrapMode;
		// if(wrapMode != WrapMode.Loop && wrapMode != WrapMode.PingPong)
		// {
		// 	AutoDisable();
		// }
		// else
		// {
		// 	StopPreviousAutoDisable();
		// }

		return this;
	}

    /// <summary>
    /// Wait equal to trigger's play time. It does not matter if it is currently playing or not. It just wait for this fixed time.
    /// </summary>
    public async UniTask WaitForTrigger(string triggerName, CancellationToken cancellationToken= default(CancellationToken)) => await UniTask.Delay((int)(animationComponent[triggerName].length * 1000), cancellationToken: cancellationToken);

    /// <summary>
    /// Wait equal to trigger's play time. It does not matter if it is currently playing or not. It just wait for this fixed time.
    /// </summary>
    public async UniTask WaitForTrigger(string triggerName, float timeModification, CancellationToken cancellationToken = default(CancellationToken)) => await UniTask.Delay((int)((animationComponent[triggerName].length + timeModification) * 1000), cancellationToken: cancellationToken);

    /// <summary>
    /// Chain this with other methods but not the first one.
	/// Probably bugged with 2nd layer I think?
    /// </summary>
    public LegacyAnimator FollowedBy(string triggerName)
	{
		CheckTrigger(triggerName);
		cumulativePlayTime += SpeedAdjust(triggerName);

		animationComponent.PlayQueued(triggerName,QueueMode.CompleteOthers);

		// WrapMode wrapMode = animationComponent[triggerName].wrapMode;
		// if(wrapMode != WrapMode.Loop && wrapMode != WrapMode.PingPong)
		// {
		// 	AutoDisable();
		// }
		// else
		// {
		// 	StopPreviousAutoDisable();
		// }

		return this;
	}

	private float SpeedAdjust(string triggerName)
	{
		float speed = 1 + nodeSearch[triggerName].SpeedAdjust;
		//Debug.Log($"Speed is {speed}");
		if(speed < 0)
		{
			animationComponent[triggerName].time = animationComponent[triggerName].length;
		}
        animationComponent[triggerName].speed = speed;
		return animationComponent[triggerName].length / Mathf.Abs(speed);
	}

    /// <summary>
    /// Kills all animations and wait.
    /// </summary>
	public LegacyAnimator Wait(float seconds)
	{
		cumulativePlayTime = seconds;
		SetWaitTime(seconds);
		animationComponent.enabled = true;

		animationComponent.Stop();
		animationComponent.Play(waitClipName);

		// AutoDisable();
		return this;
	}

    /// <summary>
    /// Chain this with other methods but not the first one.
    /// </summary>
    public LegacyAnimator AndWait(float seconds)
	{
		cumulativePlayTime += seconds;
		SetWaitTime(seconds);
		animationComponent.PlayQueued(waitClipName,QueueMode.CompleteOthers);
		// AutoDisable();
		return this;
	}

	private void CheckTrigger(string triggerName)
	{
		if(animationComponent[triggerName] == null)
		{
			throw new KeyNotFoundException("No trigger named " + triggerName + " in LegacyAnimator of " + gameObject.name);
		}
	}

    public void Stop()
    {
        animationComponent.Stop();
        // AutoDisable();
    }

    private void StopBeforePlayLogic(AnimationState aState)
    {
        //If main layer -> stop everything
        //If 2nd layer -> stop only second layer ones
        if (aState.layer == 0)
        {
            animationComponent.Stop(firstLayerCurrentlyPlaying);
			firstLayerCurrentlyPlaying = aState.name;
        }
        else
        {
            animationComponent.Stop(secondLayerCurrentlyPlaying);
			secondLayerCurrentlyPlaying = aState.name; 
        }
    }

	public bool IsPlaying(string triggerName)
	{
		CheckTrigger(triggerName);
		return animationComponent[triggerName].enabled;
	}

    /// <summary>
    /// Useful for preparing/hiding something before play. Usually the first frame is the appropriate visual.
	/// Also you can't use FollowedBy after an animation with only the first keyframe. Use this to sample one-frame animation instead of SetTrigger.
    /// </summary>
    public void SampleFirstFrame(string triggerName)
    {
		Prepare();
		CheckTrigger(triggerName);
		animationComponent.enabled = true;
		animationComponent.Stop();
		animationComponent[triggerName].enabled = true;
		animationComponent[triggerName].weight = 1;
		animationComponent.Sample();
		animationComponent[triggerName].enabled = false;
		animationComponent.enabled = false;
	}

    /// <summary>
    /// Triggers the first node
    /// </summary>
    public void Trigger()
    {
        if (nodes.Length < 1)
        {
            throw new System.Exception("Add something to the node..");
        }
        SetTrigger(nodes[0].Trigger);
	}

    /// <summary>
    /// Triggers the second node
    /// </summary>
    public void TriggerSecondary()
	{
        if (nodes.Length < 2)
        {
            throw new System.Exception("Add two things or more to the node..");
        }
        SetTrigger(nodes[1].Trigger);
	}

    // private void AutoDisable()
    // {
	// 	StopPreviousAutoDisable();
    //     autoDisableRoutine = AutoDisableRoutine(cumulativePlayTime);
    //     StartCoroutine(autoDisableRoutine);
    // }

    // private void StopPreviousAutoDisable()
    // {
    //     if (autoDisableRoutine != null)
    //     {
    //         StopCoroutine(autoDisableRoutine);
    //     }
    // }

    // private IEnumerator autoDisableRoutine;
    // private IEnumerator AutoDisableRoutine(float inTime)
    // {
    //     yield return new WaitForSeconds(inTime);
    //     yield return null;
    //     animationComponent.enabled = false;
    // }

    private bool prepared = false;
    private void Prepare()
    {
        if (!prepared)
        {
            nodeSearch = new Dictionary<string, LegacyAnimatorNode>();
            variableBool = new Dictionary<string, bool>();
            InitializeAnimationComponent();
            if (nodes != null)
            {
                foreach (LegacyAnimatorNode lan in nodes)
                {
                    animationComponent.AddClip(lan.AnimationClip, lan.Trigger);
                    animationComponent[lan.Trigger].layer = lan.SecondLayer ? 1 : 0;
                    nodeSearch.Add(lan.Trigger, lan);
                }

                AnimationClip waitOneSecond = new AnimationClip();
                waitOneSecond.legacy = true;
                animationComponent.AddClip(waitOneSecond, waitClipName);
            }
        }
        prepared = true;
    }

    [ContextMenu("Match Animation")]
	public void MatchAnimation()
    {
		InitializeAnimationComponent();
        foreach (AnimationState ast in animationComponent)
        {
            //Debug.Log(ast.name + " " + ast.clip.name);
            animationComponent.RemoveClip(ast.clip.name);
        }
        if (nodes != null)
        {
            foreach (LegacyAnimatorNode lan in nodes)
            {
				if(lan.AnimationClip != null)
				{
					animationComponent.AddClip(lan.AnimationClip, lan.ClipName);
				}
            }
        }
    }

    private void InitializeAnimationComponent()
	{
        animationComponent = GetComponent<Animation>();
		if(animationComponent != null)
		{
			DestroyImmediate(animationComponent);
		}
        if (animationComponent == null)
        {
            animationComponent = gameObject.AddComponent<Animation>();
        }
        animationComponent.playAutomatically = false;
	}

	public void Awake()
	{
		Prepare();
	}

	public void Start()
	{
        if (waitTrigger != "")
		{
			SampleFirstFrame(waitTrigger);
		}
		if(autoplayTrigger != "")
		{
			SetTrigger(autoplayTrigger);
		}
	}

	public void SetBool(string name, bool val)
	{
		if(variableBool.ContainsKey(name))
		{
			variableBool[name] = val;
		}
		else
		{
			variableBool.Add(name,val);
		}
	}

	public bool GetBool(string name)
	{
		if(variableBool.ContainsKey(name))
		{
			return variableBool[name];
		}
		else
		{
			return false;
		}
	}

	private void SetWaitTime(float seconds)
	{
		foreach(AnimationState ast in animationComponent)
		{
			if(ast.name == waitClipName)
			{
				ast.speed = 1/seconds;
			}
		}
	}


}
