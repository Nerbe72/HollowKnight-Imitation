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
        //db���� �ҷ�����
        try
        {
            m_bindString = PlayerPrefs.GetString("Jump");
            //player
            //Input.actions["Jump"].ApplyBindingOverride(m_bindingString);
        }
        catch
        {
            //db�� ����� ������ ������ pass �н�
        }
        //m_playerInput.actions["Jump"].ApplyBindingOverride("DB���� �ҷ��� Ű");
    }

    public void SetKeyBind(string key, string bind)
    {
        PlayerPrefs.SetString(key, bind);
        //m_playerInput.actions[key].ApplyBindingOverride(bind);
    }
}
