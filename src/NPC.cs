using Godot;
using System;

//Types from: https://mafportal.com/en/types-of-players-in-Mafia
public enum VILLAGER_TYPE { Paranoid, Intuit, Silent, Instigator, Bug, Clueless}
public class NPC : KinematicBody2D
{
    //Components
    private Area2D interaction_area;
    protected GameManager gameManager;
    private DialogueTree dialogue;
    private InterestLocation mArea; //Instigators know something there (or think they do)

    //Controls
    private bool inTalkingRange = false;
    private VILLAGER_TYPE mType = VILLAGER_TYPE.Paranoid;
    public DIALOGUE_TAG speakingLevel = DIALOGUE_TAG.CASUAL;
    private string name = "[OCC. FIRST]";
    public int dialogueToPlay = 0;

    public override void _Ready()
    {
        //Get componenets
        interaction_area = GetNode<Area2D>("Talk Area");
        gameManager = GetTree().Root.GetNode<GameManager>("Root");
    }

    public void onSpawn(string n, Color c, VILLAGER_TYPE type, InterestLocation area)
    {
        name = n;
        Modulate = c;
        //Determine personality
        mType = type;
        //Get dialogue based on personality
        mArea = area;
        dialogue = new DialogueTree();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (inTalkingRange && Input.IsActionJustPressed("Interact") && !Player.inInteraction)
        {
            onInteraction();
        }
    }

    public void getDialogue()
    {
        //Each villager comes with 2-4 casual lines from their list (Bugs pull from all)
        //Paranoid villagers point fingers at everyone, Intuit villagers focus on one person
        //Instigators are like Intuits, but do have a bit of evidence, their dialogue typically leads to clues
        switch (mType)
        {
            case VILLAGER_TYPE.Paranoid:
                //Casual remarks
                break;

            case VILLAGER_TYPE.Intuit:
                string target = gameManager.getRandomVillager(name);
                //Casual remarks
                dialogue.createDialogueOption(12, 13, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                break;

            case VILLAGER_TYPE.Instigator:
                target = gameManager.getRandomVillager(name);
                //Casual remarks
                dialogue.createDialogueOption(20, 21, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                break;
        }
    }

    public void _on_Talk_Area_body_entered(Node body)
    {
        //Check if node is player
        if (body.GetType() == typeof(Player))
        {
            inTalkingRange = true;
            Player.showInteractionIcon = true;
        }
    }

    public void _on_Talk_Area_body_exited(Node body)
    {
        //Check if node is player
        if (body.GetType() == typeof(Player))
        {
            inTalkingRange = false;
            Player.showInteractionIcon = false;
        }
    }

    public string getName()
    {
        return name;
    }

    public VILLAGER_TYPE getPersonality()
    {
        return mType;
    }

    public virtual void onInteraction()
    {
        //Get random option based on level
        gameManager.getDialogueBox().activateTextBox(name, dialogue.getRandomOptionByTag(speakingLevel));
    }
}
