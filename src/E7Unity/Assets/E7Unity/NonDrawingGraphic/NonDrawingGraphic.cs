using UnityEngine;
using UnityEngine.UI;

namespace E7.E7Unity
{
    /// <summary>
    /// A <see cref="Graphic"/> that receives raycasts across its rectangle without drawing anything.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A press area needs a graphic for the event system to hit, but not a visible one. An empty legacy <c>Text</c>
    /// or a fully transparent <see cref="Image"/> both happen to work, at the cost of a font dependency in the first
    /// case and a transparent quad going through the batch in the second. This produces no geometry at all while
    /// still filling its rect for hit-testing.
    /// </para>
    /// <para>
    /// The dirty-marking calls are deliberately inert, so it never queues itself for a rebuild it has nothing to
    /// contribute to.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(CanvasRenderer))]
    public class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty()
        {}

        public override void SetVerticesDirty()
        {}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
