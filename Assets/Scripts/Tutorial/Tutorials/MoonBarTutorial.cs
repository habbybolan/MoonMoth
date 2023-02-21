using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonBarTutorial : Tutorial
{
    private const int Group0 = 0;
    public override void SetupTutorial()
    {
        base.SetupTutorial();

#if UNITY_ANDROID && !UNITY_EDITOR
        AddChecklistItem("Tap [Dash Button] to toggle increased movement speed", 0, Group0, true, "Both abilities use the same meter with a shared cooldown");
        AddChecklistItem("Tap [Aim-Mode Button] to toggle slowing down time", 0, Group0, false);
#else
        AddChecklistItem("Hold [A button] trigger to increase movement speed", 0, Group0, true, "Both abilities use the same meter with a shared cooldown");
        AddChecklistItem("Hold [Left Trigger] to slow down time", 0, Group0, false);
#endif

        m_PlayerController.MoonBarAbility.d_DashEndDelegate += DashEnded;
        m_PlayerController.MoonBarAbility.d_AimModeEndDelegate += AimModeEnded;

#if UNITY_ANDROID && !UNITY_EDITOR
        m_PlayerController.AimModeButton.StartHighlighting();
        m_PlayerController.DashButton.StartHighlighting();
#endif

        TutorialSetupFinished();
    }

    public override void EndTutorialLogic()
    {
        m_PlayerController.MoonBarAbility.d_DashEndDelegate -= DashEnded;
        m_PlayerController.MoonBarAbility.d_AimModeEndDelegate -= AimModeEnded;

#if UNITY_ANDROID && !UNITY_EDITOR
        m_PlayerController.AimModeButton.StopHighlighting();
        m_PlayerController.DashButton.StopHighlighting();
#endif

        base.EndTutorialLogic();
    }

    private void DashEnded()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_PlayerController.DashButton.StartHighlighting();
#endif

        UpdateTutorial(0, Group0);
    }

    private void AimModeEnded()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_PlayerController.AimModeButton.StartHighlighting();
#endif
        
        UpdateTutorial(1, Group0);
    }
}
