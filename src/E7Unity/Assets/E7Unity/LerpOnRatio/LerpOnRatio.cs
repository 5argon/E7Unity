using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Evaluates a single <c>0..1</c> float from the screen's aspect ratio, so a layout can be tuned continuously
    /// between a squarish tablet and a long phone instead of branching on a handful of breakpoints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The curve's time axis is the <b>long side divided by the short side</b>, so it reads the same whether the game
    /// runs in portrait or landscape. The default curve created by <c>Reset</c> spans the two ratios worth caring
    /// about on mobile: <c>4:3</c> (squarish, iPad) maps to <c>0</c> and <c>2:1</c> (long, modern phones) maps to
    /// <c>1</c>, with <c>16:9</c> placed proportionally in between.
    /// </para>
    /// <para>
    /// Because the value is read from <see cref="Screen"/> on every access, it follows device rotation and resizing
    /// with no extra bookkeeping. Feed the result into anything that takes a normalized amount — a
    /// <see cref="Mathf.Lerp(float,float,float)"/> between two anchored positions, a padding, a font size.
    /// </para>
    /// </remarks>
    public class LerpOnRatio : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip("Time axis is width/height ratio when on landscape orientation e.g. 4/3, 16/9, 2/1")]
        [SerializeField] private AnimationCurve lerpProgressionCurve;
#pragma warning restore 0649

        void Reset()
        {
            lerpProgressionCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe( 4f/3f, 0,1.5f,1.5f),
                new Keyframe( 16f/9f, Mathf.InverseLerp(4/3f, 2/1f, 16/9f), 1.5f, 1.5f),
                new Keyframe( 2f/1f, 1, 1.5f,1.5f),
            });
        }

        /// <summary>
        /// The curve sampled at the current screen's aspect ratio, re-read from <see cref="Screen"/> on every access.
        /// </summary>
        /// <value>
        /// Whatever the curve yields for the current ratio — normally <c>0..1</c>, though the curve itself is free to
        /// leave that range.
        /// </value>
        public float Value
        {
            get
            {
                bool landscape = Screen.width > Screen.height;
                if (landscape)
                {
                    return lerpProgressionCurve.Evaluate(Screen.width / (float)Screen.height);
                }
                else
                {
                    return lerpProgressionCurve.Evaluate(Screen.height / (float)Screen.width);
                }
            }
        }

        /// <summary>
        /// Lets the component stand in for its own <see cref="Value"/>, so it can be used directly in arithmetic.
        /// </summary>
        /// <param name="lor">The component to sample.</param>
        /// <returns>The current <see cref="Value"/>.</returns>
        public static implicit operator float(LerpOnRatio lor) => lor.Value;
    }
}
