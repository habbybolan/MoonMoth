using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checklist : MonoBehaviour
{
    [SerializeField] private CheckListItem m_ChecklistItemPrefab;
    [SerializeField] private GameObject m_ChecklistContainer;
    
    private Dictionary<int, CheckListItem> m_CheckListDictionary;
    private VerticalLayoutGroup m_ChecklistVerticalContainer;

    private void Awake()
    {
        m_CheckListDictionary = new Dictionary<int, CheckListItem>();
        m_ChecklistVerticalContainer = GetComponentInChildren<VerticalLayoutGroup>();
    }

    public void AddChecklistItem(TutorialInfo tutorialInfo)
    {
        CheckListItem checklistItem = Instantiate(m_ChecklistItemPrefab);
        checklistItem.InitializeItem(tutorialInfo.TextToDisplay, tutorialInfo.MaxCount);
        checklistItem.transform.SetParent(m_ChecklistContainer.transform);

        m_CheckListDictionary.Add(tutorialInfo.Id, checklistItem);
    }

    public void AddChecklistItems(List<TutorialInfo> tutorialInfoList)
    {
        foreach (TutorialInfo info in tutorialInfoList)
        {
            AddChecklistItem(info);
        }
    }

    public void Reset()
    {
        foreach (Transform child in m_ChecklistContainer.transform)
        {
            Destroy(child.gameObject);
        }
        m_CheckListDictionary.Clear();
    }

    public void UpdateChecklistItem(int itemID)
    {
        if (m_CheckListDictionary.TryGetValue(itemID, out CheckListItem checklistItem))
        {
            checklistItem.UpdateText();
        }
    }
}
