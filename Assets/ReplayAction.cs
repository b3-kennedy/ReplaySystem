using System;
using UnityEngine;


[System.Serializable]
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

[System.Serializable]
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

    public virtual bool IsInterpolated()
    {
        return false;
    }

}

[System.Serializable]
public class MovementAction : ReplayAction
{
    public JsonVector prevPosition;
    public JsonVector targetPosition;

    public bool isInterpolated = true;

    public float duration;

    public float startTime;



    public MovementAction(float timeStamp, Vector3 targetPos, Vector3 prevPos, float dur,string oId) : base(timeStamp)
    {
        objectId = oId;
        targetPosition = new(targetPos);
        prevPosition = new(prevPos);
        duration = dur;
        type = GetActionType();
        startTime = timeStamp - dur;
    }

    public override bool IsInterpolated()
    {
        return true;
    }

    public override void Process()
    {
        if (ReplayManager.Instance.objects.TryGetValue(objectId, out var o))
        {
            float currentReplayTime = ReplayManager.Instance.GetReplayTimer();

            if (currentReplayTime < startTime)
            {
                return;
            }

            // Only interpolate if within valid time window
            if (currentReplayTime >= startTime && currentReplayTime <= timeStamp)
            {
                float t = Mathf.InverseLerp(startTime, timeStamp, currentReplayTime);
                Vector3 pos = Vector3.Lerp(prevPosition.ToVector3(), targetPosition.ToVector3(), t);
                o.transform.position = pos;
            }
            else if (currentReplayTime > timeStamp)
            {
                // Snap to final position after interpolation ends
                o.transform.position = targetPosition.ToVector3();
            }
        }

    }
}

[System.Serializable]
public class RotationAction: ReplayAction
{
    public JsonQuaternion targetRotation;

    public RotationAction(float timeStamp, Quaternion targetRot, string oId) : base(timeStamp)
    {
        objectId = oId;
        targetRotation = new(targetRot);
        type = GetActionType();
    }

    // public override bool IsInterpolated()
    // {
    //     return true;
    // }

    public override void Process()
    {
        if(ReplayManager.Instance.objects.TryGetValue(objectId, out var o))
        {
            o.transform.rotation = targetRotation.ToQuaternion();
        }
    }
}

[System.Serializable]
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

    public override bool IsInterpolated()
    {
        return false;
    }
}

[System.Serializable]
public class SpawnAction: ReplayAction
{
    public string prefabName;
    public string objectName;


    [NonSerialized] GameObject spawnedObject;


    public SpawnAction(float timeStamp, string oName, string id):base(timeStamp)
    {
        objectId = id;
        objectName = oName;
        type = GetActionType();
    }

    public override bool IsInterpolated()
    {
        return false;
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
            Debug.Log("SetPlayer");
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

[System.Serializable]
public class ScoreAction : ReplayAction
{

    public ScoreAction(float timeS) : base(timeS)
    {
        type = GetActionType();

    }

    public override bool IsInterpolated()
    {
        return false;
    }

    public override void Process()
    {
        
    }
}

[System.Serializable]
public class ChargeBarAction : ReplayAction
{
    public float previousPct;
    public float nextPct;

    public float chargeForce;

    public float duration;

    float startTime;
    public ChargeBarAction(float timeS, float previousPercent, float nextPercent, float dur, float cForce) : base(timeS)
    {
        type = GetActionType();
        previousPct = previousPercent;
        nextPct = nextPercent;
        duration = dur;
        startTime = timeStamp - dur;
        chargeForce = cForce;
    }

    public override bool IsInterpolated()
    {
        return false;
    }

    public override void Process()
    {
            float currentReplayTime = ReplayManager.Instance.GetReplayTimer();
            Shoot shootScript = ReplayManager.Instance.player.GetComponent<Shoot>();

            shootScript.SetChargeBarPercent(nextPct);
            shootScript.SetChargeForce(chargeForce);
    }
}
