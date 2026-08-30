using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClueUIManager : MonoBehaviour
{
    [Header("Left Side - Scroll List")]
    public Transform clueListContent;
    public GameObject clueButtonPrefab;

    [Header("Right Side - Inspector")]
    public TextMeshProUGUI inspectionNameText;
    public TextMeshProUGUI inspectionDescriptionText;

    // pentru cand voi introduce o imagine de inspect
    // public Image inspectionImage;
    
    public void UpdateUI(List<ClueInfo> collectedClues)
    {
        // am gtija sa nu creez butoane duplicate de fiecare data cand apas pe c
        foreach (Transform child in clueListContent)
        {
            Destroy(child.gameObject);
        }

        // parcurg toate indiciile gasite de jucator
        foreach (ClueInfo clue in collectedClues)
        {
            GameObject newButton = Instantiate(clueButtonPrefab, clueListContent);

            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = clue.ClueName;
            }

            Button buttonComponent = newButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => DisplayClueDetails(clue));
            }
        }

        // afisez automat primul indiciu daca acesta exista
        if (collectedClues.Count > 0)
        {
            DisplayClueDetails(collectedClues[0]);
        }
        else
        {
            ClearInspectionPanel();
        }
    }

    public void DisplayClueDetails(ClueInfo clue)
    {
        inspectionNameText.text = clue.ClueName;
        inspectionDescriptionText.text = clue.clueDescription;
        // pentru cand voi avea imagine
        // inspectionImage.sprite = clue.clueIcon;
    }

    private void ClearInspectionPanel()
    {
        inspectionNameText.text = "No clues found";
        inspectionDescriptionText.text = "???";
    }

}
