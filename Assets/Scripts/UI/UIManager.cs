using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIManager : MonoBehaviour
{
    public GameObject currentWindow;
    public Stack<GameObject> previousWindows = new Stack<GameObject>();
    private List<WindowState> savedWindows = new List<WindowState>();

    public void OpenWindow(GameObject window, bool closePrevious = true)
    {
        if (currentWindow != null)
        {
            previousWindows.Push(currentWindow);
            if (closePrevious)
                currentWindow.SetActive(false);
        }
        currentWindow = window;
        window.SetActive(true);
    }

    public void CloseCurrentWindow()
    {
        if (currentWindow == null)
            return;
        currentWindow.SetActive(false);
        if (previousWindows.Count > 0)
        {
            currentWindow = previousWindows.Pop();
            currentWindow.SetActive(true);
        }
        else if (savedWindows.Count > 0)
        {
            LoadWindowState();
        }
        else
        {
            currentWindow = null;
        }
    }

    public void SaveWindowState()
    {
        foreach (GameObject g in previousWindows)
        {
            savedWindows.Add(new WindowState(g, g.activeSelf));
        }
        savedWindows.Add(new WindowState(currentWindow, true));
    }

    private void LoadWindowState()
    {
        previousWindows.Clear();
        foreach (WindowState windowState in savedWindows)
        {
            windowState.window.SetActive(windowState.activeStatus);
            previousWindows.Push(windowState.window);
        }
        currentWindow = previousWindows.Pop();
        savedWindows.Clear();
    }

    private struct WindowState
    {
        public GameObject window;
        public bool activeStatus;

        public WindowState(GameObject window, bool activeStatus)
        {
            this.window = window ?? throw new ArgumentNullException(nameof(window));
            this.activeStatus = activeStatus;
        }
    }
}
