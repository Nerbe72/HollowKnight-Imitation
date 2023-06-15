using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;

    private PlayerStats m_playerStats;
    private PlayerInput m_uiInput;
    [SerializeField] private Sprite m_remainSprite;
    [SerializeField] private Sprite m_equipSprite;
    [SerializeField] private Sprite m_overloadSprite;
    [SerializeField] private Sprite m_charmBackgroundSprite;

    private StyleBackground m_CharmBackground;

    #region ELEMENT
    private VisualElement m_frameElement;
    private VisualTreeAsset m_listCharmEntryTemplate;
    private VisualTreeAsset m_listCostEntryTemplate;
    private UIDocument m_uiDocument;
    private VisualElement m_mainElement;

    private VisualElement m_infoImage;
    private Label m_infoName;
    private VisualElement m_infoCost;
    private Label m_infoDesc;

    private ScrollView m_scrollViewSlot;
    private ScrollView m_scrollViewCost;
    private ScrollView m_scrollViewCharm;
    private List<CharmVisual> m_charmsVisual = new List<CharmVisual>();
    #endregion

    private float m_lerpTime = 0;
    private int m_slotIndex = -1;

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
        m_uiDocument = GetComponent<UIDocument>();
        m_uiInput = GetComponent<PlayerInput>();
        m_CharmBackground = new StyleBackground(m_charmBackgroundSprite);
    }

    private void Start()
    {
        m_playerStats = PlayerStats.instance;
        EnumerateAllCharms();
        InitUI();

        StartCoroutine(InitUICo());
    }

    private void EnumerateAllCharms()
    {
        for (int i = 0; i < m_playerStats.Charms.Count; i++)
        {
            CharmVisual charmVisual = new CharmVisual(m_playerStats.Charms[i]);
            charmVisual.RegisterCallback<MouseOverEvent>(OnPointerOver);
            charmVisual.RegisterCallback<PointerDownEvent>(OnPointerDown);
            charmVisual.index = i;

            m_charmsVisual.Add(charmVisual);
        }
    }

    private void InitUI()
    {
        var root = m_uiDocument.rootVisualElement;
        m_frameElement = root.Q<VisualElement>("Frame");
        m_mainElement = root.Q<VisualElement>("Main");

        m_scrollViewSlot = root.Q<ScrollView>("SlotList");
        m_scrollViewCharm = root.Q<ScrollView>("CharmList");
        m_scrollViewCost = root.Q<ScrollView>("CostList");

        m_infoImage = root.Q<VisualElement>("InfoImage");
        m_infoName = root.Q<Label>("InfoName");
        m_infoCost = root.Q<VisualElement>("InfoCost");
        m_infoDesc = root.Q<Label>("InfoText");
    }

    private void InitFrames()
    {
        InitSlotFrame();
        InitCharmFrame();
        InitCostFrame();
    }

    private void InitSlotFrame()
    {
        for (int i = 0; i <= m_playerStats.CharmEquip.Count; i++)
        {
            AddSlotFrame();
        }
    }

    private void AddSlotFrame()
    {
        VisualElement backgroundVE = new VisualElement();
        backgroundVE.style.width = 80 * (Screen.currentResolution.width / 1920);
        backgroundVE.style.height = 80 * (Screen.currentResolution.width / 1920);
        backgroundVE.style.backgroundImage = m_CharmBackground;
        m_scrollViewSlot.Add(backgroundVE);
        m_slotIndex++;
    }

    private void InitCharmFrame()
    {
        for (int i = 0; i < m_playerStats.Charms.Count; i++)
        {
            //부적 총 갯수만큼 배경 추가
            VisualElement backgroundVE = new VisualElement();
            backgroundVE.style.width = 80 * (Screen.currentResolution.width / 1920);
            backgroundVE.style.height = 80 * (Screen.currentResolution.width / 1920);
            backgroundVE.style.backgroundImage = m_CharmBackground;
            m_scrollViewCharm.Add(backgroundVE);

            //최초 1회 소지한 부적만 등록
            if (m_playerStats.CharmHaveShop[m_playerStats.Charms[i].name][0])
            {
                m_scrollViewCharm[i].Add(m_charmsVisual[i]);
            }
        }
    }

    private void InitCostFrame()
    {
        for (int i = 0; i < m_playerStats.CharmSlotTotalCount + m_playerStats.CharmSlotOverloadCount + 4; i++)
        {
            AddCostFrame();
        }

        for (int i = m_playerStats.CharmSlotTotalCount; i < m_scrollViewCost.childCount; i++)
        {
            (m_scrollViewCost[i] as Image).sprite = null;
        }

        SetEquipCost();
    }

    private void AddCostFrame()
    {
        Image img = new Image();
        img.sprite = m_remainSprite;
        img.style.width = 30;
        img.style.height = 30;
        m_scrollViewCost.Add(img);
    }

    public void ReDrawCharmFrame()
    {
        for (int i = 0; i < m_playerStats.Charms.Count; i++)
        {
            //해당 부적을 가지고 있는지 확인
            if (m_playerStats.CharmHaveShop[m_playerStats.Charms[i].name][0])
            {
                //장착중이라면 slot으로 이동
                if (m_playerStats.CharmEquip.Contains(m_playerStats.Charms[i]))
                {
                    m_scrollViewSlot.Add(m_charmsVisual[i]);
                }
                //장착중이 아니라면 charm리스트로 이동
                else
                {
                    m_scrollViewCharm[i].Add(m_charmsVisual[i]);
                }
            }
        }
    }


    private void SetEquipCost()
    {
        for (int i = 0; i < m_playerStats.CharmSlotEquipCount; i++)
        {
            (m_scrollViewCost[i] as Image).sprite = m_equipSprite;
        }

        for (int i = m_playerStats.CharmSlotTotalCount; i < m_playerStats.CharmSlotTotalCount + m_playerStats.CharmSlotOverloadCount; i++)
        {
            (m_scrollViewCost[i] as Image).sprite = m_equipSprite;
            (m_scrollViewCost[i] as Image).tintColor = Color.yellow;
        }
    }

    private void UnSetEquipCost()
    {
        //장착한 코스트 이후 ~ 총 코스트까지의 이미지를 remain 이미지로 교체
        for (int i = m_playerStats.CharmSlotEquipCount; i < m_playerStats.CharmSlotTotalCount; i++)
        {
            (m_scrollViewCost[i] as Image).sprite = m_remainSprite;
        }

        for (int i = m_playerStats.CharmSlotTotalCount + m_playerStats.CharmSlotOverloadCount; i < m_scrollViewCost.childCount; i++)
        {
            (m_scrollViewCost[i] as Image).sprite = null;
            (m_scrollViewCost[i] as Image).tintColor = Color.white;
        }
    }

    private void SetOverloadCostList()
    {
        //m_scrollViewCost.Add();
        //and set overload animation sprite
        //and set take damage double
    }

    private void UnSetOverloadCostList()
    {

    }


    private void OnPointerOver(MouseOverEvent _evt)
    {
        if (_evt.button != 0) return;

        m_infoImage.Clear();

        Image infoImg = new Image();
        infoImg.sprite = (_evt.currentTarget as CharmVisual).charm.sprite;
        m_infoImage.Add(infoImg);

        m_infoName.text = (_evt.currentTarget as CharmVisual).charm.name;

        m_infoCost.Clear();
        for (int i = 0; i < (_evt.currentTarget as CharmVisual).charm.cost; i++)
        {
            Image infoCostImg = new Image();
            infoCostImg.sprite = m_equipSprite;
            infoCostImg.style.width = new StyleLength(Length.Percent(10f));
            infoCostImg.style.height = new StyleLength(Length.Percent(90f));
            m_infoCost.Add(infoCostImg);
        }

        m_infoDesc.text = (_evt.currentTarget as CharmVisual).charm.description;
    }

    private void OnPointerDown(PointerDownEvent _evt)
    {
        if (_evt.button != 0 || !m_playerStats.isCharmEditable) return;

        //장착 해제
        if (m_playerStats.CheckContainCharm((_evt.currentTarget as CharmVisual).charm))
        {
            m_playerStats.UnEquipCharm((_evt.currentTarget as CharmVisual).charm);
            StartCoroutine(MoveCharmCo((_evt.currentTarget as CharmVisual),
                (_evt.currentTarget as CharmVisual).parent,
                m_scrollViewCharm[(_evt.currentTarget as CharmVisual).index]));
            UnSetEquipCost();
        }
        //장착
        else
        {
            var status = m_playerStats.EquipCharm((_evt.currentTarget as CharmVisual).charm);
            switch (status)
            {
                case CostStatus.Success:
                case CostStatus.Overload:
                    StartCoroutine(MoveCharmCo((_evt.currentTarget as CharmVisual), (_evt.currentTarget as CharmVisual).parent,
                    m_scrollViewSlot[m_slotIndex]));
                    AddSlotFrame();
                    SetEquipCost();
                    break;
                case CostStatus.Fail:
                    //do nothing
                    break;
            }
        }
    }


    //Call by InputSystem
    private void OnOpenInventory(InputValue _value)
    {
        float input = _value.Get<float>();
        if (input == 1f)
        {
            if (m_frameElement.style.display == DisplayStyle.Flex)
            {
                m_frameElement.style.display = DisplayStyle.None;
                PlayerController.instance.m_playerInput.enabled = true;
                m_uiInput.actions["Arrow"].Disable();
            }
            else
            {
                m_frameElement.style.display = DisplayStyle.Flex;
                PlayerController.instance.m_playerInput.enabled = false;
                m_uiInput.actions["Arrow"].Enable();
            }
        }
    }


    //Coroutine
    private IEnumerator MoveCharmCo(CharmVisual _target, VisualElement _from, VisualElement _to)
    {
        Vector2 fromVector = m_mainElement.WorldToLocal(_from.worldBound.position) - (Vector2.one * 5);
        Vector2 toVector = m_mainElement.WorldToLocal(_to.worldBound.position) - (Vector2.one * 5);

        _from.Remove(_target);
        if (_from.parent.name == "SlotList")
        {
            _from.parent.Remove(_from);
            m_slotIndex--;
        }
            

        _target.style.top = fromVector.y;
        _target.style.left = fromVector.x;
        m_mainElement.Add(_target);

        while (true)
        {
            m_lerpTime += Time.deltaTime * 5;
            _target.style.top = Mathf.Lerp(fromVector.y, toVector.y, m_lerpTime);
            _target.style.left = Mathf.Lerp(fromVector.x, toVector.x, m_lerpTime);

            if (m_lerpTime >= 1f)
            {
                m_lerpTime = 0;
                m_mainElement.Remove(_target);
                _target.style.top = 0;
                _target.style.left = 0;
                _to.Add(_target);
                break;
            }
            yield return new WaitForNextFrameUnit();
        }
        yield break;
    }

    private IEnumerator InitUICo()
    {
        yield return new WaitForEndOfFrame();
        InitFrames();
    }
}
