using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Save : MonoBehaviour
{
    InventoryUI m_inventoryUI;

    [SerializeField] GameObject m_sitUI;
    [SerializeField] private Transform m_sitPos;

    private bool m_staySit = false;

    private string m_saveMap;
    private Vector2 m_savePosition;
    private PlayerController m_player;
    private PlayerInput m_saveInput;
    private PlayerStats m_playerStats;


    private void Awake()
    {
        m_saveInput = GetComponent<PlayerInput>();
        if (m_sitUI.activeSelf)
            m_sitUI.SetActive(false);
    }

    void Start()
    {
        m_player = PlayerController.instance;
        m_inventoryUI = InventoryUI.instance;
        m_playerStats = PlayerStats.instance;

        //저장 스팟이 존재하는 씬 이름과 포지션을 저장함
        m_saveMap = gameObject.scene.name;
        m_savePosition = transform.position;
        m_saveInput.DeactivateInput();
    }

    private void Update()
    {
        ShowSitText();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            //앉기 라는 텍스트를 띄움
            m_saveInput.ActivateInput();
            m_staySit = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_playerStats.isCharmEditable = false;
        m_saveInput.DeactivateInput();
        m_sitUI.SetActive(false);
        PlayerActionTrigger(true);
        m_player.SetSitToggle(false);
        m_staySit = false;
    }

    private void ShowSitText()
    {
        if (m_staySit && !m_sitUI.activeSelf && !m_player.GetSit())
        {
            m_sitUI.SetActive(true);
        }
        else if ((m_sitUI.activeSelf && m_player.GetSit()) || !m_staySit)
        {
            m_sitUI.SetActive(false);
        }
    }

    private void OnSave(InputValue _value)
    {
        float input = _value.Get<float>();

        if (input == 1f)
        {
            if (m_player.GetSit()) return;

            SetSavePoint();
            PlayerActionTrigger(false);
            m_player.SetSitToggle(true);
            StartCoroutine(SitLerpCo());
        }
    }

    private void PlayerActionTrigger(bool _isActionTrue)
    {
        if (_isActionTrue)
        {
            m_player.m_playerInput.actions["Attack"].Enable();
            m_player.m_playerInput.actions["Dash"].Enable();
            m_player.m_playerInput.actions["Jump"].Enable();
            m_player.m_playerInput.actions["Skill"].Enable();
        }
        else
        {
            m_player.m_playerInput.actions["Attack"].Disable();
            m_player.m_playerInput.actions["Dash"].Disable();
            m_player.m_playerInput.actions["Jump"].Disable();
            m_player.m_playerInput.actions["Skill"].Disable();
        }
    }

    private void SetSavePoint()
    {
        //reset stats
        //save respawn position
        m_playerStats.RespawnMap = m_saveMap;
        m_playerStats.RespawnPosition = m_savePosition;
        m_playerStats.CurrentHealth = m_playerStats.MaxHealth;

        //save to DB
        PlayerPrefs.SetString("SpawnMap", m_saveMap);
        PlayerPrefs.SetFloat("SpawnX", m_sitPos.transform.position.x);
        PlayerPrefs.SetFloat("SpawnY", m_sitPos.transform.position.y);
    }

    private IEnumerator SitLerpCo()
    {
        Vector2 originalPos = PlayerController.instance.transform.position;
        float timeCheck = 0;
        while (true)
        {
            timeCheck += Time.deltaTime * 10f;

            PlayerController.instance.transform.position = Vector2.Lerp(originalPos, m_sitPos.position, timeCheck);

            if (timeCheck >= 1f) break;

            yield return new WaitForEndOfFrame();
        }
        yield break;
    }
}
