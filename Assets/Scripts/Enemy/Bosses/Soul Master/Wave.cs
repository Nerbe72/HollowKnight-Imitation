using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
{
    private bool m_moveLeft;
    private float m_waveSpeed = 0.03f;

    private Vector2 m_defaultPos;
    private float m_moveCheck;

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;
        if (!collider.CompareTag("Player")) return;

        collider.GetComponent<PlayerController>().TakeDamage(1);
    }

    private void Update()
    {
        m_moveCheck += Time.deltaTime * m_waveSpeed;

        if (m_moveLeft)
            transform.position = Vector3.Lerp(transform.position, transform.position + (Vector3.left * 15), m_moveCheck);
        else
            transform.position = Vector3.Lerp(transform.position, transform.position + (Vector3.right * 15), m_moveCheck);

        if (m_moveCheck >= 1)
        {
            Destroy(gameObject);
        }
    }

    public void InitPosDirection(Vector2 _pos, bool _isLeft)
    {
        transform.position = _pos;
        m_defaultPos = _pos;

        m_moveLeft = _isLeft;

        if (m_moveLeft)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = Vector3.one;
    }
}
