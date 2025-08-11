
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnce = true;
    bool hasPlayed;

    void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && playOnce) return;
        if (!other.CompareTag("Player")) return;

        hasPlayed = true;
        if (director)
        {
            GameManager.I.SetPlayerControl(false);
            director.stopped += OnStopped;
            director.Play();
        }
    }

    void OnStopped(PlayableDirector d)
    {
        director.stopped -= OnStopped;
        GameManager.I.SetPlayerControl(true);
    }
}
