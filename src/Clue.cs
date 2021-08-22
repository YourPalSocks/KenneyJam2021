using Godot;
using System;

/*
 * Clues derive from NPCs just to get the interaction basis, once collected the clue removes itself but adds information to the GameManager that 
 * Doctor Monroe can use to question the residents.
 */
public class Clue : NPC
{
    private string clueName = "[CLUE ITEM]";

    public void changeClueName(string s)
    {
        clueName = s;
    }

    public override string ToString()
    {
        return clueName;
    }

    public override void onInteraction()
    {
        gameManager.getDialogueBox().activateTextBox(0, clueName + "!", true);
        Player.showInteractionIcon = false;
        gameManager.addToClueBank(this);
        QueueFree();
    }
}
