using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTutorial : MonoBehaviour
{
    [SerializeField] GameObject m_text;

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
