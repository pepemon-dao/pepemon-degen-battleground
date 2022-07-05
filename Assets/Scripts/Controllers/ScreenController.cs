using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup)), RequireComponent(typeof(Animator))]
public class ScreenController : MonoBehaviour
{
    #region Enums
    protected enum ScreenState { SHOWN, HIDDEN }
    #endregion

    #region Editor Exposed Data
    [Title("Screen Settings")]
    [SerializeField] bool _startHidden = false;


    [Title("Screen Events")]
    private UnityEvent OnShown;
    private UnityEvent OnHidden;

    [Title("Debugging")]
    [Button(Name = "DEBUG Show")] private void Debug_Show() => Show();
    [Button(Name = "DEBUG Hide")] private void Debug_Hide() => Hide();
    #endregion

    #region Object Data
    protected ScreenState _state = ScreenState.SHOWN;

    protected bool _animating = false;
    #endregion

    #region Onboard Components
    protected CanvasGroup _canvasGroup;
    protected Animator _animator;
    #endregion

    #region INIT
    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        _state = _startHidden ? ScreenState.HIDDEN : ScreenState.SHOWN;
    }

    private void OnValidate()
    {
        var cg = GetComponent<CanvasGroup>();
        if (_startHidden)
        {
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        else
        {
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
    #endregion

    #region State Management Logic
    protected async virtual void UpdateState(ScreenState newState)
    {
        if (_animating) return;
        if (newState == _state) return;
        _state = newState;
        _animating = true;

        switch (_state)
        {
            case ScreenState.SHOWN:
                await Show_Task();
                OnShown?.Invoke();
                break;
            case ScreenState.HIDDEN:
                await Hide_Task();
                OnHidden?.Invoke();
                break;
        }

        _animating = false;
    }

    public virtual void Show() => UpdateState(ScreenState.SHOWN);
    public virtual void Hide() => UpdateState(ScreenState.HIDDEN);
    #endregion

    #region Show/Hide Logic
    protected async virtual Task Show_Task()
    {
        _animator.SetTrigger("OnShow");

        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Show Complete"))
            await Task.Delay(100);
    }
    protected async virtual Task Hide_Task()
    {
        _animator.SetTrigger("OnHide");

        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Hide Complete"))
            await Task.Delay(100);
    }
    #endregion
}
