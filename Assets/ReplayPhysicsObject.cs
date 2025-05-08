using System;
using System.Security.Cryptography;
using UnityEditor.SearchService;
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


        

        lastPosition = rb.position;
        lastRotation = rb.rotation;
    }

    public void GetId()
    {
        id = GetComponent<ObjectId>().GetId();
    }

    public void OnStartReplay()
    {

        
        if (!ReplayManager.Instance.objects.TryGetValue(id, out var obj))
        {
            Debug.Log($"Added {gameObject.name} with id {id} to dictionary");
            ReplayManager.Instance.objects.Add(id, gameObject);
        }
        if(ReplayManager.Instance.isWatchingReplay)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }
    }
    void FixedUpdate()
    {
        if(ReplayManager.Instance.isWatchingReplay || string.IsNullOrEmpty(id)) return;

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
