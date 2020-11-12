// --------------------------------
// Platform Code
// --------------------------------

function GameLoop(timeStamp) {
    Emotion.manager.invokeMethod("RunLoop", timeStamp);
    window.requestAnimationFrame(GameLoop);
}

function onResize() {
    if (!Emotion.canvas) return;

    Emotion.canvas.width = window.innerWidth;
    Emotion.canvas.height = window.innerHeight;
    Emotion.manager.invokeMethodAsync("HostResized", Emotion.canvas.width, Emotion.canvas.height);
}

window.InitJavascript = (manager) => {
    var canvasContainer = document.getElementById("container");
    var canvases = canvasContainer.getElementsByTagName("canvas") || [];
    var canvas = canvases.length ? canvases[0] : null;
    var context;
    if (canvas) {
        context = canvas.getContext("webgl2");
    }

    window.Emotion = {
        manager: manager,
        canvas: canvas,
        gl: context || false
    };

    window.addEventListener("resize", onResize);
    onResize();
    window.requestAnimationFrame(GameLoop);
};

// --------------------------------
// WebGL Driver
// --------------------------------

function glGet(id) {
    var value = Emotion.gl.getParameter(id);
    if (value === undefined) {
        value = 0;
    }
    if (typeof(value) === "string") {
        return BINDING.js_to_mono_obj(value);
    }
    if (typeof(value) === "number") {
        value = [value];
    }
    value = new Int32Array(value);
    return BINDING.js_typed_array_to_array(value);
}

function GetGLExtensions() {
    return BINDING.js_to_mono_obj(Emotion.gl.getSupportedExtensions().join(" "));
}

function glGetError() {
    return Emotion.gl.getError();
}

gBuffers = [];

function glGenBuffers(count) {
    const buffers = new Uint32Array(count);
    for (let i = 0; i < count; i++) {
        const newBuffer = Emotion.gl.createBuffer();
        gBuffers.push(newBuffer);
        buffers[i] = gBuffers.length;
    }
    return BINDING.js_typed_array_to_array(buffers);
}

function glBindBuffer(target, bufferId) {
    const buffer = bufferId < 0 ? null : gBuffers[bufferId - 1];
    Emotion.gl.bindBuffer(target, buffer);
}

function glBufferData(target, size, ptr) {
    const memory = new Uint8Array(wasmMemory.buffer, ptr, size);
    // All WebGL buffers are marked as STREAM_DRAW (0x88E0) as it's the most commonly used in Emotion.
    // The real reason is that Unmarshalled call is limited to 3 arguments :)
    Emotion.gl.bufferData(target, memory, 0x88E0, 0, size);
}