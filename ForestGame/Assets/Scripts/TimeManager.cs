using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button timeScaleButton;
    [SerializeField] private TMP_Text timeScaleText;

    private void Awake()
    {
        Instance = this;

        timeScaleButton.onClick.AddListener(scaleTime);

        timeScaleText.text = "x1";
    }

    private void Update()
    {
        timer += Time.deltaTime * timeScale;

        if (timer >= dayLength)
        {
            timer = 0f;
            AdvanceDay();
        }

        timeText.text = "Day: " + day + "\n" + "Month: " + month + "\n" + "Year: " + year;
    }

    private void scaleTime()
    {

        if (TutorialManager.IsTutorialActive) return;

        if (timeScale == 1f) timeScale = 2f;
        else if (timeScale == 2f) timeScale = 5f;
        else if (timeScale == 5f) timeScale = 10f;
        else if (timeScale == 10f) timeScale = 25f;
        else if (timeScale == 25f) timeScale = 100f;
        else timeScale = 1f;

        timeScaleText.text = "x" + timeScale;
    }

    public void scaleTime(float scale)
    {
        timeScale = scale;
        timeScaleText.text = "x" + timeScale;
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