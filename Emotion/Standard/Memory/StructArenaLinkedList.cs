#nullable enable



#nullable enable

namespace Emotion.Standard.Memory;

public interface IStructArenaLinkedListItem
{
    public int NextItem { get; set; }
}

public struct StructArenaLinkedList<T> where T : struct, IStructArenaLinkedListItem
{
    public int StartIndex;
    public int EndIndex;

    public void Reset()
    {
        StartIndex = -1;
        EndIndex = -1;
    }

    public void AddToList(int itemIndex, StructArenaAllocator<T> arena)
    {
        ref T item = ref arena[itemIndex];
        item.NextItem = -1;

        if (StartIndex == -1)
        {
            StartIndex = itemIndex;
        }

        if (EndIndex != -1)
        {
            ref T oldEnd = ref arena[EndIndex];
            oldEnd.NextItem = itemIndex;
        }

        EndIndex = itemIndex;
    }
}