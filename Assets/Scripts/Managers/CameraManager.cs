using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public Camera mainCamera;
    PlayerController player;

    public bool m_haveCamPos;
    public bool m_setCamPos;

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

    private Vector3 clampPos;

    private Coroutine co;
    private Vector3 m_originalCamPos;

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

        mainCamera = Camera.main;
    }

    void Start()
    {
        player = PlayerController.instance;
    }

    void Update()
    {
        CameraClamp();
    }

    private void FixedUpdate()
    {
        CameraMove();
    }

    public void SetCameraClamp(bool _xmin, bool _xmax, bool _ymin, bool _ymax)
    {
        XMaxClamp = _xmax;
        YMaxClamp = _ymax;
        XMinClamp = _xmin;
        YMinClamp = _ymin;
    }

    public void SetCameraClampSize(float _xmin, float _xmax, float _ymin, float _ymax)
    {
        xMax = _xmax;
        yMax = _ymax;
        xMin = _xmin;
        yMin = _ymin;
    }

    private void CameraMove()
    {
        //if (m_haveCamPos)
        //{
        //    if (m_setCamPos)
        //    {
        //        transform.position = GameObject.FindGameObjectWithTag("CamPos").transform.position;
        //        m_originalCamPos = transform.position;
        //        m_setCamPos = false;
        //    }
        //    return;
        //}

        transform.position = Vector3.Lerp(transform.position, player.transform.position + (Vector3.forward * -10), Time.deltaTime * 5);
    }

    private void CameraClamp()
    {
        clampPos = transform.position;

        if (XMaxClamp && XMinClamp)
            clampPos.x = Mathf.Clamp(clampPos.x, xMin, xMax);
        else if (XMaxClamp)
            clampPos.x = Mathf.Clamp(clampPos.x, clampPos.x, xMax);
        else if (XMinClamp)
            clampPos.x = Mathf.Clamp(clampPos.x, xMin, clampPos.x);

        if (YMaxClamp && YMinClamp)
            clampPos.y = Mathf.Clamp(clampPos.y, yMin, yMax);
        else if (YMaxClamp)
            clampPos.y = Mathf.Clamp(clampPos.y, clampPos.y, yMax);
        else if (YMinClamp)
            clampPos.y = Mathf.Clamp(clampPos.y, yMin, clampPos.y);

        transform.position = clampPos;
    }

    /// <summary>
    /// 화면 흔들림 코루틴
    /// </summary>
    /// <param name="magnitude">흔들림 세기(크기)</param>
    /// <param name="time">흔들림 시간</param>
    /// <param name="lerptime">흔들림 후 선형 보간 소요시간(time에서 lerptime만큼 제외됨)</param>
    /// <returns>코루틴</returns>
    public void ObjectShake(GameObject _target = null, float _magnitude = 0.03f, float _time = 0.3f, float _lerpTime = 0.04f)
    {
        if (_target == null)
            _target = gameObject;

        if (co != null)
            StopCoroutine(co);

        co = StartCoroutine(CameraShakeCo(_target, _magnitude, _time, _lerpTime));
    }

    private IEnumerator CameraShakeCo(GameObject _target, float _magnitude, float _time, float _lerpTime)
    {
        _time -= _lerpTime;
        m_originalCamPos = _target.transform.position;

        //카메라 흔들림
        while (_time > 0)
        {
            _time -= Time.deltaTime;

            _target.transform.position += new Vector3(Random.Range(-_magnitude, _magnitude),
                                                        Random.Range(-_magnitude, _magnitude), 0);
            yield return new WaitForEndOfFrame();
        }

        Vector3 latePos = _target.transform.position;
        float maxLerpTime = _lerpTime * 10;
        float currentLerpTime = 0;

        //카메라 위치 보간
        while (currentLerpTime < maxLerpTime)
        {
            if (_target.GetComponent<CameraManager>() != null)
                m_originalCamPos = player.transform.position + (Vector3.forward * -10);

            _target.transform.position = Vector3.Lerp(latePos, m_originalCamPos, currentLerpTime);
            currentLerpTime += Time.deltaTime * 10;
            yield return new WaitForEndOfFrame();
        }

        _target.transform.position = m_originalCamPos;
        co = null;
        yield break;
    }
}
