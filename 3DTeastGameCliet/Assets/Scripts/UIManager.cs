using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;

    private void Awake(){
        if(instance == null){
            instance = this;

        }
        if (instance != this){
            Debug.Log("Instance Already exists, destroying object!");
            Destroy(this);
            
        }
    }

    public void connectToServer(){
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
        
    }
}
