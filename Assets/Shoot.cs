
using System;
using System.Security.Cryptography;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public Transform basketballPos;
    ReplayManager replayManager;

    public float chargeForceMultiplier = 1;

    float chargeForce = 0;

    GameObject basketball;

    public GameObject basketballPrefab;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        replayManager = ReplayManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(replayManager.isWatchingReplay) return;

        Hooping();

        if(Input.GetKeyDown(KeyCode.B))
        {
            GameObject bBall = Instantiate(basketballPrefab, new Vector3(0, 5, 0), Quaternion.identity);

            bBall.GetComponent<ObjectId>().SetId(Guid.NewGuid().ToString());
            SpawnAction action = new(ReplayManager.Instance.GetReplayTime(), bBall.name, bBall.GetComponent<ObjectId>().GetId());
            ReplayManager.Instance.actions.Add(action);
        }

    }

    void Hooping()
    {
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
