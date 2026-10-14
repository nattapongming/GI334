using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShipInPort : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image unitImage;

    [SerializeField] private NavalUnit navalUnit;
    public NavalUnit NavalUnit { get { return navalUnit; } set { navalUnit = value; } }

    [SerializeField] private TMP_Text turnText;
    public TMP_Text TurnText { get { return turnText; } set { turnText = value; } }

    [SerializeField] private ShipDrag shipDrag;

    [SerializeField] private UiManager uiMgr;

    void Awake()
    {
        uiMgr = UiManager.instance;
    }

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

    public void UnitInit(NavalUnit navalUnit, bool genShipDrag)
    {
        unitImage.sprite = navalUnit.UnitSprite.sprite;
        this.navalUnit = navalUnit;

        if (genShipDrag)
        {
            shipDrag = GenerateShipDrag(navalUnit);
        }
    }

    public void UpdateTurnText(int Turn)
    {
        turnText.gameObject.SetActive(true);
        turnText.text = ($"{Turn}\nTurn(s)");
        turnText.color = Color.yellow;
    }

    public ShipDrag GenerateShipDrag(NavalUnit ship)
    {
        GameObject shipDragObj = 
            Instantiate(uiMgr.ShipDragPrefab, transform.position, 
            Quaternion.identity, transform);

        ShipDrag shipDrag = shipDragObj.GetComponent<ShipDrag>();
        shipDrag.Init(ship);

        return shipDrag;
    }
}
