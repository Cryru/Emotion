namespace Rationale.Interop
{
    public enum MessageType
    {
        // System
        GetListener,
        MessageLogged,

        // Monitoring
        CurrentFPS,
        CurrentTPS,

        // Asset debugger
        RequestAssetData,
        LoadedAssetData,
        AssetData,

        // Script debugging
        ScriptHook,
        ScriptInto,
        ScriptOut,
        ScriptUnhook
    }
}