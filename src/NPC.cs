using Godot;
using System;

/*
 * This script should probably become an abstract one once we develop more villager types, for now this'll work fine
 * The following applies to all villagers and all villagers should derive from this script in some fashion to add basic functionality.
 */
public class NPC : KinematicBody2D
{
    //Components
    private Area2D interaction_area;
    private GameManager gameManager;

    //Controls
    private bool inTalkingRange = false;
    private string name = "[OCC. FIRST]";

    public override void _Ready()
    {
        //Get componenets
        interaction_area = GetNode<Area2D>("Talk Area");
        gameManager = GetTree().Root.GetNode<GameManager>("Root");
    }

    public void onSpawn(string n, Color c)
    {
        name = n;
        Modulate = c;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (inTalkingRange && Input.IsActionJustPressed("Interact") && !Player.inInteraction)
        {
            gameManager.getDialogueBox().activateTextBox(name, 0, 5);
        }
    }

    public void _on_Talk_Area_body_entered(Node body)
    {
        //Check if node is player
        if (body.GetType() == typeof(Player))
        {
            inTalkingRange = true;
        }
    }

    public void _on_Talk_Area_body_exited(Node body)
    {
        //Check if node is player
        if (body.GetType() == typeof(Player))
        {
            inTalkingRange = false;
        }
    }

    #region Configuring Custom NPCs
    /*
     * Every texture is on a spritesheet (colored_transparent_packed.png) so all that's needed is to change the Region's x and y values
     */
    public void changeTexture(int x, int y)
    {
        
    }


    #endregion
}
