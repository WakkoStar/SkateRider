using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridCustomableDisplayer))]
public class InventoryDisplayer : MonoBehaviour
{

    //SETINGS
    [SerializeField] private Customable[] defaultInventory;

    [SerializeField] private Material WheelMat;
    [SerializeField] private Material TruckMat;
    [SerializeField] private Material GripMat;
    [SerializeField] private Material BoardMat;


    //STATE
    private GridCustomableDisplayer _gridCustomableDisplayer;
    private List<SerializableCustomable> _inventory;

    void Start()
    {
        _gridCustomableDisplayer = GetComponent<GridCustomableDisplayer>();
        _gridCustomableDisplayer.OnClickOnElement = SelectCustomable;
        _gridCustomableDisplayer.OnClickOnElement += (value) => FindObjectOfType<AudioManager>().Play("applat");
        _gridCustomableDisplayer.OnDisplayElement = SetInventoryCustomableDisplayer;

        UpdateInventory();
    }

    public void UpdateInventory()
    {
        _gridCustomableDisplayer.customables = GetInventory();
        _gridCustomableDisplayer.UpdateGrid();
    }

    public void SetInventoryCustomableDisplayer(SerializableCustomable customable, CustomableDisplayer customableDisplayer)
    {
        customableDisplayer.SetSelected(IsCustomableSelected(customable));
    }

    void SelectCustomables(List<SerializableCustomable> customables)
    {
        foreach (var customable in customables)
        {
            SelectCustomable(customable);
        }
    }
    void SelectCustomable(SerializableCustomable customable)
    {
        var texToApply = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        texToApply.LoadRawTextureData(customable.textureToApply);
        texToApply.Apply();

        switch (customable.type)
        {
            case CustomableType.Wheels:
                WheelMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_wheels", customable.nameId);
                break;
            case CustomableType.Trucks:
                TruckMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_trucks", customable.nameId);
                break;
            case CustomableType.Board:
                BoardMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_board", customable.nameId);
                break;
            case CustomableType.Grip:
                GripMat.mainTexture = texToApply;
                PlayerPrefs.SetString("skate_grip", customable.nameId);
                break;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var customableDisplayer = transform.GetChild(i).gameObject.GetComponent<CustomableDisplayer>();
            customableDisplayer.SetSelected(IsCustomableSelected(customableDisplayer.GetCustomable()));
        }
    }

    List<SerializableCustomable> GetInventory()
    {
        var inventory = DataManager.LoadData<SerializableCustomable>("inventory");

        if (inventory == null)
        {
            var inventoryWrapper = new Wrapper<SerializableCustomable>();
            inventoryWrapper.items = Customable.SerializeCustomables(defaultInventory.ToList());
            DataManager.SaveData<SerializableCustomable>(inventoryWrapper, "inventory");

            inventory = inventoryWrapper.items;
        }

        var selectedCustomableInInventory = inventory.FindAll(customable => IsCustomableSelected(customable));

        SelectCustomables(
        selectedCustomableInInventory.Count == 0
            ? Customable.SerializeCustomables(defaultInventory.ToList()) //FALLBACK
            : selectedCustomableInInventory
         );

        return inventory;
    }

    private bool IsCustomableSelected(SerializableCustomable customable)
    {
        var isSelected = false;
        switch (customable.type)
        {
            case CustomableType.Wheels:
                isSelected = PlayerPrefs.GetString("skate_wheels") == customable.nameId;
                break;
            case CustomableType.Trucks:
                isSelected = PlayerPrefs.GetString("skate_trucks") == customable.nameId;
                break;
            case CustomableType.Board:
                isSelected = PlayerPrefs.GetString("skate_board") == customable.nameId;
                break;
            case CustomableType.Grip:
                isSelected = PlayerPrefs.GetString("skate_grip") == customable.nameId;
                break;
        }

        return isSelected;
    }
}
