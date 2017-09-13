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

    public bool mainSide;
    protected T otherSide;

    private static string ConnectorTagName = "Connector";

    protected virtual void Awake()
    {
       LoadAndConnect(); 
    }

    //if you manually call this, you need to wait 1 frame for the connected variables to be available.
    //since the new frame will be loaded on the next frame.
    public void LoadAndConnect()
    {
        exchanged = false;
        //load
        //somehow IsValid acts like what isLoaded should have been...
        if (mainSide && !SceneManager.GetSceneByName(SceneToConnect).IsValid())
        {
            SceneManager.LoadScene(SceneToConnect, LoadSceneMode.Additive);
        }

        //connect

        otherSide = FindOtherSide();
        if(otherSide != null) //it will surely be null the first time, since the other side is not loaded yet.
        {
            otherSide.otherSide = otherSide.FindOtherSide();
        }

        if(otherSide != null && otherSide.otherSide == this)
        {
            BeginExchanging();
        }
    }

    //you might want to preserve a certain scene to pair with other scene
    //in that case prep the scene with this method call
    public void PrepareForExchangeAgain()
    {
        exchanged = false;
    }

    private bool exchanged;
    private void BeginExchanging()
    {
        if(!exchanged)
        {
            if(mainSide)
            {
                MainSideAssignment();
            }
            else
            {
                OtherSideAssignment();
            }
            exchanged = true;
            otherSide.BeginExchanging();
        }
    }

    protected T FindOtherSide()
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
    protected abstract void OtherSideAssignment();

}
