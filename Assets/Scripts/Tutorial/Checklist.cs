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

    [Header("Skipping")]
    [SerializeField] private Button m_SkipButton;
    [SerializeField] private Image m_SkipImageMobile;
    [SerializeField] private Image m_SkipImagePC;
    [SerializeField] private Image m_SkipFillBar;

    private int m_NumGroupsFinished = 0;
    private int m_NumGroups = 0;

    private Coroutine m_SkipCoroutine;
    
    public delegate void SkipButtonDownDelegate();
    public SkipButtonDownDelegate skipButtonDownDelegate;
    public delegate void SkipButtonUpDelegate();
    public SkipButtonUpDelegate skipButtonUpDelegate;

    public Button SkipButton
    {
        get { return m_SkipButton; }
    }

    private float m_CurrSkipAmount = 0;

    // Holds groupID and the checklist(Item/Group)
    private Dictionary<int, ChecklistGroup> m_CheckListGroupDictionary;
    private VerticalLayoutGroup m_TaskContainerLayoutGroup;

    private bool m_IsSkipping = false;

    private void Awake()
    {
        m_CheckListGroupDictionary = new Dictionary<int, ChecklistGroup>();
        m_TaskContainerLayoutGroup = m_TaskContainer.GetComponent<VerticalLayoutGroup>();

        // Set proper skip graphics as enabled
        m_SkipImagePC.enabled = false;
        m_SkipImageMobile.enabled = false;
        m_SkipFillBar.fillAmount = 0;

#if UNITY_STANDALONE_WIN
        m_SkipImageMobile.enabled = true;
#elif UNITY_ANDROID
        m_SkipImagePC.enabled = true;
#endif
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

    public void StartSkipping(float SkipLength)
    {
        if (m_IsSkipping) StopSkipping();
        m_SkipCoroutine = StartCoroutine(SkipGraphicCoroutine(SkipLength));
    }

    private IEnumerator SkipGraphicCoroutine(float SkipLength)
    {
        m_IsSkipping = true;
        while (m_CurrSkipAmount < SkipLength)
        {
            m_CurrSkipAmount += Time.deltaTime;
            float SkipPercent = m_CurrSkipAmount / SkipLength;
            m_SkipFillBar.fillAmount = SkipPercent;

            yield return null;
        }
    }

    public void StopSkipping()
    {
        m_IsSkipping = false;
        StopCoroutine(m_SkipCoroutine);
        m_SkipFillBar.fillAmount = 0;
        m_CurrSkipAmount = 0;
    }

    public void OnSkipButtonDown()
    {
        skipButtonDownDelegate();
    }

    public void OnSkipButtonUp()
    {
        skipButtonUpDelegate();
    }
}
