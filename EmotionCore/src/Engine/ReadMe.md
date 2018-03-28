# Emotion.Engine

This namespace contains objects which make up the core of the Emotion engine. They are the same across platforms.

## ScriptingEngine - Context.ScriptingEngine

A javascript engine for executing scripts. Uses Jint - https://github.com/sebastienros/jint

Related Settings:

- ScriptTimeout:Timespan - The maximum time a script can run before it is stopped. Since scripts are not async and block the main thread you should be careful with how much time they can run for. Can only be changed at context initialization.
