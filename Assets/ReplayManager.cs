using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering;



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

    public static ReplayManager Instance;
    public List<ReplayAction> actions = new List<ReplayAction>();
    Queue<ReplayAction> actionQueue = new Queue<ReplayAction>();

    public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    bool isRecording;

    public bool isWatchingReplay;
    float recordStartTime;

    float replayTime;

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
        //StartRecording();
        //LoadReplayFromFile(Application.persistentDataPath+"/replay_2025-05-01_16-24-35.json");
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

        if (actionQueue.Count > 0 && actionQueue.Peek().timeStamp <= replayTime)
        {
            
            var action = actionQueue.Dequeue();
            action.Process();
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

    public void Load()
    {
        actions.Clear();
        isWatchingReplay = true;
        PlayerSpawner.Instance.menuCamera.SetActive(false);
        PlayerSpawner.Instance.menuCanvas.SetActive(false);
        actions = LoadReplayFromFile("/replay_2025-05-01_18-46-52.json").replayActions;
        actionQueue = new Queue<ReplayAction>(actions);
        replayTime = 0;
        
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
