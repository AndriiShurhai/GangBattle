using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterActionPanelUI : MonoBehaviour
{
    public static CharacterActionPanelUI Instance { get; private set; }

    [SerializeField] private Button buttonTemplate;
    [SerializeField] private GameObject actionButtonsContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void SetCharacterActionsPanel(CharacterActionsSO characterActions)
    {
        ResetCharacterActionsPanel();

        for (int i = 0; i < characterActions.characterAbilities.Length; i++)
        {
            int index = i;
            Button button = Instantiate(buttonTemplate, actionButtonsContainer.transform);
            button.GetComponent<Image>().sprite = characterActions.characterAbilities[i];
            button.onClick.AddListener(() => OnActionButtonClicked(index));
            button.gameObject.SetActive(true);
        }
    }


    public void ResetCharacterActionsPanel()
    {
        foreach (Transform child in actionButtonsContainer.transform)
        {
            if (child != buttonTemplate.transform) 
            {
                Destroy(child.gameObject);
            }
        }
    }
    private void OnActionButtonClicked(int actionIndex)
    {
        Debug.Log($"Action {actionIndex} clicked");
    }
}
