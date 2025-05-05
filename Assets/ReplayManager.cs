using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.UI;



[System.Serializable]
public class ReplayDataWrapper
{
   public List<ReplayAction> replayActions;
    

    public ReplayDataWrapper(List<ReplayAction> actions)
    {
        replayActions = actions;
    }
}

public class ReplayActionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ReplayAction);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load the JSON object
        JObject jo = JObject.Load(reader);
        string type = jo["type"]?.ToString();

        ReplayAction result;

        switch (type)
        {
            case "MovementAction":
                result = new MovementAction(0, Vector3.zero, "");
                break;
            case "RotationAction":
                result = new RotationAction(0, Quaternion.identity, "");
                break;
            case "ClickAction":
                result = new ClickAction(0);
                break;
            case "SpawnAction":
                result = new SpawnAction(0,"","");
                break;
            default:
                throw new Exception("Unknown action type: " + type);
        }

        // Populate the object with data from JSON
        serializer.Populate(jo.CreateReader(), result);

        return result;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.FromObject(value, serializer);
        jo.WriteTo(writer);
    }
}

public class ReplayManager : MonoBehaviour
{

    [HideInInspector] public GameObject player = null;
    [HideInInspector] public GameObject cam = null;
    public static ReplayManager Instance;
    public List<ReplayAction> actions = new List<ReplayAction>();
    public Dictionary<string, Queue<ReplayAction>> tracks = new Dictionary<string, Queue<ReplayAction>>();

    public GameObject replayPanel;
    public GameObject replayObjectPrefab;

    public GameObject replayCanvas;
    GameObject replayTimelineBar;

    bool isRecording;

    public bool isWatchingReplay;
    float recordStartTime;

    float replayTime;

    float replayLength;

    public GameObject freeCameraPrefab;
    GameObject freeCam;

    public Button pauseButton;
    GameObject playIcon;
    GameObject pauseIcon;

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
        replayCanvas.SetActive(false);

        pauseIcon = pauseButton.transform.GetChild(0).gameObject;
        playIcon = pauseButton.transform.GetChild(1).gameObject;        
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


        if(!isWatchingReplay && actions.Count > 0) return;

        replayTime += Time.deltaTime;

        
        while(index < actions.Count && actions[index].timeStamp <= replayTime)
        {
            actions[index].Process();
            index++;
        }

        replayTimelineBar.transform.localScale = new Vector3(replayTime/replayLength, 1,1);

        if(replayTime > replayLength)
        {
            replayTime = replayLength;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            freeCam.SetActive(true);
            cam.SetActive(false);
        }

        // if (actionQueue.Count > 0 && actionQueue.Peek().timeStamp <= replayTime)
        // {
            
        //     var action = actionQueue.Dequeue();
        //     Debug.Log(action.objectId);
        //     action.Process();
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
            Time.timeScale = 0.0000001f;
            playIcon.SetActive(true);
            pauseIcon.SetActive(false);
        }
        else
        {
            Time.timeScale = 1;
            playIcon.SetActive(false);
            pauseIcon.SetActive(true);
        }
    }

    public void StartRecording()
    {
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
        if(isWatchingReplay) return;
        SaveReplayToFile();
    }

    void SaveReplayToFile()
    {
        string json = JsonConvert.SerializeObject(new ReplayDataWrapper(actions), Formatting.Indented);
        
        string formattedDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.persistentDataPath, "replay_" + formattedDate + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"Replay saved to {path}");
    }

    public void ShowReplayPanel()
    {
        replayPanel.SetActive(true);
        Transform replayParent = replayPanel.transform.GetChild(2);
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        foreach (var file in files)
        {
            GameObject replayObject = Instantiate(replayObjectPrefab, replayParent);
            TextHolder textHolder = replayObject.GetComponent<TextHolder>();
            Button watchButton = replayObject.transform.GetChild(1).GetComponent<Button>();
            TextMeshProUGUI replayFileName = replayObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            string[] fileName = Path.GetFileName(file).Split(".");
            replayFileName.text = fileName[0];
            textHolder.text = fileName[0];
            watchButton.onClick.AddListener(delegate{Load(fileName[0]);});
        }

    }

    public void Load(string path)
    {
        var physicsObjects = FindObjectsByType<ReplayPhysicsObject>(FindObjectsSortMode.None);
        foreach (var o in physicsObjects)
        {
            Rigidbody rb = o.GetComponent<Rigidbody>();
            if(rb)
            {
                rb.isKinematic = true;
            }
        }


        actions.Clear();
        isWatchingReplay = true;
        PlayerSpawner.Instance.menuCamera.SetActive(false);
        PlayerSpawner.Instance.menuCanvas.SetActive(false);
        actions = LoadReplayFromFile("/"+path+".json").replayActions;

        replayLength = actions[actions.Count-1].timeStamp;

        replayTime = 0;
        freeCam = Instantiate(freeCameraPrefab, Vector3.zero, Quaternion.identity);
        freeCam.SetActive(false);
        replayCanvas.SetActive(true);
        
    }


    public ReplayDataWrapper LoadReplayFromFile(string path)
    {
        string correctPath = Application.persistentDataPath+path;
        if (!File.Exists(correctPath))
        {
            Debug.LogError("Replay file not found: " + correctPath);
            return null;
        }

        string json = File.ReadAllText(correctPath);
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new ReplayActionConverter() },
            TypeNameHandling = TypeNameHandling.None
        };

        return JsonConvert.DeserializeObject<ReplayDataWrapper>(json, settings);
    }
}
