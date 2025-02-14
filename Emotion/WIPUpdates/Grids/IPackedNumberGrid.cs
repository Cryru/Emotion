#nullable enable

using Emotion.Editor;

namespace Emotion.WIPUpdates.Grids;

public interface IPackedNumberGrid<T, TNumeric> : IGrid<T>
    where T : struct
    where TNumeric : struct, INumber<TNumeric>
{
    [DontShowInEditor]
    public string DataPacked
    {
        get => NumericDataPacking.PackData<T, TNumeric>(GetRawData());
        set => SetRawData(NumericDataPacking.UnpackData<T, TNumeric>(value), GetSize());
    }
}

public interface IPackedNumberData<T, TNumeric>
    where T : struct
    where TNumeric : struct, INumber<TNumeric>
{
    [DontShowInEditor]
    public string DataPacked
    {
        get => NumericDataPacking.PackData<T, TNumeric>(GetRawData());
        set => SetRawData(NumericDataPacking.UnpackData<T, TNumeric>(value));
    }

    T[] GetRawData();

    void SetRawData(T[] data);
}