using UnityEngine;
using UnityEngine.EventSystems;

public class OutSideSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Town town;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject unitObj = eventData.pointerDrag;
        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
        unitDrag.IconParent = transform;

        unitDrag.QuitOldTerrainSlot(); // old slot remove this labor
        unitDrag.LandUnit.UnitStatus = UnitStatus.None;

        
    }
}