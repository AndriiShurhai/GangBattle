using UnityEngine;
using UnityEngine.UI;
public class UnitHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    private Camera mainCamera;

    private HealthComponent _healthComponent;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void Initialize(HealthComponent healthComponent)
    {
        _healthComponent = healthComponent;
        healthComponent.OnHealthChanged += UpdateHealth;

        UpdateHealth(healthComponent.CurrentHealth, healthComponent.MaxHealth);
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthSlider.value = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }

    private void OnDestroy()
    {
        _healthComponent.OnHealthChanged -= UpdateHealth;    
    }
}
