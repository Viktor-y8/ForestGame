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


        WeatherText.text = "Season : " + TimeManager.Instance.CurrentSeason.ToString() + "\n" + "Weather: " + currentWeather.ToString();
    } 

    public void GenerateWeather()
    {
        Season season = TimeManager.Instance.CurrentSeason;

        WeatherText.text = season.ToString();

        if (currentWeather == WeatherType.Rain) SoundManager.Instance.StopLoopingSFX(this);

        float chanceRain, chanceDrought, chanceHeatwave;

        switch (season)
        {
            case Season.Spring:
                chanceRain = 0.35f;
                chanceDrought = 0.05f;
                chanceHeatwave = 0.00f;
                break;

            case Season.Summer:
                chanceRain = 0.10f;
                chanceDrought = 0.25f;
                chanceHeatwave = 0.20f;
                break;

            case Season.Autumn:
                chanceRain = 0.20f;
                chanceDrought = 0.10f;
                chanceHeatwave = 0.00f;
                break;

            case Season.Winter:
                chanceRain = 0.25f;
                chanceDrought = 0.05f;
                chanceHeatwave = 0.00f;
                break;

            default:
                chanceRain = 0.20f;
                chanceDrought = 0.10f;
                chanceHeatwave = 0.00f;
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
            FireManager.Instance.OnRain();
            SoundManager.Instance.PlayLoopingSFX(this, rainSFX, transform.position, false);
        }
        else
            currentWeather = WeatherType.Normal;

        UpdateRainVisual();

        float fireChance = 0.05f;

        if (currentWeather == WeatherType.Drought) fireChance = 0.25f;
        if (currentWeather == WeatherType.Heatwave) fireChance = 0.40f;

        if (Random.value < fireChance)
            StartRandomFire();


        WeatherText.text = "Season : " + season.ToString() + "\n" + "Weather: " + currentWeather.ToString();
    }

    public void GenerateWeatherSilent()
    {
        Season season = TimeManager.Instance.CurrentSeason;

        WeatherText.text = season.ToString();

        float chanceRain, chanceDrought, chanceHeatwave;

        switch (season)
        {
            case Season.Spring:
                chanceRain = 0.35f;
                chanceDrought = 0.05f;
                chanceHeatwave = 0.00f;
                break;

            case Season.Summer:
                chanceRain = 0.10f;
                chanceDrought = 0.25f;
                chanceHeatwave = 0.20f;
                break;

            case Season.Autumn:
                chanceRain = 0.20f;
                chanceDrought = 0.10f;
                chanceHeatwave = 0.00f;
                break;

            case Season.Winter:
                chanceRain = 0.25f;
                chanceDrought = 0.05f;
                chanceHeatwave = 0.00f;
                break;

            default:
                chanceRain = 0.20f;
                chanceDrought = 0.10f;
                chanceHeatwave = 0.00f;
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
            FireManager.Instance.OnRain();
        }
        else
            currentWeather = WeatherType.Normal;

        UpdateRainVisual();

        WeatherText.text = "Season : " + season.ToString() + "\n" + "Weather: " + currentWeather.ToString();
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

    private void StartRandomFire()
    {
        List<Soil> candidates = GameManager.Instance.GetAllSoils()
            .FindAll(s => !s.isLocked && !s.isOnFire && s.moisture < 0.55f);

        if (candidates.Count == 0)
            candidates = GameManager.Instance.GetAllSoils()
                .FindAll(s => !s.isLocked && !s.isOnFire);

        if (candidates.Count == 0) return;

        candidates.Sort((a, b) => a.moisture.CompareTo(b.moisture));
        int pickFrom = Mathf.Max(1, candidates.Count / 3);
        Soil target = candidates[Random.Range(0, pickFrom)];

        bool fireStarted = FireManager.Instance.StartFire(target);

        if (fireStarted)
            TimeManager.Instance.scaleTime(1f);
    }
}