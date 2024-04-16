using System;
using UnityEngine;
using UnityEngine.Events;

class SelectionItem : MonoBehaviour
{
    public UnityEvent onSelected;
    public UnityEvent onDeselected;

    public bool isSelected;

    public void ToggleSelected()
    {
        GetComponentInParent<SelectionGroup>().ToggleSelected(this);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            onSelected?.Invoke();
            isSelected = true;
        }
        else
        {
            onDeselected?.Invoke();
            isSelected = false;
        }
    }
}
