using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChecklistGroup : MonoBehaviour
{
    [SerializeField] private CheckListItem m_ChecklistItemPrefab;
    [SerializeField] private VerticalLayoutGroup m_ItemLayoutGroup;
    [SerializeField] private TextMeshProUGUI m_GroupTitle;

    // Keeps track of all checklist items added
    private Dictionary<int, CheckListItem> m_CheckListDictionary;

    private int m_GroupID;

    private int m_Numitems = 0;
    private int m_NumFinishedItems = 0;

    private bool m_IsFinished = false;

    private void Awake()
    {
        m_CheckListDictionary = new Dictionary<int, CheckListItem>();
    }

    public bool GetIsFinished()
    {
        return false;
    }

    public void InitializeGroup(int groupID, bool hasTitle = false, string groupTitle = "")
    {
        if (hasTitle)
        {
            m_GroupTitle.text = groupTitle;
        } else
        {
            // if no title, then remove left offset
            Destroy(m_GroupTitle.gameObject);
            m_ItemLayoutGroup.padding.left = 0;
        }
        m_GroupID = groupID;
    }

    public void InitializeItem(int itemID, string text, int maxCount, VerticalLayoutGroup taskContainer)
    {
        CheckListItem checklistItem = Instantiate(m_ChecklistItemPrefab);
        checklistItem.InitializeItem(itemID, text, maxCount, taskContainer, m_GroupID);
        checklistItem.transform.SetParent(m_ItemLayoutGroup.transform, false);
        m_CheckListDictionary.Add(itemID, checklistItem);
        m_Numitems++;
    }

    public bool UpdateChecklistItem(int itemID)
    {
        if (m_CheckListDictionary.TryGetValue(itemID, out CheckListItem checklistItem))
        {
            if (checklistItem.UpdateChecklistItem())
            {
                m_NumFinishedItems++;
            }
        }

        // strike out group title if all checklist items finished
        if (!m_IsFinished && m_NumFinishedItems >= m_Numitems)
        {
            m_IsFinished = true;
            m_GroupTitle.text = $"<s>{m_GroupTitle.text}</s>";
            return true;
        }
        return false;
    }
}
