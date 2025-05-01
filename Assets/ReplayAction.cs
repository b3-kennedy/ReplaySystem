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
    public string objectName;
    public JsonVector targetPosition;

    public MovementAction(float timeStamp, Vector3 targetPos, string oName) : base(timeStamp)
    {
        objectName = oName;
        targetPosition = new(targetPos);
        type = GetActionType();
    }



    public override void Process()
    {
        Debug.Log(objectName);
        if(ReplayManager.Instance.objects.TryGetValue(objectName, out var o))
        {
            o.GetComponent<Rigidbody>().MovePosition(targetPosition.ToVector3());
        }
    }
}

public class RotationAction: ReplayAction
{
    public string objectName;
    public JsonQuaternion targetRotation;

    public RotationAction(float timeStamp, Quaternion targetRot, string oName) : base(timeStamp)
    {
        objectName = oName;
        targetRotation = new(targetRot);
        type = GetActionType();
    }



    public override void Process()
    {
        Debug.Log(objectName);
        if(ReplayManager.Instance.objects.TryGetValue(objectName, out var o))
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
        Debug.Log("Process Click");
    }
}

public class SpawnAction: ReplayAction
{
    public string objectId;
    public string prefabName;
    public string objectName;
    public SpawnAction(float timeStamp, string oName, string id):base(timeStamp)
    {
        objectId = id;
        objectName = oName;
        type = GetActionType();
    }

    public override void Process()
    {
        string[] split = objectName.Split("(");
        prefabName = split[0];
        GameObject o = Resources.Load<GameObject>(prefabName);
        GameObject spawnedObject = PlayerSpawner.Instance.SpawnObject(o, objectId);
        ReplayManager.Instance.objects.Add(objectId, spawnedObject);
        

    }
}
