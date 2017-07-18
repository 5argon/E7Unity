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
[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class UIPrefabInstantiator : MonoBehaviour {

	public ButtonBool togglePrefab;
	public ButtonBool addPrefab;
	public ButtonBool removePrefab;
	[Space]
	public GameObject prefab;
	[Space]
    /// <summary>
    /// Normally the script will clear all children and wait for you to do the instantiation.
    /// Since the goal is to sync to prefab, reinstantiating is the way to refresh it run-time.
    /// </summary>
    public bool preserveChildrenOnAwake;
    /// <summary>
    /// Normally it will instantiate 1 game object for you. Check to prevent this.
    /// </summary>
    public bool preventAutoInstantiation;

	private GameObject instantiatedPrefab;

    public void Awake()
    {
        if (Application.isPlaying)
        {
            if(!preserveChildrenOnAwake)
            {
                DestroyAll();
            }
            if(!preventAutoInstantiation)
            {
                Instantiate();
            }
        }
    }

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
                    Instantiate();
                }
            }
            else if (addPrefab.Pressed)
            {
                Instantiate();
            }
            else if (removePrefab.Pressed)
            {
                Destroy();
            }
        }
    }

    private bool IsInstantiated
    {
        get
        {
            if (gameObject.transform.childCount > 0 && instantiatedPrefab != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public T GetComponentOfInstantiated<T>() where T : MonoBehaviour 
    {
        if (IsInstantiated)
        {
            return instantiatedPrefab.GetComponent<T>();
        }
        else
        {
            return null;
        }
	}

	public T Instantiate<T>() where T : MonoBehaviour
	{
		Instantiate();
		return GetComponentOfInstantiated<T>();
	}

    [ContextMenu("Instantiate")]
    public void Instantiate()
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
        instantiatedPrefab.transform.SetParent(gameObject.transform);
        instantiatedPrefab.transform.localScale = Vector3.one;
		RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();
        rect.position = Vector3.zero;
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMax = Vector2.zero;
		rect.offsetMin = Vector2.zero;
	}

	[ContextMenu("Destroy")]
	public void Destroy()
	{
		if(IsInstantiated || gameObject.transform.childCount > 0)
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
