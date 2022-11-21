using System;
using UnityEngine;
using System.Collections;
public class ScreenManager : MonoBehaviour
{
    [Serializable]
    public class NamedScreen
    {
        public string name;
        public CanvasGroup canvas;
    }

    [SerializeField] private NamedScreen[] screens;

    public void Display(string name)
    {
        var screen = Array.Find(screens, s => s.name == name);
        if (screen == null)
        {
            Debug.LogWarning("Unable to find this screen :" + name);
            return;
        }

        screen.canvas.alpha = 1;
        screen.canvas.interactable = true;
        screen.canvas.blocksRaycasts = true;
    }

    public void Hide(string name)
    {
        var screen = Array.Find(screens, s => s.name == name);
        if (screen == null)
        {
            Debug.LogWarning("Unable to find this screen :" + name);
            return;
        }

        StartCoroutine(HideCoroutine(0.5f, screen));
    }

    IEnumerator HideCoroutine(float delay, NamedScreen screen)
    {
        yield return new WaitForSeconds(delay);
        screen.canvas.alpha = 0;
        screen.canvas.interactable = false;
        screen.canvas.blocksRaycasts = false;
    }


}
