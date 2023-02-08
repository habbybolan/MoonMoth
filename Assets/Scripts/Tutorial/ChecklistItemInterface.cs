using UnityEngine.UI;

// Used to generify ChecklistItem and ChecklistItemGroup
public interface ChecklistItemInterface
{
    public void InitializeItem(int itemID, string text, int maxCount, VerticalLayoutGroup taskContainer, int groupID);
    public bool UpdateChecklistItem(int itemID);
    public bool GetIsFinished();
}
