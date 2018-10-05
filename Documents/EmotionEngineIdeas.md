Proposals for Emotion Engine Features


# Map Buffer

Remake into an object with MapVertex and drawing constraints on both ends.
Then abstract with things such as MapQuad, MapLine.
Then abstract drawing with a DrawAsLines, DrawAsQuads etc.
Abstract multiple texture support.
Allow for changing individual vertices.
Auto map start and finish.
Maybe inherit from IEnum.
Test active resizing.

# Save File Controller

Inherit from a managed object.
System syncs changes to the object with a json file on the system.
IsSaving variable, with async IO.
Remove Soul.IO as a dependency.

# Soul

Move MathHelper to Emotion.Math

# Sound

Sound on another thread.
Manipulated through actions similar to the OpenGL ThreadManager.

# Debugger

Move thread start to module start so Context running can be used. Also move Context IsRunning to before module setup.

# Settings

Separate settings into individual objects.