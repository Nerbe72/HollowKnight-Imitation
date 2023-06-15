using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharmController : MonoBehaviour
{
    public static CharmController instance;

    //모든 부적
    public List<Charm> m_listCharms = new List<Charm>();
    public TemplateContainer m_charmTemplateContainer;
    
    //현재 장착한 부적
    public List<Charm> m_equippedCharms = new List<Charm>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
            return;
        }

        FillCharmList();
    }

    private void FillCharmList()
    {
        m_listCharms.AddRange(Resources.LoadAll<Charm>("Charms"));
    }

    public void SetVisualElement(TemplateContainer _tc)
    {
        m_charmTemplateContainer = _tc;
    }

    public void SetCharacterData(Charm s)
    {
        m_equippedCharms.Add(s);
    }
}
