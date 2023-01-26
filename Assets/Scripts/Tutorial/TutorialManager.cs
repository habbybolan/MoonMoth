using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<Tutorial> AllTutorials;


    protected bool m_isTutorialsRunning = false;
    protected bool m_IsTutorialEnded = false;
    protected int m_CurrTutorialIndex = 0;

    private void Start()
    {
        // TODO:
        // Spawn all tutorial objects
        // Cache all tutorials
        // Bind to tutorial finishing delegate
    }

    public void StartTutorials()
    {
        // TODO:
        // Initialize player for tutorial
        // Initialize First tutorial
    }

    public void GotoNextTutorial()
    {
        // TODO:
        // Check if all tutorials finished
        // If not, increment index and set delay on going to next tutorial
    }

    // Called to manually end the tutorial section, such as player skipping the tutorial
    public void SetTutorialsFinished()
    {
        // TODO:
        // End current tutorial
        // Set tutorials as not running
    }

    private void StartNextTutorial()
    {
        // TODO:
        // Get next tutorial and set it up
        // notify player next tutorial started
    }

    List<TutorialInfo> GetCurrentTutorialInfo()
    {
        return AllTutorials[m_CurrTutorialIndex].TutorialInfoList;
    }
}
