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

#if UNITY_ANDROID && !UNITY_EDITOR
        AddChecklistItem("Move up", 0, GroupMovement, true, "Hold the left side of the screen to bring up the [virtual joystick] to move");
        AddChecklistItem("Move down", 0, GroupMovement);
        AddChecklistItem("Move left", 0, GroupMovement);
        AddChecklistItem("Move right", 0, GroupMovement);
        AddChecklistItem("Dodge", 2, GroupDodge, true, "[Swipe] right side of your screen to dodge in direction of the swipe");
#else
        AddChecklistItem("Move up", 0, GroupMovement, true, "Use [Left Joystick] to move");
        AddChecklistItem("Move down", 0, GroupMovement);
        AddChecklistItem("Move left", 0, GroupMovement);
        AddChecklistItem("Move right", 0, GroupMovement);
        AddChecklistItem("Dodge", 2, GroupDodge, true, "Move [Left Stick] to choose a direction and press [B button] to dodge");
#endif

        TutorialSetupFinished();
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
