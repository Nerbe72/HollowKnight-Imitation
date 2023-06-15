using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class SelectablePortal : MonoBehaviour
{
    private PlayerInput m_enterInput;

    [Header("Portal")]
    [SerializeField] private MapConnection m_connection;
    [SerializeField] private string m_destinationScene;
    [SerializeField] private Transform m_spawnPosition;

    [Header("Floating Text")]
    [SerializeField] string m_text;
    [SerializeField] TMP_Text m_tmp;
    [SerializeField] GameObject m_arrow;

    [Header("Set Camera Clamp")]
    [Header("X Clamp")]
    [SerializeField] private bool XMaxClamp;
    [SerializeField] private float xMax;
    [SerializeField] private bool XMinClamp;
    [SerializeField] private float xMin;

    [Header("Y Clamp")]
    [SerializeField] private bool YMaxClamp;
    [SerializeField] private float yMax;
    [SerializeField] private bool YMinClamp;
    [SerializeField] private float yMin;

    private void Awake()
    {
        m_enterInput = GetComponent<PlayerInput>();
        m_enterInput.DeactivateInput();
        m_arrow.SetActive(false);
        m_tmp.text = m_text;

        if (m_connection == MapConnection.SelectedConnection)
        {
            var player = PlayerController.instance;
            player.transform.position = m_spawnPosition.position;
            //점프 및 추락으로 이동할 경우 상태가 초기화
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            CameraManager.instance.transform.position = m_spawnPosition.position;
            CameraManager.instance.SetCameraClampSize(xMin, xMax, yMin, yMax);
            CameraManager.instance.SetCameraClamp(XMinClamp, XMaxClamp, YMinClamp, YMaxClamp);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            //텍스트 표시
            m_enterInput.ActivateInput();
            m_arrow.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            //텍스트 숨김
            m_enterInput.DeactivateInput();
            m_arrow.SetActive(false);
        }
    }

    private void OnInteraction(InputValue _value)
    {
        float input = _value.Get<float>();

        if (input == 1f)
        {
            MapConnection.SelectedConnection = m_connection;
            SceneManager.LoadScene(m_destinationScene);
        }
    }
}
