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
    const float SECONDS_IN_DAY = 600f;

    public static int hours = 0;
    public static bool isNewDay = false;

    // Tổng số ngày đã trải qua từ đầu game (KHÔNG reset theo tháng).
    // Dùng cho: EconomicSimulator (daysSinceSale), DemandEventManager, EndingManager.
    public static int TotalDays = 1;

    // Tham chiếu tĩnh để SleepManager gọi SleepToNextMorning().
    public static TimeManager Instance;

    public static Season currentSeason = Season.Spring;

    private void Awake() => Instance = this;

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

    // ─────────────────────────────────────────
    // SLEEP -> NEXT MORNING  (ngủ nhảy tới 6:00 AM hôm sau)
    // ─────────────────────────────────────────
    // +1 ngày (tái dùng IncrementDate/IncrementDay), nhảy đồng hồ về 6AM, bắn OnNewDay.
    // RESET cờ isNewDay/dayProcessed để hệ đếm ngày tự động (trigger ở 2AM) KHÔNG cộng thêm 1 ngày nữa.
    // Dùng trong: SleepManager.FinishSleep().
    public void SleepToNextMorning()
    {
        IncrementDate();
        IncrementDay();
        SetDayText();

        // Đặt đồng hồ về 6:00 AM: cần (Time.time + offsetTime) % SECONDS_IN_DAY == 360 (6h*60).
        const float MORNING = 360f;
        offsetTime = (MORNING - (Time.time % SECONDS_IN_DAY) + SECONDS_IN_DAY) % SECONDS_IN_DAY;

        // Dọn cờ để Update() không tự trigger StartNewDay lần nữa.
        isNewDay = false;
        dayProcessed = false;

        OnNewDay?.Invoke();
    }

    private void IncrementDate()
    {
        currentDate = (Date)(((int)currentDate + 1) % System.Enum.GetValues(typeof(Date)).Length);
    }

    private void IncrementDay()
    {
        TotalDays++;        // đếm tổng ngày cho kinh tế & ending
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
