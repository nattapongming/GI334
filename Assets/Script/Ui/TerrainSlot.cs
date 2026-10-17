using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class TerrainSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Hex hex;
    public Hex Hex { get { return hex; } }

    [SerializeField] private Image terrainImage;
    [SerializeField] private Image forestImage;
    [SerializeField] private Image townImage;
    [SerializeField] private Transform yieldParent;

    [SerializeField] private Transform laborParent;
    public Transform LaborParnet { get { return laborParent; } }

    //0-food, 1=suger, 2-tobacco, 3-cotten, 4-fur, 5-lumber, 6-ore, 7-silver
    [SerializeField] private int[] normalYield = new int[8]; 
    public int[] NormalYield { get { return normalYield; } set { normalYield = value; } }

    [SerializeField] private int[] actualYield = new int[8]; // Actual Yield labor is currently working
    public int[] ActualYield { get { return actualYield; } set { actualYield = value; } }

    [SerializeField] private bool centerHex;

    [SerializeField] private TMP_Text yieldText;

    [SerializeField] List<GameObject> yieldIconList = new List<GameObject>();
    public List<GameObject> YieldIconList { get { return yieldIconList; } set { yieldIconList = value; } }

    // Other component

    [SerializeField] private GameManager gameMgr;
    [SerializeField] private UiManager uiMgr;

    private void Awake()
    {
        gameMgr = GameManager.instance;
        uiMgr = UiManager.instance;
    }

    public void HexSlotInit(Hex hexData)
    {
        hex = hexData;
        gameObject.SetActive(hex != null); // hide the slot at the map edge
        if (hex == null) return;

        SetHexImage(terrainImage, hex.TerrainSprite.sprite);
        SetupNormalYield();

        forestImage.gameObject.SetActive(hex.HasForest);
        if (hex.HasForest)
            SetHexImage(forestImage, hex.ForestSprite.sprite);

        townImage.gameObject.SetActive(hex.HasTown);
        if (hex.HasTown)
            SetHexImage(townImage, hex.Town.TownSprite.sprite);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (centerHex || hex == null || hex.Labor != null) return;
        if (hex.HexType == HexType.Ocean || hex.HasTown) return;

        GameObject unitObj = eventData.pointerDrag;
        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();

        if (unitDrag == null) return;

        unitDrag.QuitOldTerrainSlot(); // old slot remove this labor
        unitDrag.WorkAtNewTerrainSlot(this); // this slot is remembered in this labor

        unitDrag.IconParent = laborParent;
        unitObj.transform.position = laborParent.position;

        unitDrag.LandUnit.UnitStatus = UnitStatus.WorkInField;
        hex.Labor = unitDrag.LandUnit;

        uiMgr.SelectProfession(this);
    }

    public GameObject GenerateYieldIcon(int i)
    {
        GameObject yieldObj = Instantiate(uiMgr.YieldIconPrefab, yieldParent);
        Image iconImg = yieldObj.GetComponent<Image>();

        iconImg.sprite = gameMgr.ProductDatas[i].icons[0];

        if (i == 0)
        {
            if (hex.HexType == HexType.Ocean)
                iconImg.sprite = gameMgr.ProductDatas[0].icons[1];
            else
                iconImg.sprite = gameMgr.ProductDatas[0].icons[0];
        }

        return yieldObj;
    }

    public void GenerateYieldIcons(int id)
    {
        yieldText.text = actualYield[id].ToString();
        yieldText.gameObject.SetActive(true);

        for (int i = 0; i < actualYield[id]; i++)
        {
            GameObject iconobj = GenerateYieldIcon(id);
            yieldIconList.Add(iconobj);
        }
        uiMgr.SetupParentSpacing(actualYield[id], yieldParent, 64, 250);
    }

    public void ConvertNormalToActual(int id)
    {
        if (hex.YieldID == -1) return;

        // Formula to Adjust NormalYield *

        // Convert normal Yield to actual yield
        actualYield[id] = normalYield[id];
    }

    public void AdjustActualYield()
    {
        ConvertNormalToActual(hex.YieldID);

        // accumulate actual yield to total production

        // generate all yield icons in one hex
        GenerateYieldIcons(hex.YieldID);
    }

    private void Accumulate()
    {
        gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID] += actualYield[hex.YieldID];
    }

    public void ReduceTownYield()
    {
        if (hex.YieldID != -1)
        {
            gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID] -= actualYield[hex.YieldID];
            actualYield[hex.YieldID] = 0;
        }
    }

    public void RemoveYieldIcons()
    {
        foreach (GameObject obj in yieldIconList)
        {
            Destroy(obj);
        }
        yieldIconList.Clear();
        yieldText.gameObject.SetActive(false);

        if (hex.YieldID == 0)
            uiMgr.UpdateTotalFoodIcons();
    }

    private void SetupNormalYield()
    {
        for (int i = 0; i < 8; i++)
        {
            normalYield[i] = hex.ResourceYields[i];

            if (i == 4 || i == 5) // Fur or Lumber
            {
                if (!hex.HasForest)
                    normalYield[i] = 0;
            } else // Agriculture
            {
                if (hex.HasForest)
                    normalYield[i] -= 2; // Reduce agriculture
            }

            if (hex.HasForest)
                normalYield[i] -= 2;

            if (hex.HasTown)
                normalYield[i] = Mathf.CeilToInt(normalYield[i]/2f);

            if (normalYield[i] < 0)
                normalYield[i] = 0;
        }
    }

    public void SelectYield(int i)
    {
        hex.YieldID = i;
        AdjustActualYield();
        Accumulate();
    }

    // Make a UI Image match its sprite like the world SpriteRenderer (100 UI px = 1 world unit)
    private void SetHexImage(Image img, Sprite sprite)
    {
        img.sprite = sprite;
        RectTransform rt = img.rectTransform;
        rt.sizeDelta = sprite.rect.size * (100f / sprite.pixelsPerUnit);
        rt.pivot = sprite.pivot / sprite.rect.size;   // use the sprite's own pivot
        rt.anchoredPosition = Vector2.zero;           // pivot = hex center = slot center
    }

}