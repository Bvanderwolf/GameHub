﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISystem : MonoBehaviour
{
    public static UISystem Instance { get; private set; }

    [SerializeField] private float scaleSpeed;
    [SerializeField] private float fadeSpeed;

    public event Action OnUISystemRestartEvent;

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
            //hook up OnGameOver function to GridSystem event
            InputSystem.Instance.OnGameRestartInput += OnUISystemRestart;
            GameManagement.Instance.OnLoadHubStart += OnLoadHubStart;
        }
    }

    private void OnLoadHubStart ()
    {
        StopAllCoroutines();
    }

    private void OnUISystemRestart ()
    {
        OnUISystemRestartEvent?.Invoke();
    }

    public void PopupText (GameObject textGo, string text = "", Action callback = null)
    {
        if (textGo)
        {
            if (text != "") textGo.GetComponent<Text>().text = text;
            StartCoroutine(PopupTextEnumerator(textGo, callback));
        }
    }

    public void PopupText (GameObject textGo, Action callback)
    {
        if (textGo)
        {
            StartCoroutine(PopupTextEnumerator(textGo, callback));
        }
    }

    public void CountDown (GameObject textGo, int count, bool withFade = false, Action onEnd = null)
    {
        if (textGo)
        {
            StartCoroutine(DoCountDown(textGo, count, withFade, onEnd));
        }
    }

    public void PopupTextWithFade (GameObject textGo, string text = "", Action callback = null)
    {
        if (textGo)
        {
            if (text != "") textGo.GetComponent<Text>().text = text;
            StartCoroutine(PopupTextEnumerator(textGo, callback, true));
        }
    }

    public void PopupTextWithFade (GameObject textGo, Action callback)
    {
        if (textGo)
        {
            StartCoroutine(PopupTextEnumerator(textGo, callback, true));
        }
    }

    private IEnumerator DoCountDown (GameObject go, int count, bool withFade = false, Action onEnd = null)
    {
        Text goText = go.GetComponent<Text>();
        Color goTextColor = goText.color;

        for (int current = count; current > 0; current--)
        {
            goText.text = current.ToString();
            yield return StartCoroutine(PopupTextEnumerator(go, null, withFade));
            go.transform.localScale = Vector3.zero;
            go.GetComponent<Text>().color = goTextColor;
        }
        onEnd?.Invoke();
    }

    private IEnumerator PopupTextEnumerator (GameObject go, Action callback, bool withFade = false)
    {
        yield return StartCoroutine(ScaleText(go.transform));

        if (withFade)
        {
            Text textComp = go.GetComponent<Text>();
            yield return StartCoroutine(FadeText(textComp, textComp.color));
        }

        callback?.Invoke();
    }

    private IEnumerator ScaleText (Transform transform)
    {
        float currentLerpTime = 0;

        while (transform.localScale != Vector3.one)
        {
            currentLerpTime += Time.deltaTime * scaleSpeed;
            if (currentLerpTime > 1)
            {
                currentLerpTime = 1;
            }

            float perc = currentLerpTime / 1;
            transform.localScale = Vector3.zero + (perc * (Vector3.one - Vector3.zero));
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator FadeText (Text text, Color startColor)
    {
        float currentLerpTime = 0;

        while (text.color.a != 0)
        {
            currentLerpTime += Time.deltaTime * fadeSpeed;
            if (currentLerpTime > 1)
            {
                currentLerpTime = 1;
            }

            float perc = currentLerpTime / 1;
            text.color = Color.Lerp(startColor, Color.clear, perc);
            yield return new WaitForFixedUpdate();
        }
    }
}