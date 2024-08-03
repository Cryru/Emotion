using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Network;

public class Client : NetworkCommunicator
{
    public IPEndPoint _serverEndPoint;

    public Client()
    {
    }

    public void SendMessageToServer(string msg)
    {
        SendMessage(msg, _serverEndPoint);
    }
}
