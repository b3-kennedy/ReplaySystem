using UnityEngine;

public class Follow : MonoBehaviour
{

    public Transform followPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!followPos) return;

        transform.position = followPos.position;
        
    }
}
