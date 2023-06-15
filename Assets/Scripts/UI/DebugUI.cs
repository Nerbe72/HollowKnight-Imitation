using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DebugUI : MonoBehaviour
{
    enum MinimapStatus
    {
        Simple,
        Real,
        Off,
        Count
    }

    public static DebugUI instance;

    private Minimap m_map;

    private UIDocument m_uiDocument;
    private VisualElement root;
    private Label m_workText;
    private Button m_killPlayer;
    private Button m_killBoss;
    private Button m_hpUp;
    private Button m_warpBoss;
    private Button m_getGeo;
    private Button m_minimap;
    private Button m_hpDown;
    private Button m_warpStart;
    private Button m_reviveBoss;

    private Coroutine workCo;

    MinimapStatus minimap = MinimapStatus.Simple;

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
    }

    private void Start()
    {
        InitUI();
    }

    private void InitUI()
    {
        m_uiDocument = GetComponent<UIDocument>();
        root = m_uiDocument.rootVisualElement;

        m_workText = root.Q<Label>("workText");

        m_killPlayer = root.Q<Button>("killPlayer");
        m_killBoss = root.Q<Button>("killBoss");
        m_hpUp = root.Q<Button>("hpUp");
        m_warpBoss = root.Q<Button>("warpBoss");

        m_getGeo = root.Q<Button>("getGeo");
        m_minimap = root.Q<Button>("minimap");
        m_hpDown = root.Q<Button>("hpDown");
        m_warpStart = root.Q<Button>("warpStart");
        m_reviveBoss = root.Q<Button>("reviveBoss");


        m_killPlayer.clicked += KillPlayer;
        m_killBoss.clicked += KillBoss;
        m_hpUp.clicked += HpUP;
        m_warpBoss.clicked += WarpBoss;

        m_getGeo.clicked += GetGeo;
        m_minimap.clicked += ToggleMinimap;
        m_hpDown.clicked += HpDown;
        m_warpStart.clicked += WarpStart;
        m_reviveBoss.clicked += ReviveBoss;

        root.style.display = DisplayStyle.None;
    }

    private void KillPlayer()
    {
        try
        {
            PlayerController player = PlayerController.instance;
            player.TakeDamage(999);
            SetWorkText("플레이어를 성공적으로 죽였습니다");
        } catch
        {
            SetWorkText("플레이어를 죽이는데 실패했습니다");
        }
    }

    private void KillBoss()
    {
        try
        {
            SoulMaster boss = GameObject.FindGameObjectWithTag("Boss").GetComponent<SoulMaster>();
            boss.TakeDamage(999);
            SetWorkText("보스를 성공적으로 죽였습니다");
        } catch
        {
            SetWorkText("보스룸이 아니거나\n보스가 이미 제거되었습니다");
        }
    }

    private void HpUP()
    {
        try
        {
            PlayerStats playerStats = PlayerStats.instance;
            playerStats.CurrentHealth = playerStats.MaxHealth;
            SetWorkText("체력 회복 성공");
        } catch
        {
            SetWorkText("체력 회복에 실패했습니다");
        }
    }

    private void WarpBoss()
    {
        try
        {
            if (SceneChangeManager.instance.currentMap != "SoulMaster")
            {
                PlayerController player = PlayerController.instance;
                player.transform.position = new Vector2(14.32f, -7.74f);
                SetWorkText("보스룸 워프 성공");
            } else
                SetWorkText("이미 보스룸입니다");
        } catch
        {
            SetWorkText("워프에 실패했습니다");
        }
    }

    private void GetGeo()
    {
        try
        {
            PlayerStats playerStats = PlayerStats.instance;
            playerStats.Geo += 10;
            SetWorkText("10지오 획득 성공");
        } catch
        {
            SetWorkText("지오 획득 실패");
        }
    }

    private void ToggleMinimap()
    {
        try
        {
            m_map = Minimap.instance;
            minimap = (MinimapStatus)(((int)minimap + 1) % (int)MinimapStatus.Count);
            m_map.mapCamera.backgroundColor = new Color(1f, 1f, 1f, 0.12f);

            switch (minimap)
            {
                case MinimapStatus.Real:
                    m_map.mapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Minimap"));
                    SetWorkText("원본이미지 미니맵 활성화");
                    break;
                case MinimapStatus.Simple:
                    m_map.mapCamera.cullingMask = (1 << LayerMask.NameToLayer("Minimap"));
                    SetWorkText("간소화 미니맵 활성화");
                    break;
                case MinimapStatus.Off:
                default:
                    m_map.mapCamera.backgroundColor = Color.clear;
                    m_map.mapCamera.cullingMask = ~-1;
                    SetWorkText("미니맵 꺼짐");
                    break;
            }
        } catch { }
    }

    private void HpDown()
    {
        try
        {
            PlayerController player = PlayerController.instance;
            player.TakeDamage(1);
            SetWorkText("체력 감소");
        } catch
        {
            SetWorkText("체력 감소에 실패했습니다");
        }
    }

    private void WarpStart()
    {
        try
        {
            if (SceneChangeManager.instance.currentMap != "Start")
            {
                PlayerController player = PlayerController.instance;
                player.transform.position = new Vector2(14.32f, -11f);
                SetWorkText("시작지점으로 워프 성공");
            }else
                SetWorkText("이미 시작지점입니다");
        } catch
        {
            SetWorkText("워프에 실패했습니다");
        }
    }

    private void ReviveBoss()
    {
        try
        {
            PlayerPrefs.SetString("SoulMaster", false.ToString());
            SetWorkText("보스 부활 성공\n보스룸 재입장시 적용됩니다");
        }
        catch
        {
            SetWorkText("이미 부활했거나\n작동 실패했습니다");
        }
    }

    private void SetWorkText(string _txt)
    {
        if (workCo != null)
        {
            StopAllCoroutines();
            m_workText.RemoveFromClassList("work");
            m_workText.style.display = DisplayStyle.None;
        }

        m_workText.text = _txt;
        workCo = StartCoroutine(WorkTextCo());
    }

    private void OnToggle(InputValue _value)
    {
        float input = _value.Get<float>();
        
        if (input == 1f)
        {
            if (root.style.display == DisplayStyle.Flex)
                root.style.display = DisplayStyle.None;
            else
                root.style.display = DisplayStyle.Flex;
        }
    }

    private IEnumerator WorkTextCo()
    {
        m_workText.style.display = DisplayStyle.Flex;
        m_workText.AddToClassList("work");
        float time = 0;

        while (true)
        {
            time += Time.deltaTime * 4f;

            if (time >= 1)
            {
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        m_workText.RemoveFromClassList("work");
        m_workText.style.display = DisplayStyle.None;
        yield break;
    }
}