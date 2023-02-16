using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Checklist : MonoBehaviour
{
    [SerializeField] private CheckListItem m_ChecklistItemPrefab;
    [SerializeField] private ChecklistGroup m_ChecklistGroupPrefab;
    [SerializeField] private GameObject m_TaskContainer;
    [SerializeField] private GameObject m_ChecklistContainer;
    [SerializeField] private TextMeshProUGUI m_TitleText;

    private int m_NumGroupsFinished = 0;
    private int m_NumGroups = 0;

    // Holds groupID and the checklist(Item/Group)
    private Dictionary<int, ChecklistGroup> m_CheckListGroupDictionary;
    private VerticalLayoutGroup m_TaskContainerLayoutGroup;

    private void Awake()
    {
        m_CheckListGroupDictionary = new Dictionary<int, ChecklistGroup>();
        m_TaskContainerLayoutGroup = m_TaskContainer.GetComponent<VerticalLayoutGroup>();
    }

    public void AddChecklistItem(TutorialInfo tutorialInfo)
    {
        ChecklistGroup checklistGroup;
        // If group doesn't yet exist
        if (!m_CheckListGroupDictionary.TryGetValue(tutorialInfo.GroupID, out checklistGroup))
        {
            checklistGroup = Instantiate(m_ChecklistGroupPrefab);
            m_CheckListGroupDictionary.Add(tutorialInfo.GroupID, checklistGroup);
            checklistGroup.transform.SetParent(m_TaskContainerLayoutGroup.transform, false);
            checklistGroup.InitializeGroup(tutorialInfo.GroupID, tutorialInfo.HasGroupTitle, tutorialInfo.GroupName);
            m_NumGroups++;
        }
        checklistGroup.InitializeItem(tutorialInfo.Id, tutorialInfo.TextToDisplay, tutorialInfo.MaxCount, m_TaskContainerLayoutGroup);
    }

    public void NotifyAllItemsAdded()
    {
        StartCoroutine(CRUseMarkLayoutForRebuild());
    }

    private IEnumerator CRUseMarkLayoutForRebuild()
    {
        yield return new WaitForEndOfFrame();

        foreach (var layoutGroup in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
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
        foreach (Transform child in m_TaskContainer.transform)
        {
            Destroy(child.gameObject);
        }
        m_CheckListGroupDictionary.Clear();
    }

    /**
     * Update a checklist item, in group or outside
     * @param itemID    ID of checklist item to update
     * @param groupID   ID of group that holds the checklist to update. -1 if the checklist has no group
     */
    public bool UpdateChecklistItem(int itemID, int groupID)
    {
        if (m_CheckListGroupDictionary.TryGetValue(groupID, out ChecklistGroup checklistItem))
        {
            if (checklistItem.UpdateChecklistItem(itemID))
            {
                m_NumGroupsFinished++;
            }
        }
        return m_NumGroupsFinished >= m_NumGroups;
    }

    public void UpdateTitle(string title)
    {
        m_TitleText.text = title;
    }
}
