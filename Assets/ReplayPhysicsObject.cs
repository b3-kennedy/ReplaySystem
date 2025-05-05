using System;
using System.Security.Cryptography;
using UnityEngine;

public class ReplayPhysicsObject : MonoBehaviour
{

    Rigidbody rb;
    string id;

    
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        id = GetComponent<ObjectId>().GetId();

        ReplayManager.Instance.objects.Add(id, gameObject);
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
        MovementAction action = new(ReplayManager.Instance.GetReplayTime(), rb.position,id);
        RotationAction rotAction = new(ReplayManager.Instance.GetReplayTime(), rb.rotation, id);
        ReplayManager.Instance.actions.Add(action);
        ReplayManager.Instance.actions.Add(rotAction);
    }
}
