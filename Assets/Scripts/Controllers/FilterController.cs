using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterController : MonoBehaviour
{
    [SerializeField] private ScreenEditDeck deck;
    [SerializeField] private GameObject filterGo;

    public static FilterController Instance;

    public int currentFilter { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetFilter(0);
    }

    public void SetFilter(int filter)
    {
        currentFilter = filter;
        deck.FilterCards(filter);
        FilterToggle();
    }

    public void FilterToggle()
    {
        filterGo.SetActive(!filterGo.activeSelf);
    }
}
