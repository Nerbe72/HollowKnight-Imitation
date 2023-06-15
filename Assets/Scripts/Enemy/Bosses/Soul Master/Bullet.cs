using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D m_rigidbody;

    private bool m_isHoming;
    PlayerController m_player; 
    private int m_damage;
    private float m_time;
    private bool m_isUsers = false;
    private bool m_isLeft = false;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        m_player = PlayerController.instance;
    }

    private void FixedUpdate()
    {
        if (m_isHoming)
        {
            Vector2 direction = (transform.position - m_player.transform.position).normalized;
            float value;
            if (m_isLeft)
            {
                value = Vector3.Cross(direction, transform.right).z;
                m_rigidbody.angularVelocity = 300f * value;
                m_rigidbody.velocity = transform.right * 6f;

            }
            else
            {
                value = Vector3.Cross(direction, -transform.right).z;
                m_rigidbody.angularVelocity = 300f * value;
                m_rigidbody.velocity = -transform.right * 6f;
            }

        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Ground))) Destroy(gameObject);
        if (collider.CompareTag(GameTagMask.Tag(Tags.Slidable))) Destroy(gameObject);

        if (collider.name.Equals("Attack"))
        {
            //공격 방향에 따라 튕겨내기
            //다가오는 방향 + 공격 방향의 중간 방향 +- random.range 방향으로 나아감
        }

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            collider.GetComponent<PlayerController>().TakeDamage(m_damage);
            Destroy(gameObject);
        }
    }

    public void HomingBullet(int _damage, bool _isLeft)
    {
        m_isHoming = true;
        m_damage = _damage;
        m_isLeft = _isLeft;
        //StartCoroutine(HomingCo());
    }

    public void FollowBullet(Vector2 _destination, float _angle)
    {
        m_isHoming = false;
    }
}
