using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom menu with one button per shape. Pressing a button selects that shape,
/// pressing the selected button again deselects it.
/// </summary>
public class ARShapeMenu : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        public string label;
        public GameObject prefab;
        public Button button;
    }

    [SerializeField] ARShapePlacer placer;
    [SerializeField] List<Item> items = new List<Item>();
    [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] Color selectedColor = new Color(1f, 1f, 1f, 0.6f);

    int m_Index = -1;

    void Awake()
    {
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            if (items[i].button != null)
                items[i].button.onClick.AddListener(() => OnPressed(index));
        }
        Refresh();
    }

    void OnPressed(int index)
    {
        m_Index = (m_Index == index) ? -1 : index;
        if (placer != null)
            placer.SelectPrefab(m_Index >= 0 ? items[m_Index].prefab : null);
        Refresh();
    }

    void Refresh()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].button != null && items[i].button.targetGraphic != null)
                items[i].button.targetGraphic.color = (i == m_Index) ? selectedColor : normalColor;
        }
    }
}
