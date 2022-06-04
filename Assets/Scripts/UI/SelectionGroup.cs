using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

class SelectionGroup : MonoBehaviour
{
    public bool multiSelect = false;
    public int maxSelected = 60;

    public HashSet<SelectionItem> selection;
    public UnityEvent onSelectionChanged;

    private void Start()
    {
        selection = new HashSet<SelectionItem>();
    }

    public void ToggleSelected(SelectionItem item)
    {
        if (multiSelect)
        {
            if (selection.Contains(item))
            {
                selection.Remove(item);
                item.SetSelected(false);
            }
            else
            {
                selection.Add(item);
                item.SetSelected(true);
            }
        }
        else
        {
            foreach (SelectionItem child in GetComponentsInChildren<SelectionItem>())
            {
                child.SetSelected(child == item);
            }

            selection.Clear();
            selection.Add(item);
        }
    }
}
