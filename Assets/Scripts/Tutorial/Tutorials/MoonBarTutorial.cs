using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonBarTutorial : Tutorial
{
    public override void SetupTutorial()
    {
        base.SetupTutorial();

        AddChecklistItem("Hold right trigger to perform dash", 0);
        AddChecklistItem("Hold left trigger to enter aim mode", 0);

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
        UpdateTutorial(0);
    }

    private void AimModeEnded()
    {
        UpdateTutorial(1);
    }
}
