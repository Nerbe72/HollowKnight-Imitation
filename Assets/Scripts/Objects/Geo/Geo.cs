using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geo : MonoBehaviour
{
    private Animator m_animator;
    private Rigidbody2D m_rigidbody;

    private int m_geoCost = 1;

    private int bounceCount = 2;
    private float bounce = 0.6f;
    private Vector2 maxVelocity = Vector2.zero;

    [SerializeField] private RuntimeAnimatorController[] m_coinAnimators;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (maxVelocity.x > m_rigidbody.velocity.x)
        {
            maxVelocity.x = m_rigidbody.velocity.x;
        }
        if (maxVelocity.y > m_rigidbody.velocity.y)
        {
            maxVelocity.y = m_rigidbody.velocity.y;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider == null) return;

        if (collision.collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            PlayerStats.instance.Geo += m_geoCost;
            Destroy(gameObject);
        }

        if (collision.collider.CompareTag(GameTagMask.Tag(Tags.Ground)))
        {
            if (bounceCount-- > 0)
            {
                maxVelocity *= -1 * bounce;
                m_rigidbody.velocity = maxVelocity;
            }
        }
    }

    public void InitGeo(Vector2 _pos, GeoAmount _price = GeoAmount.One)
    {
        transform.position = _pos;
        m_rigidbody.velocity = new Vector2(Random.Range(-4f, 4f), 9f);
        m_geoCost = (int)_price;

        switch (_price)
        {
            case GeoAmount.One:
                m_animator.runtimeAnimatorController = m_coinAnimators[0];
                break;
            case GeoAmount.Silver:
                m_animator.runtimeAnimatorController = m_coinAnimators[1];
                break;
            case GeoAmount.Gold:
                m_animator.runtimeAnimatorController = m_coinAnimators[2];
                break;
        }
    }
}
