using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckListItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI m_ItemTextbox;

    private int m_MaxCount;
    private int m_CurrCount = 0;
    private string m_ItemText;

    public void InitializeItem(string text, int maxCount)
    {
        m_ItemText = text;
        m_MaxCount = maxCount;
        SetText();
    }

    public void UpdateText()
    {
        m_CurrCount++;
        SetText();
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
