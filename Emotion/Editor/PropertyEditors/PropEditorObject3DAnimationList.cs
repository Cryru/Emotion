#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.Animation3D;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;

#endregion

namespace Emotion.Editor.PropertyEditors
{
    // Specialized editor to be used for MapEditorObjectPropertiesPanel when it opens a GameObject3D
    public class PropEditorObject3DAnimationList : EditorButtonDropDown, IPropEditorGeneric
    {
        public XMLFieldHandler Field { get; set; }

        public GameObject3D Object;

        private EditorDropDownItem[] _noAnimationItems =
        {
            new()
            {
                Name = "No Animation",
            }
        };

        public PropEditorObject3DAnimationList(GameObject3D obj)
        {
            Object = obj;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            FillAnimationItems();
        }

        public void EntityChanged()
        {
            FillAnimationItems();
        }

        public void SetValue(object value)
        {
            if (Helpers.AreObjectsEqual(Object.CurrentAnimation, value)) return;
            ObjectSetAnimation((string) value);
        }

        public object GetValue()
        {
            return Object.CurrentAnimation;
        }

        public void SetCallbackValueChanged(Action<object> callback)
        {
            // nop
        }

        private void FillAnimationItems()
        {
            SkeletalAnimation[] animations = Object.Entity?.Animations;
            if (animations != null)
            {
                var currentIdx = 0;
                var animationButtons = new EditorDropDownItem[animations.Length + 1];
                animationButtons[0] = _noAnimationItems[0];
                _noAnimationItems[0].Click = (_, __) => { ObjectSetAnimation(null); };
                for (var i = 0; i < animations.Length; i++)
                {
                    SkeletalAnimation anim = animations[i];
                    animationButtons[i + 1] = new EditorDropDownItem
                    {
                        Name = anim.Name,
                        Click = (_, __) => { ObjectSetAnimation(anim.Name); }
                    };

                    if (Object.CurrentAnimation == anim.Name)
                        currentIdx = i + 1;
                }

                SetItems(animationButtons, currentIdx);
            }
            else
            {
                SetItems(_noAnimationItems, 0);
            }
        }

        private void ObjectSetAnimation(string? animName)
        {
            // Special case to serialize the set animation.
            if (Object is GenericObject3D obj3D)
            {
                obj3D.SetAnimation(animName);
                return;
            }

            Object.SetAnimation(animName);
        }
    }
}