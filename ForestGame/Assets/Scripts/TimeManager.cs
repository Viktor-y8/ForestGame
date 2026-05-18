using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time")]
    public int year = 1;
    public int month = 1;
    public int day = 1;

    [Tooltip("Real seconds per in-game day")]
    public float dayLength = 2f;

    private float timer;
    public float timeScale = 1f;

    public event Action OnDayPassed;
    public event Action OnMonthPassed;
    public event Action OnYearPassed;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        timer += Time.deltaTime * timeScale;

        if (timer >= dayLength)
        {
            timer = 0f;
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        day++;

        OnDayPassed?.Invoke();

        if (day > 30)
        {
            day = 1;
            month++;

            WeatherManager.Instance.GenerateWeather();

            OnMonthPassed?.Invoke();

            if (month > 12)
            {
                month = 1;
                year++;

                OnYearPassed?.Invoke();
            }
        }

        Debug.Log($"Year {year}, Month {month}, Day {day}");
    }


    public Season CurrentSeason
    {
        get
        {
            if (month >= 3 && month <= 5) return Season.Spring;
            if (month >= 6 && month <= 8) return Season.Summer;
            if (month >= 9 && month <= 11) return Season.Autumn;

            return Season.Winter;
        }
    }
}