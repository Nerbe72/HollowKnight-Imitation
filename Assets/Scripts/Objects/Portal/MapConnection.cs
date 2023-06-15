using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Connection")]
public class MapConnection : ScriptableObject
{
    public static MapConnection SelectedConnection { get; set; }
}
