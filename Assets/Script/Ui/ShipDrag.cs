using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShipDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image shipImage;

    [SerializeField] NavalUnit ship;
    public NavalUnit Ship { get { return ship; } set { ship = value; } }

    [SerializeField] private Transform iconParent;
    public Transform IconParent { get { return iconParent; } set { iconParent = value; } }

    public void Init(NavalUnit ship)
    {
        shipImage.sprite = ship.UnitSprite.sprite;
        this.ship = ship;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        iconParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        shipImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Mouse.current.position.value;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(iconParent);
        shipImage.raycastTarget = true;
        transform.position = iconParent.position;
    }
}