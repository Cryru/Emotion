# Emotion.Game.Objects.Camera

Game viewports and cameras.

## CameraBase : Transform

The base for a camera. Provides a target the camera is pointed at and implements Transform for camera location and size.

## final MouseCamera : CameraBase

A camera which stays between the target and the mouse cursor on the screen.

## final TargetCamera : CameraBase

A standard camera which follows the target with smoothing.