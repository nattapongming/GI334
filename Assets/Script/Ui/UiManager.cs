using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] private GameObject townPanel;
    public GameObject TownPanel { get { return townPanel; } set { townPanel = value; } }

    [SerializeField] private TerrainSlot centerSlot;

    [SerializeField] private TerrainSlot[] areaSlots;

    [SerializeField] private GameObject unitDragPrefab;

    [SerializeField] private GameObject outsideTownParent;

    [SerializeField] private GameObject yieldIconPrefab;
    public GameObject YieldIconPrefab { get { return yieldIconPrefab; } }

    [SerializeField] private List<GameObject> allUnitDrag;
    public List<GameObject> AllUnitDrag { get { return allUnitDrag; } set { allUnitDrag = value; } }

    [SerializeField] private TerrainSlot currentSlot;

    [SerializeField] private GameObject blockImage;

    [SerializeField] private GameObject professionPanel;

    [SerializeField] private TMP_Text labelQuestionText;

    [SerializeField] private TMP_Text[] btnYieldTexts;
    
    [SerializeField] private Transform foodParent;

    [SerializeField] private TMP_Text foodText;

    [SerializeField] private List<GameObject> foodIconList = new List<GameObject>();

    public static UiManager instance;

    void Awake()
    {
        instance = this;
    }

    public void SetUpHexSlots(Hex centerHex, Hex[] aroundHexes)
    {
        centerSlot.HexSlotInit(centerHex);

        // Setup auto Food production on ConterSlot
        centerSlot.Hex.YieldID = 0; //Food
        centerSlot.AdjustActualYieldAndAccumulate();

        for (int i = 0; i < areaSlots.Length; i++)
        {
            areaSlots[i].HexSlotInit(aroundHexes[i]);
        }
    }

    public void SetupCurrentTown(Hex curHex, Hex[] aroundHexes) 
    {
        DestoryOldUnitDrag();
        SetUpHexSlots(curHex, aroundHexes);
        SetupUnitDragOutSideTown(curHex);
        SetUpUnitDragWorkingInTerrain();
        SetupYieldInTerrain();
        UpdateTotalFoodIcons();
    }

    public void SetupParentSpacing(int n, Transform parent, int iconWidth, int parentWidth)
    {
        if (n <= 1) return;

        HorizontalLayoutGroup layout = parent.GetComponent<HorizontalLayoutGroup>();

        int totalWidth = iconWidth * n;
        int excessWidth = totalWidth - parentWidth;

        if (excessWidth <= 0) return;

        int result = excessWidth / (n - 1);
        layout.spacing = -result;
    }

    private void DestoryOldUnitDrag()
    {
        foreach (GameObject obj in allUnitDrag)
        {
            Destroy(obj);
        }
        allUnitDrag.Clear();
    }

    private void HideUnitWorkInTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitStatus == UnitStatus.WorkInField)
                unit.gameObject.SetActive(false);
        }
    }

    private void SetupUnitDragOutSideTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Land && unit.UnitStatus == UnitStatus.None)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, outsideTownParent.transform);
                allUnitDrag.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)unit);
            }
        }
    }

    public void SetUpUnitDragWorkingInTerrain()
    {
        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null) continue;

            if (terrainSlot.Hex.Labor != null)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, terrainSlot.LaborParnet);
                allUnitDrag.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)terrainSlot.Hex.Labor);

                unitDrag.IconParent = terrainSlot.LaborParnet;
                unitDrag.WorkAtNewTerrainSlot(terrainSlot);
            }
        }
    }

    private void SetupYieldInTerrain()
    {
        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null) continue;

            if (terrainSlot.Hex.Labor != null && terrainSlot.Hex.YieldID != -1)
            terrainSlot.AdjustActualYieldAndAccumulate();
        }
    }

    public void RemoveAllYieldIcons()
    {
        centerSlot.RemoveYieldIcons();

        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot == null) continue;

            terrainSlot.RemoveYieldIcons();
        }
    }

    public void ToggleTownPanel(bool show)
    {
        if (show == false)
        {
            DestoryOldUnitDrag();
            RemoveAllYieldIcons();
            HideUnitWorkInTown(GameManager.instance.CurTown.CurHex);
        }
        townPanel.SetActive(show);
    }

    public void UpdateLabelQuestionText()
    {
        if (currentSlot == null) return;

        string s = string.Format("Select a profession for {0}", currentSlot.Hex.Labor.UnitName);
        labelQuestionText.text = s;
    }

    public void UpdateButtonTextsYield()
    {
        if (currentSlot == null) return;

        for (int i = 0; i < btnYieldTexts.Length; i++)
        {
            string s = string.Format("{0} {1}", 
            currentSlot.NormalYield[i],
            GameManager.instance.ProductDatas[i].productName);

            btnYieldTexts[i].text = s;
        }
    }

    public void SelectProfession(TerrainSlot slot)
    {
        currentSlot = slot;

        UpdateLabelQuestionText();
        UpdateButtonTextsYield();

        blockImage.SetActive(true);
        professionPanel.SetActive(true);
    }

    public void SelectYield(int i) // Link to Select Profession Button on UI
    {
        Debug.Log($"Select: {i}");

        if (currentSlot != null)
            currentSlot.SelectYield(i);

        blockImage.SetActive(false);
        professionPanel.SetActive(false);

        if (currentSlot.Hex.YieldID == 0)
            UpdateTotalFoodIcons();
    }

    public void UpdateTotalFoodIcons()
    {
        foreach (GameObject obj in foodIconList) Destroy(obj);

        foodIconList.Clear();

        foodText.text = GameManager.instance.CurTown.TotalYieldThisTurn[0].ToString();
        foodText.gameObject.SetActive(true);
        
        for (int i = 0; i < GameManager.instance.CurTown.TotalYieldThisTurn[0]; i++)
        {
            GameObject iconobj = GenerateFoodIcon();
            foodIconList.Add(iconobj);
        }

        SetupParentSpacing(GameManager.instance.CurTown.TotalYieldThisTurn[0], foodParent, 64, 300);
    }

    public GameObject GenerateFoodIcon()
    {
        GameObject foodObj = Instantiate(yieldIconPrefab, foodParent);
        Image iconImg = foodObj.GetComponent<Image>();

        iconImg.sprite = GameManager.instance.ProductDatas[0].icons[0];

        return foodObj;
    }
}