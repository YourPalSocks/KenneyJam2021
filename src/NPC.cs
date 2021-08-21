using Godot;
using System;

//Types from: https://mafportal.com/en/types-of-players-in-Mafia
public enum VILLAGER_TYPE { Paranoid = 1, Intuit = 2, Silent = 3, Instigator = 4, Bug = 5, Clueless = 6, MISC = 7}
public class NPC : KinematicBody2D
{
    //Components
    private Area2D interaction_area;
    protected GameManager gameManager;
    public bool isBugForm = false; //Used for Bugs
    public bool isNest = false;
    private DialogueTree dialogue;
    private InterestLocation mArea; //Instigators know something there (or think they do)
    private AudioStreamPlayer2D audio;

    //Controls
    private bool inTalkingRange = false;
    protected VILLAGER_TYPE mType = VILLAGER_TYPE.Paranoid;
    public DIALOGUE_TAG speakingLevel = DIALOGUE_TAG.CASUAL;
    private string name = "[OCC. FIRST]";
    public int dialogueToPlay = 0;

    public override void _Ready()
    {
        //Get componenets
        interaction_area = GetNode<Area2D>("Talk Area");
        gameManager = GetTree().Root.GetNode<GameManager>("Root");
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

    public virtual void onSpawn(string n, Color c, VILLAGER_TYPE type, InterestLocation area)
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
        if (isBugForm || !Visible) //Obviosly, villagers wont talk in bug form
        {
            return;
        }

        if (inTalkingRange && Input.IsActionJustPressed("Interact") && !Player.inInteraction)
        {
            onInteraction();
        }
    }

    public void getDialogue(VILLAGER_TYPE t)
    {
        //Each villager comes with 2-4 casual lines from their list (Bugs pull from all)
        //Paranoid villagers point fingers at everyone, Intuit villagers focus on one person
        //Instigators are like Intuits, but do have a bit of evidence, their dialogue typically leads to clues
        switch (t)
        {
            case VILLAGER_TYPE.Paranoid:
                //Casual remarks
                dialogue.createDialogueOption(7, 8, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(8, 9, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(9, 10, "", gameManager.getRandomVillager(name), DIALOGUE_TAG.CASUAL, "");
                break;

            case VILLAGER_TYPE.Intuit:
                string target = gameManager.getRandomVillager(name);
                //Casual remarks
                dialogue.createDialogueOption(12, 13, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                break;

            case VILLAGER_TYPE.Instigator:
                target = gameManager.getRandomVillager(name);
                //Casual remarks
                dialogue.createDialogueOption(21, 22, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(22, 23, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                break;

            case VILLAGER_TYPE.Silent:
                //Casual remarks
                dialogue.createDialogueOption(16, 17, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(17, 18, "", "", DIALOGUE_TAG.CASUAL, "");
                break;

            case VILLAGER_TYPE.Clueless:
                //Casual remarks
                dialogue.createDialogueOption(25, 26, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(26, 27, "", gameManager.getRandomVillager(name), DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(27, 28, "", "", DIALOGUE_TAG.CASUAL, "");
                break;

            case VILLAGER_TYPE.Bug:
                //Bugs will choose a random personality type and blend in with it
                GD.Randomize();
                VILLAGER_TYPE imp = (VILLAGER_TYPE)(GD.Randi() % 4 + 1);
                GD.Print("A bug is " + name);
                getDialogue(imp);
                mType = VILLAGER_TYPE.Bug;
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

    public void _on_AudioStreamPlayer2D_finished()
    {
        QueueFree();
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
        //Create nest interaction
        if (isNest)
        {
            gameManager.getDialogueBox().activateTextBox(1, "", false);
        }
        else 
        {
            gameManager.getDialogueBox().activateTextBox(name, dialogue.getRandomOptionByTag(speakingLevel));
        }
    }

    public virtual void onHit()
    {
        if (isBugForm || isNest)
        {
            return;
        }
        Visible = false;
        audio.Play();
        //Check if bug
        if (mType == VILLAGER_TYPE.Bug)
        {
            //Expose identity
            gameManager.exposeVillagerAsBug(this);
            return;
        }
        //This assumes the villager is not a bug, they require an override of this method
        gameManager.killVillager(this);
    }
}
