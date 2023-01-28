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

    private List<(int, int)> ChecklistCounter = new List<(int, int)>(); // index corresponds to the checklist id, values is the curr and max count

    private List<TutorialInfo> m_TutorialInfoList;
    public List<TutorialInfo> TutorialInfoList
    {
        get { return m_TutorialInfoList; }
    }

    protected int CurrID = 0;
    private bool m_IsEnded = false;
    
    virtual public void SetupTutorial()
    {
        // Override in child
        m_IsEnded = false;
    }

    public void EndTutorial()
    {
        if (m_IsEnded) return;

        m_IsEnded = true;
        Invoke("EndTutorialLogic", m_EndTutorialDelay);
    }

    virtual public void EndTutorialLogic()
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist != null)
        {
            checklist.Reset();
        }
        d_TutorialFinishedDelegate();
    }

    public void UpdateTutorial(int checklistId)
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist != null) 
        {
            checklist.UpdateChecklistItem(checklistId);
        }
        ChecklistCounter[checklistId] = (ChecklistCounter[checklistId].Item1 + 1, ChecklistCounter[checklistId].Item2);
        if (IsTutorialFinished())
        {
            EndTutorial();
        }
    }

    private bool IsTutorialFinished()
    {
        foreach ((int, int) Checker in ChecklistCounter)
        {
            if (Checker.Item1 <= 0 || Checker.Item1 < Checker.Item2) return false;
        }
        return true;
    }

    public void AddChecklistItem(string textToDisplay, int maxCount)
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist == null)
        {
            throw new NullReferenceException("Cannot add to a checklist before it's initialized for the player.");
        }
        TutorialInfo info = new TutorialInfo();
        info.Initialize(CurrID++, textToDisplay, maxCount);
        checklist.AddChecklistItem(info);
        ChecklistCounter.Add((0, maxCount));
    }

    public virtual void ReceiveTutorialInput(TutorialInputs input)
    {
        // Override in child
    }
}
