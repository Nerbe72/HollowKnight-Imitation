using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Shop : MonoBehaviour
{
    private PlayerStats m_playerStats;

    [SerializeField] private VisualTreeAsset m_itemFrame;
    private VisualElement m_itemImage;
    private Label m_itemCost;

    private UIDocument m_uiDocument;
    private VisualElement m_frame;
    private VisualElement m_mainFrame;
    private ScrollView m_itemScrollFrame;

    private VisualElement m_buyFrame;
    private VisualElement m_buyItem;

    private VisualElement m_afterBuy;
    private Label m_afterText;
    private Button m_afterBack;

    private List<CharmVisual> m_shopCharmVisuals = new List<CharmVisual>();

    private TemplateContainer m_currentCharmContainer;

    private void Awake()
    {
        m_uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        StartCoroutine(InitCo());
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
        }
    }

    private void InitCharmVisuals()
    {
        m_playerStats = PlayerStats.instance;
        int count = m_playerStats.Charms.Count;

        for (int i = 0; i < count; i++)
        {
            //���� ����Ʈ���� �Ǹ��� ���� true �� ������ ������
            if (m_playerStats.CharmHaveShop[m_playerStats.Charms[i].name][1])
            {
                m_shopCharmVisuals.Add(new CharmVisual(m_playerStats.Charms[i]));
            }
        }
    }

    private void InitUI()
    {
        var root = m_uiDocument.rootVisualElement;
        m_frame = root.Q<VisualElement>("Frame");
        m_mainFrame = root.Q<VisualElement>("Main");
        m_itemScrollFrame = root.Q<ScrollView>("Items");
        m_buyFrame = root.Q<VisualElement>("Buy");
        m_afterBuy = root.Q<VisualElement>("AS");
        m_afterText = root.Q<Label>("success");
        m_afterBack = root.Q<Button>("Back");

        //ui �ּ� ǥ�� ���� �ʱ�ȭ
        m_frame.style.display = DisplayStyle.None;
        m_itemScrollFrame.style.display = DisplayStyle.Flex;
        m_buyFrame.style.display = DisplayStyle.None;
        m_afterBuy.style.display = DisplayStyle.None;

        m_afterBack.clicked += ClickBack;
        
        int count = m_shopCharmVisuals.Count;

        for (int i = 0; i < count; i++)
        {
            //�ش��ϴ� ������ �°� �̹����� ������ ����
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
    }

    private void BuyItem(PointerDownEvent _evt)
    {
        /*
         * �̰� ����â���� �ѱ��
         * ���� �Ϸ��
         * charmhaveshop[name][0] = true; //����
         * charmhaveshop[name][1] = false; //���� Stock
         */

        m_buyFrame.style.display = DisplayStyle.Flex;
        m_mainFrame.style.display = DisplayStyle.None;

        m_currentCharmContainer = (_evt.currentTarget as TemplateContainer);

        m_buyFrame.Q<Label>("BuyName").text = ((_evt.currentTarget as TemplateContainer).ElementAt(0)[0][0][0] as CharmVisual).charm.name;
        m_buyItem = m_buyFrame.Q<VisualElement>("BuyImage");

        //�������� �̹� �����ϸ� �ش� �������� �����ϰ� ���� - �̹����� �ߺ��ؼ� ǥ�õǴ� ���� �ذ�
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

            //ǥ�� ����
            m_buyFrame.style.display = DisplayStyle.None;
            m_mainFrame.style.display = DisplayStyle.None;
            m_afterBuy.style.display = DisplayStyle.Flex;
            m_afterText.text = "���Ÿ� �Ϸ��߽��ϴ�";
        }
        else
        {
            m_buyFrame.style.display = DisplayStyle.None;
            m_mainFrame.style.display = DisplayStyle.None;
            m_afterBuy.style.display = DisplayStyle.Flex;
            m_afterText.text = "�ܾ��� �����մϴ�";
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

    private IEnumerator InitCo()
    {
        yield return new WaitForEndOfFrame();
        InitCharmVisuals();
        InitUI();
        yield break;
    }
}
