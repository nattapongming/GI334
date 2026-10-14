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

    [SerializeField] private GameManager gameMgr;

    [SerializeField] private EuropeManager EUmgr;

    void Awake()
    {
        gameMgr = GameManager.instance;
        uiMgr = UiManager.instance;
        EUmgr = EuropeManager.instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StockInit(int i, Town t, bool Europe)
    {
        town = t;
        productId = i;
        quantity = town.Warehouse[productId];
        stockText.text = quantity.ToString();
        image.sprite = gameMgr.ProductDatas[i].icons[0];

        if (Europe)
            stockText.text = $"{EUmgr.EuropeStocks[i].BidPrice}/{EUmgr.EuropeStocks[i].AskPrice}";
        else
            stockText.text = quantity.ToString();

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
        if (uiMgr.InEurope)
        {
            EUmgr.EuropeStocks[productId].Quantity += n;
            quantity = EUmgr.EuropeStocks[productId].Quantity;

            stockText.text = $"{EUmgr.EuropeStocks[productId].BidPrice}/{EUmgr.EuropeStocks[productId].AskPrice}";

        } 
        else
        {
            town.Warehouse[productId] += n;
            quantity = town.Warehouse[productId];
            stockText.text = quantity.ToString();
        }
        
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

        int amount = cargoDrag.Cargo.Quantity;
        UpdateQuantityStock(amount);
        cargoDrag.Cargo.Quantity = 0;
        cargoDrag.RemoveCargoListFromShip();

        if (uiMgr.InEurope)
        {
            uiMgr.UpdateCargoSlots(cargoDrag.Ship, uiMgr.CargoSLotEurope);
            // Sell to market

            gameMgr.PlayerFaction.Money += 
            amount * EUmgr.EuropeStocks[productId].BidPrice;

            uiMgr.UpdateMoneyEuropeText();
        }
        else
        {
            uiMgr.UpdateCargoSlots(cargoDrag.Ship, uiMgr.CargoSlots);
        }

        //Debug.Log("CargoDrag - On Drop - On StockSlot");
        uiMgr.ToggleStockDragRayCast(true);

    }

    public void StockInitEurope(int i)
    {
        productId = i;
        image.sprite = gameMgr.ProductDatas[i].icons[0];

        quantity = EUmgr.EuropeStocks[productId].Quantity;
        stockText.text =
        $"{EUmgr.EuropeStocks[productId].BidPrice}/{EUmgr.EuropeStocks[productId].AskPrice}";

        if (stockDrag == null)
        {
            stockDrag = GenerateStockDrag(productId, this);
        }
    }
}
