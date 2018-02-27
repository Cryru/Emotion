// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Microsoft.Xna.Framework;
using Soul.Engine.Components.UI;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Systems.UI
{
    internal class MouseEvents : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(MouseHandler)};
        }

        protected internal override void Setup()
        {
            // Run after the renderer.
            Order = 10;
        }

        internal override void Update(Entity link)
        {
            // Get component.
            MouseHandler mouseHandler = link.GetComponent<MouseHandler>();

            // Get mouse position
            Vector2 mPos = Input.MouseLocation();

            // Check if the mouse is inside.
            if (link.Bounds.Intersects(mPos))
            {
                // Check if last frame it wasn't.
                if (!mouseHandler.WasIn)
                {
                    // Then fire entered.
                    mouseHandler.OnEnter?.Invoke();

                    // Set tracker to true.
                    mouseHandler.WasIn = true;
                }

                // Check if clicked.
                if (Input.MouseButtonPressed(MouseButton.Left))
                    mouseHandler.OnClick?.Invoke();

                // Check if held.
                if (Input.MouseButtonHeld(MouseButton.Left))
                {
                    mouseHandler.IsHeld = true;
                }                    
            }
            else
            {
                // Check if last frame it was in.
                if (mouseHandler.WasIn)
                {
                    // Then fire the leave event.
                    mouseHandler.OnLeave?.Invoke();

                    // Set tracker to false.
                    mouseHandler.WasIn = false;
                }
            }

            // Check if considered held.
            if (mouseHandler.IsHeld)
            {
                // Check if mouse is still being held.
                if (Input.MouseButtonHeld(MouseButton.Left))
                {
                    mouseHandler.OnHeld?.Invoke();
                }
                else
                {
                    // Otherwise it is no longer held.
                    mouseHandler.IsHeld = false;
                    mouseHandler.OnLetGo?.Invoke();
                }
            }
        }
    }
}