using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TutorialInfo
{
    public void Initialize(int id, string textToDisplay, int maxCount)
    {
        Id = id;
        TextToDisplay = textToDisplay;
        MaxCount = maxCount;
    }

    public int Id;
    public string TextToDisplay;
    public int MaxCount;
}

public class Tutorial : MonoBehaviour
{
    [SerializeField] private float m_EndTutorialDelay = 1f;

    // TODO: DElegate for tutorial finished

    private List<TutorialInfo> m_TutorialInfoList;
    public List<TutorialInfo> TutorialInfoList
    {
        get { return m_TutorialInfoList; }
    }
    
    public void SetupTutorial()
    {
        // TODO:
        // Notify player that tutorial started
        // Spawn anything needed
        // Setup tasks to finish
    }

    public void EndTutorial()
    {
        // TODO:
        // Start delay to go to next tutorial
    }

    public void EndTutorialLogic()
    {
        // TODO:
        // Acutally end tutorial
        // Destroy checklist
        // call delegate that tutorial ended for tutorial manager to go to next tutorial
    }

    public void AddChecklistItem(int id, string textToDisplay, int maxCount)
    {
        // TODO:
        // Create checklist struct item
        // Notify player to add checklist item
    }
}
