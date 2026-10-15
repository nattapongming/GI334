using UnityEngine;

[CreateAssetMenu(fileName = "NavelUnitData", menuName = "Scriptable Objects/NavelUnitData")]
public class NavalUnitData : ScriptableObject
{
    public string unitName;
    public NavalUnitType navalUnitType;
    public int strength;
    public int movePointMax;
    public int visualRange;
    public Sprite shipIcon;
    public bool armed;
    public int cargoHoldNum;
    public int price;
}