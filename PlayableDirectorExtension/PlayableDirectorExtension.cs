using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public static class PlayableDirectorExtension
{
    /// <summary>
    /// Pause() stops PlayableGraph from sampling the value and return to the remembered base value, where this freeze play head and repeatedly sample the same spot.
    /// </summary>
    public static void Hold(this PlayableDirector pd)
    {
        pd.playableGraph.GetRootPlayable(0).SetSpeed(0);
    }

    /// <summary>
    /// Use after Hold() to continue the play head.
    /// </summary>
    public static void Continue(this PlayableDirector pd)
    {
        pd.playableGraph.GetRootPlayable(0).SetSpeed(1);
    }

}
