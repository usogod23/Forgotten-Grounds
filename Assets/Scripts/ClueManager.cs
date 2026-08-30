using System.Collections.Generic;
using UnityEngine;

public class ClueManager : MonoBehaviour
{

    public List<ClueInfo> collectedClues = new List<ClueInfo>();

    public ClueUIManager clueMenuUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // pornesc si opresc meniul
            if (clueMenuUI != null)
            {
                bool isMenuOpen = !clueMenuUI.gameObject.activeSelf;
                clueMenuUI.gameObject.SetActive(isMenuOpen);

                if (isMenuOpen)
                {
                    // pun pauza la joc si eliberez mouse-ul
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;


                    clueMenuUI.UpdateUI(collectedClues);
                }
                else
                {
                    // se reia jocul
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }

    public void AddClue(ClueInfo newClue)
    {
        bool alreadyHasClue = false;

        foreach (ClueInfo c in collectedClues)
        {
            if (c.ClueName == newClue.ClueName)
            {
                alreadyHasClue = true;
                break;
            }
        }

        if (!alreadyHasClue)
        {
            collectedClues.Add(newClue);
            Debug.Log("Clue added. Total: " + collectedClues.Count);
        }
    }

    public List<string> GetCollectedClueNames()
    {
        List<string> savedNames = new List<string>();
        foreach (ClueInfo clue in collectedClues)
        {
            savedNames.Add(clue.ClueName);
        }

        return savedNames;
    }

    public void RestoreLoadedClues(List<string> loadedNames)
    {
        collectedClues.Clear();

        // pentru a preveni duplicate
        HashSet<string> namesToFind = new HashSet<string>(loadedNames);

        //foreach (string name in loadedNames)
        //{
        //    namesToFind.Add(name);
        //}

        ClueInfo[] allCluesInScene = Resources.FindObjectsOfTypeAll<ClueInfo>();

        foreach (ClueInfo sceneClue in allCluesInScene)
        {
            if (sceneClue.gameObject.scene.IsValid() && namesToFind.Contains(sceneClue.ClueName))
            {
                collectedClues.Add(sceneClue);
                sceneClue.gameObject.SetActive(false);

                namesToFind.Remove(sceneClue.ClueName);
            }
        }

        if (clueMenuUI != null && clueMenuUI.gameObject.activeSelf)
        {
            clueMenuUI.UpdateUI(collectedClues);
        }
    }
}
