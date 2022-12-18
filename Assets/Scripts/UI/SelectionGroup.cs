using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

class SelectionGroup : MonoBehaviour
{
    public bool keepSelection = false;
    public bool multiSelect = false;
    public int maxSelected = 60;

    public HashSet<SelectionItem> selection;
    public UnityEvent onSelectionChanged;
    public UnityEvent onSelectionEmpty;

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
            if (keepSelection)
            {
                foreach (SelectionItem child in GetComponentsInChildren<SelectionItem>())
                {
                    child.SetSelected(child == item);
                }
                selection.Clear();
                selection.Add(item);
            }
            else // allows de-selecting currently selected item
            {
                foreach (SelectionItem child in GetComponentsInChildren<SelectionItem>())
                {
                    // only leaving the selected child selected
                    if (child == item && !selection.Contains(item))
                    {
                        child.SetSelected(true);
                        selection.Clear();
                        selection.Add(item);
                    }
                    // de-selecting the selected child if it was selected already
                    else if(child == item && selection.Contains(item))
                    {
                        child.SetSelected(false);
                        selection.Clear();
                    }
                    // de-selecting everything else
                    else
                    {
                        child.SetSelected(false);
                    }
                }
            }
        }

        if (selection.Count == 0)
        {
            onSelectionEmpty?.Invoke();
        }
        else
        {
            onSelectionChanged?.Invoke();
        }
    }
}
