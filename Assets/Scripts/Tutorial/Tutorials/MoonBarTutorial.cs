using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonBarTutorial : Tutorial
{
    private const int Group0 = 0;
    public override void SetupTutorial()
    {
        base.SetupTutorial();

        AddChecklistItem("Hold A trigger to perform dash", 0, Group0, false);
        AddChecklistItem("Hold left trigger to enter aim mode", 0, Group0, false);

        PlayerManager.PropertyInstance.PlayerController.MoonBarAbility.d_DashEndDelegate += DashEnded;
        PlayerManager.PropertyInstance.PlayerController.MoonBarAbility.d_AimModeEndDelegate += AimModeEnded;
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
