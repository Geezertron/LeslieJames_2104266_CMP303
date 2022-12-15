using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        private static void sendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void sendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.sendData(_packet);
        }

        private static void sendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <=Server.maxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        

        private static void sendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        private static void sendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].udp.sendData(_packet);
            }
        }



        private static void sendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.sendData(_packet);
                }
            }
        }


        #region Packets
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                sendTCPData(_toClient, _packet);
            }
        }

        public static void spawnPlayer(int _toClient,Player _player)
        {
            using(Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                //use tcp for spawinging as happens once and is important
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                sendTCPData(_toClient, _packet);
            }
        }

        public static void playerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);

                sendUDPDataToAll(_packet);
            }
        }

        public static void playerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                sendUDPDataToAll(_player.id, _packet);
            }
        }
        #endregion
    }
}
