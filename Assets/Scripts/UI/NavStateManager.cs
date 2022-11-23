using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavStateManager : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] List<GameObject> navElements;
    // Start is called before the first frame update
    void Start()
    {
        navElements[0].GetComponent<Image>().sprite = selectedSprite;
    }

    public void SelectElement(string elementName)
    {
        foreach (var element in navElements)
        {
            element.GetComponent<Image>().sprite = normalSprite;
        }


        var selectedElement = navElements.Find(navElement => navElement.name == elementName);

        if (selectedElement == null)
        {
            Debug.LogWarning(elementName + "is not founded, check your names");
            return;
        }

        selectedElement.GetComponent<Image>().sprite = selectedSprite;
    }
}
