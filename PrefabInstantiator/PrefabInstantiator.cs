//Sirawat Pitaksarit / 5argon - Exceed7 Experiments
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Instantiate UI prefab and stretch the rect to the current rect transform
/// Use the toggle when working on the game and turn it off when we are applying outer prefab to circumvent the nested prefab problem.
/// </summary>
[ExecuteInEditMode]
public class PrefabInstantiator : MonoBehaviour {

	public ButtonBool togglePrefab;
	public ButtonBool addPrefab;
	public ButtonBool removePrefab;
	[Space]
	public GameObject prefab;
	[Space]
    public Vector2 instantiateScale = Vector2.one;
    /// <summary>
    /// Normally the script will clear all children and wait for you to do the instantiation.
    /// Since the goal is to sync to prefab, reinstantiating is the way to refresh it run-time.
    /// </summary>
    public bool preserveChildrenOnAwake;
    /// <summary>
    /// Normally it will instantiate 1 game object for you. Check to prevent this.
    /// </summary>
    public bool preventAutoInstantiation;

    /// <summary>
    /// Correctly fits RectTransform to the parent.
    /// </summary>
    public bool uiPrefab;

    /// <summary>
    /// In case that you preserve childrens, this will be the first child.
    /// </summary>
	private GameObject instantiatedPrefab;

    public GameObject FirstInstantiated => IsInstantiated ? instantiatedPrefab : null;

    private bool awoken;

    public void Awake()
    {
        AwakeAction();
    }

/// <summary>
/// Force the Awake, and don't run Awake again.
/// Useful when you can't wait for Awake. (Maybe it is still inactive but you want to instantiate.)
/// </summary>
    public void AwakeAction()
    {
        if(!awoken)
        {
            if (Application.isPlaying)
            {
                if (!preserveChildrenOnAwake)
                {
                    DestroyAll();
                }
                if (!preventAutoInstantiation)
                {
                    //Debug.LogWarning("UIPrefab Start : " + gameObject.name);
                    InstantiatePrefab();
                }
                awoken = true;
            }
        }
    }

#if UNITY_EDITOR
    public void Update()
    {
        if (!Application.isPlaying)
        {
            if (togglePrefab.Pressed)
            {
                if (IsInstantiated)
                {
                    Destroy();
                }
                else
                {
                    InstantiatePrefab();
                }
            }
            else if (addPrefab.Pressed)
            {
                InstantiatePrefab();
            }
            else if (removePrefab.Pressed)
            {
                Destroy();
            }
        }
    }
#endif

    public bool IsInstantiated
    {
        get
        {
            if (gameObject.transform.childCount > 0 )
            {
                if(Application.isPlaying && preventAutoInstantiation == false && awoken == false)
                {
                    //There IS a child even if not yet Awake
                    //It means not instantiated yet
                    return false;
                }
                if(instantiatedPrefab == null)
                {
                    instantiatedPrefab = gameObject.transform.GetChild(0).gameObject;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public T GetComponentOfInstantiated<T>() where T : Component 
    {
        if (IsInstantiated)
        {
            //Debug.Log("Instantiated " + instantiatedPrefab);
            return instantiatedPrefab.GetComponent<T>();
        }
        else
        {
            return null;
        }
	}

	public T InstantiatePrefab<T>() where T : MonoBehaviour
	{
		InstantiatePrefab();
		return GetComponentOfInstantiated<T>();
	}

    [ContextMenu("Instantiate Prefab")]
    public GameObject InstantiatePrefab()
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            instantiatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
#endif
        }
        else
        {
            instantiatedPrefab = Instantiate(prefab);
        }

        if (uiPrefab)
        {
            instantiatedPrefab.transform.SetParent(gameObject.transform);
            RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();
            if(rect != null)
            {
                rect.localPosition = Vector3.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = Vector2.zero;
            }
        }
        else
        {
            instantiatedPrefab.transform.SetParent(gameObject.transform);
            instantiatedPrefab.transform.localScale = Vector3.one;
            instantiatedPrefab.transform.localPosition = Vector3.zero;
            instantiatedPrefab.transform.localRotation = Quaternion.identity;
        }

         if(instantiateScale != Vector2.zero)
         {
             instantiatedPrefab.transform.localScale = instantiateScale;
         }
         return instantiatedPrefab;
	}

	[ContextMenu("Destroy")]
	public void Destroy()
	{
		if(IsInstantiated)
		{
			DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
			instantiatedPrefab = null;
		}
	}

	[ContextMenu("Destroy All")]
	public void DestroyAll()
	{
		while(gameObject.transform.childCount > 0)
		{
            if(!Application.isPlaying)
            {
                DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
            }
            else
            {
                DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
            }
		}
        instantiatedPrefab = null;
	}

}
