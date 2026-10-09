using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CargoDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;
    public Image Image { get { return image; } set { image = value; } }

    [SerializeField] private Cargo cargo;
    public Cargo Cargo { get { return cargo; } set { cargo = value; } }

    [SerializeField] private Transform iconParent;
    public Transform IconParent { get { return iconParent; } set { iconParent = value; } }

    [SerializeField] private TMP_Text cargoText;

    [SerializeField] private NavalUnit ship;
    public NavalUnit Ship { get { return ship; } set { ship = value; } }

    [SerializeField] private int cargoListId;

    [SerializeField] private UiManager uiMgr;

    void Awake()
    {
        uiMgr = UiManager.instance;
    }

    public void UpdateCargoText()
    {
        cargoText.text = cargo.Quantity.ToString();
    }

    public void CargoDragDeductQuantity()
    {
        ship.CargoList[cargoListId].Quantity = 0;
    }

    public void CargoDragInit(NavalUnit ship, Sprite sprite, int id)
    {
        this.ship = ship;
        image.sprite = sprite;

        Cargo c = ship.CargoList[id];
        cargo = new Cargo(c.ProductID, c.Quantity);
        cargoListId = id;

        UpdateCargoText();
    }

    public void RemoveCargoListFromShip()
    {
        ship.CargoList.RemoveAt(cargoListId);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        iconParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;

        CargoDragDeductQuantity();
        uiMgr.ToggleStockDragRayCast(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Mouse.current.position.value;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(iconParent);
        image.raycastTarget = true;
        transform.position = iconParent.transform.position;

        // return
        if (cargoListId < ship.CargoList.Count && cargo.Quantity > 0)
        ship.CargoList[cargoListId].Quantity += cargo.Quantity;

        uiMgr.ToggleStockDragRayCast(true);
    }
}