using System;
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

public abstract class Tutorial : MonoBehaviour
{
    [SerializeField] private float m_EndTutorialDelay = 1f;

    public delegate void TutorialFinishedDelegate();
    public TutorialFinishedDelegate d_TutorialFinishedDelegate;

    private List<(int, int)> ChecklistCounter; // index corresponds to the checklist id, values is the curr and max count

    private List<TutorialInfo> m_TutorialInfoList;
    public List<TutorialInfo> TutorialInfoList
    {
        get { return m_TutorialInfoList; }
    }
    
    virtual public void SetupTutorial()
    {
        // TODO:
        // Spawn anything needed
        // Setup tasks to finish
    }

    public void EndTutorial()
    {
        Invoke("EndTutorialLogic", m_EndTutorialDelay);
    }

    virtual public void EndTutorialLogic()
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist != null)
        {
            checklist.Reset();
        }
        // TODO:
        // Acutally end tutorial
        // Destroy checklist
        // call delegate that tutorial ended for tutorial manager to go to next tutorial
    }

    public void UpdateTutorial(int checklistId)
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist != null) 
        {
            checklist.UpdateChecklistItem(checklistId);
        }
        (int, int) Checker = ChecklistCounter[checklistId];
        Checker.Item1 = Checker.Item1 + 1;
        if (IsTutorialFinished())
        {
            d_TutorialFinishedDelegate();
        }
    }

    private bool IsTutorialFinished()
    {
        foreach ((int, int) Checker in ChecklistCounter)
        {
            if (Checker.Item1 >= Checker.Item2) return true;
        }
        return false;
    }

    public void AddChecklistItem(int id, string textToDisplay, int maxCount)
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist == null)
        {
            throw new NullReferenceException("Cannot add to a checklist before it's initialized for the player.");
        }
        TutorialInfo info = new TutorialInfo();
        info.Initialize(id, textToDisplay, maxCount);
        checklist.AddChecklistItem(info);
    }
}
