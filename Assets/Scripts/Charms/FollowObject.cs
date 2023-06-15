using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public static FollowObject instance;

    private PlayerController player;
    
    Vector3 preset;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player = PlayerController.instance;
        gameObject.SetActive(false);
    }

    void Update()
    {
        preset = new Vector3(-1.5f * player.LookSign(), 1.2f, 0);
        transform.position = Vector3.Lerp(transform.position, player.transform.position + preset, Time.deltaTime * 5);

        //정해진 위치(플레이어에서 일정 거리 떨어진 위치) 에 도달했을 때 플레이어가 바라보는 방향으로 flip
        if (Vector2.Distance(transform.position, player.transform.position + preset) <= 0.1f)
        {
            transform.localScale = new Vector3(player.LookSign(), 1f, 1f);
        }
    }

    public void DestroySelf()
    {
        Destroy(this);
    }
}
