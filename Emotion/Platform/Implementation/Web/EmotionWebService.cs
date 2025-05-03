#region Using

using Emotion.Common.Serialization;
using System.Threading.Tasks;

#endregion

#nullable enable

namespace Emotion.Platform.Implementation.Web;

[DontSerialize]
public class EmotionWebService
{
    public Func<Configurator, Task>? SetupEngine;
}