using UnityEngine;

public class CargoSlot : MonoBehaviour
{
    [SerializeField] private NavalUnit ship;

    [SerializeField] private int holdId;

    [SerializeField] private UiManager uiMgr;

    private void Awake()
    {
        uiMgr = UiManager.instance;
    }

    public void SlotInit(NavalUnit ship, int holdId)
    {
        this.ship = ship;
        this.holdId = holdId;
    }
}