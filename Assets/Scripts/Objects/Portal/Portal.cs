using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal")]
    [SerializeField] private MapConnection m_connection;
    [SerializeField] private string m_destinationScene;
    [SerializeField] private Transform m_spawnPosition;

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

    void Start()
    {
        if (m_connection == MapConnection.SelectedConnection)
        {
            var player = PlayerController.instance;
            SceneChangeManager.instance.currentMap = gameObject.scene.name;
            player.transform.position = m_spawnPosition.position;
            //점프 및 추락으로 이동할 경우 상태가 초기화
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            CameraManager.instance.transform.position = m_spawnPosition.position;
            CameraManager.instance.SetCameraClampSize(xMin, xMax, yMin, yMax);
            CameraManager.instance.SetCameraClamp(XMinClamp, XMaxClamp, YMinClamp, YMaxClamp);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            MapConnection.SelectedConnection = m_connection;
            SceneManager.LoadScene(m_destinationScene);
        }
    }
}
