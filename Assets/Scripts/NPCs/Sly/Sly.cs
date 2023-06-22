using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Sly : MonoBehaviour
{
    private PlayerInput m_npcInput;

    [SerializeField] private Shop m_shop;
    [SerializeField] private GameObject m_talk;
    [SerializeField] private GameObject m_untalkCamera;
    [SerializeField] private GameObject m_talkCamera;

    private void Awake()
    {
        m_npcInput = GetComponent<PlayerInput>();
        m_npcInput.DeactivateInput();

        if (m_talk.activeSelf)
            m_talk.SetActive(false);

        if (m_talkCamera.activeSelf)
            m_talkCamera.SetActive(false);

        if (!m_talkCamera.activeSelf)
            m_untalkCamera.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            m_npcInput.ActivateInput();
            m_talk.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            m_npcInput.DeactivateInput();
            m_talk.SetActive(false);
        }
    }

    //input system에 의해 호출됨
    private void OnTalk(InputValue _value)
    {
        float input = _value.Get<float>();

        if (input == 1f)
        {
            if (!m_shop.IsToggled())
            {
                m_shop.ToggleUI(true);
                PlayerController.instance.m_playerInput.DeactivateInput();
                Pause.instance.m_input.DeactivateInput();
                m_talkCamera.SetActive(true);
                m_untalkCamera.SetActive(false);
            }
        }
    }
    private void OnClose(InputValue _value)
    {
        float input = _value.Get<float>();

        if (input == 1f)
        {
            if (m_shop.IsToggled())
            {
                m_shop.ToggleUI(false);
                StartCoroutine(ActivateCo());
                m_talkCamera.SetActive(false);
                Pause.instance.m_input.ActivateInput();
                m_untalkCamera.SetActive(true);
            }
        }
    }

    private IEnumerator ActivateCo()
    {
        yield return new WaitForSeconds(0.2f);
        PlayerController.instance.m_playerInput.ActivateInput();
        yield break;
    }
}
