#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.Animation3D;
using Emotion.Game.World3D;
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

		private EditorDropDownButtonDescription[] _noAnimationItems =
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
			Object.SetAnimation((string) value);
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
				var animationButtons = new EditorDropDownButtonDescription[animations.Length + 1];
				animationButtons[0] = _noAnimationItems[0];
				_noAnimationItems[0].Click = (_, __) =>
				{
					Object.SetAnimation(null);
				};
				for (var i = 0; i < animations.Length; i++)
				{
					SkeletalAnimation anim = animations[i];
					animationButtons[i + 1] = new EditorDropDownButtonDescription
					{
						Name = anim.Name,
						Click = (_, __) =>
						{
							Object.SetAnimation(anim.Name);
						}
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
	}
}