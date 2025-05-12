using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;



[System.Serializable]
public class ReplayDataWrapper
{
   public List<ReplayAction> replayActions;
    

    public ReplayDataWrapper(List<ReplayAction> actions)
    {
        replayActions = actions;
    }
}


public class ReplayManager : MonoBehaviour
{

    public GameObject player = null;
    [HideInInspector] public GameObject cam = null;
    public static ReplayManager Instance;
    public List<ReplayAction> actions = new List<ReplayAction>();

    List<ReplayAction> interpolatedActions = new List<ReplayAction>();

    List<ReplayAction> nonInterpolatedActions = new List<ReplayAction>();
    public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();


    bool isRecording;

    public bool isWatchingReplay;
    float recordStartTime;

    float replayTime;

    float replayLength;

    public GameObject freeCameraPrefab;
    GameObject freeCam;


    [Header("UI")]
    public GameObject replayPanel;
    public GameObject replayObjectPrefab;

    public GameObject scoredMarker;

    public GameObject replayCanvas;
    GameObject replayTimelineBar;
    GameObject replayTimelineBackground;

    public Button pauseButton;
    GameObject playIcon;
    GameObject pauseIcon;

    public Button increasePlaybackButton;
    public Button decreasePlaybackButton;

    public TextMeshProUGUI playbackSpeedText;

    ReplayPhysicsObject[] rpObjects;



    bool isPaused;

    bool isFreecam;

    int index = 0;

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


    void Start()
    {
        replayTimelineBar = replayCanvas.transform.GetChild(0).GetChild(0).gameObject;
        replayTimelineBackground = replayCanvas.transform.GetChild(0).gameObject;
        replayCanvas.SetActive(false);

        pauseIcon = pauseButton.transform.GetChild(0).gameObject;
        playIcon = pauseButton.transform.GetChild(1).gameObject;  

        playbackSpeedText.text = Time.timeScale.ToString() + "X";

         IdSceneObjects();
        
    }

    void IdSceneObjects()
    {
        rpObjects = FindObjectsByType<ReplayPhysicsObject>(FindObjectsSortMode.None);
        foreach (var o in rpObjects)
        {
            ObjectId oId = o.GetComponent<ObjectId>();
            o.GetComponent<Rigidbody>().isKinematic = true;
            if(oId)
            {
                oId.SetId(o.gameObject.name + o.transform.position.ToString());
                Debug.Log("id " + oId.id);

            }
            o.GetId();
        }
    }

    void Update()
    {
        if(!isWatchingReplay)
        {
            if(Input.GetButtonDown("Fire1"))
            {
                ClickAction action = new(GetReplayTime());
                actions.Add(action);
            }
        }


        if(!isWatchingReplay) return;

        ReplayHotkeyInputs();

        if(!isPaused)
        {
            replayTime += Time.deltaTime;
        }

        
        while (index < nonInterpolatedActions.Count && nonInterpolatedActions[index].timeStamp <= replayTime)
        {
            nonInterpolatedActions[index].Process();
            index++;
        }

        foreach (var interpolated in interpolatedActions)
        {
            interpolated.Process();
        }

        replayTimelineBar.transform.localScale = new Vector3(replayTime/replayLength, 1,1);

        if(replayTime > replayLength)
        {
            replayTime = replayLength;
        }


        

    }

    public int GetActionIndex()
    {
        return index;
    }

    public float GetReplayTimer()
    {
        return replayTime;
    }

