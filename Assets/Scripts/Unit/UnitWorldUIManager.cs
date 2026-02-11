using DG.Tweening;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class UnitWorldUIManager : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private GameObject healthPopupPrefab;

    private Dictionary<Unit, UnitHealthUI> activeHealthBars = new();


    private void OnEnable()
    {
        Unit.OnAnyUnitSpawned += HandleUnitSpawned;
        Unit.OnAnyUnitDied += HandleUnitDied;
        Unit.OnAnyUnitTookDamage += HandleUnitTookDamage;
        Unit.OnAnyUnitHealed += HandleUnitHealed;
    }

    private void OnDisable()
    {
        Unit.OnAnyUnitSpawned -= HandleUnitSpawned;
        Unit.OnAnyUnitDied -= HandleUnitDied;
        Unit.OnAnyUnitTookDamage -= HandleUnitTookDamage;
        Unit.OnAnyUnitHealed -= HandleUnitHealed;   
    }

    private void HandleUnitSpawned(Unit unit)
    {
        Transform healthBarAttachPoint = unit.GetHealthBarAttachPoint();
        GameObject barObj = Instantiate(healthBarPrefab, healthBarAttachPoint.position, Quaternion.identity, healthBarAttachPoint);

        UnitHealthUI healthUI = barObj.GetComponent<UnitHealthUI>();
        healthUI.Initialize(unit.Health);

        activeHealthBars.Add(unit, healthUI);
    }

    private void HandleUnitDied(Unit unit)
    {
        if (activeHealthBars.TryGetValue(unit, out UnitHealthUI healthUI))
        {
            Destroy(healthUI.gameObject);
            activeHealthBars.Remove(unit);
        }
    }

    private void HandleUnitHealed(Unit unit, int amount, int currentHealth)
    {

        float offsetX = UnityEngine.Random.Range(-1f, 1f);
        float offsetY = -offsetX;

        Vector3 spawnPosition = new Vector3(unit.transform.position.x + offsetX, unit.transform.position.y + offsetY, unit.transform.position.z);
        GameObject textPopup = Instantiate(healthPopupPrefab, spawnPosition, Quaternion.identity);

        textPopup.GetComponent<HpPopupText>().Setup(amount, Color.green);
    }

    private void HandleUnitTookDamage(Unit unit, int amount, int currentHealth)
    {
        unit.transform.DOShakePosition(0.3f, 0.3f, 10, 90, false, true);


        float offsetX = UnityEngine.Random.Range(-1f, 1f);
        float offsetY = -offsetX;

        Vector3 spawnPosition = new Vector3(unit.transform.position.x + offsetX, unit.transform.position.y + offsetY, unit.transform.position.z);
        GameObject textPopup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);

        textPopup.GetComponent<HpPopupText>().Setup(amount, Color.red);
    }
}
