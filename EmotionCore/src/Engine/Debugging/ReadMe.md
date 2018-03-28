# Emotion.Engine.Debugging

Diagnostic and debugging assistance. Is only compiled when the constant DEBUG is provided. Be careful when referencing the namespace as it will be empty if the constant is missing.

## Debugger - Context.Debugger

Opens a console window which will display debugging messages, and accepts input which is directly sent to the ScriptEngine instance of its parental context. Debugging messages are also logged to files, with prepended timestamps, in the Logs folder.

Functions:

```void Log(MessageType, MessageSource, Message)``` - Creates a debugging message of the specified type, coming from the specified source.

## MessageSource

The source of the message. Used for easily determining which part of the system logged it.

## MessageType

The type of message. Determines the color of the message in the console, and is used to easily determine what kind of message it is.

- Trace - General message spam.
- Info - Informing of an action.
- Warning - Possibly unwanted behavior.
- Error - Something went wrong. Most errors crash the engine so optimally this will be only seen in the log files.