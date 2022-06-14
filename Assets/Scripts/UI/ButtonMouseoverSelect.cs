using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonMouseoverSelect : Button, IPointerEnterHandler, IPointerExitHandler
{
    private Button m_Button;
    private TextMeshProUGUI m_ButtonLabel;
    [SerializeField] private Image m_SelectIcon;

    BaseEventData m_BaseEvent;

    public Color m_DefaultColor = new Color(.5f, .5f, .5f, .5f);
    public Color m_SelectedColor = Color.white;

    protected override void Awake()
    {
        m_Button = GetComponent<Button>();
        m_ButtonLabel = GetComponentInChildren<TextMeshProUGUI>();

        if (GetComponent<ButtonAutoSelect>())
        {
            ButtonSelected();
        }
        else ButtonUnselected();
    }

    private void Update()
    {
#if UNITY_EDITOR

#else
        if (gameObject == EventSystem.current.currentSelectedGameObject) ButtonSelected();
        else ButtonUnselected();
#endif
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        ButtonUnselected();
    }

    private void ButtonSelected()
    {
        m_ButtonLabel.color = m_SelectedColor;
        m_SelectIcon.gameObject.SetActive(true);
    }

    private void ButtonUnselected()
    {
        m_ButtonLabel.color = m_DefaultColor;
        m_SelectIcon.gameObject.SetActive(false);
    }
}
