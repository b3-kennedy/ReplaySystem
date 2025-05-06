using UnityEngine;

public class Basketball : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Hoop"))
        {
            ScoreAction action = new ScoreAction(ReplayManager.Instance.GetReplayTime());
            ReplayManager.Instance.actions.Add(action);
        }
    }
}
