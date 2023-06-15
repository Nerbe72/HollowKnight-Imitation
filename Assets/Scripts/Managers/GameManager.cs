using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 120;
        //SetResolutionTo916();
    }

    public void TimeBasePause(float _pauseTime)
    {
        StartCoroutine(StopTime(_pauseTime));
    }

    public void PauseGame(bool _true)
    {
        Time.timeScale = _true ? 0 : 1;
    }

    private IEnumerator StopTime(float _time)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(_time);
        Time.timeScale = 1;
    }

    private void SetResolutionTo916()
    {
        float targetRatio = 9f / 16f;
        float currentSize = Screen.height / Screen.width;
        float scaleHeight = currentSize / targetRatio;
        float fixedWidth = (float)Screen.width / scaleHeight;
        Screen.SetResolution((int)fixedWidth, Screen.height, true);
    }
}
