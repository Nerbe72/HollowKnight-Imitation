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
            SetWorkText("�÷��̾ ���������� �׿����ϴ�");
        } catch
        {
            SetWorkText("�÷��̾ ���̴µ� �����߽��ϴ�");
        }
    }

    private void KillBoss()
    {
        try
        {
            SoulMaster boss = GameObject.FindGameObjectWithTag("Boss").GetComponent<SoulMaster>();
            boss.TakeDamage(999);
            SetWorkText("������ ���������� �׿����ϴ�");
        } catch
        {
            SetWorkText("�������� �ƴϰų�\n������ �̹� ���ŵǾ����ϴ�");
        }
    }

    private void HpUP()
    {
        try
        {
            PlayerStats playerStats = PlayerStats.instance;
            playerStats.CurrentHealth = playerStats.MaxHealth;
            SetWorkText("ü�� ȸ�� ����");
        } catch
        {
            SetWorkText("ü�� ȸ���� �����߽��ϴ�");
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
                SetWorkText("������ ���� ����");
            } else
                SetWorkText("�̹� �������Դϴ�");
        } catch
        {
            SetWorkText("������ �����߽��ϴ�");
        }
    }

    private void GetGeo()
    {
        try
        {
            PlayerStats playerStats = PlayerStats.instance;
            playerStats.Geo += 10;
            SetWorkText("10���� ȹ�� ����");
        } catch
        {
            SetWorkText("���� ȹ�� ����");
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
                    SetWorkText("�����̹��� �̴ϸ� Ȱ��ȭ");
                    break;
                case MinimapStatus.Simple:
                    m_map.mapCamera.cullingMask = (1 << LayerMask.NameToLayer("Minimap"));
                    SetWorkText("����ȭ �̴ϸ� Ȱ��ȭ");
                    break;
                case MinimapStatus.Off:
                default:
                    m_map.mapCamera.backgroundColor = Color.clear;
                    m_map.mapCamera.cullingMask = ~-1;
                    SetWorkText("�̴ϸ� ����");
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
            SetWorkText("ü�� ����");
        } catch
        {
            SetWorkText("ü�� ���ҿ� �����߽��ϴ�");
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
                SetWorkText("������������ ���� ����");
            }else
                SetWorkText("�̹� ���������Դϴ�");
        } catch
        {
            SetWorkText("������ �����߽��ϴ�");
        }
    }

    private void ReviveBoss()
    {
        try
        {
            PlayerPrefs.SetString("SoulMaster", false.ToString());
            SetWorkText("���� ��Ȱ ����\n������ ������� ����˴ϴ�");
        }
        catch
        {
            SetWorkText("�̹� ��Ȱ�߰ų�\n�۵� �����߽��ϴ�");
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