using Pepemon.Battle;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPreviewController : MonoBehaviour
{
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private GameObject gameCard;
    [SerializeField] private GameObject pepemonCard;

    [Header("Game Card")]
    [SerializeField] private Image _cardImg;

    // Optional. Until these are wired in the scene the preview shows artwork only, which is
    // why players could not read what any support card actually did.
    [SerializeField] private TextMeshProUGUI _cardNameText;
    [SerializeField] private TextMeshProUGUI _cardDescriptionText;

    [Header("Pepemon Card")]
    [BoxGroup("Images"), SerializeField] private Image _backDropImage;
    [BoxGroup("Images"), SerializeField] private Image _cardContent;

    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _nameText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _hpText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _levelText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _atkText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _sAtkText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _defText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _sDefText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _spdText;
    [BoxGroup("Text"), SerializeField] private TextMeshProUGUI _intText;

    private float clickTime;
    private float doubleClickTimeThreshold = 0.2f;

    private GameObject currentObject;
    private GameObject prevObject;

    private bool isInPreview = false;

    private void Update()
    {
        // A visible tutorial beat owns the screen. Without this, the tap that dismisses a
        // beat also lands here, and closing the preview would unfreeze the battle running
        // behind a modal beat.
        if (BotTextTutorial.Instance != null && BotTextTutorial.Instance.IsBeatVisible)
        {
            return;
        }

        if (isInPreview)
        {
            if (Input.GetMouseButtonDown(0) || HasTouchBegan())
            {
                HidePreview();
                clickTime = 0f;
                return;
            }
            else
            {
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Unscaled: Time.time does not advance while the game is frozen.
            if (Time.unscaledTime - clickTime < doubleClickTimeThreshold)
            {
                GetObject(out currentObject);
                //check if the objects doesn't changed between the clicks
                if (currentObject == prevObject)
                {
                    SetPreview(currentObject);
                }
            }
            else
            {
                GetObject(out currentObject);
                prevObject = currentObject;
            }
            clickTime = Time.unscaledTime;
        }

        if (Input.GetMouseButtonDown(1))
        {
            GetObject(out currentObject);
            SetPreview(currentObject);
        }

        HandleTouchPreview();
    }

    /// <summary>Longest touch still counted as a tap rather than a fast-forward hold.</summary>
    private const float MaxTapSeconds = 0.3f;

    private readonly Dictionary<int, float> _touchStartTimes = new Dictionary<int, float>();

    /// <summary>
    /// Touch has no right-click, so a quick tap directly on a card opens its preview.
    ///
    /// Resolved on Ended and duration-limited: a held touch is the battle fast-forward gesture,
    /// and previewing on Began would freeze the game the moment the player tried to speed it up.
    /// Touch duration is tracked here because Touch.deltaTime is the time since the last frame,
    /// not how long the finger has been down.
    /// </summary>
    private void HandleTouchPreview()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                _touchStartTimes[touch.fingerId] = Time.unscaledTime;
                continue;
            }

            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) continue;

            var wasQuickTap = _touchStartTimes.TryGetValue(touch.fingerId, out var startedAt)
                              && Time.unscaledTime - startedAt <= MaxTapSeconds;

            _touchStartTimes.Remove(touch.fingerId);

            if (touch.phase == TouchPhase.Canceled || !wasQuickTap) continue;

            GetObject(out currentObject, touch.position);
            if (currentObject != null)
            {
                SetPreview(currentObject);
                return;
            }
        }
    }

    /// <summary>
    /// Touch devices have no right-click and no reliable double-click, so a begun touch is
    /// treated as a preview gesture. Mouse emulation does not fire on every mobile browser,
    /// which is why this is checked explicitly rather than relying on GetMouseButtonDown.
    /// </summary>
    private bool HasTouchBegan()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began) return true;
        }
        return false;
    }

    private void HidePreview()
    {
        TimeControl.Unfreeze(TimeControl.HolderCardPreview);
        isInPreview = false;
        previewPanel.SetActive(false);
        gameCard.SetActive(false);
        pepemonCard.SetActive(false);
    }

    private void GetObject(out GameObject currentObject)
    {
        GetObject(out currentObject, Input.mousePosition);
    }

    private void GetObject(out GameObject currentObject, Vector2 screenPosition)
    {
        currentObject = null;

        if (EventSystem.current == null) return;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = screenPosition;
        List<RaycastResult> raycastResultList = new List<RaycastResult>(); 
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            var go = raycastResultList[i].gameObject;
            bool pepemonCard = go.GetComponentInParent<PepemonCardController>() != null;
            bool card = go.GetComponent<CardController>() != null;
            bool cardInDeck = go.GetComponentInParent<CardPreview>() != null;

            if (card)
            {
                currentObject = go;
            }
            else if (pepemonCard)
            {
                currentObject = go.transform.parent.gameObject;
            }
            else if (cardInDeck)
            {
                currentObject = go;
            }
        }
    }

    private void SetPreview(GameObject cardGo)
    {
        if (cardGo == null)
        {
            return;
        }

        isInPreview = true;
        PepemonCardController cardPepemon = cardGo.GetComponentInParent<PepemonCardController>();
        CardController card = cardGo.GetComponent<CardController>();
        CardPreview cardInDeck = cardGo.GetComponentInParent<CardPreview>();

        if (card != null)
        {
            SetCardPreview(card);
        } else if (cardPepemon != null)
        {
            SetPepemonCardPreview(cardPepemon);
        } else if (cardInDeck != null)
        {
            SetCardInDeckPreview(cardInDeck);
        }

        previewPanel.SetActive(true);

        TimeControl.Freeze(TimeControl.HolderCardPreview);
    }

    private void SetPepemonCardPreview(PepemonCardController card)
    {
        PopulateCard(card.BattleCard);
        pepemonCard.SetActive(true);
    }
    
    private void SetCardInDeckPreview(CardPreview card)
    {
        _cardImg.sprite = card._cardImage.sprite;

        bool isPepemon = card._hpText != null;
        if (isPepemon)
        {
            SetPepemonCardPreview(card);
            pepemonCard.SetActive(true);
        }

        gameCard.SetActive(!isPepemon);
    }
    
    private void SetCardPreview(CardController card)
    {
        _cardImg.sprite = card.HostedCard.CardEffectSprite;

        if (_cardNameText != null) _cardNameText.text = card.HostedCard.DisplayName;
        if (_cardDescriptionText != null) _cardDescriptionText.text = card.HostedCard.CardDescription;

        gameCard.SetActive(true);
    }

    public void SetPepemonCardPreview(CardPreview card)
    {
        _nameText.text = card._text.text;
        _hpText.text = card._hpText.text;
        _levelText.text = card._lvlText.text;
        _atkText.text = card._attackText.text;
        _sAtkText.text = card._sattackText.text;
        _defText.text = card._defendText.text;
        _sDefText.text = card._sdefendText.text;
        _spdText.text = card._speedText.text;
        _intText.text = card._intellegenceText.text;

        _backDropImage.sprite = card._cardImage.sprite;

    }

    public void PopulateCard(BattleCard pepemonData)
    {
        _nameText.text = pepemonData.name;
        _hpText.text = pepemonData.HealthPoints.ToString();
        _levelText.text = pepemonData.Level;
        _atkText.text = pepemonData.Attack.ToString();
        _sAtkText.text = pepemonData.SAttack.ToString();
        _defText.text = pepemonData.Defense.ToString();
        _sDefText.text = pepemonData.SDeffense.ToString();
        _spdText.text = pepemonData.Speed.ToString();
        _intText.text = pepemonData.Intelligence.ToString();

        if (pepemonData.CardContentBackdrop != null)
        {
            _backDropImage.sprite = pepemonData.CardContentBackdrop;
        }
        if (pepemonData.CardContent != null)
        {
            _cardContent.sprite = pepemonData.CardContent;
        }   
    }
}
