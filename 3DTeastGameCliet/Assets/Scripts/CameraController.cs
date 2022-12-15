using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;
    public float sensitivity = 100.0f;
    public float clampAngle = 85.0f;

    private float verticalRotation;
    private float horizontalRotation;

    private void Start(){
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;

    }

    private void Update(){
        Look();
        Debug.DrawRay(transform.position, transform.forward * 2, Color.yellow);
    }

    private void Look(){
        float _mouseVertical = -Input.GetAxis("Mouse Y");
        float _mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += _mouseVertical * sensitivity * Time.deltaTime;
        horizontalRotation += _mouseHorizontal * sensitivity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle,clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation,0.0f,0.0f);
        player.transform.rotation = Quaternion.Euler(0.0f,horizontalRotation,0.0f);
    }
}
