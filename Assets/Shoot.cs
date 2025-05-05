
using System.Security.Cryptography;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public Transform basketballPos;
    ReplayManager replayManager;

    public float chargeForceMultiplier = 1;

    float chargeForce = 0;

    GameObject basketball;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        replayManager = ReplayManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(replayManager.isWatchingReplay) return;

        if(Input.GetKeyDown(KeyCode.E))
        {
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit ,100))
            {
                if(hit.collider.CompareTag("basketball"))
                {
                    basketball = hit.collider.gameObject;
                    hit.collider.enabled = false;
                    Rigidbody rb = basketball.GetComponent<Rigidbody>();
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.linearVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    basketball.transform.SetParent(basketballPos);
                    basketball.transform.localPosition = Vector3.zero;
                    
                    
                }

            }
        }

        if(basketballPos.childCount > 0)
        {
            if(Input.GetButton("Fire1"))
            {
                chargeForce += Time.deltaTime * chargeForceMultiplier;
            }
            else if(Input.GetButtonUp("Fire1"))
            {
                basketball.transform.SetParent(null);
                basketball.GetComponent<Collider>().enabled = true;
                Rigidbody rb = basketball.GetComponent<Rigidbody>();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.isKinematic = false;
                rb.AddForce(Camera.main.transform.forward * chargeForce, ForceMode.Impulse);
                chargeForce = 0;
            }
        }
    }
}
