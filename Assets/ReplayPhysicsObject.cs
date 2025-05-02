using System;
using UnityEngine;

public class ReplayPhysicsObject : MonoBehaviour
{

    Rigidbody rb;
    string id;

    
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        id = GetComponent<ObjectId>().GetId();
        if(ReplayManager.Instance.isWatchingReplay)
        {
            rb.isKinematic = true;
        }
        ReplayManager.Instance.objects.Add(id, gameObject);
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
