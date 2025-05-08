using UnityEngine;

public class Basketball : MonoBehaviour
{

    public AudioClip bounceSound;

    void Start()
    {
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Hoop"))
        {
            ScoreAction action = new ScoreAction(ReplayManager.Instance.GetReplayTime());
            ReplayManager.Instance.actions.Add(action);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        AudioManager.Instance.PlayClipAtPoint(bounceSound, transform.position, 1, Random.Range(0.5f, 1.5f));
    }
}
