using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFlatten : MonoBehaviour {

	public Transform targetTransform;
	[Tooltip("The flattened order will be like in this array.")]
	public MaskableGraphic[] texts;

	[Tooltip("If the animator has control to the text, it will attemps to animate the text one frame after flattening.")]
	public Animator[] stopAnimators;
	public Canvas[] disableCanvases;

	private Transform[] parents;
	private int[] siblingIndexes;

    public bool IsFlattened { get; private set; }

	public void Awake()
	{
		ClearArrays();
	}

	private void ClearArrays()
	{
		parents = new Transform[texts.Length];
		siblingIndexes = new int[texts.Length];
	}

	public void Remember()
	{
		ClearArrays();
        for (int i = 0; i < texts.Length; i++)
		{
			parents[i] = texts[i].transform.parent;
			//Debug.Log(parents[i].gameObject.name);
			siblingIndexes[i] = texts[i].transform.GetSiblingIndex();
		}
	}

	[ContextMenu("Flatten")]
	public void Flatten()
	{
		Remember();
		if(!IsFlattened)
		{
			foreach(Animator a in stopAnimators)
			{
				a.enabled = false;
			}
			foreach(Canvas c in disableCanvases)
			{
				c.enabled = false;
			}
            foreach (MaskableGraphic t in texts)
            {
                t.transform.SetParent(targetTransform, true);
                t.transform.SetAsLastSibling();
            }
            IsFlattened = true;
		}
	}

    [ContextMenu("Restore")]
    public void Restore()
    {
        if (IsFlattened)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].transform.SetParent(parents[i], true);
                texts[i].transform.SetSiblingIndex(siblingIndexes[i]);
            }
            foreach (Animator a in stopAnimators)
            {
                a.enabled = true;
            }
			foreach(Canvas c in disableCanvases)
			{
				c.enabled = true;
			}
			IsFlattened = false;
        }
	}
	
}
