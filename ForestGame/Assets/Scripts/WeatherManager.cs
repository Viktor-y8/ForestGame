using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;
    public WeatherType currentWeather;

    private void Awake() => Instance = this;

    public void GenerateWeather()
    {
        Season season = TimeManager.Instance.CurrentSeason;

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
    }
}