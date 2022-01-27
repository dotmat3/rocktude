using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

    public List<AudioSource> sources;
    private static AudioController instance;

    private void Start() {
        instance = this;
    }

    public static void PlayOneShot(AudioClip clip, int priority) {
        if (instance == null)
            throw new Exception("An instance of AudioController is unavailable.");
        if (priority < 0 || priority >= instance.sources.Count)
            throw new Exception($"{priority} is not a valid priority. Only 0-{instance.sources.Count} values are allowed.");
        instance.sources[priority].PlayOneShot(clip);
    }
}
