using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] internal AudioSource bg_adudio;
    [SerializeField] internal AudioSource audioPlayer_wl;
    [SerializeField] internal AudioSource audioPlayer_button;
    [SerializeField] internal AudioSource audioSpin_button;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioClip[] Bonusclips;

    internal bool muteAudio = false;
    internal bool muteMusic = false;

    private void Start()
    {
        if (bg_adudio) bg_adudio.Play();
        audioPlayer_button.clip = clips[clips.Length - 1];
        audioSpin_button.clip = clips[clips.Length - 2];
    }

    internal void SwitchBGSound(bool isbonus)
    {
        if (isbonus)
        {
            // if (bg_audioBonus) bg_audioBonus.enabled = true;
            if (bg_adudio) bg_adudio.enabled = false;
        }
        else
        {
            //if (bg_audioBonus) bg_audioBonus.enabled = false;
            if (bg_adudio) bg_adudio.enabled = true;
        }
    }

    internal void PlayWLAudio(string type)
    {
        if (muteAudio) return;
        audioPlayer_wl.loop = false;
        int index = 0;
        switch (type)
        {
            case "ding":
                index = 0;
                // audioPlayer_wl.loop = true;
                break;
            case "button":
                index = 1;
                break;
            case "b2":
                index = 2;
                break;
            case "diamond":
                index = 3;
                break;
            case "bomb":
                index = 4;
                break;
            case "start":
                index = 5;
                break;
        }
        StopWLAaudio();
        audioPlayer_wl.clip = clips[index];
        Debug.Log("STOP SPIN INDEX :" + index);
        audioPlayer_wl.Play();


    }

    internal void PlayBonusAudio(string type)
    {
        audioPlayer_wl.loop = false;
        int index = 0;
        switch (type)
        {
            case "win":
                index = 0;
                break;
            case "lose":
                index = 1;
                break;
            case "cycleSpin":
                index = 2;
                break;
        }
        //StopBonusAaudio();
        // audioPlayer_Bonus.clip = Bonusclips[index];
        // audioPlayer_Bonus.Play();

    }

    internal void PlayButtonAudio()
    {
        audioPlayer_button.Play();
    }

    internal void PlaySpinButtonAudio()
    {
        audioSpin_button.Play();
    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
    }

    // internal void StopBonusAaudio()
    // {
    //     audioPlayer_Bonus.Stop();
    //     audioPlayer_Bonus.loop = false;
    // }

    internal void StopBgAudio()
    {
        bg_adudio.Stop();
    }

    private bool isForceMuted = false;

    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted) return;
        isForceMuted = forceMute;
        ApplyMuteState();
    }

    private void ApplyMuteState()
    {
        bool soundMuted = isForceMuted || muteAudio;
        bool musicMuted = isForceMuted || muteMusic;
        audioPlayer_wl.mute = soundMuted;
        audioPlayer_button.mute = soundMuted;
        audioSpin_button.mute = soundMuted;
        bg_adudio.mute = musicMuted;
    }

    internal void SetSoundMuted(bool muted)
    {
        muteAudio = muted;
        isForceMuted = false;
        ApplyMuteState();
    }

    internal void SetMusicMuted(bool muted)
    {
        muteMusic = muted;
        isForceMuted = false;
        ApplyMuteState();
    }

    private void OnApplicationFocus(bool focus)
    {
        SetMuteAll(!focus);
    }

}
