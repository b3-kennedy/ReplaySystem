
using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public Transform basketballPos;
    ReplayManager replayManager;

    public float chargeForceMultiplier = 1;

    float chargeForce = 0;

    GameObject basketball;

    public GameObject basketballPrefab;

    public GameObject playerCanvas;

    public GameObject chargeBar;
    RectTransform chargeRect;

    public float maxCharge = 100;
    public TextMeshProUGUI chargeText;

    float previousPercent;

    float lastPercentTime = 0;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        replayManager = ReplayManager.Instance;
        chargeRect = chargeBar.GetComponent<RectTransform>();
        chargeRect.localScale = new Vector3(chargeRect.localScale.x, 0, chargeRect.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        ReplayChargeBar();

        if(replayManager.isWatchingReplay) return;

        Hooping();

        if(Input.GetKeyDown(KeyCode.B))
        {
            GameObject bBall = Instantiate(basketballPrefab, new Vector3(0, 5, 0), Quaternion.identity);
            
            bBall.GetComponent<ObjectId>().SetId(Guid.NewGuid().ToString());
            SpawnAction action = new(ReplayManager.Instance.GetReplayTime(), bBall.name, bBall.GetComponent<ObjectId>().GetId());
            bBall.GetComponent<ReplayPhysicsObject>().GetId();
            ReplayManager.Instance.actions.Add(action);
        }

    }

    void ReplayChargeBar()
    {
        float pct = chargeForce/maxCharge;
        if(pct > 1)
        {
            chargeText.text = "x" + pct.ToString("F1");
            SetChargeBarPercent(1);
        }
        else
        {
            chargeText.text = "";
        }
        
        if(pct != previousPercent)
        {
            float currentTime = ReplayManager.Instance.GetReplayTime();
            float duration = currentTime - lastPercentTime;
            ChargeBarAction action = new(currentTime, previousPercent, pct,duration,chargeForce);
            ReplayManager.Instance.actions.Add(action);
            lastPercentTime = currentTime;
            previousPercent = pct;
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
                    Debug.Log(basketball.GetComponent<ObjectId>().id);
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
                float pct = chargeForce/maxCharge;
                if(pct > 1)
                {
                    SetChargeBarPercent(1);
                    
                }
                else
                {
                    SetChargeBarPercent(pct);
                }
                
                
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
                chargeText.text = "";
                SetChargeBarPercent(0);
                
            }
        }
    }

    public void SetChargeBarPercent(float value)
    {


        if(chargeRect)
        {
            chargeRect.localScale = new Vector3(chargeRect.localScale.x, value, chargeRect.localScale.z);
        }
        

        
        
    }

    public void SetChargeForce(float value)
    {
        chargeForce = value;
    }
}
