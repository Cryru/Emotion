function GameLoop(timeStamp) {
    Emotion.manager.invokeMethod("RunLoop", timeStamp);
    window.requestAnimationFrame(GameLoop);
}

function onResize() {
    if (!Emotion.canvas) return;
    console.log("Javascript detected resize.");

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

function glGet(id) {

    var value = Emotion.gl.getParameter(id);
    if (value === undefined) {
        value = 0;
    }
    if (typeof(value) === "number") {
        value = [value];
    }
    return value;
}

function GetGLExtensions() {
    return Emotion.gl.getSupportedExtensions().join(" ");
}

function glGetError() {
    return Emotion.gl.getError();
}