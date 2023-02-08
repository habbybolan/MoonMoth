using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckListItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI m_ItemTextbox;

    private int m_MaxCount;
    private int m_CurrCount = 0;
    private string m_ItemText;
    private int ID;
    private int m_GroupID;

    private bool m_IsFinished = false;

    public bool GetIsFinished()
    {
        return false;
    }

    public void InitializeItem(int id, string text, int maxCount, VerticalLayoutGroup taskContainer, int groupID)
    {
        ID = id;
        m_GroupID = groupID;
        m_ItemText = text;
        m_MaxCount = maxCount;
        SetText();
    }

    /**
     * Update checklist and checks if checklist just finished
     * @returns True if the checklist just finished, false if not finished yet or was finished previously
     */
    public bool UpdateChecklistItem()
    {
        m_CurrCount++;
        SetText();
        if (!m_IsFinished && m_CurrCount >= m_MaxCount)
        {
            m_IsFinished = true;
            return true;
        }
        return false;
    }
    
    private void SetText()
    {
        string text;
        if (m_MaxCount <= 0)
        {
            text = $" - {m_ItemText}";
        } else
        {
            text = $" - {m_ItemText} {m_CurrCount} / {m_MaxCount}";
        }
        
        // Check if checklist item finished
        if (m_CurrCount > 0 && m_CurrCount >= m_MaxCount)
        {
            text = $"<s>{text}</s>";
        }
        m_ItemTextbox.text = text;
    }
}
