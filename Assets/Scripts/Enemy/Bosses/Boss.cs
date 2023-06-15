using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Boss : MonoBehaviour
{
    #region COMPONENTS
    protected Animator m_animator;
    protected SpriteRenderer m_spriteRenderer;
    protected CircleCollider2D m_collider;
    protected Rigidbody2D m_rigidbody;
    #endregion


    #region UI
    [SerializeField] protected UIDocument m_nameUI;
    protected VisualElement m_visualElement;
    #endregion

    public int Damage;
    protected int m_maxHp;
    protected int m_currentHp;
    protected bool m_isDead = false;
    protected bool m_alreadyDead = false;
    protected bool m_meet = false;
    protected bool m_isWorking = false;
    protected bool m_isHurt = false;
    public bool m_isEncounter = false;

    private Coroutine co = null;
    private float m_recoverTime = 0.6f;

    protected void Awake()
    {
        BossSlayedCheck();
        InitComponents();
        InitUI();
    }

    public void TakeDamage(int damage)
    {
        if (co != null) return;

        StopCoroutine(HurtEffectCo());
        co = StartCoroutine(HurtEffectCo());

        m_currentHp = Mathf.Clamp(m_currentHp - damage, 0, m_maxHp);
        m_isHurt = true;

        if (m_currentHp <= 0)
        {
            m_isHurt = false;
            Dead();
        }
    }

    protected virtual void Dead()
    {
        Debug.Log("Dead");
        //Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        CameraManager.instance.m_haveCamPos = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (collider.CompareTag(GameTagMask.Tag(Tags.Player)))
        {
            PlayerController.instance.TakeDamage(Damage);
        }
    }

    //db에서 보스를 잡은적이 있는지 확인
    protected virtual void BossSlayedCheck()
    {
        //dead(bool)를 db에서 불러와 true일 경우 gameObject를 제거하여 보스전을 영구 제거
        if (PlayerPrefs.GetString(gameObject.name) == null)
            PlayerPrefs.SetString(gameObject.name, false.ToString());

        m_isDead = PlayerPrefs.GetString(gameObject.name) == "true" ? true : false;
    }

    protected virtual void InitComponents()
    {
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<CircleCollider2D>();
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    protected virtual void InitUI()
    {
        var root = m_nameUI.rootVisualElement;
        m_visualElement = root.Q<VisualElement>("Name");
    }

    private IEnumerator HurtEffectCo()
    {
        Color color = new Color(255, 59, 0);
        m_spriteRenderer.color = color;
        float recoverCheck = 0;

        while (true)
        {
            recoverCheck += Time.deltaTime / m_recoverTime;

            m_spriteRenderer.color = Color.Lerp(color, Color.white, recoverCheck);

            if (recoverCheck >= 1)
            {
                m_spriteRenderer.color = Color.white;
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        co = null;
        yield break;
    }
}
