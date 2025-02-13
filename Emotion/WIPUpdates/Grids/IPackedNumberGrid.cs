#nullable enable

using Emotion.Editor;

namespace Emotion.WIPUpdates.Grids;

public interface IPackedNumberGrid<T> : IGrid<T> where T : struct, INumber<T>
{
    [DontShowInEditor]
    public string DataPacked
    {
        get => NumericDataPacking.PackData<T>(GetRawData());
        set => SetRawData(NumericDataPacking.UnpackData<T>(value), GetSize());
    }
}