using System;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Graphics;
using ImGuiNET;
using Jint.Runtime.Debugger;

namespace Rationale.Interop
{
    public class ScriptDebugger : Window
    {
        private string _currentScriptExec = "";
        private string _currentStatement = "";
        private Action<StepMode> _stepFunction;

        private bool _canStepInto;
        private bool _canStepOut;

        public ScriptDebugger() : base("Script Debugger", new Vector2(300, 300))
        {
            // Attach to the Jint interpreter.
            Engine.ScriptingEngine.Interpreter.Step += Interpreter_Step;
            Engine.ScriptingEngine.Interpreter.Break += Interpreter_Break;
        }

        private Jint.Runtime.Debugger.StepMode Interpreter_Break(object sender, Jint.Runtime.Debugger.DebugInformation e)
        {
            return Jint.Runtime.Debugger.StepMode.Into;
        }

        private Jint.Runtime.Debugger.StepMode Interpreter_Step(object sender, Jint.Runtime.Debugger.DebugInformation e)
        {
            // Check if on GL Thread - we don't want to block it.
            if (GLThread.IsGLThread())
            {
                _currentScriptExec = "Found script, but it was on the GL Thread where it can't be debugged.";
                return StepMode.None;
            }

            // If this errors it will break the script, that's why it should be in a try-catch.
            try
            {
                // Get the source and write it down.
                string sourceFull = e.CurrentStatement.Location.Source;
                if (string.IsNullOrEmpty(sourceFull))
                {
                    _currentScriptExec = "Unknown Script Source";
                }
                else
                {
                    _currentScriptExec = sourceFull;
                    _currentStatement = sourceFull.Substring(e.CurrentStatement.Range[0], e.CurrentStatement.Range[1] - e.CurrentStatement.Range[0]);
                }

                _canStepInto = true;
                if (e.CallStack.Count > 0) _canStepOut = true;

                bool wait = true;
                StepMode exit = StepMode.None;
                _stepFunction = mode =>
                {
                    exit = mode;
                    wait = false;
                };

                while (wait)
                {
                    if (!Open) return StepMode.Into;
                    Engine.ScriptingEngine.Interpreter.ResetTimeoutTicks();
                }

                _canStepInto = false;
                _canStepOut = false;

                return exit;
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Rationale script debugger encountered an error: {ex}", Adfectus.Logging.MessageSource.Other);
                return StepMode.Into;
            }
        }

        protected override void DrawContent()
        {
            Vector2 windowSize = ImGui.GetWindowSize();
            bool showCurrentStatement = false;


            ImGui.PushItemWidth(-10);
            ImGui.InputTextMultiline("", ref _currentScriptExec, 1000 * 10, new Vector2(windowSize.X - 30, windowSize.Y - 100), ImGuiInputTextFlags.ReadOnly);
            showCurrentStatement = ImGui.IsItemHovered();

            if (_canStepInto)
            {
                bool step = ImGui.Button("Step Into");
                showCurrentStatement = showCurrentStatement || ImGui.IsItemHovered();
                if (step)
                {
                    _stepFunction(StepMode.Into);
                }
            }

            if (_canStepOut)
            {
                bool step = ImGui.Button("Step Out");
                showCurrentStatement = showCurrentStatement || ImGui.IsItemHovered();
                if (step)
                {
                    _stepFunction(StepMode.Out);
                }
            }

            if (showCurrentStatement)
            {
                {
                    ImGui.BeginTooltip();
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), _currentStatement);
                    ImGui.EndTooltip();
                }
            }
        }

        public override void Close()
        {
            // Detach from the Jint interpreter.
            Engine.ScriptingEngine.Interpreter.Step -= Interpreter_Step;
            Engine.ScriptingEngine.Interpreter.Break -= Interpreter_Break;
        }
    }
}