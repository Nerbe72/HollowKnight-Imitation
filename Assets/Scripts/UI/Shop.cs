using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Shop : MonoBehaviour
{
    private PlayerStats m_playerStats;

    [SerializeField] private VisualTreeAsset m_itemFrame;
    [Header("For Frame Animation")]
    [SerializeField] private Sprite[] m_topSprites;
    [SerializeField] private Sprite[] m_middleSprites;
    [SerializeField] private Sprite[] m_bottomSprites;


    private VisualElement m_itemImage;
    private Label m_itemCost;

    private UIDocument m_uiDocument;
    private VisualElement m_frame;

    private VisualElement m_topFrame;
    private VisualElement m_middleFrame;
    private VisualElement m_bottomFrame;

    private VisualElement m_mainFrame;
    private ScrollView m_itemScrollFrame;

    private VisualElement m_buyFrame;
    private VisualElement m_buyItem;

    private VisualElement m_afterBuy;
    private Label m_afterText;
    private Button m_afterBack;

    private List<CharmVisual> m_shopCharmVisuals = new List<CharmVisual>();

    private TemplateContainer m_currentCharmContainer;

    enum FramePos
    {
        Top,
        Middle,
        Bottom,
    }

    private (IVisualElementScheduledItem top, IVisualElementScheduledItem middle, IVisualElementScheduledItem bottom) m_frameTask;
    private (int top, int middle, int bottom) m_frameCount;
    private (int top, int middle, int bottom) m_frameCheck = (0, 0, 0);

    private void Awake()
    {
        InitCharmVisuals();
        Init();
        InitState();
    }

    public bool IsToggled()
    {
        if (m_frame.style.display == DisplayStyle.Flex)
            return true;
        else
            return false;
    }

    public void ToggleUI(bool _isTrigger)
    {
        m_frame.style.display = _isTrigger ? DisplayStyle.Flex : DisplayStyle.None;

        if (_isTrigger)
        {
            m_itemScrollFrame.style.display = DisplayStyle.Flex;
            m_buyFrame.style.display = DisplayStyle.None;
            m_afterBuy.style.display = DisplayStyle.None;
            m_frameTask.top = m_topFrame.schedule.Execute(() => { SwapSprites(FramePos.Top); }).Every(50);
            m_frameTask.middle = m_middleFrame.schedule.Execute(() => { SwapSprites(FramePos.Middle); }).Every(60);
            m_frameTask.bottom = m_bottomFrame.schedule.Execute(() => { SwapSprites(FramePos.Bottom); }).Every(70);
        }
    }

    private void InitCharmVisuals()
    {
        m_playerStats = PlayerStats.instance;
        int count = m_playerStats.Charms.Count;

        for (int i = 0; i < count; i++)
        {
            //부적 리스트에서 판매중 값이 true 인 부적을 가져옴
            if (m_playerStats.CharmHaveShop[m_playerStats.Charms[i].name][1])
            {
                m_shopCharmVisuals.Add(new CharmVisual(m_playerStats.Charms[i]));
            }
        }
    }

    private void Init()
    {
        m_uiDocument = GetComponent<UIDocument>();
        var root = m_uiDocument.rootVisualElement;
        m_frame = root.Q<VisualElement>("Frame");
        m_mainFrame = root.Q<VisualElement>("Main");
        m_itemScrollFrame = root.Q<ScrollView>("Items");
        m_buyFrame = root.Q<VisualElement>("Buy");
        m_afterBuy = root.Q<VisualElement>("AS");
        m_afterText = root.Q<Label>("success");
        m_afterBack = root.Q<Button>("Back");

        m_topFrame = root.Q<VisualElement>("top");
        m_middleFrame = root.Q<VisualElement>("middle");
        m_bottomFrame = root.Q<VisualElement>("bottom");
    }

    private void InitState()
    {
        m_frame.style.display = DisplayStyle.None;
        m_itemScrollFrame.style.display = DisplayStyle.Flex;
        m_buyFrame.style.display = DisplayStyle.None;
        m_afterBuy.style.display = DisplayStyle.None;

        m_afterBack.clicked += ClickBack;

        int count = m_shopCharmVisuals.Count;

        for (int i = 0; i < count; i++)
        {
            //해당하는 부적에 맞게 이미지와 가격을 설정
            var itemRoot = m_itemFrame.CloneTree();
            m_itemImage = itemRoot.Q<VisualElement>("Image");
            m_itemCost = itemRoot.Q<Label>("Cost");
            m_shopCharmVisuals[i].style.width = new StyleLength(Length.Percent(100f));
            m_shopCharmVisuals[i].style.height = new StyleLength(Length.Percent(100f));
            m_itemImage.Add(m_shopCharmVisuals[i]);
            m_itemCost.text = m_shopCharmVisuals[i].charm.price.ToString();

            //add click event
            itemRoot.style.width = new StyleLength(Length.Percent(100f));
            itemRoot.style.height = new StyleLength(Length.Percent(100f));
            itemRoot.RegisterCallback<PointerDownEvent>(BuyItem);
            m_itemScrollFrame.Add(itemRoot);
        }

        m_frameCount.top = m_topSprites.Length;
        m_frameCount.middle = m_middleSprites.Length;
        m_frameCount.bottom = m_bottomSprites.Length;
    }

    private void BuyItem(PointerDownEvent _evt)
    {
        /*
         * 이걸 구매창으로 넘기고
         * 구매 완료시
         * charmhaveshop[name][0] = true; //소지
         * charmhaveshop[name][1] = false; //상점 Stock
         */

        m_buyFrame.style.display = DisplayStyle.Flex;
        m_mainFrame.style.display = DisplayStyle.None;

        m_currentCharmContainer = (_evt.currentTarget as TemplateContainer);

        m_buyFrame.Q<Label>("BuyName").text = ((_evt.currentTarget as TemplateContainer).ElementAt(0)[0][0][0] as CharmVisual).charm.name;
        m_buyItem = m_buyFrame.Q<VisualElement>("BuyImage");

        //아이템이 이미 존재하면 해당 아이템을 제거하고 넣음 - 이미지가 중복해서 표시되는 문제 해결
        if (m_buyItem.childCount >= 1)
        {
            m_buyItem.Clear();
        }

        m_buyItem.Add(((_evt.currentTarget as TemplateContainer).ElementAt(0)[0][0][0] as CharmVisual).CloneElement());
        m_buyItem[0].style.width = new StyleLength(Length.Percent(100));
        m_buyItem[0].style.height = new StyleLength(Length.Percent(100));

        m_buyFrame.Q<Label>("BuyPrice").text = (m_buyItem[0] as CharmVisual).charm.price.ToString();

        m_buyFrame.Q<Button>("Yes").RegisterCallback<ClickEvent>(ClickYes);
        m_buyFrame.Q<Button>("No").RegisterCallback<ClickEvent>(ClickNo);
    }

    private void ClickYes(ClickEvent _click)
    {
        if (PlayerStats.instance.Geo >= (m_currentCharmContainer[0][0][0][0] as CharmVisual).charm.price)
        {
            PlayerStats.instance.CharmHaveShop[(m_buyItem[0] as CharmVisual).name][0] = true;
            PlayerStats.instance.CharmHaveShop[(m_buyItem[0] as CharmVisual).name][1] = false;
            PlayerStats.instance.Geo -= (m_currentCharmContainer[0][0][0][0] as CharmVisual).charm.price;
            InventoryUI.instance.ReDrawCharmFrame();
            m_itemScrollFrame.Remove(m_currentCharmContainer);

            //표시 변경
            m_buyFrame.style.display = DisplayStyle.None;
            m_mainFrame.style.display = DisplayStyle.None;
            m_afterBuy.style.display = DisplayStyle.Flex;
            m_afterText.text = "구매를 완료했습니다";
        }
        else
        {
            m_buyFrame.style.display = DisplayStyle.None;
            m_mainFrame.style.display = DisplayStyle.None;
            m_afterBuy.style.display = DisplayStyle.Flex;
            m_afterText.text = "잔액이 부족합니다";
        }
    }

    private void ClickBack()
    {
        m_buyFrame.style.display = DisplayStyle.None;
        m_mainFrame.style.display = DisplayStyle.Flex;
        m_afterBuy.style.display = DisplayStyle.None;
    }

    private void ClickNo(ClickEvent _click)
    {
        m_buyFrame.style.display = DisplayStyle.None;
        m_mainFrame.style.display = DisplayStyle.Flex;
        m_afterBuy.style.display = DisplayStyle.None;
    }

    private void SwapSprites(FramePos _pos)
    {
        switch (_pos)
        {
            case FramePos.Top:
                if (m_frameCheck.top >= m_frameCount.top)
                {
                    m_frameCheck.top = 0;
                    m_frameTask.top.Pause();
                    return;
                }
                m_topFrame.style.backgroundImage = new StyleBackground(m_topSprites[m_frameCheck.top]);
                m_frameCheck.top++;
                break;
            case FramePos.Middle:
                if (m_frameCheck.middle >= m_frameCount.middle)
                {
                    m_frameCheck.middle = 0;
                    m_frameTask.middle.Pause();
                    return;
                }
                m_middleFrame.style.backgroundImage = new StyleBackground(m_middleSprites[m_frameCheck.middle]);
                m_frameCheck.middle++;
                break;
            case FramePos.Bottom:
                if (m_frameCheck.bottom >= m_frameCount.bottom)
                {
                    m_frameCheck.bottom = 0;
                    m_frameTask.bottom.Pause();
                    return;
                }
                m_bottomFrame.style.backgroundImage = new StyleBackground(m_bottomSprites[m_frameCheck.bottom]);
                m_frameCheck.bottom++;
                break;
        }
    }

}
