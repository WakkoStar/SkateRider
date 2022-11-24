using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class GridCustomableDisplayer : MonoBehaviour
{
    [HideInInspector] public List<SerializableCustomable> customables; // Must be set by shop or inventory
    public GameObject CustomableElement;
    public Action<SerializableCustomable> OnClickOnElement;
    public Action<SerializableCustomable, CustomableDisplayer> OnDisplayElement;

    private CustomableType _selectedCustomableType = CustomableType.All;

    public void UpdateGrid()
    {
        if (_selectedCustomableType == CustomableType.All)
        {
            DisplayAll();
        }
        else
        {
            FilterBy((int)_selectedCustomableType);
        }
    }

    public void FilterBy(int type)
    {
        _selectedCustomableType = (CustomableType)type;
        DisplayCustomables(customables.FindAll(p => p.type == (CustomableType)type));
    }

    public void DisplayAll()
    {
        _selectedCustomableType = CustomableType.All;
        DisplayCustomables(customables);
    }

    public void DisplayCustomables(List<SerializableCustomable> filteredCustomables)
    {
        DestroyChilds();

        foreach (var customable in filteredCustomables)
        {
            var customableDisplayer = Instantiate(CustomableElement.gameObject, transform).GetComponent<CustomableDisplayer>();
            customableDisplayer.DeserializeCustomable(customable);

            customableDisplayer.gameObject.GetComponent<Button>().onClick.AddListener(() => OnClickOnElement(customable));
            OnDisplayElement(customable, customableDisplayer);
        }
    }

    void DestroyChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
