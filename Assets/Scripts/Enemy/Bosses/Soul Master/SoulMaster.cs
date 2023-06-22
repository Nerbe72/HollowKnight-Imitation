using Mono.Cecil.Cil;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SoulMaster : Boss
{
    private enum Pattern
    {
        A,
        B,
        C,
        D,
        Count
    }
    private enum Warp
    {
        Start,
        End,
    }

    private PlayerController m_player;
    [SerializeField] private ParticleController m_particle;

    #region POS
    [Header("Position")]
    [SerializeField] private GameObject m_camPos;
    [SerializeField] private Transform m_leftPos;
    [SerializeField] private Transform m_rightPos;
    [SerializeField] private Transform m_leftBottomPos;
    [SerializeField] private Transform m_rightBottomPos;
    [SerializeField] private Transform m_topPos;
    [SerializeField] private Transform m_bottomPos;
    [SerializeField] private Transform m_wavePos;
    [SerializeField] private Transform m_bulletFireStartPos;
    #endregion

    #region Dead
    [Header("Destroy Dead")]
    [SerializeField] private GameObject m_disableCinema;
    [SerializeField] private GameObject m_breakableWall;
    [SerializeField] private GameObject m_breakableCeiling;
    [SerializeField] private GameObject m_text;

    #endregion

    #region PATTERN
    private Pattern m_pattern;
    private bool m_halfHp = true;
    private bool m_firstMet = true;
    private bool m_isScreaming = true;

    [Header("Flying Object")]
    [SerializeField] private GameObject m_bulletPrefab;

    //���� ������
    protected float m_patternCooldownMoveSpeed = 3f;
    protected float m_patternCooldownCheck = 0f;
    private float m_cooldownMultiply = 1f;

    private float m_patternDelayTime = 1.2f;
    private float m_patternDelayCheck = 0f;

    //�ִϸ��̼� üũ
    private bool m_isShaking = true;
    private bool m_isWarping = false;
    private bool m_isWarpStart = true;
    private bool m_isWarpEnd = true;

    //���� ��ġ ����
    private bool m_isSettingPos = true;
    private Vector2 m_startPos;
    private Vector2 m_beforePos;

    //���� ���Խ� 1ȸ
    private bool m_enter = true;

    //����ü 3ȸ �߻� Ƚ��
    private bool m_isLeft = false;
    private int m_aCount = 3;
    private int m_aCountCheck = 0;
    private float m_aWaitTime = 2f;
    private float m_aWaitCheck = 0f;

    //�̵�
    private float m_bMoveSpeed = 1f;
    private float m_bMoveCheck = 0f;
    private Vector2 m_endPos;

    //���� / ����ĵ��
    private bool m_dEnter = true;
    [Header("Shock Wave")]
    [SerializeField] private GameObject m_wavePrefab;
    private bool m_isGround = false;

    //���
    private float m_eWaitTime = 1.5f;
    private float m_eWaitCheck = 0f;
    #endregion
    
    void Start()
    {
        SetBossStats();

        m_player = PlayerController.instance;
        CameraManager.instance.m_haveCamPos = true;
        CameraManager.instance.m_setCamPos = true;
        SetFreezeY(true);

        if (m_isDead)
        {
            m_animator.speed = 4f;
            m_alreadyDead = true;
            Dead();
        }
        else
        {
            m_animator.speed = 1f;
        }
    }

    private void Update()
    {
        //����� ���
        if (m_isDead) return;

        //�÷��̾ �ٴ��� ������ �ൿ�� ���۵�
        if (!m_isEncounter) return;

        //����� ��ȿ & �̸� ǥ��
        if (m_firstMet)
        {
            StartCoroutine(InvisibleNameCo());
            m_animator.SetTrigger("Scream");
            PlayerController.instance.m_playerInput.DeactivateInput();
            m_firstMet = false;
        }

        //ī�޶� ����
        if (m_isShaking && m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("SoulMaster Scream") &&
               (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.25f))
        {
            CameraManager.instance.ObjectShake(m_camPos, 0.05f, 0.5f, 0.04f);
            m_isShaking = false;
        }

        //�ִϸ��̼� ���� �� ĳ���� �Է� ����ȭ
        if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("SoulMaster Scream") &&
               (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.94f))
        {
            PlayerController.instance.m_playerInput.ActivateInput();
            m_isScreaming = false;
        }

        if (m_isScreaming) return;

        //�������� üũ�Ͽ� ���� ������ �̻��� �ӵ��� ���� �� �ൿ�� �����ϵ��� ����
        //ù �ִϸ��̼� �� �̵��� ����� ������ ����
        //�� �̵��� �߶��ϴ� �ð����� ��ü
        //if (!isStateGameplaying())
        //{
        //    return;
        //}

        HpCheck();
        GroundCheck();
        PlayPattern();
        AnimationSet();
    }

    private void SetBossStats()
    {
        m_maxHp = 5;
        m_currentHp = m_maxHp;
        m_patternCooldownMoveSpeed = 3;
    }

    //private bool isStateGameplaying()
    //{
    //    if (Time.deltaTime > 0.1)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        return true;
    //    }
    //}

    //����� ���� õ���� ����

    protected override void Dead()
    {
        m_isDead = true;
        m_collider.enabled = false;
        PlayerPrefs.SetString(gameObject.name, false.ToString());
        m_animator.SetBool("Dead", true);
        m_player.m_playerInput.DeactivateInput();
    }

    /*�ִϸ��̼� ��Ʈ�ѷ��� ���� ����*/
    private void DropGround()
    {
        StartCoroutine(FallingBossCo());
    }
    private void End()
    {
        Destroy(m_breakableCeiling);
        m_disableCinema.SetActive(false);
        m_player.m_playerInput.ActivateInput();
        PlayerPrefs.SetString(gameObject.name, "true");
        m_text.SetActive(true);
    }
    /*///////////////////////////////*/

    private void HpCheck()
    {
        if (m_halfHp && m_currentHp <= m_maxHp * 0.5f)
        {
            m_patternCooldownMoveSpeed *= 1.2f;
            m_cooldownMultiply = 2f;

            m_animator.speed = 1.2f;
            m_halfHp = false;
        }
    }

    private void GroundCheck()
    {
        m_isGround = (Physics2D.Raycast(transform.position, Vector2.down, 0.55f, LayerMask.GetMask(GameTagMask.Mask(Masks.Ground)))); 
    }

    private void AnimationSet()
    {
        m_animator.SetFloat("Vertical", m_rigidbody.velocity.y);
        m_animator.SetBool("Ground", m_isGround);
    }

    private void SetWarp(Warp _isVisible)
    {
        if (_isVisible == Warp.End)
        {
            m_animator.SetTrigger("WarpEnd");
            m_spriteRenderer.enabled = true;
        }
        else
        {
            m_animator.SetTrigger("WarpStart");
            m_collider.enabled = false;
        }
        
    }

    private void PlayPattern()
    {
        if (!m_isWorking)
        {
            m_patternDelayCheck += Time.deltaTime;

            if (m_patternDelayCheck <= m_patternDelayTime) return;

            #region WarpStart
            if (!m_isWarping)
            {
                SetWarp(Warp.Start);
                m_isWarping = true;
            }

            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("SoulMaster Warp Start") &&
               (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.94f))
            {
                m_isWarpStart = false;
                m_spriteRenderer.enabled = false;
                m_particle.PlayParticle();
            }

            if (m_isWarpStart) return;
            #endregion
            if (m_isSettingPos)
            {
                m_beforePos = transform.position;

                //���� ���� ����
                m_pattern = (Pattern)Random.Range(0, (int)Pattern.Count);

                //�������� (for debug)
                //m_pattern = Pattern.B;

                SetPatternPos();

                m_enter = true;
                m_dEnter = true;
                m_isSettingPos = false;
            }

            m_patternCooldownCheck += Time.deltaTime * m_patternCooldownMoveSpeed * m_cooldownMultiply;

            transform.position = Vector2.Lerp(m_beforePos, m_startPos, m_patternCooldownCheck);

            if (m_patternCooldownCheck >= 1)
            {
                m_patternCooldownCheck = 0;
                m_patternDelayCheck = 0;
                m_isWorking = true;
                m_isWarping = false;
            }
            return;
        }

        #region WarpEnd
        if (m_enter)
        {
            SetWarp(Warp.End);
            m_enter = false;
        }

        if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("SoulMaster Warp End") &&
           (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.94f))
        {
            m_isWarpEnd = false;
            m_collider.enabled = true;
            m_particle.StopParticle();
        }

        if (m_isWarpEnd) return;
        #endregion

        switch (m_pattern)
        {
            case Pattern.A:
                PatternA();
                break;
            case Pattern.B:
                PatternB();
                break;
            case Pattern.C:
                PatternC();
                break;
            case Pattern.D:
                PatternD();
                break;
        }
    }

    private void SetPatternPos()
    {
        switch (m_pattern)
        {
            case Pattern.A:
                //�÷��̾�� Ÿ�� ������ ������ �Ÿ��� ����� ��
                //�÷��̾���� �Ÿ��� �� �հ����� �����̵�
                int range = (
                    Vector2.Distance(PlayerController.instance.transform.position, m_leftPos.position) >=
                    Vector2.Distance(PlayerController.instance.transform.position, m_rightPos.position) ? 0 : 1);
                switch (range)
                {
                    case 0:
                        m_startPos = m_leftPos.position;
                        transform.localScale = new Vector3(1, 1, 1);
                        m_isLeft = true;
                        break;
                    case 1:
                    default:
                        m_startPos = m_rightPos.position;
                        transform.localScale = new Vector3(-1, 1, 1);
                        m_isLeft = false;
                        break;
                }
                break;
            case Pattern.B:
                int rangeB = (
                    Vector2.Distance(PlayerController.instance.transform.position, m_leftBottomPos.position) >=
                    Vector2.Distance(PlayerController.instance.transform.position, m_rightBottomPos.position) ? 0 : 1);
                switch (rangeB)
                {
                    case 0:
                        m_startPos = m_leftBottomPos.position;
                        m_endPos = m_rightBottomPos.position;
                        transform.localScale = new Vector3(1, 1, 1);
                        break;
                    case 1:
                    default:
                        m_startPos = m_rightBottomPos.position;
                        m_endPos = m_leftBottomPos.position;
                        transform.localScale = new Vector3(-1, 1, 1);
                        break;
                }
                break;
            case Pattern.C:
                m_startPos = new Vector3(m_player.transform.position.x, m_topPos.position.y, 0);
                break;
            case Pattern.D:
                m_startPos = m_bottomPos.position;
                break;
        }
    }

    //���Ͽ� �ش�Ǵ� ��ġ�� �����̵�
    //�ش� ��ġ���� ����� ���߰� ����
    //���� ���� ������ ��ġ �ʱ�ȭ

    private void PatternA()
    {
        //A �÷��̾�� �����Ǵ� ����ü �߻�
        m_aWaitCheck += Time.deltaTime;
        
        // if animation time >= 0.8 fire once
        
        if (m_aWaitCheck >= m_aWaitTime)
        {
            //fire
            GameObject obj = Instantiate(m_bulletPrefab);
            obj.transform.position = m_bulletFireStartPos.position;
            obj.GetComponent<Bullet>().HomingBullet(Damage, m_isLeft);

            m_aWaitCheck = 0;
            m_aCountCheck++;
        }

        if (m_aCountCheck >= m_aCount)
        {
            m_aCountCheck = 0;
            InitPatternStatus();
        }
    }

    private void PatternB()
    {
        //B ���� ������ ������ ����
        m_bMoveCheck += Time.deltaTime * m_bMoveSpeed;

        transform.position = Vector2.Lerp(m_startPos, m_endPos, m_bMoveCheck);
        //add move animation

        if (m_bMoveCheck >= 1)
        {
            m_bMoveCheck = 0;
            InitPatternStatus();
        }
    }

    private void PatternC()
    {
        if (m_dEnter)
        {
            SetFreezeY(false);
            m_rigidbody.velocity = new Vector2(0, 9);
            m_dEnter = false;
        }

        //phase 1 ����
        //phase 2 ���� ���� ���� �ߴ� �� ����

        if (m_isGround)
        {
            if (m_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("SoulMaster Ground") &&
                (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.94f))
                return;

            CameraManager.instance.ObjectShake(m_disableCinema, 0.03f, 0.15f, 0.05f);

            //create shockwave : true/false means _isLeft
            CreateWave(true);
            CreateWave(false);

            SetFreezeY(true);
            m_patternDelayCheck = m_patternDelayTime;
            InitPatternStatus();
        }
    }

    private void PatternD()
    {
        

        // �߾ӿ��� �޽�

        //�޽��� �ǰݽ� ��� ���� ����
        if (m_isHurt)
        {
            m_eWaitCheck += m_eWaitTime;
            m_isHurt = false;
        }

        m_eWaitCheck += Time.deltaTime;


        if (m_eWaitCheck >= m_eWaitTime)
        {
            InitPatternStatus();
        }
    }

    private void InitPatternStatus()
    {
        m_isWorking = false;
        m_isWarpStart = true;
        m_isWarpEnd = true;
        m_isSettingPos = true;
    }

    private void SetFreezeY(bool _freeze)
    {
        if (_freeze)
        {
            m_rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            m_rigidbody.velocity = Vector2.zero;
        }
        else
            m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    private void CreateWave(bool _isLeft)
    {
        GameObject wave = Instantiate(m_wavePrefab);
        Wave waveController = wave.GetComponent<Wave>();
        waveController.InitPosDirection(new Vector2(transform.position.x, m_wavePos.position.y), _isLeft);
    }

    private IEnumerator InvisibleNameCo()
    {
        m_visualElement.style.display = DisplayStyle.Flex;
        m_visualElement.AddToClassList("name");
        float time = 0;

        while (true)
        {
            time += Time.deltaTime * 0.5f;
            //m_visualElement.style.opacity = new StyleFloat(Mathf.Lerp(100f, 0f, time));
            //Debug.Log(m_visualElement.style.opacity);

            if (time >= 1)
            {
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        m_visualElement.RemoveFromClassList("name");
        m_visualElement.style.display = DisplayStyle.None;
        yield break;
    }

    private IEnumerator FallingBossCo()
    {
        float time = 0;
        Vector2 originalPos = transform.position;

        if (m_alreadyDead) time += 1f;

        while (true)
        {
            time += Time.deltaTime;

            transform.position = Vector2.Lerp(originalPos, new Vector2(originalPos.x, -31.5f), time);

            if (time >= 1f) break;

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
}
