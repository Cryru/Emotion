# RenderComposer

The render composer is a buffer of commands to be executed by the renderer. It is cleared at the start of the frame and handed over
to the user to fill as they please. At the end of the frame the commands inside are executed.

The default commands are found in the [Emotion.Graphics.Command namespace](Emotion/Graphics/Command).

## RenderSprite(vec3, vec2, color, texture, rect)

This function generates a RenderSpriteCommand which is batched in a RenderSpriteBatchCommand if possible. It renders a quad sprite at
the specified location using the provided texture. If no texture is provided then it will be blank.

The provided color is used to tint the sprite, and the provided rectangle is the source rectangle (uv) in the specified texture. If it
isn't provided the whole texture will be rendered.

The maximum number of sprites that can fit in a batch is equal to the number of indices in the QuadIbo divided by 6.

## RenderVertices(vec3[], color...)

Renders the specified vertices using the SequentialIbo, in the specified color/s. The provided vertices must be clockwise (todo: check this).