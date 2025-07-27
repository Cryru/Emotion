using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorUndoableOperation
{
    public string Name;

    public virtual void Undo()
    {

    }

    public virtual void Redo()
    {

    }
}
