using System;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float sensitivity;
    public GameObject player;
    Transform playerObj;
    public Transform orientation;
    public Transform cam;

    float xRot;
    float yRot;

    ReplayManager replayManagerInstance;

    string objectId;
    string camId;

    Quaternion lastRotation;

    float lastRotationTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        replayManagerInstance = ReplayManager.Instance;
        if(!replayManagerInstance.isWatchingReplay)
        {
            
            objectId = GetComponent<ObjectId>().GetId();
            if(cam)
            {
                camId = cam.GetComponent<ObjectId>().GetId();
            }
            
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            player = ReplayManager.Instance.player.transform.GetChild(0).gameObject;
        }


    }

    // Update is called once per frame
    void Update()
    {
        if(!replayManagerInstance.isWatchingReplay)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

            yRot += mouseX;
            xRot -= mouseY;

            xRot = Mathf.Clamp(xRot, - 90f, 90f);

            if(cam != null)
            {
                cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
            }
            
            orientation.rotation = Quaternion.Euler(0, yRot,0);
            player.transform.rotation = orientation.rotation;

            Quaternion currentRotation = cam.rotation;
            if(currentRotation != lastRotation)
            {
                float currentTime = ReplayManager.Instance.GetReplayTime();
                float duration = currentTime - lastRotationTime;
                RotationAction camAction = new RotationAction(replayManagerInstance.GetReplayTime(), cam.rotation, lastRotation, duration, camId);
                replayManagerInstance.actions.Add(camAction);
                lastRotation = currentRotation;
                lastRotationTime = currentTime;
            }

        }
        else
        {
            if(replayManagerInstance.player && replayManagerInstance.cam)
            {
                replayManagerInstance.player.transform.rotation = Quaternion.Euler(0, replayManagerInstance.cam.transform.eulerAngles.y, 0);
            }
        }

 

    }
}
