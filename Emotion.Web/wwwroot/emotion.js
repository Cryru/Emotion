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

const SIZEOF_FLOAT = 4;

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

function glClearColor(vec4Col) {
    const r = Blazor.platform.readFloatField(vec4Col, 0);
    const g = Blazor.platform.readFloatField(vec4Col, SIZEOF_FLOAT);
    const b = Blazor.platform.readFloatField(vec4Col, SIZEOF_FLOAT * 2);
    const a = Blazor.platform.readFloatField(vec4Col, SIZEOF_FLOAT * 3);
    Emotion.gl.clearColor(r, g, b, a);
}

gShaders = [];

function glCreateShader(type) {
    const shader = Emotion.gl.createShader(type);
    gShaders.push(shader);
    return gShaders.length - 1;
}

function glShaderSource(shaderId, source) {
    const shader = gShaders[shaderId];
    const shaderSourceJs = BINDING.conv_string(source);
    Emotion.gl.shaderSource(shader, shaderSourceJs);
}

function glCompileShader(shaderId) {
    const shader = gShaders[shaderId];
    Emotion.gl.compileShader(shader, shader);
}

function glGetShader(shaderId, param) {
    const shader = gShaders[shaderId];
    var value = Emotion.gl.getShaderParameter(shader, param);
    if (value === null) return null;
    if (typeof(value) === "number")
        value = [value];
    else if (typeof(value) === "boolean")
        value = [value ? 1 : 0];

    value = new Int32Array(value);
    return BINDING.js_typed_array_to_array(value);
}

function glGetShaderInfo(shaderId) {
    const shader = gShaders[shaderId];
    const value = Emotion.gl.getShaderInfoLog(shader);
    return BINDING.js_to_mono_obj(value);
}