using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.IO;

public interface IHotReloadableAsset
{
    public void Reload(ReadOnlyMemory<byte> data);
}
