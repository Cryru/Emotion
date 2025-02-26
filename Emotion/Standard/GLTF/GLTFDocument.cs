#region Using

using Emotion.Standard.GLTF;
using System.Text.Json.Serialization;


#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFDocument
{
    [JsonPropertyName("buffers")]
    public GLTFBuffer[] Buffers { get; set; }

    [JsonPropertyName("bufferViews")]
    public GLTFBufferView[] BufferViews { get; set; }

    [JsonPropertyName("images")]
    public GLTFImage[]? Images { get; set; }

    [JsonPropertyName("textures")]
    public GLTFTexture[] Textures { get; set; }

    [JsonPropertyName("accessors")]
    public GLTFAccessor[] Accessors { get; set; }

    [JsonPropertyName("animations")]
    public GLTFAnimation[] Animations { get; set; }

    [JsonPropertyName("meshes")]
    public GLTFMesh[] Meshes { get; set; }

    [JsonPropertyName("materials")]
    public GLTFMaterial[]? Materials { get; set; }

    [JsonPropertyName("skins")]
    public GLTFSkins[]? Skins { get; set; }

    [JsonPropertyName("nodes")]
    public GLTFNode[] Nodes { get; set; }
}

#region OLD TEST CODE

//// Test code
//// Try to calculate the fox animation Survey at time 50

//Dictionary<int, int> nodeToJoint = new Dictionary<int, int>();
//for (int i = 0; i < skin.Joints.Length; i++)
//{
//    nodeToJoint[skin.Joints[i]] = i;
//}

//Matrix4x4[] inverseBindMatrix = new Matrix4x4[skin.Joints.Length];
//GLTFAccessor invBindMatrixAccessor = gltfDoc.Accessors[skin.InverseBindMatrices];
//ReadOnlyMemory<byte> invBindMatrixBuffer = GetGltfBuffer(gltfDoc, invBindMatrixAccessor, out int invBindMatrixStride);
//ReadOnlySpan<byte> invBindMatrixSpan = invBindMatrixBuffer.Span;
//for (int i = 0; i < skin.Joints.Length; i++)
//{
//    int matrixByteStart = i * invBindMatrixStride;
//    ReadOnlySpan<byte> thisMatrix = invBindMatrixSpan.Slice(matrixByteStart, 4 * 4 * 4);
//    ReadOnlySpan<Matrix4x4> thisMatrixFloat = MemoryMarshal.Cast<byte, Matrix4x4>(thisMatrix);
//    Assert(thisMatrixFloat.Length == 1);
//    inverseBindMatrix[i] = thisMatrixFloat[0];
//}

//GLTFAnimation? surveyAnimation = null;
//for (int i = 0; i < gltfDoc.Animations.Length; i++)
//{
//    var anim = gltfDoc.Animations[i];
//    if (anim.Name == "Survey")
//    {
//        surveyAnimation = anim;
//        break;
//    }
//}
//if (surveyAnimation == null) return;

//// Build matrix for a specific joint, for testing :)

//bool hasTranslation = false;
//Matrix4x4 translation = Matrix4x4.Identity;

//bool hasRotation = false;
//Matrix4x4 rotation = Matrix4x4.Identity;

//bool hasScale = false;
//Matrix4x4 scale = Matrix4x4.Identity;

//for (int i = 0; i < surveyAnimation.Channels.Length; i++)
//{
//    var channel = surveyAnimation.Channels[i];
//    var sampler = surveyAnimation.Samplers[channel.Sampler];

//    int samplerInputAccessorIdx = sampler.Input;
//    GLTFAccessor inputAccessor = gltfDoc.Accessors[samplerInputAccessorIdx];
//    ReadOnlyMemory<byte> inputBuffer = GetGltfBuffer(gltfDoc, inputAccessor, out int inputStride);
//    ReadOnlySpan<byte> inputDataSpan = inputBuffer.Span;

//    float[] timeData = new float[inputAccessor.Count];
//    for (int t = 0; t < inputAccessor.Count; t++)
//    {
//        ReadOnlySpan<byte> valueForThisOne = inputDataSpan.Slice(t * inputStride, inputAccessor.GetDataSize());
//        ReadOnlySpan<float> valueAsFloat = MemoryMarshal.Cast<byte, float>(valueForThisOne);
//        timeData[t] = valueAsFloat[0];
//    }

//    // Convert to milliseconds.
//    for (int t = 0; t < timeData.Length; t++)
//    {
//        timeData[t] *= 1000f;
//    }

//    float inputData = debugTime;
//    int inputDataAtIndex = -1;
//    for (var t = 0; t < timeData.Length; t++)
//    {
//        float key = timeData[t];
//        if (key >= inputData)
//        {
//            inputDataAtIndex = t == 0 ? 0 : t - 1;
//            break;
//        }
//    }
//    if (inputDataAtIndex == -1)
//    {
//        inputData = timeData[^1];
//        inputDataAtIndex = timeData.Length - 2;
//    }

