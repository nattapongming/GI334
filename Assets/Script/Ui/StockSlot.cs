using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StockSlot : MonoBehaviour
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
    }

    public void stockInit(int i, Town t)
    {
        town = t;
        productId = i;
        quantity = town.Warehouse[productId];
        stockText.text = quantity.ToString();
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
}
