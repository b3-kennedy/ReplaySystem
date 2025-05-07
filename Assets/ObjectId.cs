using System;
using UnityEngine;
using UnityEngine.Rendering;

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

    public void SetId(string value)
    {
        id = value;
    }

    public bool HasId()
    {
        return string.IsNullOrEmpty(id);
    }
}
