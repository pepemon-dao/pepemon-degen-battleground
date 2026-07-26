using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _healthFrontImage;
    [SerializeField] private UnityEngine.UI.Image _healthBackImage;

    private int _health = 20;

    private float chipSpeed = 1f;
    private float starterHealth;
    private float lerpTimer;

    public bool IsDead { get; private set; } = false;

    public void SetHealth(int value)
    {
        _health = value;
        starterHealth = _health;
    }

    public void TakeDamage(int value)
    {
        if (IsDead) return;

        if (_health - value <= 0)
        {
            _health = 0;
            IsDead = true;
        }
        else
        {
            _health -= value;
        }

        // Restart the drain in both cases. The lethal branch used to return before this, so
        // the killing blow - the single most dramatic moment in the battle - snapped the bar
        // to empty instead of animating.
        lerpTimer = 0f;
    }

    private void Update()
    {
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (_healthBackImage == null || _healthFrontImage == null) return;
        if (starterHealth <= 0f) return;

        float fillB = _healthBackImage.fillAmount;
        float hFraction = _health / starterHealth;

        if (fillB > hFraction)
        {
            _healthFrontImage.fillAmount = hFraction;
            _healthBackImage.color = Color.white;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            _healthBackImage.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
    }
}
