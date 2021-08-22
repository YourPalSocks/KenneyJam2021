using Godot;
using System;

public class EndScreen : Node2D
{
    private StatsObserver stats;
    private Label statsLabel;

    public override void _Ready()
    {
        stats = GetTree().Root.GetNode<StatsObserver>("StatsObserver");
        statsLabel = GetNode<Label>("CanvasLayer/MarginContainer/VBoxContainer/Setup");
        statsLabel.Text = "";
        //Set up the statsLabel, did we win or lose
        if (stats.getScenario() == 0) //Win
        {
            statsLabel.Text += "> YOU WIN" + '\n'
                + "> The Doctors rid the town of the blight and rescued the remaining townspeople." + '\n';
        }
        else
        {
            statsLabel.Text += "> YOU LOSE" + '\n';
            statsLabel.Text += getScenarioEnding();
        }
        //Villager deaths
        if (stats.getDead() != 1)
        {
            statsLabel.Text += "> " + stats.getDead() + " innocent Villagers were slain (/16)." + '\n';
        }
        else
        {
            statsLabel.Text += "> " + stats.getDead() + " innocent Villager was slain (/16)." + '\n';
        }
        //Bug names
        statsLabel.Text += "The bugs were: " + stats.getBugNames();
    }

    public string getScenarioEnding()
    {
        string toRet = "";
        switch (stats.getScenario())
        {
            case 1: //Only Monroe Left
                toRet = "> Doctor Monroe, ever the pacifist, was overrun and left defenseless. With his partners dead, he and the rest of the town followed.";
                break;

            case 2: //All Villagers dead
                toRet = "> There is no one left. Everyone in Thornmouth is either a hive or a bug. The Doctors escaped, lucky to be alive, " +
                    "but wracked with the grief of their failure.";
                break;

            case 3: //Doctor killed innocent
                toRet = "> The Doctors were responsible for the death of an innocent Villager, and were swiftly jailed by law enforcement.";            
                break;

            case 4: //All Doctors killed
                toRet = "> 3 Doctors entered, but none left Thornmouth that day. Now nothing stands between Thornmouth and violent takeover by mutant insects.";
                break;
        }
        toRet += '\n';
        return toRet;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Interact"))
        {
            stats.clear();
            GetTree().ChangeScene("res://Scenes/Title Screen.tscn"); //Main Menu
        }
    }
}
