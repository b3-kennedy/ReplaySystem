using System;
using System.Security.Cryptography;
using UnityEngine;

public class ReplayPhysicsObject : MonoBehaviour
{

    Rigidbody rb;
    string id;

    Vector3 lastPosition;
    Quaternion lastRotation;

    
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        id = GetComponent<ObjectId>().GetId();

        ReplayManager.Instance.objects.Add(id, gameObject);

        lastPosition = rb.position;
        lastRotation = rb.rotation;
    }

    public void OnStartReplay()
    {
        if(ReplayManager.Instance.isWatchingReplay)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }
    }
    void FixedUpdate()
    {
        if(ReplayManager.Instance.isWatchingReplay) return;

        Vector3 currentPosition = rb.position;
        Quaternion currentRotation = rb.rotation;

        if(currentPosition != lastPosition)
        {
            MovementAction action = new(ReplayManager.Instance.GetReplayTime(), rb.position,id);
            ReplayManager.Instance.actions.Add(action);
            lastPosition = currentPosition;
        }

        if(currentRotation != lastRotation)
        {
            RotationAction rotAction = new(ReplayManager.Instance.GetReplayTime(), rb.rotation, id);
            ReplayManager.Instance.actions.Add(rotAction);
            lastRotation = currentRotation;
        }

        

    }
}
