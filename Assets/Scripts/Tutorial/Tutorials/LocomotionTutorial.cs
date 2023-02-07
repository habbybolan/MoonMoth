using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionTutorial : Tutorial
{
    public override void SetupTutorial()
    {
        base.SetupTutorial();

        // TODO: Display differnt text based on input type and platform
        AddChecklistItem("Move the left joystick Up", 0);
        AddChecklistItem("Move the left joystick Down", 0);
        AddChecklistItem("Move the left joystick Left", 0);
        AddChecklistItem("Move the left joystick Right", 0);
        AddChecklistItem("Press b to dodge", 2);
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
                UpdateTutorial(0);
                break;
            case TutorialInputs.DOWN:
                UpdateTutorial(1);
                break;
            case TutorialInputs.LEFT:
                UpdateTutorial(2);
                break;
            case TutorialInputs.RIGHT:
                UpdateTutorial(3);
                break;
            case TutorialInputs.DODGE:
                UpdateTutorial(4);
                break;
            default:    // Wrong input, do nothing
                break;
        }
    }
}
