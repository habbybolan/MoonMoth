using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonBarTutorial : Tutorial
{
    private const int Group0 = 0;
    public override void SetupTutorial()
    {
        base.SetupTutorial();

        AddChecklistItem("Hold [A] trigger to perform dash", 0, Group0, true, "Both abilities use the same meter with a shared cooldown");
        AddChecklistItem("Hold [Left Trigger] to enter aim mode", 0, Group0, false);

        PlayerManager.PropertyInstance.PlayerController.MoonBarAbility.d_DashEndDelegate += DashEnded;
        PlayerManager.PropertyInstance.PlayerController.MoonBarAbility.d_AimModeEndDelegate += AimModeEnded;

        TutorialSetupFinished();
    }

    public override void EndTutorialLogic()
    {
        PlayerManager.PropertyInstance.PlayerController.MoonBarAbility.d_DashEndDelegate -= DashEnded;
        PlayerManager.PropertyInstance.PlayerController.MoonBarAbility.d_AimModeEndDelegate -= AimModeEnded;
        base.EndTutorialLogic();
    }

    private void DashEnded()
    {
        UpdateTutorial(0, Group0);
    }

    private void AimModeEnded()
    {
        UpdateTutorial(1, Group0);
    }
}
