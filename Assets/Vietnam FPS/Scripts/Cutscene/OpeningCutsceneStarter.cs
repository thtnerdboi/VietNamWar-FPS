using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class OpeningCutsceneStarter : MonoBehaviour
{
    public PlayableDirector director;
    public Transform player;
    public Vector3 dropEnd;
    public float dropSeconds = 5f;

    CharacterController cc;

    IEnumerator Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        cc = player ? player.GetComponent<CharacterController>() : null;

        GameManager.I?.SetPlayerControl(false);
        if (director) director.Play();

        if (cc) cc.enabled = false;

        Vector3 start = player.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, dropSeconds);
            float s = Mathf.SmoothStep(0f, 1f, t);
            player.position = Vector3.Lerp(start, dropEnd, s);
            yield return null;
        }

        if (cc)
        {
            cc.enabled = true;
            cc.Move(Vector3.zero);
        }

        GameManager.I?.SetPlayerControl(true);
        enabled = false;
    }
}
