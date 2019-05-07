using E7.E7Unity;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AnimatorTriggerMarker))]
[CanEditMultipleObjects]
public class AnimatorTriggerMarkerDrawer : Editor
{

    private Animator GetBoundAnimator()
    {
        //Marker could get to its track, but a track couldn't get to its bound object.
        //The bound object actually is a function of PlayableDirector (even though you see the slot on the track, that's just for your convenience)
        //So we need to get the director that is "previewing" the timeline right now, that is the inspected director. 
        //Then use the track asset as a key to get the correct binding.
        if(target is AnimatorTriggerMarker atm && TimelineEditor.inspectedDirector != null)
        {
            if(TimelineEditor.inspectedDirector.GetGenericBinding(atm.parent) is Animator ani)
            {
                ani.Rebind(); //Somehow the trigger list returns 0 elements if we touch them until domain reload, if not forcing rebind.
                return ani;
            }
        }
        return null;
    }

    /// <summary>
    /// Removed background image and the selection dot on the right from object field.
    /// </summary>
    private class ReadOnlyObjectField<T> : ObjectField where T : UnityEngine.Object
    {
        public ReadOnlyObjectField(T obj) : base()
        {
            SetValueWithoutNotify(obj);
            this.Q(name: null, className: "unity-object-field__selector").RemoveFromClassList("unity-object-field__selector");
            this.Q(name: null, className: "unity-object-field-display").RemoveFromClassList("unity-object-field-display");
        }

        public ReadOnlyObjectField(T obj, string label) : base(label)
        {
            SetValueWithoutNotify(obj);
            this.Q(name: null, className: "unity-object-field__selector").RemoveFromClassList("unity-object-field__selector");
            this.Q(name: null, className: "unity-object-field-display").RemoveFromClassList("unity-object-field-display");
        }

        public float MeasureWidth()
        {
            Image icon = this.Q<Image>();
            Label label = this.Q<Label>();
            string labelText = label.text; //"Director (Animator)"
            return icon.style.maxWidth.value.value + EditorStyles.objectField.CalcSize(new GUIContent(labelText)).x;
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        var vis = new VisualElement();

        var prop = serializedObject.FindProperty(nameof(AnimatorTriggerMarker.trigger));
        var pf = new PropertyField(prop);
        //[ToolTip] is not included in the FindProperty, we need to do it mannually here.
        pf.tooltip = "Notify this trigger string to INotificationReceiver (It should be the AnimatorTriggerReceiver) on the same GameObject as the bound Animator.";
        vis.Add(pf);

        Animator animator = GetBoundAnimator();

        if (animator != null)
        {
            var box = new Box();
            box.style.height = 1;
            box.style.marginTop = 5;
            box.style.marginBottom = 5;
            vis.Add(box);

            var horizontalEqually = new VisualElement();

            var animatorPreview = new ReadOnlyObjectField<Animator>(animator);
            animatorPreview.style.width = animatorPreview.MeasureWidth();
            horizontalEqually.Add(animatorPreview);

            var arrow = new VisualElement();
            arrow.AddToClassList("unity-foldout__toggle");
            var arrowInner = new VisualElement();
            arrowInner.AddToClassList("unity-toggle__checkmark");
            arrow.style.width = 20;
            arrow.style.marginLeft = 0;
            arrow.Add(arrowInner);
            horizontalEqually.Add(arrow);

            var rac = new ReadOnlyObjectField<RuntimeAnimatorController>(animator.runtimeAnimatorController, animator.runtimeAnimatorController == null ? "No animator controller" : string.Empty);
            rac.style.width = rac.MeasureWidth();
            horizontalEqually.Add(rac);

            horizontalEqually.style.flexDirection = FlexDirection.Row;

            vis.Add(horizontalEqually);

            var availableTriggers = animator.parameters.Where(x => x.type == AnimatorControllerParameterType.Trigger).Select(x => x.name).ToList();

            var label = new Label(availableTriggers.Count > 0 ? "Available Triggers" : "No trigger defined");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            vis.Add(label);

            if (availableTriggers.Count > 0)
            {

                foreach (string trigger in availableTriggers)
                {
                    Button btn = new Button(() =>
                    {
                        prop.stringValue = trigger;
                        serializedObject.ApplyModifiedProperties();
                    })
                    { text = trigger };

                    btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                    vis.Add(btn);
                }
            }
        }
        return vis;
    }
}
