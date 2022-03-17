using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; } // 音频管理器的实例

    private AudioSource source; // 音频资源组件

    void Start()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }

    public void AudioPlay(AudioClip clip) // 播放音效
    {
        source.PlayOneShot(clip);
    }
}
