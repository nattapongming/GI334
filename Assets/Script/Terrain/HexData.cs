using UnityEngine;

[CreateAssetMenu(fileName = "HexData", menuName = "Scriptable Objects/HexData")]
public class HexData : ScriptableObject
{
    public string hexName;
    public HexType hexType;
    public Sprite[] terrainSprites;
    public Sprite[] forestSprites;
    public int[] resourceYields;
    public int moveCost;
}
