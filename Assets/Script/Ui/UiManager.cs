using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
}