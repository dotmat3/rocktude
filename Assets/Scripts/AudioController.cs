using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

    public List<AudioSource> sources;
    private static AudioController instance;
    private static HashSet<string> playingClips = new HashSet<string>();

    private void Start() {
        instance = this;
    }

    public static void PlayOneShot(AudioClip clip, int priority) {
        if (instance == null)
            throw new Exception("An instance of AudioController is unavailable.");
        if (priority < 0 || priority >= instance.sources.Count)
            throw new Exception($"{priority} is not a valid priority. Only 0-{instance.sources.Count} values are allowed.");

        AudioSource audioSrc = instance.sources[priority];
        if (!playingClips.Contains(clip.name)) { 
            audioSrc.PlayOneShot(clip);
            playingClips.Add(clip.name);
            instance.StartCoroutine(ClipEnded(clip));
        }
    }

    private static IEnumerator ClipEnded(AudioClip clip) {
        yield return new WaitForSeconds(clip.length);
        playingClips.Remove(clip.name);
    }


}
