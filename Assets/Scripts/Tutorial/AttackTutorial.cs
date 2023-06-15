using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackTutorial : MonoBehaviour
{
    [SerializeField] private GameObject m_text;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;
        if (!collider.CompareTag(GameTagMask.Tag(Tags.Player))) return;

        m_text.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == null) return;
        if (!collider.CompareTag(GameTagMask.Tag(Tags.Player))) return;

        m_text.SetActive(false);
    }
}
