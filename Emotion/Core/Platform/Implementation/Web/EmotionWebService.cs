#nullable enable

using System.Threading.Tasks;

namespace Emotion.Core.Platform.Implementation.Web;

[DontSerialize]
public class EmotionWebService
{
    public Func<Configurator, Task>? SetupEngine;
}