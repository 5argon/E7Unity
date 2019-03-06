using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class CrossSceneConnector<T> : MonoBehaviour where T : CrossSceneConnector<T>{

    //Can load scene additively then optionally connects variables.
    //How to use is to subclass from this and implement the abstracts.

    //the problem of this is, even if this is an Awake and has prioritized execition time, a scene is loaded sequentially
    //So if you have scene A (with one CrossSceneConnector) and scene B (with another CrossSceneConnector), and have them ready together on the hierarchy, it is the same with using LoadScene(A) and then LoadScene(B).
    //The active one will load first. If it is A then :
    // A Awake() -> B Awake() -> A Update() -> B Update()
    // This cause scripts in A, that needed dependency in its Awake not working since you must wait for B Awake() to come to be able to FindGameObjectWithTag successfully.

    //In short, you cannot use dependency in Awake in your active scene. Move to Start().

    //Also, it is not guaranteed that the being loaded scene will have it's Awake called before the current scene's Start!
    //Turn Start into IEnumerator and wait for the connector to complete is highly advised.

#pragma warning disable 0649
    [Tooltip("Prevents Awake script from running.")]
    [SerializeField] private bool disable;
    [Space]
    [SerializeField] private bool mainSide;
#pragma warning restore 0649
    protected bool MainSide => mainSide;

    /// <summary>
    /// otherSide has different meaning depending on which side this connector instance is. ("Main Side" or "Target Side")
    /// </summary>
    protected T otherSide;
    
    private static string ConnectorTagName = "Connector";

    protected virtual void Awake()
    {
        //Debug.Log("CSC Awake " + SceneToConnect + " " + mainSide + " Generic: " + typeof(T).Name + " go: " + gameObject.name);
        if (!disable)
        {
            LoadAndConnect();
        }
    }

    /// <summary>
    /// If you manually call this, you need to wait 1 frame for the connected variables to be available.
    /// Since the new scene will be loaded on the next frame.
    /// </summary>
    public void LoadAndConnect()
    {
        Connected = false;
        //load
        //somehow IsValid acts like what isLoaded should have been...
        if (mainSide && !SceneManager.GetSceneByName(SceneToConnect).IsValid())
        {
            //Debug.Log("Loading connected " + SceneToConnect);
            SceneManager.LoadScene(SceneToConnect, LoadSceneMode.Additive);

            //Scene s = SceneManager.GetSceneByName(SceneToConnect);
            //Debug.Log($"{s.name} {s.isLoaded}");
        }

        //connect

        otherSide = FindTargetSide();
        //Debug.Log( SceneToConnect + " Find result " + otherSide + " main: "  + mainSide);
        if(otherSide != null) //it will surely be null the first time, since the other side is not loaded yet.
        {
            otherSide.otherSide = otherSide.FindTargetSide();
        }

        if(otherSide != null && otherSide.otherSide == this)
        {
            //Debug.Log(SceneToConnect + " Exchange begin " + mainSide);
            BeginExchanging();
        }
    }

    //you might want to preserve a certain scene to pair with other scene
    //in that case prep the scene with this method call
    public void PrepareForExchangeAgain()
    {
        //Debug.Log("Prepare to exchange again! " + SceneToConnect + " " + mainSide);
        Connected = false;
        if(otherSide != null)
        {
            otherSide.Connected = false;
        }
    }

    /// <summary>
    /// Only for one side, but it is very likely that the other side have already connected too...
    /// </summary>
    public bool Connected { get; private set; }

    private void BeginExchanging()
    {
        if(!Connected)
        {
            if(mainSide)
            {
                MainSideAssignment();
            }
            else
            {
                TargetSideAssignment();
            }
            Connected = true;
            otherSide.BeginExchanging();
        }
    }

    private T FindTargetSide()
    {
        T otherSide = default(T);
        GameObject[] connectors = GameObject.FindGameObjectsWithTag(ConnectorTagName);
        foreach(GameObject go in connectors)
        {
            if(go.name != this.name)
            {
                otherSide = go.GetComponent<T>();
                if(otherSide != null)
                {
                    break;
                }
            }
        }
        return otherSide;
    }

    protected abstract string SceneToConnect { get; }
    protected abstract void MainSideAssignment();
    protected abstract void TargetSideAssignment();

}
