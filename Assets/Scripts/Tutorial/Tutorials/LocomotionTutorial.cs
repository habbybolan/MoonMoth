using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionTutorial : Tutorial
{
    private const int GroupMovement = 0;
    private const int GroupDodge = 1;
    public override void SetupTutorial()
    {
        base.SetupTutorial();

        AddChecklistItem("Move the left joystick Up", 0, GroupMovement, true, "Move left joystick to move");
        AddChecklistItem("Move the left joystick Down", 0, GroupMovement);
        AddChecklistItem("Move the left joystick Left", 0, GroupMovement);
        AddChecklistItem("Move the left joystick Right", 0, GroupMovement);
        AddChecklistItem("Press b to dodge", 2, GroupDodge);
    }

    public override void EndTutorialLogic()
    {
        base.EndTutorialLogic();
    }

    public override void ReceiveTutorialInput(TutorialInputs input)
    {
        base.ReceiveTutorialInput(input);

        switch (input)
        {
            case TutorialInputs.UP:
                UpdateTutorial(0, GroupMovement);
                break;
            case TutorialInputs.DOWN:
                UpdateTutorial(1, GroupMovement);
                break;
            case TutorialInputs.LEFT:
                UpdateTutorial(2, GroupMovement);
                break;
            case TutorialInputs.RIGHT:
                UpdateTutorial(3, GroupMovement);
                break;
            case TutorialInputs.DODGE:
                UpdateTutorial(4, GroupDodge);
                break;
            default:    // Wrong input, do nothing
                break;
        }
    }
}
