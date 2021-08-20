using Godot;
using System;
using System.Collections.Generic;

public class GameManager : Node2D
{
    //Components
    private ColorRect CRT_EFFECT;
    private DialogueBox dialogue;
    [Export]
    private PackedScene NPCBase;
    Color[] possibleColors = { Colors.LightGreen, Colors.LightPink, Colors.LightYellow, Colors.Lime, 
        Colors.MediumAquamarine, Colors.NavajoWhite, Colors.Olive, Colors.OrangeRed, 
        Colors.Orange, Colors.PeachPuff, Colors.Salmon, Colors.RosyBrown };
    private List<string> names = new List<string>();
    private List<string> jobs = new List<string>();
 


    public override void _Ready()
    {
        //Enable the CRT filter
        CRT_EFFECT = GetNode<ColorRect>("CanvasLayer/ColorRect");
        CRT_EFFECT.Visible = true;

        dialogue = GetNode<DialogueBox>("CanvasLayer/Dialogue Box");

        //Build name dictionary
        File f = new File();
        f.Open("res://Assets/FirstNames.tres", File.ModeFlags.Read);
        string[] lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            names.Add(l);
        }
        f.Close();
        //Jobs too
        f.Open("res://Assets/Jobs.tres", File.ModeFlags.Read);
        lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            jobs.Add(l);
        }
        f.Close();

        //Test creating an NPC
        createNPC();

    }

    /*
     * createNPC is responsible for just that, creating NPCs. 
     */
    private void createNPC()
    {
        NPC n_npc = (NPC) NPCBase.Instance();
        //Set name and colors
        n_npc.onSpawn(getJobandName(), getRandomColor());
        //Position --TODO: Based on map stuff
        n_npc.Position = new Vector2(111, -50);
        //Set sprite
        //Set personality
        //Tasks?

        AddChild(n_npc);
    }

    public DialogueBox getDialogueBox()
    {
        return dialogue;
    }

    private Color getRandomColor()
    {
        GD.Randomize();
        return possibleColors[GD.Randi() % possibleColors.Length];
    }

    private string getJobandName()
    {
        GD.Randomize();
        int j = (int)GD.RandRange(0, jobs.Count);
        int n = (int)GD.RandRange(0, names.Count);
        return jobs[j] + " " + names[n];
    }
}
