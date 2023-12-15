namespace ImGuiNET
{
    public unsafe partial struct ImDrawDataPtr
    {
        public RangePtrAccessor<ImDrawListPtr> CmdListsRange
        {
            get => new RangePtrAccessor<ImDrawListPtr>(CmdLists.ToPointer(), CmdListsCount);
        }
    }
}