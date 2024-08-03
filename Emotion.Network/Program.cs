using Emotion.Common;
using Emotion.Scenography;
using System.Collections;

namespace Emotion.Network;

public class Program
{
    public static void Main(string[] args)
    {
        Engine.Start(new Configurator()
        {
            DebugMode = true
        },
        StartRoutine);
    }

    public static IEnumerator StartRoutine()
    {
        yield return Engine.SceneManager.SetScene(new ServerStatusScene());
    }
}


public class ServerStatusScene : Scene
{
    public Server Server;
    public List<Client> Clients = new List<Client>();

    protected override IEnumerator LoadSceneRoutineAsync()
    {
        Server = NetworkCommunicator.CreateServer(1337);

        Clients = new List<Client>();
        for (int i = 0; i < 10; i++)
        {
            Clients.Add(NetworkCommunicator.CreateClient("127.0.0.1", 1337));
            Clients[i].SendMessageToServer("hi");
        }

        Engine.CoroutineManagerAsync.StartCoroutine(NetworkingUpdateAsyncRoutine());

        yield break;
    }

    public IEnumerator NetworkingUpdateAsyncRoutine()
    {
        while (true)
        {
            Server.Update();
            for (int i = 0; i < Clients.Count; i++)
            {
                var client = Clients[i];
                client.Update();
            }
            yield return null;
        }
    }
}