using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;

    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake(){
        if(instance == null){
            instance = this;

        }
        if (instance != this){
            Debug.Log("Instance Already exists, destroying object!");
            Destroy(this);
            
        }
    }

    private void Start(){
        tcp = new TCP();
        udp = new UDP();
    }

    public void ConnectToServer(){
        initialiseClientData();

        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet recievedData;
        private byte[] recieveBuffer;

        public void Connect(){
            socket = new TcpClient{
                ReceiveBufferSize= dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            recieveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, connectCallback, socket);
        }

        private void connectCallback(IAsyncResult _result){
            socket.EndConnect(_result);

            if (!socket.Connected){
                return;
            }
            stream = socket.GetStream();

            recievedData = new Packet();

            stream.BeginRead(recieveBuffer, 0 , dataBufferSize, recieveCallback, null);
        }

        public void sendData(Packet _packet){
            try
            {
                if (socket != null){
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex){
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        } 

        private void recieveCallback(IAsyncResult _result){
            try{
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0){
                    //todo disconnect
                    return;
                }
                byte[] _data = new byte[_byteLength];
                Array.Copy(recieveBuffer, _data, _byteLength);

                recievedData.Reset(handleData(_data));
                stream.BeginRead(recieveBuffer, 0, dataBufferSize, recieveCallback,null);
            }
            catch{
                //todo disconnect
            }
        }

        private bool handleData(byte[] _data){
            int _packetLength = 0;

            recievedData.SetBytes(_data);

            if(recievedData.UnreadLength() >= 4){
                _packetLength = recievedData.ReadInt();

                if(_packetLength <= 0 ){
                    return true;
                }
            }
            while (_packetLength > 0 && _packetLength <= recievedData.UnreadLength()){
                byte[] _packetBytes = recievedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>{
                        using(Packet _packet = new Packet(_packetBytes)){
                            int _packetId = _packet.ReadInt();
                            packetHandlers[_packetId](_packet);

                        }
                });

                _packetLength = 0;
                if(recievedData.UnreadLength() >= 4){
                _packetLength = recievedData.ReadInt();

                if(_packetLength <= 0 ){
                    return true;
                }
            }
            }

            if (_packetLength <= 1){
                return true;
            }
            return false;
        }
    }

    public class UDP 
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP(){
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip),instance.port);
        }

        public void Connect(int _localpPort){
            socket = new UdpClient(_localpPort);
            socket.Connect(endPoint);
            socket.BeginReceive(recieveCallback, null);

            using(Packet _packet = new Packet()){
                sendData(_packet);
            }
        }

        public void sendData(Packet _packet){
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null){
                    socket.BeginSend(_packet.ToArray(), _packet.Length(),null,null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }


        private void recieveCallback(IAsyncResult _result){
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(recieveCallback,null);

                if (_data.Length < 4){
                    //todo disconnect
                    return;
                }

                handleData(_data);
            }
            catch
            {
                //todo disconnect
            }
        }

        private void handleData(byte[] _data){
            using(Packet _packet = new Packet(_data)){
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>{
                    using(Packet _packet = new Packet(_data)){
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
            });
        }
        
    }


    private void initialiseClientData(){
        packetHandlers = new Dictionary<int, PacketHandler>(){
            { (int)ServerPackets.welcome, ClientHandle.Welcome},
            {(int)ServerPackets.spawnPlayer, ClientHandle.spawnPlayer},
            {(int)ServerPackets.playerPosition, ClientHandle.playerPosition},
            {(int)ServerPackets.playerRotation, ClientHandle.playerRotation}
        
        };
        Debug.Log("Iniutialised packets");
    }
}
