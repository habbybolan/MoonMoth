using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checklist : MonoBehaviour
{
    [SerializeField] private CheckListItem m_ChecklistItemPrefab;
    
    // TODO: Property for vertical list of tutorials
    private Dictionary<int, CheckListItem> m_CheckListDictionary;

    public void AddChecklistItem(TutorialInfo tutorialInfo)
    {
        // TODO:
        // Create Checklist item UI element
        // Add checklist item to map
    }

    public void UpdateChecklistItem(int itemID)
    {
        // TODO:
        // Get checklist item by id and notify to update it
    }
}
