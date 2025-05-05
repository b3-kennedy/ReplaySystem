using System;
using UnityEngine;


public class JsonVector
{
    public float x;
    public float y;
    public float z;

    public JsonVector(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x,y,z);
    }
}

public class JsonQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public JsonQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x,y,z,w);
    }
}

[System.Serializable]
public abstract class ReplayAction
{
    public string objectId;
    public string type;
    public float timeStamp;

    public ReplayAction(float timeS)
    {
        timeStamp = timeS;
    }

    public string GetActionType()
    {
        return GetType().ToString();
    }

    public abstract void Process();

}

[System.Serializable]
public class MovementAction : ReplayAction
{
    public JsonVector targetPosition;

    public MovementAction(float timeStamp, Vector3 targetPos, string oId) : base(timeStamp)
    {
        objectId = oId;
        targetPosition = new(targetPos);
        type = GetActionType();
    }



    public override void Process()
    {
        if(ReplayManager.Instance.objects.TryGetValue(objectId, out var o))
        {
            o.transform.position = targetPosition.ToVector3();
        }
    }
}

public class RotationAction: ReplayAction
{
    public JsonQuaternion targetRotation;

    public RotationAction(float timeStamp, Quaternion targetRot, string oId) : base(timeStamp)
    {
        objectId = oId;
        targetRotation = new(targetRot);
        type = GetActionType();
    }



    public override void Process()
    {
        if(ReplayManager.Instance.objects.TryGetValue(objectId, out var o))
        {
            o.transform.rotation = targetRotation.ToQuaternion();
        }
    }
}

public class ClickAction: ReplayAction
{
    public ClickAction(float timeStamp):base(timeStamp)
    {
        type = GetActionType();
    }

    public override void Process()
    {
        //Debug.Log("Process Click");
    }
}

public class SpawnAction: ReplayAction
{
    public string prefabName;
    public string objectName;

    GameObject spawnedObject;


    public SpawnAction(float timeStamp, string oName, string id):base(timeStamp)
    {
        objectId = id;
        objectName = oName;
        type = GetActionType();
    }

    public override void Process()
    {
        
        if(!ReplayManager.Instance.objects.TryGetValue(objectId, out var obj))
        {
            string[] split = objectName.Split("(");
            prefabName = split[0];
            GameObject o = Resources.Load<GameObject>(prefabName);
            spawnedObject = PlayerSpawner.Instance.SpawnObject(o, objectId);
            ReplayManager.Instance.objects.Add(objectId, spawnedObject);
        }
        



        if(spawnedObject.GetComponent<PlayerMovement>())
        {
            ReplayManager.Instance.player = spawnedObject;
        }

        if(spawnedObject.GetComponent<Follow>())
        {
            
            ReplayManager.Instance.cam = spawnedObject;
        }

        if(ReplayManager.Instance.cam && ReplayManager.Instance.player)
        {
            ReplayManager.Instance.cam.GetComponent<Follow>().followPos = ReplayManager.Instance.player.transform.GetChild(3);
        }
        
        

    }
}
