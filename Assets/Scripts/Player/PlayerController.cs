using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
 * ��ó��
 * #if UNITY_EDITOR
 *          UnityEditor.EditorApplication.isPlaying = false;
 * #else
 *          Application.Quit();
 * #endif
 * */

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    #region ENUM
    private enum AnimationString
    {
        Vertical,
        Horizontal,
        Sliding,
        Attack,
        Dash,
        Jump,
        SecondJump,
        AttackDirection,
    }

    private enum AttackDirection
    {
        Side,
        Up,
        Down,
    }
    #endregion

    #region COMPONENTS
    [HideInInspector] public PlayerStats m_playerStats;
    [HideInInspector] public PlayerInput m_playerInput;
    private Rigidbody2D m_rigidbody;
    private Animator m_animator;
    private PlayerAttack m_attack;
    private GameObject m_attackRotation;
    private SpriteRenderer m_spriteRenderer;
    #endregion

    #region BOOL
    private bool m_isSit = false; //for sitting bench
    private bool m_isAttack = false;
    private bool m_isGround = true;
    private bool m_isCeiling = false;
    private bool m_isSliding = false;
    private bool m_isHurt = false;
    private bool m_doubleJump = true;
    private bool m_isSlideJumping = false;
    private bool m_pressJump = false;
    #endregion

    #region MOVE
    private float m_moveX;
    private float m_moveY;
    private float m_moveSpeed = 6f;
    private float m_jumpHeight = 15f;
    private bool m_isLeftSide = false;
    #endregion

    #region DASH
    private bool m_isDash = false;
    private bool m_isDashDelay = false;
    private float m_dashSpeed = 25f;
    private float m_dashTime = 0.128f;
    private float m_dashTimeCheck = 0f;
    #endregion

    #region HURT
    private float m_hurtCooldown = 0.8f;
    private float m_hurtCooldownTime = 0;
    Coroutine co;
    #endregion

    #region ACTIONS
    private Vector3 m_lookFlip = new Vector3(1f, 1f, 1f);
    private Vector2 m_attackAxis;
    private float m_slideJumpDelay;
    private AttackDirection m_attackDirection = AttackDirection.Side;
    #endregion

    #region SKILL
    private bool m_isHealing = false;
    private float m_skillChargeTime = 0.5f;
    private float m_skillChargeTimeCheck = 0f;
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_playerInput = GetComponent<PlayerInput>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        m_playerStats = PlayerStats.instance;
        m_attack = GetComponentInChildren<PlayerAttack>();
        m_attackRotation = transform.Find("AttackRotation").gameObject;
    }

    void Update()
    {
        AnimationController();
        GroundCheck();
        CeilingCheck();
        WallCheck();
        LookFlip();
        SetAttackDirection();
        HitObstacle();
        SkillCheck();
        Healing();
        SlidingJumpCheck();
        DashCheck();
        SetGravity();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag(GameTagMask.Tag(Tags.Obstacle)))
            TakeDamage(1);

        //������ �õ��ߴ� ��ġ�� �ǵ��ƿ�
        //�ش� ���� ������ ����
    }

    //�ִϸ����Ϳ� �� ����
    private void AnimationController()
    {
        m_animator.SetFloat(AnimationString.Horizontal.ToString(), Mathf.Abs(m_rigidbody.velocity.x));
        m_animator.SetFloat(AnimationString.Vertical.ToString(), m_rigidbody.velocity.y);
        m_animator.SetBool(AnimationString.Sliding.ToString(), m_isSliding);
        m_animator.SetInteger(AnimationString.AttackDirection.ToString(), (int)m_attackDirection);
    }

    //���� üũ
    private void GroundCheck()
    {
        m_isGround = false;
        if (m_moveY > 0f)
            return;
        m_isGround = (Physics2D.Raycast(transform.position, Vector2.down, 0.665f, LayerMask.GetMask(GameTagMask.Mask(Masks.Ground))));
        if (!m_doubleJump)
            m_doubleJump = m_isGround;
    }

    private void CeilingCheck()
    {
        m_isCeiling = (Physics2D.Raycast(transform.position, Vector2.up, 0.65f, LayerMask.GetMask(GameTagMask.Mask(Masks.Ground))));
    }

    //�� üũ

    private void WallCheck()
    {
        if (!m_isGround)
        {
            m_isSliding = false;
            if (m_moveY > 0f)
                return;

            m_isSliding = Physics2D.Raycast(transform.position,
                Vector2.right * LookSign(),
                0.4f,
                LayerMask.GetMask(GameTagMask.Mask(Masks.Slidable)));
        }
    }

    //�̵� ���⿡ ���� �÷��̾ �ٶ󺸴� ������ �ٲ�
    private void LookFlip()
    {
        if (m_lookFlip.x != -Mathf.Sign(m_moveX) && m_moveX != 0)
            m_lookFlip.x = -Mathf.Sign(m_moveX);
        transform.localScale = m_lookFlip;
    }

    //�߷� ����
    private void SetGravity()
    {
        if (!m_isGround)
        {
            m_moveY = Mathf.Clamp(m_moveY - 9.81f * 4f * Time.deltaTime, -12.5f, 15);

            if (m_moveY > 0f && m_isCeiling)
                m_moveY /= 2f;

            if (m_isSliding)
            {
                m_moveY /= 2f;
                
                //�����̵��� �����ϸ�
                if (m_pressJump)
                {
                    if (LookSign() < 0)
                        m_isLeftSide = true;
                    else
                        m_isLeftSide = false;

                    m_isSlideJumping = true;
                    m_pressJump = false;
                    m_moveY = m_jumpHeight;
                }
            } else
            {
                if (m_pressJump)
                {
                    m_animator.SetTrigger("SecondJump");
                    m_doubleJump = false;
                    m_pressJump = false;
                    m_moveY = m_jumpHeight;
                }
            }
        }
        else
        {
            if (m_pressJump)
            {
                m_pressJump = false;
                m_moveY = m_jumpHeight;
            } else
            {
                m_moveY = 0f;
            }
        }

        if (m_isSit)
            m_moveY = 0;

        m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, m_moveY);
    }

    //���ݹ��� ����
    private void SetAttackDirection()
    {
        if (m_attackAxis.y > 0)
        {
            m_attackDirection = AttackDirection.Up;
            m_attackRotation.transform.rotation = Quaternion.Euler(0f, 0f, 90f * LookSign());

        } else if (m_attackAxis.y < 0 && !m_isGround)
        {
            m_attackDirection = AttackDirection.Down;
            m_attackRotation.transform.rotation = Quaternion.Euler(0f, 0f, -90f * LookSign());
        } else if (m_attackAxis.y == 0)
        {
            m_attackDirection = AttackDirection.Side;
            m_attackRotation.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    //��ֹ� (�Ǵ� ��)�� �������� ��
    private void HitObstacle()
    {
        if (m_attack.m_isHit)
        {
            m_attack.m_isHit = false;
            switch (m_attackDirection)
            {
                case AttackDirection.Up:
                    m_moveY = -10f;
                    break;
                case AttackDirection.Down:
                    m_moveY = 12f;
                    break;
                case AttackDirection.Side:
                default:
                    m_moveX = -LookSign() * 5;
                    break;
            }

            //m_attackRotation�� localPosition�� ������� �ݴ� �������� �з������� ����
            //attackRotation�� x���� 90���� ��ȭ�ϹǷ� �밢������ �з��� ���ɼ��� ����
        }
    }

    private void SlidingJumpCheck()
    {
        if (m_isSlideJumping)
        {
            m_slideJumpDelay += Time.deltaTime;

            if (m_isSlideJumping)
            {
                if (m_isLeftSide)
                    m_moveX = m_moveY / 1.5f;
                else
                    m_moveX = m_moveY / -1.5f;
            }

            if (m_slideJumpDelay >= 0.2f)
            {
                m_isSlideJumping = false;
                m_isSlideJumping = false;
                m_slideJumpDelay = 0;
            }
        }
        else
            m_moveX = m_attackAxis.x;
    }

    private void DashCheck()
    {
        if (m_isDash)
        {
            m_dashTimeCheck += Time.deltaTime;

            m_animator.SetTrigger(AnimationString.Dash.ToString());
            FreezeY(m_isDash);

            m_moveX = m_dashSpeed * LookSign();

            if (m_dashTimeCheck >= m_dashTime)
            {
                m_isDash = false;
                m_dashTimeCheck = 0;
                FreezeY(false);
                m_animator.SetBool(AnimationString.Dash.ToString(), false);
                m_moveX = m_attackAxis.x;
                m_moveY /= 2f;
            }
        }
    }

    private void FreezeY(bool b)
    {
        if (b)
            m_rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        else
            m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Movement()
    {
        m_rigidbody.velocity = new Vector2(m_moveX, m_rigidbody.velocity.y);
    }

    private void SkillCheck()
    {
        if (m_playerInput.actions["Skill"].IsPressed())
        {
            m_skillChargeTimeCheck += Time.deltaTime;

            if (m_skillChargeTimeCheck >= m_skillChargeTime)
            {
                m_isHealing = true;
            } else
            {
                if (m_attackAxis.y > 0)
                {
                    //soul scream skill
                }
                else
                {
                    
                }
            }

        } else
        {
            m_skillChargeTimeCheck = 0;
            m_isHealing = false;
        }
    }

    private void Healing()
    {
        if (m_isHealing)
        {
            //slow moving
            m_moveX /= 3;
            m_moveY /= 3;
            //decrease energy

            //if no energy => m_isHealing = false
            
            //heal time check
            //if heal time over 1f => reset heal time check + heal 1hp
        }
    }

    public float LookSign()
    {
        return -transform.localScale.x;
    }

    public void TakeDamage(int damage)
    {
        if (!m_isHurt)
        {
            if (co != null)
                StopCoroutine(co);
            m_playerStats.CurrentHealth = Mathf.Clamp(m_playerStats.CurrentHealth - damage, 0, m_playerStats.MaxHealth);
            co = StartCoroutine(HurtDelay());
        }

        if (m_playerStats.CurrentHealth <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        //respawn
        transform.position = new Vector2(PlayerPrefs.GetFloat("SpawnX"), PlayerPrefs.GetFloat("SpawnY"));
        m_playerStats.CurrentHealth = m_playerStats.MaxHealth;

        SceneManager.LoadScene(PlayerPrefs.GetString("SpawnMap"));

    }

    public void SetSitToggle(bool _isSit)
    {
        m_isSit = _isSit;
        m_animator.SetBool("Sit", _isSit);
        m_playerStats.isCharmEditable = _isSit;
    }
    public bool GetSit()
    {
        return m_isSit;
    }

    //input system�� ���� �̺�Ʈ�� ȣ��Ǵ� �Լ���
    private void OnMove(InputValue _value)
    {
        Vector2 input = _value.Get<Vector2>();
        if (input != null)
        {
            //move
            m_moveX = (int)input.x * m_moveSpeed;

            //AttackAxis
            m_attackAxis.x = (int)input.x * m_moveSpeed;
            m_attackAxis.y = input.y;

            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Sit" &&
                m_animator.GetCurrentAnimatorStateInfo(0).length >= 0.7f)
                SetSitToggle(false);
        }
    }
    private void OnJump(InputValue _value)
    {
        float input = _value.Get<float>();

        if (input == 1f)
        {
            SetJump();
        }
        else
        {
            if (m_moveY > 0f)
                m_moveY /= 2f;
        }
    }
    private void OnAttack(InputValue _value)
    {
        float input = _value.Get<float>();
        if (input == 1f)
        {
            SetAttack();
        }
    }
    private void OnDash(InputValue _value)
    {
        float input = _value.Get<float>();
        if (input == 1f)
        {
            SetDash();
        }
    }
    //private void OnSkill(InputValue _value)
    //{
    //    float input = _value.Get<float>();
    //    if (input == 1f)
    //    {
    //        //skill timer on (not cooldown)
    //        //long press to heal
    //        // if not => 
    //    } else
    //    {
    //        //if timer >= xx => heal
    //        if (true)
    //        {
    //            //heal cancel
    //            //m_isHealing = false;
    //        }

    //        //if timer < xx
    //        if (true)
    //        {
    //            // use skill
    //            if (m_attackAxis.y > 0)
    //            {
    //                //soul scream skill
    //            }
    //            else
    //            {
    //                //look sign base skill
    //            }
    //        }
            
    //    }
    //}

    //���� ���� ����

    private void SetJump()
    {
        if (m_isGround)
        {
            m_pressJump = true;
        }
        else if (m_isSliding)
        {
            m_pressJump = true;
        }
        else if (!m_isGround && m_doubleJump)
        {
            m_pressJump = true;
        }
    }

    //���� ����
    private void SetAttack()
    {
        if (!m_isAttack)
        {
            m_animator.SetTrigger("Attack");

            //PlayerAttack ���ο��� �ִϸ��̼��� ���� collider2d�� on/off�ϴ� �Լ�
            m_attack.ChangeAttackCollider();
        }
    }

    //�뽬 ���� ����
    private void SetDash()
    {
        if (!m_isDashDelay)
        {
            m_animator.SetTrigger("Dash");
            StartCoroutine(DashDelay());
        }
    }

    //�ǰ� ������
    private IEnumerator HurtDelay()
    {
        m_isHurt = true;
        int blinkCount = 1;
        Color from = Color.black;
        Color to = Color.white;
        //ī�޶� ��鸲. ���� �÷��̾ �ٽ� �ջ��� ���� �� �ִ� ���°� �� �� ���� ĳ���Ͱ� ������
        m_animator.SetTrigger("Hurt");
        //GameManager.instance.TimeBasePause(0.2f);
        CameraManager.instance.ObjectShake();

        while (true)
        {
            m_hurtCooldownTime += Time.deltaTime;
            if (blinkCount % 2 == 0)
            {
                from = Color.black;
                to = Color.white;
            } else
            {
                from = Color.white;
                to = Color.black;
            }

            m_spriteRenderer.color = Color.Lerp(from, to, m_hurtCooldownTime / m_hurtCooldown / blinkCount * 6);

            if (m_hurtCooldownTime >= m_hurtCooldown * blinkCount / 6)
            {
                blinkCount++;
            }

            yield return new WaitForFixedUpdate();

            if (m_hurtCooldownTime >= m_hurtCooldown)
            {
                m_hurtCooldownTime = 0;
                blinkCount = 1;
                break;
            }
        }

        m_isHurt = false;
        yield break;
    }

    private IEnumerator DashDelay()
    {
        m_isDashDelay = true;
        m_isDash = true;
        yield return new WaitForSeconds(m_playerStats.DashCooldown);
        m_isDashDelay = false;
        yield break;
    }
}
