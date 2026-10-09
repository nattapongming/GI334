using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShipInPort : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image unitImage;

    [SerializeField] private NavalUnit navalUnit;
    public NavalUnit NavalUnit { get { return navalUnit; } set { navalUnit = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click at a ship");
    }

    public void UnitInit(NavalUnit navalUnit)
    {
        unitImage.sprite = navalUnit.UnitSprite.sprite;
        this.navalUnit = navalUnit;
    }
}
