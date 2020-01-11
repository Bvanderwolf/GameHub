using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start ()
    {
        if (Instance == this)
        {
            GameManagement.Instance.OnLoadHubStart += OnLoadHubStart;
        }
    }

    private void OnLoadHubStart ()
    {
        timers.Clear();
    }

    private void FixedUpdate ()
    {
        float delta = Time.deltaTime;
        if (timers.Count != 0)
        {
            foreach (KeyValuePair<string, Timer> timer in timers)
            {
                timer.Value.Tick(delta);
            }
            if (timers["gamestart"]?.RemainingTime == 0f) { timers.Remove("gamestart"); }
        }
    }

    public bool IsTimingStartTimer ()
    {
        return timers.ContainsKey("gamestart");
    }

    public void AddTimer (string name, Timer newTimer)
    {
        if (timers.Count == 0 && !timers.Any((item) => item.Value.RemainingTime == 0f))
        {
            timers.Add(name, newTimer);
        }
        else
        {
            string keyOfReplacement = timers.Where((item) => item.Value.RemainingTime == 0f).First().Key;
            timers.Remove(keyOfReplacement);
            timers.Add(name, new Timer(newTimer));
        }
    }
}