//    int nextIndex = inputDataAtIndex + 1;
//    float percent = Maths.FastInverseLerp(
//        timeData[inputDataAtIndex],
//        timeData[nextIndex],
//        inputData
//    );

//    int samplerOutputAccessorIdx = sampler.Output;
//    GLTFAccessor outputAccessor = gltfDoc.Accessors[samplerOutputAccessorIdx];
//    ReadOnlyMemory<byte> outputBuffer = GetGltfBuffer(gltfDoc, outputAccessor, out int outputStride);
//    ReadOnlySpan<byte> outputDataSpan = outputBuffer.Span;

//    Vector4[] outputVec4 = new Vector4[outputAccessor.Count];
//    for (int t = 0; t < outputAccessor.Count; t++)
//    {
//        if (outputAccessor.Type == "VEC3")
//        {
//            ReadOnlySpan<byte> valueForThisOne = outputDataSpan.Slice(t * outputStride, outputAccessor.GetDataSize());
//            ReadOnlySpan<Vector3> valueAsVec3 = MemoryMarshal.Cast<byte, Vector3>(valueForThisOne);
//            outputVec4[t] = valueAsVec3[0].ToVec4();
//        }
//        else
//        {
//            ReadOnlySpan<byte> valueForThisOne = outputDataSpan.Slice(t * outputStride, outputAccessor.GetDataSize());
//            ReadOnlySpan<Vector4> valueAsVec4 = MemoryMarshal.Cast<byte, Vector4>(valueForThisOne);
//            outputVec4[t] = valueAsVec4[0];
//        }
//    }

//    var targetNodeIdx = channel.Target.Node;
//    var targetNode = gltfDoc.Nodes[targetNodeIdx];
//    var targetNodePath = channel.Target.Path;

//    Matrix4x4 outputMatrix = Matrix4x4.Identity;
//    if (targetNodePath == "rotation")
//    {
//        Vector4 inputLeft = outputVec4[inputDataAtIndex];
//        Vector4 inputRight = outputVec4[nextIndex];

//        Quaternion inputLeftQuart = new Quaternion(inputLeft.X, inputLeft.Y, inputLeft.Z, inputLeft.W);
//        Quaternion inputRightQuart = new Quaternion(inputRight.X, inputRight.Y, inputRight.Z, inputRight.W);

//        Quaternion outputValue = Quaternion.Slerp(inputLeftQuart, inputRightQuart, percent);
//        outputMatrix = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(outputValue));
//    }
//    else if (outputAccessor.Type == "VEC4" || outputAccessor.Type == "VEC3")
//    {
//        Vector4 inputLeft = outputVec4[inputDataAtIndex];
//        Vector4 inputRight = outputVec4[nextIndex];
//        Vector4 outputValue = Vector4.Lerp(inputLeft, inputRight, percent);
//        if (targetNodePath == "translation")
//        {
//            outputMatrix = Matrix4x4.CreateTranslation(outputValue.ToVec3());
//        }
//        else if (targetNodePath == "scale")
//        {
//            outputMatrix = Matrix4x4.CreateScale(outputValue.ToVec3());
//        }
//    }

//    Assert(nodeToJoint.ContainsKey(targetNodeIdx));
//    int jointId = nodeToJoint[targetNodeIdx];
//    if (jointId == debugJoint)
//    {
//        if (targetNodePath == "translation")
//        {
//            hasTranslation = true;
//            translation = outputMatrix;
//        }
//        else if (targetNodePath == "scale")
//        {
//            hasScale = true;
//            scale = outputMatrix;
//        }
//        else if (targetNodePath == "rotation")
//        {
//            hasRotation = true;
//            rotation = outputMatrix; 
//        }

//        DebugThing.Joint = targetNode.Name;
//        DebugThing.Time = debugTime;
//        DebugThing.Translation = translation;
//        DebugThing.Rotation = rotation;
//        DebugThing.Scale = scale;
//    }
//}

//var nodeToDebugIdx = skin.Joints[debugJoint];
//var nodeToDebug = gltfDoc.Nodes[nodeToDebugIdx];
//if (!hasTranslation && nodeToDebug.Translation != null)
//{
//    DebugThing.Translation = Matrix4x4.CreateTranslation(nodeToDebug.Translation[0], nodeToDebug.Translation[1], nodeToDebug.Translation[2]);
//}

//if (!hasRotation && nodeToDebug.Rotation != null)
//{
//    DebugThing.Rotation = Matrix4x4.CreateFromQuaternion(
//        Quaternion.Normalize(
//            new Quaternion(nodeToDebug.Rotation[0], nodeToDebug.Rotation[1], nodeToDebug.Rotation[2], nodeToDebug.Rotation[3])
//        ));
//}

//if (!hasScale && nodeToDebug.Scale != null)
//{
//    DebugThing.Scale = Matrix4x4.CreateScale(nodeToDebug.Scale[0], nodeToDebug.Scale[1], nodeToDebug.Scale[2]);
//}

//DebugThing.InverseBindMatrix = inverseBindMatrix[debugJoint];

#endregion