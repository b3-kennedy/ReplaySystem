using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.UI;
using UnityEditor.ShaderGraph.Internal;
using Unity.VisualScripting;



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

    public GameObject replayCanvas;
    GameObject replayTimelineBar;

    public Button pauseButton;
    GameObject playIcon;
    GameObject pauseIcon;

    public Button increasePlaybackButton;
    public Button decreasePlaybackButton;

    public TextMeshProUGUI playbackSpeedText;



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
        replayCanvas.SetActive(false);

        pauseIcon = pauseButton.transform.GetChild(0).gameObject;
        playIcon = pauseButton.transform.GetChild(1).gameObject;  

        playbackSpeedText.text = Time.timeScale.ToString() + "X";
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

        ReplayHotkeyInputs();

        if(!isPaused)
        {
            replayTime += Time.deltaTime;
        }

        
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


        

    }

    void ReplayHotkeyInputs()
    {
        if(Input.GetKeyDown(KeyCode.F) && !isFreecam)
        {
            freeCam.transform.position = cam.transform.position;
            freeCam.transform.rotation = cam.transform.rotation;
            freeCam.SetActive(true);
            cam.SetActive(false);
            isFreecam = true;
        }
        else if(Input.GetKeyDown(KeyCode.F) && isFreecam)
        {
            freeCam.SetActive(false);
            cam.SetActive(true);
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
        index = 0; // Reset the index since we'll reprocess actions

        // Process all actions up to the current replay time to rebuild state
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].timeStamp <= replayTime)
            {
                actions[i].Process();
                index = i + 1;
            }
            else
            {
                break;
            }
        }
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

    void SaveReplayToFile()
    {
        string json = JsonConvert.SerializeObject(new ReplayDataWrapper(actions), Formatting.Indented);
        
        string formattedDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.persistentDataPath+"/Replays/", "replay_" + formattedDate + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"Replay saved to {path}");
    }

    public void ShowReplayPanel()
    {
        replayPanel.SetActive(true);
        Transform replayParent = replayPanel.transform.GetChild(2);
        string[] files = Directory.GetFiles(Application.persistentDataPath+"/Replays/");
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
        actions = LoadReplayFromFile("/Replays/"+path+".json").replayActions;

        replayLength = actions[actions.Count-1].timeStamp;

        replayTime = 0;
        freeCam = Instantiate(freeCameraPrefab, Vector3.zero, Quaternion.identity);
        freeCam.SetActive(false);
        replayCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        
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
