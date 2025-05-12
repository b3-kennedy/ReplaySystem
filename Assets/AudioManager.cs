using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;

    public List<AudioClip> audioClips;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = position;

        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.pitch = pitch;
        aSource.spatialBlend = 1f; // make it 3D
        aSource.Play();

        int index = audioClips.IndexOf(clip);

        AudioAction action = new(ReplayManager.Instance.GetReplayTime(), index, 1f, pitch, position);
        ReplayManager.Instance.actions.Add(action);

        Destroy(tempGO, clip.length / pitch); // destroy after playback
    }
}
