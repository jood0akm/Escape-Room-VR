using UnityEngine;

// Room soundtrack: clock ticking + eerie ambience loop while playing, plus one-shot effects.
// Loops start when NEW GAME finishes loading and stop when the player escapes.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    public AudioSource tick;      // looping clock
    public AudioSource ambient;   // looping scary drone
    public AudioSource sfx;       // one-shots (hint chime, success)
    public AudioClip hintClip;
    public AudioClip successClip;
    public AudioClip wrongClip;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    public void StartLoops()
    {
        if (tick != null && !tick.isPlaying) tick.Play();
        if (ambient != null && !ambient.isPlaying) ambient.Play();
    }

    public void StopLoops()
    {
        if (tick != null) tick.Stop();
        if (ambient != null) ambient.Stop();
    }

    public void PlayHint()
    {
        if (sfx != null && hintClip != null) sfx.PlayOneShot(hintClip);
    }

    // Parts arranged in the wrong order.
    public void PlayWrong()
    {
        if (sfx != null && wrongClip != null) sfx.PlayOneShot(wrongClip);
    }

    // Player reached the open door and escaped.
    public void PlayEscaped()
    {
        StopLoops();
        if (sfx != null && successClip != null) sfx.PlayOneShot(successClip);
    }
}
