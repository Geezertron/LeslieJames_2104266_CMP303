using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;
        //create references
        public int id;
        public TCP tcp;
        public UDP udp;
        public Player player;

        public Client(int _clientId)
        {
            //initialise references
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {   //create references
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet recievedData;
            private byte[] recieveBuffer;

            //initialise TCP
            public TCP(int _id)
            {
                id = _id;
            }


            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();
                recievedData = new Packet();
                recieveBuffer = new byte[dataBufferSize];

                stream.BeginRead(recieveBuffer, 0, dataBufferSize, recieveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if(socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }


            private void recieveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        //todo disconnect
                        return;
                    }
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(recieveBuffer, _data, _byteLength);

                    recievedData.Reset(handleData(_data));
                    //todo handle data
                    stream.BeginRead(recieveBuffer, 0, dataBufferSize, recieveCallback, null);
                }
                catch(Exception _ex)
                {
                    Console.WriteLine($"Error recieving TCP data: {_ex}");
                    //todo disconnect
                }
            }
            private bool handleData(byte[] _data)
            {
                int _packetLength = 0;

                recievedData.SetBytes(_data);

                if (recievedData.UnreadLength() >= 4)
                {
                    _packetLength = recievedData.ReadInt();

                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
                while (_packetLength > 0 && _packetLength <= recievedData.UnreadLength())
                {
                    byte[] _packetBytes = recievedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);

                        }
                    });

                    _packetLength = 0;
                    if (recievedData.UnreadLength() >= 4)
                    {
                        _packetLength = recievedData.ReadInt();

                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }
                return false;
            }
        }
    
        public class UDP
        {
            public IPEndPoint endPoint;
            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
                
            }

            public void sendData(Packet _packet)
            {
                Server.sendUDPData(endPoint, _packet);
            }

            public void handleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                }


                );

            }

        }

        public void sendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, new Vector3(0, 0, 0));

            foreach(Client _Client in Server.clients.Values)
            {
                if(_Client.player != null)
                {
                    if(_Client.id != id)
                    {
                        ServerSend.spawnPlayer(id, _Client.player);
                    }
                }
            }

            foreach(Client _client in Server.clients.Values)
            {
                if(_client.player != null)
                {
                    ServerSend.spawnPlayer(_client.id, player);
                }
            }
        }

    }

    
}
