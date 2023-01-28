using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocomotionTutorial : Tutorial
{
    public override void SetupTutorial()
    {
        base.SetupTutorial();

        // TODO: Display differnt text based on input type and platform
        AddChecklistItem("Move the left joystick left", 0);
        AddChecklistItem("Move the left joystick down", 0);
        AddChecklistItem("Move the left joystick left", 0);
        AddChecklistItem("Move the left joystick right", 0);
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
            default:    // Wrong input, do nothing
                break;
        }
    }
}
