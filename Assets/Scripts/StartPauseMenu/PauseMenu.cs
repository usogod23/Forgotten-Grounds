using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PauseMenu : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.M;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private CursorLocker cursorLocker;
    [SerializeField] private Behaviour[] gameplayBehaviours;
    [SerializeField] private GameObject[] hudElements;

    private bool isOpen;
    private float timeScaleBeforeMenu = 1f;
    private bool[] gameplayBehaviourStates;

    private void Awake()
    {
        if (menuPanel == null)
        {
            Debug.LogError("PauseMenu: menuPanel nu e setat in Inspector.", this);
            enabled = false;
            return;
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        gameplayBehaviourStates = new bool[gameplayBehaviours.Length];
        menuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    private void OnDestroy()
    {
        resumeButton?.onClick.RemoveListener(Resume);
        quitButton?.onClick.RemoveListener(QuitGame);
    }

    public void ToggleMenu()
    {
        SetMenuOpen(!isOpen);
    }

    public void Resume()
    {
        SetMenuOpen(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

            for (int i = 0; i < gameplayBehaviours.Length; i++)
            {
                Behaviour b = gameplayBehaviours[i];
                gameplayBehaviourStates[i] = b != null && b.enabled;
                if (b != null) b.enabled = false;
            }

            SetHudVisible(false);

            if (EventSystem.current != null && resumeButton != null)
            {
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
        }
        else
        {
            Time.timeScale = timeScaleBeforeMenu;

            if (cursorLocker != null)
            {
                cursorLocker.enabled = true;
                cursorLocker.LockCursor();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            for (int i = 0; i < gameplayBehaviours.Length; i++)
            {
                if (gameplayBehaviours[i] != null)
                {
                    gameplayBehaviours[i].enabled = gameplayBehaviourStates[i];
                }
            }

            SetHudVisible(true);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void SetHudVisible(bool visible)
    {
        foreach (GameObject hud in hudElements)
        {
            if (hud != null) hud.SetActive(visible);
        }
    }
}