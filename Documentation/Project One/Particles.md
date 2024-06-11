# Particle System

*namespace Emotion.Game.Particles*

ver. 0.1

Status: Unimplemented

## Use Case

Particles can be used to imitate chaotic systems like fire, smoke, magic spells, dust, clouds, sparks, and others. It does this by simulating and rendering many short lived sprites (called Particles) which follow predefined behaviors with some optional randomness.

## Usage Overview

The particle system works by continously spawning particles at a specified interval and interpolating each particle through a "lifecycle". At the end of a lifecycle the particle is removed.
The interpolation of each particle goes through a series of keyframes `typeof(ParticleKeyFrame)`. Keyframes define a timestamp in the particle lifecycle the values the particle should have at that moment, meaning as time progresses the particle's values change.
Keyframes can define any or all of the following properties:

+ **Direction** (normalized) _typeof(Vector3)_
    > The direction the particle will move in.
+ **DirectionRandomMax** (normalized) _typeof(Vector3)_
    > If both this and Direction is defined, the particle's direction will be randomized between the two values.
+ **Acceleration** (world space units) _typeof(float)_
    > Acceleration is added to the particle each tick in the particle's direction.
+ **Rotation** (angle degrees 0-360) _typeof(float)_
    > The rotation of the particle.
+ **Color** _typeof(Color)_
    > The vertex color of the particle.
+ **Size** (world space units) _typeof(Vector2)_
    > The size of the particle.

Any properties the keyframe does not define will not be interpolated. Check the examples section for more information.

### Usage

To setup and use particles first you want to define a `ParticleSystem` instance. There are three parameters to consider `Lifetime`, `Periodicity`, and the `InitialFrame`. All three are taken in by the constructor.

+ **Lifetime** (milliseconds) _typeof(float)_
    > How long each particle will live for.
+ **Periodicity** (milliseconds) _typeof(float)_
    > The interval for spawning particles.
+ **InitialFrame** _typeof(ParticleKeyFrame)_
    > The initial key frame which defines the initial state of newly spawned particles.

Then you setup the particle's kay frames via the `KeyFrames` property.
You might also want to set the ParticleSystem's `Position` property which defined where it is in space.

Once you have your particle system setup you should call its `Update` function every tick, and its `Render` function every frame. The system will then continously spawn, simulate, and render particles.

### Examples

Say we have a particle system with the following parameters:

```text
Lifetime: 1000
Periodicity: 100
InitialFrame: Size=vec2(5), Color=red
KeyFrames: [ { Time = 500, Size=vec2(25) }, {Time=1000, Color=black, Size=vec2(80)} ]
```

Particles in this system start of with size 5 and red. One particle is spawned. 100 milliseconds later, a second particle will have spawned.

Let's stick to following the first particle. At this point it would have completed **10%** *(100/1000)* of its lifecycle.
There are two keyframes ahead, the first one interpolates size. The particle is **0.2f** of the way to this keyframe *(100/500)* meaning its size will be `Lerp(vec2(5), vec2(25), 0.2f)`. The second keyframe specifies both a size and a color, however since the first specifies a size already only the color is interpolated at this time. This means the particle's color at this point in time is `Lerp(red, black, 0.1f)`.

Fast forward 400 milliseconds into the future. The first particle is at **50%** *(500/1000)* of its lifecycle. At this point its size is **vec2(25)** and it's color is `Lerp(red, black, 0.1f)`. Since it has passed the first keyframe it will now start interpolating its size towards the value the next keyframe specifies (if any), in this case thats **vec2(80)**.

100 milliseconds later the particle is already at **600** milliseconds of its life and its size is `Lerp(vec2(25), vec2(80), 0.2f)`, since it is 20% of the way between the two keyframes *(600 - 500) / (1000 - 500)*. It's color is `Lerp(red, black, 0.6f)`.

400 milliseconds later this particle's color is **black** and its size is **vec2(80)**. The particle has reached the end of its life and it will now despawn.

## Customizing

If you don't like the default particle simulation model (Direction + Acceleration) you can create a class that inherits from `ParticleSystem` and override the `SimulateParticleMotion` to create custom motion and shapes.

## Technical Overview

Each particle system reuses a static array of particle structs to prevent allocation during simulation.

Notes:
- Particle systems are drawn using the sprite render stream.
- Particle positions are in their center, rather than top-left as is with normal sprites.

#### Shader Uniforms

When rendering particles the system uploads the following information for each particle.

+ *uniform float LifeLeft*
    > A 0-1 float corresponding to where the particle is in its lifecycle. The value is 1 when it spawns, and it linearly interpolates towards 0.

+ *uniform vec3 ParticleRandom*
    > A vector3 holding three random values between 0-1 unique for this particle.

## Files and Tests

Code found at: Emotion/Game/Particles
