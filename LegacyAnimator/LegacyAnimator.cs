using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyAnimator : MonoBehaviour {

	public Animation animationComponent;
	public string startStateTrigger;
	[Space]
	public LegacyAnimatorNode[] nodes;
	private Dictionary<string,bool> variableBool;

	private const string waitClipName = "WAIT_CLIP";

    public void Prepare()
    {
		Reset();
        if (nodes != null)
        {
            foreach (LegacyAnimatorNode lan in nodes)
            {
                animationComponent.AddClip(lan.AnimationClip, lan.Trigger);
            }

            AnimationClip waitOneSecond = new AnimationClip();
            waitOneSecond.legacy = true;
            animationComponent.AddClip(waitOneSecond, waitClipName);
        }
    }

    [ContextMenu("Match Animation")]
	public void MatchAnimation()
    {
		Reset();
        foreach (AnimationState ast in animationComponent)
        {
            //Debug.Log(ast.name + " " + ast.clip.name);
            animationComponent.RemoveClip(ast.clip.name);
        }
        if (nodes != null)
        {
            foreach (LegacyAnimatorNode lan in nodes)
            {
                animationComponent.AddClip(lan.AnimationClip, lan.ClipName);
            }
        }
    }

    public void Reset()
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
		variableBool = new Dictionary<string, bool>();
		Prepare();
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


    /// <summary>
    /// Kills all animations and play a new one.
    /// </summary>
    public LegacyAnimator SetTrigger(string triggerName)
    {
        animationComponent.Play(triggerName, PlayMode.StopAll);
		return this;
	}

    /// <summary>
    /// Chain this with other methods but not the first one.
    /// </summary>
    public LegacyAnimator FollowedBy(string triggerName)
	{
		animationComponent.PlayQueued(triggerName,QueueMode.CompleteOthers);
		return this;
	}

    /// <summary>
    /// Kills all animations and wait.
    /// </summary>
	public LegacyAnimator Wait(float seconds)
	{
		SetWaitTime(seconds);
		animationComponent.Play(waitClipName,PlayMode.StopAll);
		return this;
	}

    /// <summary>
    /// Chain this with other methods but not the first one.
    /// </summary>
    public LegacyAnimator AndWait(float seconds)
	{
		SetWaitTime(seconds);
		animationComponent.PlayQueued(waitClipName,QueueMode.CompleteOthers);
		return this;
	}

}
