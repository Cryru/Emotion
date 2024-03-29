﻿#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Input;
using Emotion.Web.RazorTemplates;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web.Platform
{
    public class WebHost : PlatformBase
    {
        public Action TickAction;
        public Action DrawAction;

        private RenderCanvas _canvasElement;
        private Vector2 _size;

        public IJSUnmarshalledRuntime JsRuntime
        {
            get => _canvasElement.JsRuntime;
        }

        public WebHost(RenderCanvas canvasElement)
        {
            DisplayMode = DisplayMode.Windowed; // Needed to be able to set the size.
            NamedThreads = false; // Threads can be named here, but it causes weird behavior.

            _canvasElement = canvasElement;
            _canvasElement.JsRuntimeMarshalled.InvokeVoid("InitJavascript", DotNetObjectReference.Create(this));
            Context = new WebGLContext(JsRuntime);
            Audio = new NullAudioContext(this);
            // Audio : https://developer.mozilla.org/en-US/docs/Web/API/MediaStreamAudioSourceNode
        }

        public void InitLoop(Action tick, Action draw)
        {
            TickAction = tick;
            DrawAction = draw;
        }

        public async Task AsyncSetup()
        {
            var webAssetSource = new WebAssetSource("AssetBlobs", _canvasElement.HttpClient);
            webAssetSource.StartLoad();
            await webAssetSource.LoadingTask;
            Engine.AssetLoader.AddSource(webAssetSource);
        }

        protected override void SetupInternal(Configurator config)
        {
            Engine.AssetLoader.AddStore(new WebAssetStore(this));
        }

        public override void DisplayMessageBox(string message)
        {
            Console.WriteLine(message);
        }

        protected override bool UpdatePlatform()
        {
            return true;
        }

        public override WindowState WindowState { get; set; }

        protected override Vector2 GetPosition()
        {
            return Vector2.Zero;
        }

        protected override void SetPosition(Vector2 position)
        {
        }

        protected override Vector2 GetSize()
        {
            return _size;
        }

        protected override void SetSize(Vector2 size)
        {
            _size = size;
            Resized(size);
        }

        public override IntPtr LoadLibrary(string path)
        {
            return IntPtr.Zero;
        }

        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return IntPtr.Zero;
        }

        protected override void UpdateDisplayMode()
        {
        }

        #region JS API

        [JSInvokable]
        public void SetSizeJs(int width, int height)
        {
            SetSize(new Vector2(width, height));
        }

        [JSInvokable]
        public void RunLoop(float mouseX, float mouseY)
        {
            if (Engine.Status != EngineStatus.Running) return;
            MousePosition = new Vector2(mouseX, mouseY);
            TickAction();
            DrawAction();
        }

        [JSInvokable]
        public void KeyDown(int keyCode)
        {
            UpdateKeyStatus((Key) keyCode, true);
        }

        [JSInvokable]
        public void KeyUp(int keyCode)
        {
            UpdateKeyStatus((Key) keyCode, false);
        }

        [JSInvokable]
        public void MouseKeyDown(int keyCode)
        {
            UpdateKeyStatus((Key) keyCode + (int) Key.MouseKeyStart, true);
        }

        [JSInvokable]
        public void MouseKeyUp(int keyCode)
        {
            UpdateKeyStatus((Key) keyCode + (int) Key.MouseKeyStart, false);
        }

        #endregion
    }
}