using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static event Action OnNewDay;

    public TMPro.TextMeshProUGUI dayText;
    public TMPro.TextMeshProUGUI dateText;
    public TMPro.TextMeshProUGUI timeText;
    public TMPro.TextMeshProUGUI periodText;

    public float offsetTime = 1550f;
    private bool dayProcessed = false;

    private int currentDay = 1;
    private int currentYear = 1;

    private Date currentDate = Date.Mon;
    private PeriodInDay currentPeriod = PeriodInDay.AM;

    const int MAX_TIME_IN_DAY = 24;
    const int MAX_DAY_IN_MONTH = 28;
    const int MAX_TIME_IN_PERIOD = 12;
    const float SECONDS_IN_DAY = 1440f;

    public static int hours = 0;
    public static bool isNewDay = false;

    public static Season currentSeason = Season.Spring;

    private void Start() => SetDayText();

    void Update()
    {
        float elapsedTime = (Time.time + offsetTime) % SECONDS_IN_DAY;

        hours = Mathf.FloorToInt((elapsedTime % 3600f) / 60f);
        int minutes = Mathf.FloorToInt(elapsedTime % 60f);

        SetPeriodText(hours, minutes);
        SetTimeText(hours, minutes);
        SetNewDay(elapsedTime);
    }

    private void SetPeriodText(int hours, int minutes)
    {
        if (hours >= MAX_TIME_IN_PERIOD && hours < MAX_TIME_IN_DAY)
        {
            currentPeriod = PeriodInDay.PM;
        }
        else if (hours == MAX_TIME_IN_DAY || hours < MAX_TIME_IN_PERIOD)
        {
            currentPeriod = PeriodInDay.AM;
        }

        periodText.text = currentPeriod.ToString();
    }

    private void SetTimeText(int hours, int minutes)
    {
        string hourText = string.Empty;
        string minuteText = string.Empty;

        if (currentPeriod == PeriodInDay.PM)
        {
            int periodHour = hours - MAX_TIME_IN_PERIOD;
            hourText = periodHour < 10 ? $"0{periodHour}" : $"{periodHour}";
        }
        else
        {
            hourText = hours < 10 ? $"0{hours}" : $"{hours}";
        }
        minuteText = minutes < 10 ? $"0{minutes}" : $"{minutes}";

        timeText.text = $"{hourText}:{minuteText}";
    }

    private void SetNewDay(float elapsedTime)
    {
        int secondsInTime = Mathf.FloorToInt(elapsedTime);
        if (secondsInTime == 120 && !dayProcessed)
        {
            isNewDay = true;
        }
        if (isNewDay && !dayProcessed)
        {
            StartCoroutine(StartNewDay());
        }
    }

    private void SetDayText()
    {
        dateText.text = $"{currentDate},";
        dayText.text = currentDay.ToString();
    }

    IEnumerator StartNewDay()
    {
        dayProcessed = true;  

        IncrementDate();
        IncrementDay();
        SetDayText();

        offsetTime += 240f; //2AM -> 6AM (4HRS = 4 * 60)

        OnNewDay?.Invoke();

        yield return new WaitForSeconds(1f);

        isNewDay = false;  
        dayProcessed = false;
    }

    private void IncrementDate()
    {
        currentDate = (Date)(((int)currentDate + 1) % System.Enum.GetValues(typeof(Date)).Length);
    }

    private void IncrementDay()
    {
        currentDay++;
        if (currentDay > MAX_DAY_IN_MONTH)
        {
            currentDay = 1;
            currentSeason = (Season)(((int)currentSeason + 1) % System.Enum.GetValues(typeof(Season)).Length);
            if (currentSeason == Season.Spring)
            {
                currentYear++;
            }
        }
    }
}

public enum PeriodInDay { AM, PM }
public enum Date { Mon, Tue, Wed, Thu, Fri, Sat, Sun }
public enum Season { Spring, Summer, Fall, Winter }
