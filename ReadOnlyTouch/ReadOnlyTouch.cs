using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// With C# 7.2, use this with `in` parameter modifier to both pass this large struct as a reference and without the costly defensive copy.
/// https://blogs.msdn.microsoft.com/seteplia/2018/03/07/the-in-modifier-and-the-readonly-structs-in-c/
/// 
/// Also it is a gibbed version of Unity ones. Less variables...
/// </summary>
public readonly struct ReadOnlyTouch
{
    /// <summary>
    /// Convert from Unity Touch.
    /// </summary>
    public ReadOnlyTouch(in Touch t) : this(
        t.fingerId,
        t.position,
        t.deltaPosition,
        t.phase
    )
    { }

    public ReadOnlyTouch(
    int fingerId,
    float2 position,
    float2 deltaPosition,
    TouchPhase phase
    )
    {
        this.fingerId = fingerId;
        this.position = position;
        this.deltaPosition = deltaPosition;
        this.phase = phase;
        this.timestamp = 0;
        this.Valid = true;
    }

    public ReadOnlyTouch(
    int fingerId,
    float2 position,
    float2 deltaPosition,
    TouchPhase phase,
    double timestamp
    )
    {
        this.fingerId = fingerId;
        this.position = position;
        this.deltaPosition = deltaPosition;
        this.phase = phase;
        this.timestamp = timestamp;
        this.Valid = true;
    }

    /// <summary>
    /// The unique index for the touch.
    /// </summary>
    public int fingerId { get; }
    /// <summary>
    /// The position of the touch in pixel coordinates.
    /// </summary>
    public float2 position { get; }
    /// <summary>
    /// The position delta since last change.
    /// </summary>
    public float2 deltaPosition { get; }
    /// <summary>
    /// Describes the phase of the touch.
    /// </summary>
    public TouchPhase phase { get; }

    public double timestamp { get; }

    public Bool Valid { get; }
}