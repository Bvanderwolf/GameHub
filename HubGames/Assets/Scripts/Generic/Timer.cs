using System;

public class Timer
{
    public float RemainingTime { get; private set; }
    private float mDuration;

    public Timer (float duration)
    {
        RemainingTime = duration;
        mDuration = duration;
    }

    public Timer (float duration, Action onEndEvent)
    {
        RemainingTime = duration;
        mDuration = duration;
        OnTimerEnd += onEndEvent;
    }

    public Timer (Timer recycledTimer)
    {
        RemainingTime = recycledTimer.RemainingTime;
        mDuration = recycledTimer.mDuration;
        foreach (Action action in recycledTimer.OnTimerEnd.GetInvocationList())
        {
            OnTimerEnd += action;
        }
    }

    public event Action OnTimerEnd;

    public void Tick (float tickTime)
    {
        if (RemainingTime == 0f) { return; }

        RemainingTime -= tickTime;
        CheckForTimerEnd();
    }

    public void Reset ()
    {
        if (RemainingTime == 0f)
            RemainingTime = mDuration;
    }

    private void CheckForTimerEnd ()
    {
        if (RemainingTime > 0f) { return; }

        RemainingTime = 0f;
        OnTimerEnd?.Invoke();
    }
}