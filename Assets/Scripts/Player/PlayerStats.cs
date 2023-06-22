using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum CostStatus
{
    Success,
    Fail,
    Overload
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;
    public bool isCharmEditable = false;

    #region STATUS
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int Damage { get; set; }
    public float AttackSpeed { get; set; }
    public string RespawnMap { get; set; }
    public Vector2 RespawnPosition { get; set; }

    public bool AttackRangeUp = false;

    private float maxSoul = 100;
    private float currentSoul = 0;
    private int charmSlotOverloadCount = 0;
    private float dashCooldown = 1f;
    private int charmSlotTotalCount = 3;
    private int charmSlotEquipCount = 0;
    private List<Charm> m_charms = new List<Charm>();
    /// <summary>
    /// string�� ���� [bool, bool]����� ����. [0]: ������ / [1]: �����Ǹ���
    /// bool[]�� [true, true] ���� ���� �� ����
    /// </summary>
    private Dictionary<string, bool[]> m_charmHaveShop = new Dictionary<string, bool[]>();
    private List<Charm> m_charmEquip = new List<Charm>();
    private int m_geo = 0;
    #endregion

    #region GET&SET
    public float MaxSoul { get => maxSoul; set => maxSoul = value; }
    public float CurrentSoul { get => currentSoul; set => currentSoul = value; }
    public List<Charm> Charms { get => m_charms; set { m_charms = value; } }
    public int CharmSlotTotalCount { get => charmSlotTotalCount; set { charmSlotTotalCount = value; } }
    public float DashCooldown { get => dashCooldown; set { dashCooldown = value; } }
    public int CharmSlotEquipCount { get => charmSlotEquipCount; set { charmSlotEquipCount = value; } }
    public int CharmSlotOverloadCount{ get => charmSlotOverloadCount; set { charmSlotOverloadCount = value; } }
    public Dictionary<string, bool[]> CharmHaveShop { get => m_charmHaveShop; set { m_charmHaveShop = value; } }
    public List<Charm> CharmEquip { get => m_charmEquip; set {  m_charmEquip = value; } }

    public int Geo { get => m_geo; set { m_geo = value; } }
    #endregion

    private void Awake()
    {
        //call from DB
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        } else
        {
            Destroy(this);
            return;
        }
        MaxHealth = PlayerPrefs.GetInt("maxHealth");
        if (MaxHealth <= 0)
        {
            PlayerPrefs.SetInt("maxHealth", 3);
            MaxHealth = PlayerPrefs.GetInt("maxHealth");
        }
        CurrentHealth = MaxHealth;
        Damage = 1;
        AttackSpeed = 1f;

    }

    private void Start()
    {
        Charms.AddRange(Resources.LoadAll<Charm>("Charms"));

        CharmHaveShop.Add("Unbreakable Strength", new bool[]{ true, false });
        CharmHaveShop.Add("Unbreakable Health", new bool[] { true, false });
        CharmHaveShop.Add("Grimmchild", new bool[] { false, true });

        if (PlayerPrefs.GetString("SpawnMap") == string.Empty)
        {
            PlayerPrefs.SetString("SpawnMap", "Start");
            RespawnMap = PlayerPrefs.GetString("SpawnMap");
            PlayerPrefs.SetFloat("SpawnX", -2.9f);
            PlayerPrefs.SetFloat("SpawnY", -2.1f);
            RespawnPosition = new Vector2(PlayerPrefs.GetFloat("SpawnX"), PlayerPrefs.GetFloat("SpawnY"));
        }
    }

    public void CancelCharmStat(Charm charm)
    {
        MaxHealth -= charm.hp;
        CurrentHealth = MaxHealth;
        Damage -= charm.damage;
        AttackRangeUp = (AttackRangeUp && charm.range_up) ? false : true;

        if (charm.name.Equals("Grimmchild"))
        {
            //todo: grimmchild�� �����ǵ��� ����
            FollowObject.instance.gameObject.SetActive(false);
        }
    }

    public void ApplyCharmStat(Charm charm)
    {
        MaxHealth += charm.hp;
        CurrentHealth = MaxHealth;
        Damage += charm.damage;
        AttackRangeUp = (AttackRangeUp || charm.range_up) ? true : false;

        if (charm.name.Equals("Grimmchild"))
        {
            //todo: grimmchild�� �����ǵ��� ����
            FollowObject.instance.gameObject.SetActive(true);
        }
    }

    public bool CheckContainCharm(Charm _charm)
    {
        return CharmEquip.Contains(_charm);
    }

    public CostStatus EquipCharm(Charm _charm)
    {
        int remain = (charmSlotTotalCount - CharmSlotEquipCount);
        //�ڽ�Ʈ�� ���������� �ڽ�Ʈ�� 1�� �̻��� �����ִ� ��� - �����ε�
        if ((remain - _charm.cost <= 0) && (remain >= 1))
        {
            CharmEquip.Add(_charm);
            charmSlotEquipCount = Mathf.Clamp(charmSlotEquipCount + _charm.cost, 0, charmSlotTotalCount);
            CharmSlotOverloadCount += _charm.cost - remain;
            ApplyCharmStat(_charm);
            return CostStatus.Overload;
        }
        //�ڽ�Ʈ�� 0���� ��� - ����
        else if (remain == 0) {
            return CostStatus.Fail;
        }
        //�������� - ����
        else if ((remain - _charm.cost) >= 0)
        {
            CharmEquip.Add(_charm);
            charmSlotEquipCount += _charm.cost;
            ApplyCharmStat(_charm);
            return CostStatus.Success;
        }

        //if���� ���� ���� ��� ���з� ������
        return CostStatus.Fail;
    }

    public void UnEquipCharm(Charm _charm)
    {
        try
        {
            int remainCost = _charm.cost;
            CharmEquip.Remove(_charm);
            if (CharmSlotOverloadCount > 0)
            {
                //�ܿ� �ڽ�Ʈ
                //������ �Ǵ� ������ �ذ��ϱ� ���� overload���� ���ҵǴ� �ڽ�Ʈ�� ������ ���� ��� 0���� clamping ����
                remainCost = Mathf.Clamp(_charm.cost - CharmSlotOverloadCount, 0, 5);
                CharmSlotOverloadCount = Mathf.Clamp(CharmSlotOverloadCount - _charm.cost, 0, 5);
            }
            charmSlotEquipCount -= remainCost;
            CancelCharmStat(_charm);
        }
        catch
        {
            Debug.LogError("���� ������ �õ��� ������ �������� �ƴմϴ�");
        }
    }
}

