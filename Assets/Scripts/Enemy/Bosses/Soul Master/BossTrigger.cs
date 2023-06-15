using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] Boss boss;
    [SerializeField] private GameObject m_bossCamera;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            boss.m_isEncounter = true;
            Vector2 pos = GameObject.FindWithTag("CamPos").transform.position;
            m_bossCamera.SetActive(true);
            Destroy(gameObject);
        }
    }
}
