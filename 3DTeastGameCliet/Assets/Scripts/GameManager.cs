using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

   private void Awake(){
        if(instance == null){
            instance = this;

        }
        if (instance != this){
            Debug.Log("Instance Already exists, destroying object!");
            Destroy(this);
            
        }
    }

    public void spawnPlayer(int _id,string _username, Vector3 _position,Quaternion _rotation){
        GameObject _player;
        if(_id == Client.instance.myId){
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else{
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
