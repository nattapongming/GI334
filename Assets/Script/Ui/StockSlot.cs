using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Mathematics;

public class StockSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Town town;

    [SerializeField] private int productId;
    public int ProductId { get { return productId; } }

    [SerializeField] private int quantity;
    public int Quantity { get { return quantity; } set { quantity = value; } }

    [SerializeField] private TMP_Text stockText;
    public TMP_Text StockText { get { return stockText; } set { stockText = value; } }

    [SerializeField] private Image image;
    public Image Image { get { return image; } }

    [SerializeField] private UiManager uiMgr;

    [SerializeField] private StockDrag stockDrag;

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

    public void StockInit(int i, Town t, Sprite s)
    {
        town = t;
        productId = i;
        quantity = town.Warehouse[productId];
        stockText.text = quantity.ToString();
        image.sprite = s;

        if (stockDrag == null)
        stockDrag = GenerateStockDrag(productId, this);
    }

    public void StockInit(int i, Town t)
    {
        town = t;
        productId = i;
        quantity = town.Warehouse[productId];
        stockText.text = quantity.ToString();

        if (stockDrag == null)
        {
            stockDrag = GenerateStockDrag(productId, this);
        }
    }

    public void UpdateStockText()
    {
        stockText.text = town.Warehouse[productId].ToString();
    }

    public void UpdateQuantityStock(int n)
    {
        town.Warehouse[productId] += n;
        quantity = town.Warehouse[productId];
        stockText.text = quantity.ToString();
    }

    public StockDrag GenerateStockDrag(int id, StockSlot slot)
    {
        GameObject stockDragObj = Instantiate(uiMgr.StockDragPrefab, slot.Image.transform.position, 
        quaternion.identity, slot.transform);

        StockDrag stockDrag = stockDragObj.GetComponent<StockDrag>();
        stockDrag.StockDragInit(slot);

        return stockDrag;
    }

    public void ToggleRayCastStockDrag(bool flag)
    {
        if (stockDrag != null)
            stockDrag.Image.raycastTarget = flag;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject obj = eventData.pointerDrag;
        CargoDrag cargoDrag = obj.GetComponent<CargoDrag>();

        if (cargoDrag == null) return;

        if (cargoDrag.Cargo.ProductID != productId) return;

        UpdateQuantityStock(cargoDrag.Cargo.Quantity);
        cargoDrag.Cargo.Quantity = 0;
        
        cargoDrag.RemoveCargoListFromShip();
        uiMgr.UpdateCargoSlots(cargoDrag.Ship);

        Debug.Log("CargoDrag - On Drop - On StockSlot");
        uiMgr.ToggleStockDragRayCast(true);

    }
}
