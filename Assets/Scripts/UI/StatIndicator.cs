using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;

public class StatIndicator : MonoBehaviour
{
    public static StatIndicator instance;

    private PlayerStats m_playerStats;

    [SerializeField] private UnityEngine.UI.Image m_fill;

    [SerializeField] private Sprite m_emptyLife;
    [SerializeField] private Sprite m_fillLife;

    private UIDocument m_uiDocument;
    private VisualElement m_listLife;
    private Label m_geoText;
    private StyleLength m_LifeWidth = new StyleLength(Length.Percent(5f));
    private StyleLength m_LifeHeight = new StyleLength(Length.Percent(100f));

    private (int beforeHealth, int beforeGeo, int tempGeo, float beforeSoul, float tempSoul) m_uiStats;

    private Coroutine geoCo = null;
    private Coroutine soulCo = null;

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
            return;
        }

        m_uiDocument = GetComponent<UIDocument>();

        m_fill.material.mainTextureScale = new Vector2(1, 0.4f);

    }

    void Start()
    {
        m_playerStats = PlayerStats.instance;

        InitUI();
        DrawUI();

        m_uiStats.beforeHealth = m_playerStats.CurrentHealth;
        m_uiStats.beforeGeo = m_playerStats.Geo;
    }

    private void Update()
    {
        RedrawHealth();
        RedrawGeo();
    }

    private void InitUI()
    {
        var root = m_uiDocument.rootVisualElement;
        m_listLife = root.Q<VisualElement>("Life");
        m_geoText = root.Q<Label>("GeoText");
    }

    private void DrawUI()
    {
        //Set Health
        for (int i = 0; i < m_playerStats.MaxHealth; i++)
        {
            Image img = new Image();
            img.style.width = m_LifeWidth;
            img.style.height = m_LifeHeight;
            m_listLife.Add(img);
        }

        for (int i = 0; i < m_playerStats.CurrentHealth; i++)
        {
            (m_listLife[i] as Image).sprite = m_fillLife;
        }

        //Set Geo(Money)
        m_geoText.text = m_playerStats.Geo.ToString();
    }

    private void RedrawHealth()
    {
        if (m_uiStats.beforeHealth == m_playerStats.CurrentHealth) return;

        for (int i = m_playerStats.CurrentHealth; i<m_playerStats.MaxHealth; i++)
        {
            try
            {
                (m_listLife[i] as Image).sprite = m_emptyLife;
            }
            catch
            {
                Image img = new Image();
                img.style.width = m_LifeWidth;
                img.style.height = m_LifeHeight;
                m_listLife.Add(img);
                (m_listLife[i] as Image).sprite = m_emptyLife;
            }
        }

        for (int i = 0; i < m_playerStats.CurrentHealth; i++)
        {
            try
            {
                (m_listLife[i] as Image).sprite = m_fillLife;
            } catch
            {
                Image img = new Image();
                img.style.width = m_LifeWidth;
                img.style.height = m_LifeHeight;
                m_listLife.Add(img);
                (m_listLife[i] as Image).sprite = m_fillLife;
            }
        }

        for (int i = m_playerStats.MaxHealth; i < m_listLife.childCount; i++)
        {
            (m_listLife[i] as Image).sprite = null;
        }

        m_uiStats.beforeHealth = m_playerStats.CurrentHealth;
    }

    private void RedrawGeo()
    {
        if (m_uiStats.beforeGeo == m_playerStats.Geo) return;

        if (geoCo != null)
        {
            StopCoroutine(geoCo);
        }
            
        geoCo = StartCoroutine(SetGeoCo());
        m_uiStats.beforeGeo = m_playerStats.Geo;
    }

    private void RedrawSoul()
    {
        if (m_uiStats.beforeSoul == m_playerStats.CurrentSoul) return;

        if (soulCo != null)
        {
            StopCoroutine(soulCo);
        }

        soulCo = StartCoroutine(SetSoulCo());
        m_uiStats.beforeSoul = m_playerStats.CurrentSoul;
    }

    private IEnumerator SetGeoCo()
    {
        float time = 0;
        int currentGeo = m_playerStats.Geo;
        m_uiStats.tempGeo = m_uiStats.beforeGeo;

        while (true)
        {
            time += Time.deltaTime * 10f;

            m_geoText.text = ((int)Mathf.Lerp(m_uiStats.tempGeo, currentGeo, time)).ToString();

            //Debug.Log(time);

            if (time >= 1f)
            {
                m_uiStats.beforeGeo = currentGeo;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        geoCo = null;
        yield break;
    }

    private IEnumerator SetSoulCo()
    {
        float time = 0;
        float currentSoul = m_playerStats.CurrentSoul;
        float maxSoul = m_playerStats.MaxSoul;
        m_uiStats.tempSoul = m_uiStats.beforeSoul;

        while (true)
        {
            time += Time.deltaTime * 10f;

            m_fill.material.mainTextureScale = new Vector2(1, (Mathf.Lerp(m_uiStats.tempGeo, currentSoul, time) / maxSoul));

            if (time >= 1f)
            {
                m_uiStats.beforeSoul = currentSoul;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        soulCo = null;
        yield break;
    }
}
