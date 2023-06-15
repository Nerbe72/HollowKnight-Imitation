using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Charm")]
public class Charm : ScriptableObject
{
    [Header("�⺻����")]
    [Tooltip("�̹���")] public Sprite sprite;
    [Tooltip("�̸�")] public new string name;
    [Tooltip("����")] public string description;

    [Header("����")]
    [Tooltip("�����ϴ°��� �ƴ϶�� ��� 0���� ����")] public int price;

    [Header("��������")]
    [Tooltip("������ ���Ǵ� �ڽ�Ʈ")] public int cost;
    public int hp;
    public int damage;
    public int attack_speed;
    public bool range_up;
    public bool summon_grimm;
    public bool summon_bugs;

    public Charm CloneCharm()
    {
        Charm c_temp = new Charm();
        c_temp.sprite = sprite;
        c_temp.name = name;
        c_temp.description = description;
        c_temp.price = price;
        c_temp.cost = cost;
        c_temp.hp = hp;
        c_temp.damage = damage;
        c_temp.attack_speed = attack_speed;
        c_temp.range_up = range_up;
        c_temp.summon_bugs = summon_bugs;
        c_temp.summon_grimm = summon_grimm;
        return c_temp;
    }
}
