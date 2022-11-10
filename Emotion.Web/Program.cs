#region Using

using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Web.Platform;

#endregion

namespace Emotion.Web.ExecTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await LibraryBootstrap.MainLibrary(args, new EmotionSetupService
            {
                SetupEngine = async config =>
                {
                    Engine.Setup(config);
                    await ((WebHost) Engine.Host).AsyncSetup(); // Dont forget to call this!
                    await Engine.SceneManager.SetScene(new Emotion.ExecTest.Program());
                    Engine.Run();
                }
            });
        }
    }
}