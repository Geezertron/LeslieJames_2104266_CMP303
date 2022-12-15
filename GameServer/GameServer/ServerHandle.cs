using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {
        public static void welcomeRecieved(int _fromClient,Packet _packet)
        {
            //read in same order as write int>string
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if(_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[_fromClient].sendIntoGame(_username);
        }

        //create new array, read sent array, populate new array with sent data
        public static void playerMovement(int _fromClient,Packet _packet)
        {
            bool[] _inputs = new bool[_packet.ReadInt()];

            for (int i=0;i< _inputs.Length; i++)
            {
                _inputs[i] = _packet.ReadBool();
            }
            Quaternion _rotation = _packet.readQuaternion();

            Server.clients[_fromClient].player.setInput(_inputs, _rotation);
        }
    }
}
