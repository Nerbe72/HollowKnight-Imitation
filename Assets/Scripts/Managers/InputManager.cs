using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private string m_bindString;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitKeyBind();
        } else
            Destroy(gameObject);
    }

    void InitKeyBind()
    {
        //db에서 불러오기
        try
        {
            m_bindString = PlayerPrefs.GetString("Jump");
            //player
            //Input.actions["Jump"].ApplyBindingOverride(m_bindingString);
        }
        catch
        {
            //db에 저장된 데이터 없으면 pass 패스
        }
        //m_playerInput.actions["Jump"].ApplyBindingOverride("DB에서 불러온 키");
    }

    public void SetKeyBind(string key, string bind)
    {
        PlayerPrefs.SetString(key, bind);
        //m_playerInput.actions[key].ApplyBindingOverride(bind);
    }
}
