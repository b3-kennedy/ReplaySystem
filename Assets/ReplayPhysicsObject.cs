using System;
using System.Security.Cryptography;
using UnityEngine;

public class ReplayPhysicsObject : MonoBehaviour
{

    Rigidbody rb;
    string id;

    Vector3 lastPosition;
    Quaternion lastRotation;

    float lastPositionTime = 0;

    
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        id = GetComponent<ObjectId>().GetId();

        if (!ReplayManager.Instance.objects.TryGetValue(id, out var obj))
        {
            ReplayManager.Instance.objects.Add(id, gameObject);;
        }
        

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
            float currentTime = ReplayManager.Instance.GetReplayTime();
            float duration = currentTime - lastPositionTime;
            MovementAction action = new(currentTime, rb.position, lastPosition, duration,id);
            ReplayManager.Instance.actions.Add(action);
            lastPosition = currentPosition;
            lastPositionTime = currentTime;
        }

        if(currentRotation != lastRotation)
        {
            RotationAction rotAction = new(ReplayManager.Instance.GetReplayTime(), rb.rotation, id);
            ReplayManager.Instance.actions.Add(rotAction);
            lastRotation = currentRotation;
        }

        

    }
}