    void ReplayHotkeyInputs()
    {
        if(Input.GetKeyDown(KeyCode.F) && !isFreecam)
        {
            freeCam.transform.position = cam.transform.position;
            freeCam.transform.rotation = cam.transform.rotation;
            player.GetComponent<Shoot>().playerCanvas.SetActive(false);
            freeCam.SetActive(true);
            cam.SetActive(false);
            isFreecam = true;
        }
        else if(Input.GetKeyDown(KeyCode.F) && isFreecam)
        {
            freeCam.SetActive(false);
            cam.SetActive(true);
            player.GetComponent<Shoot>().playerCanvas.SetActive(true);
            isFreecam = false;
        }

        if(Input.GetKeyDown(KeyCode.Space) && !isPaused)
        {
            SetPause(true);
        }
        else if(Input.GetKeyDown(KeyCode.Space) && isPaused)
        {
            SetPause(false);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            replayTime = 0;
            index = 0;
        }

        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void IncreaseTimeScale()
    {
        Time.timeScale *= 2;
        if(Time.timeScale > 8)
        {
            Time.timeScale = 8;
        }
        playbackSpeedText.text = Time.timeScale.ToString() + "X";
    }

    public void DecreaseTimeScale()
    {
        Time.timeScale /= 2;
        if(Time.timeScale < 0.25f)
        {
            Time.timeScale = 0.25f;
        }
        playbackSpeedText.text = Time.timeScale.ToString() + "X";
    }

    public void MoveReplayToPoint(float pct)
    {
        float time = replayLength * pct;
        replayTime = time;
        index = 0;

        while (index < nonInterpolatedActions.Count && nonInterpolatedActions[index].timeStamp <= replayTime)
        {
            nonInterpolatedActions[index].Process();
            index++;
        }

        foreach (var interpolated in interpolatedActions)
        {
            interpolated.Process();
        }

        // for (int i = 0; i < actions.Count; i++)
        // {
        //     if (actions[i].timeStamp <= replayTime)
        //     {
        //         actions[i].Process();
        //         index = i + 1;
        //     }
        //     else
        //     {
        //         break;
        //     }
        // }
    }

    public void Pause()
    {
        if(pauseIcon.activeSelf)
        {
            SetPause(true);
        }
        else
        {
            SetPause(false);
        }
    }

    public void SetPause(bool value)
    {
        if(value)
        {
            isPaused = true;
            playIcon.SetActive(true);
            pauseIcon.SetActive(false);
        }
        else
        {
            isPaused = false;
            playIcon.SetActive(false);
            pauseIcon.SetActive(true);
        }
    }

    public void StartRecording()
    {

        foreach (var o in rpObjects)
        {
            o.GetComponent<Rigidbody>().isKinematic = false;
        }
        recordStartTime = Time.time;
        isRecording = true;
        actions.Clear();
        Debug.Log("Recording started");

    }

    public void StopRecording()
    {
        
        isRecording = false;
        Debug.Log("Recording stopped");
    }

    public float GetReplayTime()
    {
        return Time.time - recordStartTime;
    }

    void OnApplicationQuit()
    {
        if(isWatchingReplay || !isRecording) return;
        SaveReplayToFile();
    }



    public void ShowReplayPanel()
    {
        objects.Clear();
        replayPanel.SetActive(true);
        Transform replayParent = replayPanel.transform.GetChild(2);
        for (int i = 0; i < replayParent.childCount; i++)
        {
            Destroy(replayParent.GetChild(i).gameObject);
        }
        string[] files = Directory.GetDirectories(Application.persistentDataPath+"/Replays/");
        Array.Reverse(files);
        foreach (var file in files)
        {

            GameObject replayObject = Instantiate(replayObjectPrefab, replayParent);
            TextHolder textHolder = replayObject.GetComponent<TextHolder>();
            Button watchButton = replayObject.transform.GetChild(1).GetComponent<Button>();
            Button deleteButton = replayObject.transform.GetChild(2).GetComponent<Button>();
            TextMeshProUGUI replayFileName = replayObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            string[] fileName = Path.GetFileName(file).Split(".");
            replayFileName.text = fileName[0];
            textHolder.text = fileName[0];
            deleteButton.onClick.AddListener(delegate{DeleteFile(file);});
            watchButton.onClick.AddListener(delegate{Load(file);});
        }

    }

    void DeleteFile(string fileName)
    {
        //string path = Application.persistentDataPath+"/Replays/" + fileName + ".rpl";
        if (Directory.Exists(fileName))
        {
            Directory.Delete(fileName, true);
            Debug.Log("Deleted file: " + fileName);
        }
        ShowReplayPanel();
    }

    public void HideReplayPanel()
    {
        replayPanel.SetActive(false);
    }

    void SaveReplayToFile()
    {
        int maxActionsPerFile = 50000;

        int totalActions = actions.Count;

        int fileCount = Mathf.CeilToInt((float)totalActions / maxActionsPerFile);

        string[] replayFiles = Directory.GetFiles(Application.persistentDataPath + "/Replays/");
        int replayCount = replayFiles.Length;

        for (int i = 0; i < fileCount; i++)
        {
            List<ReplayAction> actionChunk = actions.GetRange(i * maxActionsPerFile, Mathf.Min(maxActionsPerFile, totalActions - (i * maxActionsPerFile)));

            string formattedDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string path = Path.Combine(Application.persistentDataPath + "/Replays/Replay"+replayCount+"/", $"replay_{formattedDate}_{i + 1}.rpl");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream file = File.Create(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, new ReplayDataWrapper(actionChunk));
            }

            Debug.Log($"Binary replay chunk saved to {path}");
        }
    }

