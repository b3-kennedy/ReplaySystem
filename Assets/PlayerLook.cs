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

    // Start is called before the first frame update
    void Start()
    {
        replayManagerInstance = ReplayManager.Instance;
        objectId = GetComponent<ObjectId>().GetId();
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
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
        RotationAction action = new RotationAction(replayManagerInstance.GetReplayTime(), player.transform.rotation, objectId);
        replayManagerInstance.actions.Add(action);
    }
}
