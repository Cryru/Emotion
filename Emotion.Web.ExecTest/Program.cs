#region Using

using System.Threading.Tasks;
using Emotion.Common;

#endregion

namespace Emotion.Web.ExecTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await LibraryBootstrap.MainLibrary(args, new EmotionSetupService
            {
                SetupEngine = config =>
                {
                    Engine.Setup(config);
                    Engine.SceneManager.SetScene(new Emotion.ExecTest.Program());
                    Engine.Run();
                }
            });
        }
    }
}