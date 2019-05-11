//When integration testing using "on device" button DEVELOPMENT_BUILD is automatically on.
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !UNITY_EDITOR)
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;

public static class InteHelper
{
    /// <summary>
    /// Helper methods to save your pain
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static WaitForSeconds Wait(float seconds)
    {
        return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Clean everything except the test runner object.
    /// </summary>
    public static void CleanTestScene()
    {
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name != "Code-based tests runner")
            {
                GameObject.DestroyImmediate(go);
            }
        }
    }

    public class TimeOutWaitUntil : CustomYieldInstruction
    {
        public float TimeOut { get;}
        public Func<bool> Pred { get;}

        private float timeElapsed;

        public TimeOutWaitUntil(Func<bool> predicate, float timeOut)
        {
            this.Pred = predicate;
            this.TimeOut = timeOut;
        }

        public override bool keepWaiting
        {
            get
            {
                if (Pred.Invoke() == false)
                {
                    timeElapsed += Time.deltaTime;
                    if(timeElapsed > TimeOut)
                    {
                        throw new Exception($"Wait until timed out! ({TimeOut} seconds)");
                    }
                    return true; //keep coroutine suspended
                }
                else
                {
                    return false; //predicate is true, stop waiting and move on
                }
            }
        }

    }


    /// <summary>
    /// Unfortunately could not return T upon found, but useful for waiting something to become active
    /// </summary>
    /// <returns></returns>
    public static IEnumerator WaitUntilFound<T>() where T : Component 
    {
        T t = null;
        while (t == null)
        {
            t = (T)UnityEngine.Object.FindObjectOfType(typeof(T));
            yield return new WaitForSeconds(0.1f);
        }
    }

    public static IEnumerator WaitUntilSceneLoaded(string sceneName)
    {
        while (IsSceneLoaded(sceneName) == false)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Find the first game object of type <typeparamref name="T"> AND IT MUST BE ACTIVE.
    /// </summary>
    public static T Find<T>() where T : Component
    {
        return UnityEngine.Object.FindObjectOfType<T>() as T;
    }

    /// <summary>
    /// Find the first game object of type <typeparamref name="T"> AND IT MUST BE ACTIVE.
    /// Narrow the search on only scene named <paramref name="sceneName">.
    /// </summary>
    public static T Find<T>(string sceneName) where T : Component 
    {
        T[] objs = UnityEngine.Object.FindObjectsOfType<T>() as T[];
        foreach(T t in objs)
        {
            if(t.gameObject.scene.name == sceneName)
            {
                return t;
            }
        }
        throw new GameObjectNotFoundException($"Type {typeof(T).Name} not found on scene {sceneName}!");
    }

    /// <summary>
    /// Like <see cref="Find{T}"> but it returns a game object.
    /// The object must be ACTIVE to be found!
    /// </summary>
    public static GameObject FindGameObject<T>() where T : Component 
    {
        return Find<T>().gameObject;
    }

    private static Transform FindChildRecursive(Transform transform, string childName)
    {
        Transform t = transform.Find(childName);
        if (t != null)
        {
            return t;
        }
        foreach (Transform child in transform)
        {
            Transform t2 = FindChildRecursive(child, childName);
            if (t2 != null)
            {
                return t2;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get a component of type <typeparamref name="T"> from a game object with a specific name <paramref name="gameObjectName">.
    /// </summary>
    public static T FindNamed<T>(string gameObjectName) where T : Component
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go != null)
        {
            return go.GetComponent<T>();
        }
        else
        {
            throw new GameObjectNotFoundException($"{gameObjectName} of type {typeof(T).Name} not found!");
        }
    }

    /// <summary>
    /// Check for amount of childs of a game object <paramref name="go">.
    /// </summary>
    public static int ActiveChildCount(GameObject go)
    {
        var counter = 0;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).gameObject.activeSelf) counter++;
        }
        return counter;
    }

    /// <summary>
    /// Will try to find the parent first regardless of type, then a child under that parent regardless of type, then get component of type <typeparamref name="T">.
    /// </summary>
    public static T FindNamed<T>(string parentName, string childName) where T : Component
    {
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
        {
            throw new ArgumentException($"Parent name {parentName} not found!");
        }
        Transform child = FindChildRecursive(parent.transform, childName);
        if (child == null)
        {
            throw new ArgumentException($"Child name {childName} not found!");
        }
        T component = child.GetComponent<T>();
        if (component == null)
        {
            throw new ArgumentException($"Component of type {typeof(T).Name} does not exist on {parentName} -> {childName}!");
        }
        return component;
    }

    /// <summary>
    /// This overload allows you to specify 2 types. It will try to find a child under that type with a given name.
    /// Useful for drilling down a prefab.
    /// </summary>
    public static ChildType FindNamed<ParentType, ChildType>(string childName, string sceneName = "") where ParentType : Component where ChildType : Component 
    {
        ParentType find;
        if(sceneName == "")
        {
            find = Find<ParentType>();
        }
        else
        {
            find = Find<ParentType>(sceneName);
        }
        var found = FindChildRecursive(find.gameObject.transform, childName)?.GetComponent<ChildType>();
        if(found == null)
        {
            throw new GameObjectNotFoundException($"{childName} of type {typeof(ChildType).Name} not found on parent type {typeof(ParentType).Name} !");
        }
        return found;
    }

    /// <summary>
    /// Useful in case there are many T in the scene, usually from a separate sub-scene
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public static T FindOnSceneRoot<T>(string sceneName = "") where T : Component  
    {
        Scene scene;
        if (sceneName == "")
        {
            scene = SceneManager.GetActiveScene();
        }
        else
        {
            scene = SceneManager.GetSceneByName(sceneName);
        }
        if (scene.IsValid() == true)
        {
            GameObject[] gos = scene.GetRootGameObjects();
            foreach (GameObject go in gos)
            {
                T component = go.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }
        else
        {
            throw new GameObjectNotFoundException($"{typeof(T).Name} not found on scene root!");
        }
        throw new GameObjectNotFoundException($"{typeof(T).Name} not found on scene root!");
    }

    public static bool CheckGameObject(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static Vector2 CenterOfRectNamed(string gameObjectName)
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go != null)
        {
            return CenterOfRectTransform(go.GetComponent<RectTransform>());
        }
        else
        {
            Debug.LogError("Can't find " + gameObjectName);
            return Vector2.zero;
        }
    }

    public static Vector2 CenterOfRectTransform(RectTransform rect) => RelativePositionOfRectTransform(rect, new Vector2(0.5f, 0.5f));
    // {
    //     Vector3[] corners = new Vector3[4];
    //     rect.GetWorldCorners(corners);
    //     return Vector3.Lerp(Vector3.Lerp(corners[0], corners[1], 0.5f), Vector3.Lerp(corners[2], corners[3], 0.5f), 0.5f);
    // }

    public static Vector2 RelativePositionOfRectTransform(RectTransform rect, Vector2 relativePosition)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        // foreach(Vector3 c in corners)
        // {
        //     Debug.Log(c);
        // }
        return new Vector2(Vector3.Lerp(corners[1], corners[2], relativePosition.x).x, Vector3.Lerp(corners[0], corners[1], relativePosition.y).y);
    }

    public static Vector2 CenterOfSpriteName(string gameObjectName)
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go != null)
        {
            return go.GetComponent<SpriteRenderer>().transform.position;
        }
        else
        {
            Debug.LogError("Can't find " + gameObjectName);
            return Vector2.zero;
        }
    }

    /// <summary>
    /// Currently supports Button's onClick and EventTrigger's OnPointerClick.
    /// Clicks on the center of provided RectTransform.
    /// </summary>
    public static void RaycastClick(RectTransform rect) => RaycastClick(CenterOfRectTransform(rect));

    /// <summary>
    /// Currently supports Button's onClick and EventTrigger's OnPointerClick.
    /// Clicks on a relative position in the rect.
    /// </summary>
    public static void RaycastClick(RectTransform rect, Vector2 relativePositionInRect) => RaycastClick(RelativePositionOfRectTransform(rect, relativePositionInRect));

    /// <summary>
    /// Currently supports Button's onClick and EventTrigger's OnPointerClick.
    /// </summary>
    /// <param name="screenPosition">In pixel.</param>
    public static void RaycastClick(Vector2 screenPosition)
    {
        //Debug.Log("Clicking " + screenPosition);
        PointerEventData fakeClick = new PointerEventData(EventSystem.current);
        fakeClick.position = screenPosition;
        fakeClick.button = PointerEventData.InputButton.Left;

        GraphicRaycaster[] allGfxRaycasters = (GraphicRaycaster[])GameObject.FindObjectsOfType(typeof(GraphicRaycaster));

        //Raycaster gets checked one by one, from the bottom most in hierarchy.
        foreach(GraphicRaycaster gfxRaycaster in allGfxRaycasters.Reverse())
        {
            //Debug.Log("Casting : " + gfxRaycaster.gameObject.name);

            List<RaycastResult> results = new List<RaycastResult>();
            gfxRaycaster.Raycast(fakeClick, results);

            foreach(RaycastResult rr in results)
            {
                Debug.Log("Hit : " + rr.gameObject.name);
                Selectable selectable = rr.gameObject.GetComponent<Selectable>();
                bool interactable = selectable != null ? selectable.interactable : true;

                var down = rr.gameObject.GetComponent<IPointerDownHandler>();
                if (down != null)
                {
                    down.OnPointerDown(fakeClick);
                }

                var up = rr.gameObject.GetComponent<IPointerUpHandler>();
                if (up != null)
                {
                    up.OnPointerUp(fakeClick);
                }

                var click = rr.gameObject.GetComponent<IPointerClickHandler>();
                if (click != null)
                {
                    click.OnPointerClick(fakeClick);
                }
            }

            //Test for hit. If only one raycaster hits, all other raycasters will not be casted.
            if(results.Count > 0)
            {
                break; 
            }
        }
    }

    public static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        //Valid is the scene is in the hierarchy, but it might be still "(loading)"
        return scene.IsValid() && scene.isLoaded;
    }


    public static void ActionBetweenSceneAwakeAndStart(string sceneName, System.Action action)
    {
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> unityAction = (scene, LoadSceneMode) =>
        {
            if(scene.name == sceneName)
            {
                action();
            }
        };

        SceneManager.sceneLoaded += unityAction;
    }

    /// <summary>
    /// WIP! Does not work!
    /// </summary>
    public static float AverageAmplitude(float inTheLastSeconds)
    {
        int samplesNeeded = (int)(AudioSettings.outputSampleRate * inTheLastSeconds);
        int samplesToUse = 1;
        while(samplesToUse < samplesNeeded)
        {
            samplesToUse *= 2;
        }
        float[] samplesL = new float[samplesToUse];
        float[] samplesR = new float[samplesToUse];

        AudioListener.GetOutputData(samplesL,0);
        AudioListener.GetOutputData(samplesR,0);

        return (samplesL.Average() + samplesR.Average() / 2f);
    }
}

/// <summary>
/// Test with this attribute runs only in Unity editor
/// </summary>
public class UnityEditorPlatformAttribute : UnityPlatformAttribute
{
    public UnityEditorPlatformAttribute()
    {
        this.include = new RuntimePlatform[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor };
    }
}

/// <summary>
/// Test with this attribute runs only on the real mobile device
/// </summary>
public class UnityMobilePlatformAttribute : UnityPlatformAttribute
{
    public UnityMobilePlatformAttribute()
    {
        this.include = new RuntimePlatform[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer };
    }
}

[System.Serializable]
public class GameObjectNotFoundException : System.Exception
{
    public GameObjectNotFoundException() { }
    public GameObjectNotFoundException(string message) : base(message) { }
    public GameObjectNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    protected GameObjectNotFoundException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

#endif