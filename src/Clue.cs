using Godot;
using System;

/*
 * Clues derive from NPCs just to get the interaction basis, once collected the clue removes itself but adds information to the GameManager that 
 * Doctor Monroe can use to question the residents.
 */
public class Clue : NPC
{
    private string clueName = "[CLUE ITEM]";

    public override void onInteraction()
    {
        gameManager.getDialogueBox().activateTextBox(0, clueName + "!", true);
        //TODO: Transmit data to GameManager for Monroe
        Player.showInteractionIcon = false;
        QueueFree();
    }
}
