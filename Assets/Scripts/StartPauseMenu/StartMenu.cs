using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private CursorLocker cursorLocker;
    [SerializeField] private Behaviour[] gameplayBehaviours;

    private void Awake()
    {
        if (menuPanel == null)
        {
            Debug.LogError("StartMenu: menuPanel nu e setat in Inspector.", this);
            enabled = false;
            return;
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        Time.timeScale = 0f;
        menuPanel.SetActive(true);

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

        foreach (Behaviour b in gameplayBehaviours)
        {
            if (b != null) b.enabled = false;
        }

        if (EventSystem.current != null && startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    private void OnDestroy()
    {
        startButton?.onClick.RemoveListener(StartGame);
        quitButton?.onClick.RemoveListener(QuitGame);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        menuPanel.SetActive(false);

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

        foreach (Behaviour b in gameplayBehaviours)
        {
            if (b != null) b.enabled = true;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}