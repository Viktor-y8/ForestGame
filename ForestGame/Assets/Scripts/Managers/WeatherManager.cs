using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;
    public WeatherType currentWeather;

    [SerializeField] private GameObject rainEffect;
    private ParticleSystem rainParticles;

    [SerializeField] private TMP_Text WeatherText;

    public AudioClip rainSFX;

    private void Awake()
    {

        Instance = this;

        if (rainEffect != null)
            rainParticles = rainEffect.GetComponent<ParticleSystem>();

        if (rainParticles != null)
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);


        WeatherText.text = "Season : " + RoundManager.Instance.CurrentSeason + "\n" + "Weather: " + currentWeather.ToString();
    }

    public WeatherType RollForSeason(Season season)
    {
        if (currentWeather == WeatherType.Rain) SoundManager.Instance.StopLoopingSFX(this);

        float chanceRain, chanceDrought, chanceHeatwave;

        switch (season)
        {
            case Season.Spring:
                chanceRain = 0.35f; chanceDrought = 0.05f; chanceHeatwave = 0.00f;
                break;
            case Season.Summer:
                chanceRain = 0.10f; chanceDrought = 0.25f; chanceHeatwave = 0.20f;
                break;
            case Season.Autumn:
                chanceRain = 0.20f; chanceDrought = 0.10f; chanceHeatwave = 0.00f;
                break;
            case Season.Winter:
                chanceRain = 0.25f; chanceDrought = 0.05f; chanceHeatwave = 0.00f;
                break;
            default:
                chanceRain = 0.20f; chanceDrought = 0.10f; chanceHeatwave = 0.00f;
                break;
        }

        float r = Random.value;

        if (r < chanceDrought)
            currentWeather = WeatherType.Drought;
        else if (r < chanceDrought + chanceHeatwave)
            currentWeather = WeatherType.Heatwave;
        else if (r < chanceDrought + chanceHeatwave + chanceRain)
        {
            currentWeather = WeatherType.Rain;
            SoundManager.Instance.PlayLoopingSFX(this, rainSFX, transform.position, false);

            foreach (Soil soil in GameManager.Instance.GetAllSoils())
            {
                if (!soil.isLocked) soil.Water();
            }
        }
        else
            currentWeather = WeatherType.Normal;

        UpdateRainVisual();
        WeatherText.text = "Season : " + season + "\n" + "Weather: " + currentWeather;

        return currentWeather;
    }

    private void UpdateRainVisual()
    {
        if (rainParticles == null) return;

        if (currentWeather == WeatherType.Rain)
        {
            if (!rainParticles.isPlaying)
                rainParticles.Play();
        }
        else
        {
            if (rainParticles.isPlaying)
                rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}