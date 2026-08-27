using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SaveLoadMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private PlayerPositionSaveSystem saveSystem;
    [SerializeField] private CursorLocker cursorLocker;
    [SerializeField] private Behaviour[] gameplayBehaviours;

    private bool isOpen;
    private float timeScaleBeforeMenu = 1f;
    private bool[] gameplayBehaviourStates;

    private void Awake()
    {
        if (menuPanel == null || saveButton == null || loadButton == null || saveSystem == null)
        {
            Debug.LogError("The Save/Load menu is not fully configured in the Inspector.", this);
            enabled = false;
            return;
        }

        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        gameplayBehaviourStates = new bool[gameplayBehaviours.Length];
        menuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        SetMenuOpen(!isOpen);
    }

    private void OnDestroy()
    {
        saveButton?.onClick.RemoveListener(SaveGame);
        loadButton?.onClick.RemoveListener(LoadGame);

        if (isOpen)
        {
            RestoreGameplayState();
        }
    }

    private void SaveGame()
    {
        bool success = saveSystem.SavePosition(out string message);
        ShowStatus(message, success);
        loadButton.interactable = saveSystem.HasSave;
    }

    private void LoadGame()
    {
        bool success = saveSystem.LoadPosition(out string message);
        ShowStatus(message, success);

        if (success)
        {
            SetMenuOpen(false);
        }
    }

    private void SetMenuOpen(bool shouldOpen)
    {
        if (isOpen == shouldOpen)
        {
            return;
        }

        isOpen = shouldOpen;
        menuPanel.SetActive(isOpen);

        if (isOpen)
        {
            timeScaleBeforeMenu = Time.timeScale;
            Time.timeScale = 0f;

            if (cursorLocker != null)
            {
                cursorLocker.enabled = false;
                cursorLocker.UnlockCursor();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            for (int index = 0; index < gameplayBehaviours.Length; index++)
            {
                Behaviour behaviour = gameplayBehaviours[index];
                gameplayBehaviourStates[index] = behaviour != null && behaviour.enabled;
                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }
            }

            loadButton.interactable = saveSystem.HasSave;
            ShowStatus(string.Empty, true);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(saveButton.gameObject);
            }
        }
        else
        {
            RestoreGameplayState();
        }
    }

    private void RestoreGameplayState()
    {
        Time.timeScale = timeScaleBeforeMenu;

        if (cursorLocker != null)
        {
            cursorLocker.enabled = true;
            if (IsAltHeld())
            {
                cursorLocker.UnlockCursor();
            }
            else
            {
                cursorLocker.LockCursor();
            }
        }
        else
        {
            bool altHeld = IsAltHeld();
            Cursor.lockState = altHeld ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = altHeld;
        }

        for (int index = 0; index < gameplayBehaviours.Length; index++)
        {
            if (gameplayBehaviours[index] != null)
            {
                gameplayBehaviours[index].enabled = gameplayBehaviourStates[index];
            }
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ShowStatus(string message, bool success)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusText.color = success
            ? new Color(0.65f, 0.9f, 0.68f)
            : new Color(1f, 0.55f, 0.55f);
    }

    private static bool IsAltHeld()
    {
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }
}
