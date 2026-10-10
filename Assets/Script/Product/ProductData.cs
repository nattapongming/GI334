using UnityEngine;

[CreateAssetMenu(fileName = "ProductData", menuName = "Scriptable Objects/ProductData")]
public class ProductData : ScriptableObject
{
    public int id;
    public string productName;
    public Sprite[] icons;
    public int startBit;
    public int startAsk;
}
