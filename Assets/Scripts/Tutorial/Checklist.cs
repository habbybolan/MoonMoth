using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements.Experimental;

public class Checklist : MonoBehaviour
{
    [SerializeField] private CheckListItem m_ChecklistItemPrefab;
    [SerializeField] private ChecklistGroup m_ChecklistGroupPrefab;
    [SerializeField] private GameObject m_TaskContainer;
    [SerializeField] private GameObject m_ChecklistContainer;
    [SerializeField] private TextMeshProUGUI m_TitleText;

    [Header("Emission Background")]
    [SerializeField] private Image m_EmissionBackground;
    [Range(0f, 1f)]
    [SerializeField] private float m_MaxEmissionOnBackground = 0.8f;
    [SerializeField] private float m_EmissionBoostLength = 1f;
    
    [Header("Skipping")]
    [SerializeField] private Button m_SkipButton;
    [SerializeField] private GameObject m_MobileSkipContainer;
    [SerializeField] private GameObject m_PCSkipContainer;
    [SerializeField] private Image m_MobileSkipFillBar;
    [SerializeField] private Image m_PCSkipFillBar;

    private int m_NumGroupsFinished = 0;
    private int m_NumGroups = 0;

    private Coroutine m_SkipCoroutine;
    private float m_CachedBackgroundStartEmission;
    private Coroutine m_BackgroundEmissionCoroutine;
    private float m_CurrEmissionBoostTime = 0;
    
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

        m_CachedBackgroundStartEmission = m_EmissionBackground.color.a;

        // Set proper skip graphics as enabled
        m_MobileSkipContainer.SetActive(false);
        m_PCSkipContainer.SetActive(false);
        m_MobileSkipFillBar.fillAmount = 0;
        m_PCSkipFillBar.fillAmount = 0;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        m_PCSkipContainer.SetActive(true);
#elif UNITY_ANDROID
        m_MobileSkipContainer.SetActive(true);
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
            // Check if group finished and was not previously finshed
            if (checklistItem.GetIsFinished() == false && checklistItem.UpdateChecklistItem(itemID))
            {
                // if it just finished, then update number of groups finished
                if (checklistItem.GetIsFinished())
                {
                    m_NumGroupsFinished++;
                }
                StartEmissionChecklistBoost();
            }
        }
        return m_NumGroupsFinished >= m_NumGroups;
    }

    // display an 'emission' boost on completing a tutorial task
    private void StartEmissionChecklistBoost()
    {
        if (m_BackgroundEmissionCoroutine == null)
        {
            m_BackgroundEmissionCoroutine = StartCoroutine(EmissionBoostCoroutine());
        }
    }

    private void StopEmissionChecklistBoost()
    {
        StopCoroutine(m_BackgroundEmissionCoroutine);
        m_BackgroundEmissionCoroutine = null;
    }

    // Performs the emission boost, increasing then decreasing background emission values
    private IEnumerator EmissionBoostCoroutine()
    {
        bool bIncrBoost = true;
        // If Emission going down and above zero, or emission going up and below max length
        while ((bIncrBoost == false && m_CurrEmissionBoostTime > 0) || bIncrBoost == true)
        {
            if (m_CurrEmissionBoostTime > m_EmissionBoostLength)
            {
                bIncrBoost = !bIncrBoost;
            }
            m_CurrEmissionBoostTime += bIncrBoost ? Time.deltaTime : -Time.deltaTime;
            EmissionBoostHelper();
            yield return null;
        }
        m_BackgroundEmissionCoroutine = null;
    }

    private void EmissionBoostHelper()
    {
        float t = Easing.OutCubic(m_CurrEmissionBoostTime);
        Color NewAlphaColor = m_EmissionBackground.color;
        NewAlphaColor.a = Mathf.Lerp(m_CachedBackgroundStartEmission, m_MaxEmissionOnBackground, t);
        m_EmissionBackground.color = NewAlphaColor;
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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            m_PCSkipFillBar.fillAmount = SkipPercent;
#elif UNITY_ANDROID
            m_MobileSkipFillBar.fillAmount = SkipPercent;
#endif
            yield return null;
        }
    }

    public void StopSkipping()
    {
        m_IsSkipping = false;
        StopCoroutine(m_SkipCoroutine);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        m_PCSkipFillBar.fillAmount = 0;
#elif UNITY_ANDROID
        m_MobileSkipFillBar.fillAmount = 0;
#endif
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
