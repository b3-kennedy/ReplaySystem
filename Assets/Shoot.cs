using UnityEditor.PackageManager;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit ,100))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if(rb)
                {
                    Vector3 dir = (hit.transform.position - Camera.main.transform.position).normalized;
                    rb.AddForce(dir * 10, ForceMode.Impulse);
                }
            }
        }
    }
}
