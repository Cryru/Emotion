#region Using

using System.Diagnostics;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.UI;
using Emotion.Utility;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public class EditorPlotLineWindow : UIBaseWindow
{
	private class PlotLineInternalData
	{
		public bool Ready;

		public VertexBuffer? LinesVertexBuffer;
		public VertexArrayObject<VertexData>? LinesVao;
	}

	private static readonly ObjectPool<PlotLineInternalData> PlotLineDataPool = new(() =>
	{
		var newData = new PlotLineInternalData();

		GLThread.ExecuteGLThreadAsync(() =>
		{
			newData.LinesVertexBuffer = new VertexBuffer();
			newData.LinesVao = new VertexArrayObject<VertexData>(newData.LinesVertexBuffer);
			newData.Ready = true;
		});

		return newData;
	});

	public EditorPlotLineWindow()
	{
		MinSizeY = 20;
		MaxSizeY = 20;
		MaxSizeX = 110;
		WindowColor = Color.Black;
		InputTransparent = false;
	}

	private PlotLineInternalData? _plotData;
	private VertexData[]? _data;
	private float[]? _sourceData;
	private float[]? _normalizedData;
	private float _mouseShownValue;

	public override void AttachedToController(UIController controller)
	{
		Debug.Assert(_plotData == null);

		_plotData = PlotLineDataPool.Get();
		base.AttachedToController(controller);
	}

	public override void DetachedFromController(UIController controller)
	{
		if (_plotData != null)
		{
			PlotLineDataPool.Return(_plotData);
			_plotData = null;
		}

		base.DetachedFromController(controller);
	}

	protected override void AfterLayout()
	{
		base.AfterLayout();
		if (_data != null) InitializeData(_data.Length);
	}

	public void SetData(float[] data)
	{
		float average = Helpers.GetArrayAverage(data);
		float highest = average + average * 1.5f;
		float lowest = average - average * 1.5f;

		_normalizedData = new float[data.Length];
		for (var i = 0; i < _normalizedData.Length; i++)
		{
			float val = data[i];
			_normalizedData[i] = Maths.Clamp01((val - lowest) / highest);
		}

		_sourceData = data;

		InitializeData(data.Length);
	}

	private void InitializeData(int length)
	{
		if (_data == null || _data.Length != length)
			_data = new VertexData[length];

		float spaceBetween = Width / 100;

		for (var i = 0; i < _data.Length; i++)
		{
			ref VertexData vert = ref _data[i];
			vert.Color = Color.White.ToUint();
			vert.UV = Vector2.Zero;

			vert.Vertex.X = X + spaceBetween * i;

			if (_normalizedData != null)
			{
				float normalizedVal = _normalizedData[i];
				vert.Vertex.Y = Y + Height / 2 + (normalizedVal - 0.5f) * Height / 2;
			}
		}
	}

	public override void OnMouseMove(Vector2 mousePos)
	{
		Vector2 positionWithinSelf = mousePos - _renderBounds.Position;
		float xPos = positionWithinSelf.X;

		float spaceBetweenLines = Width / 100;
		var lineIndex = (int) (xPos / spaceBetweenLines);

		if (_sourceData != null)
		{
			float value = _sourceData[lineIndex];
			_mouseShownValue = value;
		}

		base.OnMouseMove(mousePos);
	}

	protected override bool RenderInternal(RenderComposer c)
	{
		c.RenderSprite(Bounds, WindowColor);
		c.FlushRenderStream();

		// todo: LineStrip render mode in RenderStream
		if (_plotData != null && _plotData.Ready && _data != null)
		{
			Debug.Assert(_plotData.LinesVao != null);
			Debug.Assert(_plotData.LinesVertexBuffer != null);

			Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer);
			VertexArrayObject.EnsureBound(_plotData.LinesVao);
			_plotData.LinesVertexBuffer.Upload(_data);
			Gl.DrawArrays(PrimitiveType.LineStrip, 0, _data.Length);
		}

		if (MouseInside)
		{
			float fontSize = 10 * GetScale();
			FontAsset? font = FontAsset.GetDefaultBuiltIn();
			Vector3 posAboveMouse = (Engine.Host.MousePosition - new Vector2(0, fontSize)).ToVec3(Z);
			c.RenderString(posAboveMouse, Color.Yellow, _mouseShownValue.ToString(), font.GetAtlas(fontSize));
		}

		return base.RenderInternal(c);
	}
}