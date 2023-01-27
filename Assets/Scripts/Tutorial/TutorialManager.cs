using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<Tutorial> AllTutorialPrefabs;

    private List<Tutorial> AllTutorials;

    protected bool m_isTutorialsRunning = false;
    protected int m_CurrTutorialIndex = 0;

    private void Start()
    {
        // create all tutorials and bind delegates
        foreach (Tutorial tutorialPrefab in AllTutorialPrefabs)
        {
            Tutorial NewTutorial = Instantiate(tutorialPrefab);
            NewTutorial.d_TutorialFinishedDelegate += GotoNextTutorial;
            AllTutorials.Add(NewTutorial);
        }

        GameState.PropertyInstance.d_GameTutorialDelegate += StartTutorials;
    }

    public void StartTutorials()
    {
        m_isTutorialsRunning = true;
        if (AllTutorials.Count == 0)
        {
            // TODO:
            // Notify game state tutorial ended
            return;
        }
        PlayerManager.PropertyInstance.PlayerController.InitializeTutorialUI();
        StartNextTutorial();
    }

    public void GotoNextTutorial()
    {
        m_CurrTutorialIndex++;
        // if all tutorials finished
        if (m_CurrTutorialIndex >= AllTutorials.Count)
        {
            // TODO:
            // Notify game state tutorial ended
        }
        else
        {
            StartNextTutorial();
        }
    }

    // Called to manually end the tutorial section, such as player skipping the tutorial
    public void SetTutorialsFinished()
    {
        PlayerManager.PropertyInstance.PlayerController.TutorialEnded();
        m_isTutorialsRunning = false;
    }

    private void StartNextTutorial()
    {
        AllTutorials[m_CurrTutorialIndex].SetupTutorial();
    }

    List<TutorialInfo> GetCurrentTutorialInfo()
    {
        return AllTutorials[m_CurrTutorialIndex].TutorialInfoList;
    }
}
