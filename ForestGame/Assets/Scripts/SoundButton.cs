using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SoundButton : MonoBehaviour
{

    private int sfxPercent = 50;
    private int musicPercent = 50;

    [SerializeField] public bool isSFX = false;
    [SerializeField] public bool isMusic = false;

    [SerializeField] public TMP_Text sfxText;
    [SerializeField] public TMP_Text musicText;

    public void OnClick()
    {
        if (isSFX)
        {
            sfxPercent += 10;

            if (sfxPercent > 100)
                sfxPercent = 0;

            SoundManager.Instance.SetSFXVolume(sfxPercent / 100f);
            sfxText.text = $"SFX - {sfxPercent}%";
        }

        if (isMusic)
        {
            musicPercent += 10;

            if (musicPercent > 100)
                musicPercent = 0;

            SoundManager.Instance.SetMusicVolume(musicPercent / 100f);
            musicText.text = $"Music - {musicPercent}%";
        }

        SoundManager.Instance.PlaySFX("buttonSFX");

    }
}
