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

        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public GameObject SpawnObject(GameObject o, string id)
    {
        GameObject player = Instantiate(o);
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerLook>().enabled = false;
        player.GetComponent<Rigidbody>().useGravity = false;
        return player;
        
    }

    public void Spawn()
    {
        menuCamera.SetActive(false);
        menuCanvas.SetActive(false);
        GameObject playerPrefab = Resources.Load<GameObject>("Player");
        GameObject spawnedPlayer = Instantiate(playerPrefab);
        Transform followPos = spawnedPlayer.transform.GetChild(3);
        GameObject spawnedCameraHolder = Instantiate(cameraHolder);
        ReplayManager.Instance.isWatchingReplay = false;


        PlayerLook look = spawnedPlayer.GetComponent<PlayerLook>();
        Follow follow = spawnedCameraHolder.GetComponent<Follow>();
        
        follow.followPos = followPos;
        look.cam = spawnedCameraHolder.transform;
        SpawnAction action = new(ReplayManager.Instance.GetReplayTime(), spawnedPlayer.name ,spawnedPlayer.GetComponent<ObjectId>().GetId());
        ReplayManager.Instance.actions.Add(action);


    }
}
