using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SubsystemsImplementation;

public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager instance;
    public string currentMap = "";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneUnloaded += FadeOut;
        SceneManager.sceneLoaded += FadeIn;
    }

    private void FadeIn(Scene scene, LoadSceneMode mode)
    {

    }

    private void FadeOut(Scene scene)
    {

    }

}
