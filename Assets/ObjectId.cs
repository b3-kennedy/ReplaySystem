using System;
using UnityEngine;

public class ObjectId : MonoBehaviour
{

    public string id;
    // void Awake()
    // {
    //     id = Guid.NewGuid().ToString();
    // }

    public string GetId()
    {
        return id;
    }
}
