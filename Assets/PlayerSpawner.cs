using System;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{

    public GameObject player;
    public GameObject cameraHolder;

    public GameObject menuCamera;

    public GameObject menuCanvas;

    public static PlayerSpawner Instance;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public GameObject SpawnObject(GameObject o, string id)
    {
        GameObject spawnedObject = Instantiate(o);
        if(spawnedObject.GetComponent<Rigidbody>())
        {
            spawnedObject.GetComponent<Rigidbody>().isKinematic = true;
            spawnedObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
            ObjectId oId = spawnedObject.GetComponent<ObjectId>();
            if(oId)
            {
                oId.SetId(id);
                spawnedObject.GetComponent<ReplayPhysicsObject>().GetId();
            }
            
        }
        return spawnedObject;
        
    }

    void DisableComponents(GameObject obj)
    {
        foreach (var comp in obj.GetComponents<Component>())
        {

            if (comp is Behaviour behaviour && comp is not Follow)
            {
                behaviour.enabled = false;
            }
            // Add other component types as needed
        }
    }

    public void Spawn()
    {
        ReplayManager.Instance.StartRecording();
        menuCamera.SetActive(false);
        menuCanvas.SetActive(false);
        GameObject playerPrefab = Resources.Load<GameObject>("Player");
        GameObject spawnedPlayer = Instantiate(playerPrefab);
       
        Transform followPos = spawnedPlayer.transform.GetChild(3);
        GameObject spawnedCameraHolder = Instantiate(cameraHolder);
        ReplayManager.Instance.isWatchingReplay = false;


        PlayerLook look = spawnedPlayer.GetComponent<PlayerLook>();
        Follow follow = spawnedCameraHolder.GetComponent<Follow>();

        spawnedPlayer.GetComponent<ObjectId>().SetId(Guid.NewGuid().ToString());
        spawnedCameraHolder.GetComponent<ObjectId>().SetId(Guid.NewGuid().ToString());

         spawnedPlayer.GetComponent<ReplayPhysicsObject>().GetId();

        ReplayManager.Instance.player = spawnedPlayer;
        
        follow.followPos = followPos;
        look.cam = spawnedCameraHolder.transform;
        SpawnAction action = new(ReplayManager.Instance.GetReplayTime(), spawnedPlayer.name ,spawnedPlayer.GetComponent<ObjectId>().GetId());
        SpawnAction camSpawn = new(ReplayManager.Instance.GetReplayTime(), spawnedCameraHolder.name, spawnedCameraHolder.GetComponent<ObjectId>().GetId());
        ReplayManager.Instance.actions.Add(action);
        ReplayManager.Instance.actions.Add(camSpawn);


    }
}
