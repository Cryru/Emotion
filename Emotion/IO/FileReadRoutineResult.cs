#nullable enable

using Emotion.Game.Time.Routines;
using System.Threading.Tasks;

namespace Emotion.IO;

public sealed class FileReadRoutineResult : IRoutineWaiter
{
    public static FileReadRoutineResult GenericErrored = new FileReadRoutineResult() { Errored = true };

    public bool Errored { get; private set; }

    public bool Finished { get; private set; }

    public ReadOnlyMemory<byte> FileBytes = ReadOnlyMemory<byte>.Empty;

    private Task<byte[]>? _task;

    public void SetData(ReadOnlyMemory<byte> data)
    {
        FileBytes = data;
        Finished = true;
    }

    public void SetAsyncTask(Task<byte[]> task)
    {
        _task = task;
    }

    public void Update()
    {
        if (_task != null && _task.IsCompleted)
        {
            if (_task.IsCompletedSuccessfully)
            {
                SetData(_task.Result);
            }
            else
            {
                Errored = true;
                Finished = true;
            }

            _task = null;
        }
    }
}
