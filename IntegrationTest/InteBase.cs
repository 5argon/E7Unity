#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

//This is now based on Unity 5.6's test runner. Separate Integration scene no longer required.
public abstract class InteBase {

    /// <summary>
    /// Do this before doing a scene load.
    /// </summary>
    protected void ProtectTestRunner()
    {
        GameObject g = GameObject.Find("Code-based tests runner");
        GameObject.DontDestroyOnLoad(g);
    }

    /// <summary>
    /// Helper methods to save your pain
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    protected WaitForSeconds Wait(float seconds)
    {
        return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Unfortunately could not return T upon found, but useful for waiting something to become active
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitUntilFound<T>() where T : MonoBehaviour
    {
        T t = null;
        while (t == null)
        {
            t = (T)UnityEngine.Object.FindObjectOfType(typeof(T));
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected IEnumerator WaitUntilSceneLoaded(string sceneName)
    {
        while (IsSceneLoaded(sceneName) == false)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// REMEMBER!! must be active..
    /// And remember that if there are multiples it returns the first one
    /// </summary>
    /// <returns></returns>
    protected T Find<T>() where T : MonoBehaviour
    {
        return UnityEngine.Object.FindObjectOfType<T>() as T;
    }

    protected T Find<T>(string sceneName) where T : MonoBehaviour
    {
        T[] objs = UnityEngine.Object.FindObjectsOfType<T>() as T[];
        foreach(T t in objs)
        {
            if(t.gameObject.scene.name == sceneName)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// This overload allows you to specify 2 types. It will try to find a child under that type with a given name.
    /// Useful for drilling down a prefab.
    /// </summary>
    protected ChildType Find<ParentType, ChildType>(string childName, string sceneName = "") where ParentType : MonoBehaviour where ChildType : MonoBehaviour
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
        return FindChildRecursive(find.gameObject.transform, childName)?.GetComponent<ChildType>();
    }

    private Transform FindChildRecursive(Transform transform, string childName)
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
    
    //Get specific object name's component
    protected T FindNamed<T>(string gameObjectName) where T : MonoBehaviour
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go != null)
        {
            return go.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// Useful in case there are many T in the scene, usually from a separate sub-scene
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    protected T FindOnSceneRoot<T>(string sceneName = "") where T : MonoBehaviour
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
            return null;
        }
        return null;
    }

    /// <summary>
    /// REMEMBER!! must be active..
    /// </summary>
    /// <returns></returns>
    protected GameObject FindGameObject<T>() where T : MonoBehaviour
    {
        return (UnityEngine.Object.FindObjectOfType(typeof(T)) as T).gameObject;
    }

    protected bool CheckGameObject(string name)
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

    /// <summary>
    /// Time to utilize hacky way of using string...
    /// </summary>
    /// <param name="gameObjectName"></param>
    /// <returns></returns>
    public Vector2 CenterOfRectNamed(string gameObjectName)
    {
        Vector3[] corners = new Vector3[4];
        GameObject go = GameObject.Find(gameObjectName);
        if (go != null)
        {
            go.GetComponent<RectTransform>().GetWorldCorners(corners);
            return Vector3.Lerp(Vector3.Lerp(corners[0], corners[1], 0.5f), Vector3.Lerp(corners[2], corners[3], 0.5f), 0.5f);
        }
        else
        {
            Debug.LogError("Can't find " + gameObjectName);
            return Vector2.zero;
        }
    }

    public Vector2 CenterOfSpriteName(string gameObjectName)
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

    protected bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.IsValid() && scene.isLoaded;
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

    public void ActionBetweenSceneAwakeAndStart(string sceneName, System.Action action)
    {
        UnityEngine.Events.UnityAction<Scene,LoadSceneMode> unityAction = (scene,LoadSceneMode) =>
        {
            if(scene.name == sceneName)
            {
                action();
            }
        };

        SceneManager.sceneLoaded += unityAction;
    }

}

#endif