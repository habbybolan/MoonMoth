using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TutorialInfo
{
    public void Initialize(int id, string textToDisplay, int maxCount, int groupID, bool hasGroupTitle = false, string groupName = "")
    {
        Id = id;
        TextToDisplay = textToDisplay;
        MaxCount = maxCount;
        GroupID = groupID;
        HasGroupTitle = hasGroupTitle;
        GroupName = groupName;
    }

    public int Id;
    public string TextToDisplay;
    public int MaxCount;
    public int GroupID;
    public bool HasGroupTitle;
    public string GroupName;
}

public abstract class Tutorial : MonoBehaviour
{
    [SerializeField] private float m_EndTutorialDelay = 1f;
    [SerializeField] private string m_Title;

    public delegate void TutorialFinishedDelegate();
    public TutorialFinishedDelegate d_TutorialFinishedDelegate;

    private List<(int, int)> ChecklistCounter = new List<(int, int)>(); // index corresponds to the checklist id, values is the curr and max count

    private List<TutorialInfo> m_TutorialInfoList;
    public List<TutorialInfo> TutorialInfoList
    {
        get { return m_TutorialInfoList; }
    }

    protected int CurrID = 0;
    private bool m_IsRunning = false;
    protected bool IsRunning
    {
        get { return m_IsRunning; }
    }

    protected PlayerController m_PlayerController;

    private void Start()
    {
        m_PlayerController = PlayerManager.PropertyInstance.PlayerController;
    }

    virtual public void SetupTutorial()
    {
        // Override in child
        m_IsRunning = true;
        SetTitle();
    }

    public void EndTutorial()
    {
        if (!m_IsRunning) return;
        m_IsRunning = false;
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

    public void UpdateTutorial(int checklistId, int groupId)
    {
        if (!m_IsRunning) return;

        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        bool isChecklistFinished = false;
        if (checklist != null) 
        {
            isChecklistFinished = checklist.UpdateChecklistItem(checklistId, groupId);
        }
        ChecklistCounter[checklistId] = (ChecklistCounter[checklistId].Item1 + 1, ChecklistCounter[checklistId].Item2);
        if (isChecklistFinished)
        {
            EndTutorial();
        }
    }

    public void AddChecklistItem(string textToDisplay, int maxCount, int groupID, bool hasGroupTitle = false, string groupName = "")
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist == null)
        {
            throw new NullReferenceException("Cannot add to a checklist before it's initialized for the player.");
        }
        TutorialInfo info = new TutorialInfo();
        info.Initialize(CurrID++, textToDisplay, maxCount, groupID, hasGroupTitle, groupName);
        checklist.AddChecklistItem(info);
        ChecklistCounter.Add((0, maxCount));
    }

    public virtual void ReceiveTutorialInput(TutorialInputs input)
    {
        // Override in child
    }

    protected void SetTitle()
    {
        Checklist checklist = PlayerManager.PropertyInstance.PlayerController.Checklist;
        if (checklist != null)
        {
            checklist.UpdateTitle(m_Title);
        }
    }
}