    public void Load(string path)
    {

        if(!Directory.Exists(path)) return;

        string[] files = Directory.GetFiles(path);

        Debug.Log(files[0]);

        foreach (var o in rpObjects)
        {
            Debug.Log(o.gameObject.name);
            Rigidbody rb = o.GetComponent<Rigidbody>();
            if(rb)
            {
                rb.isKinematic = true;
            }
            o.GetComponent<ReplayPhysicsObject>().OnStartReplay();
        }


        actions.Clear();
        isWatchingReplay = true;
        
        foreach (var kvp in objects)
        {
            var value = kvp.Value;
            ReplayPhysicsObject rpo = value.GetComponent<ReplayPhysicsObject>();
            if(rpo)
            {
                value.GetComponent<ReplayPhysicsObject>().OnStartReplay();
            }
        }
        PlayerSpawner.Instance.menuCamera.SetActive(false);
        PlayerSpawner.Instance.menuCanvas.SetActive(false);
        foreach (var file in files)
        {
            var loadedActions = LoadReplayFromFile(file).replayActions;
            foreach (var a in loadedActions)
            {
                actions.Add(a);
            }
        }
        

        foreach (var action in actions)
        {
            if(action.IsInterpolated())
            {
                interpolatedActions.Add(action);
            }
            else
            {
                nonInterpolatedActions.Add(action);
            }
        }

        replayLength = actions[actions.Count-1].timeStamp;

        replayTime = 0;
        freeCam = Instantiate(freeCameraPrefab, Vector3.zero, Quaternion.identity);
        freeCam.SetActive(false);
        replayCanvas.SetActive(true);
        AddScoredIndicators();



        Cursor.lockState = CursorLockMode.Locked;

        MoveReplayToPoint(0);
        
    }

    void AddScoredIndicators()
    {
        foreach (var action in actions)
        {
            if(action.type == "ScoreAction")
            {
                float pct = action.timeStamp/replayLength;
                RectTransform rt = replayTimelineBackground.GetComponent<RectTransform>();
                float pctPos = rt.rect.width * pct;
                float start = replayTimelineBackground.transform.position.x - (rt.rect.width/2);
                Vector3 pos = new Vector3(start + pctPos, rt.position.y, 0);
                GameObject scoredIndicator = Instantiate(scoredMarker, pos, Quaternion.identity);
                scoredIndicator.transform.SetParent(replayTimelineBackground.transform);
            }
        }
    }


    public ReplayDataWrapper LoadReplayFromFile(string path)
    {

        if (!File.Exists(path))
        {
            Debug.LogError("Binary replay file not found: " + path);
            return null;
        }

        FileStream file = File.Open(path, FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        ReplayDataWrapper data = (ReplayDataWrapper)bf.Deserialize(file);
        file.Close();

        return data;
    }
}
