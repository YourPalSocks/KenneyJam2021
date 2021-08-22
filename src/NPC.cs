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
    private string evidenceType = "";
    public int dialogueToPlay = 0;

    private int timesInterrogated = 0; //Increases for bugs, eventually they snap and transform

    public override void _Ready()
    {
        //Get componenets
        interaction_area = GetNode<Area2D>("Talk Area");
        gameManager = GetTree().Root.GetNode<GameManager>("Root");
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

    public override string ToString()
    {
        return name;
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
        //Silent and Cluless villagers offer little to no (direct) information
        switch (t)
        {
            case VILLAGER_TYPE.Paranoid:
                //Casual remarks
                dialogue.createDialogueOption(8, 9, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(9, 10, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(10, 11, "", gameManager.getRandomVillager(name), DIALOGUE_TAG.CASUAL, "");
                //Interrogation remarks
                dialogue.createDialogueOption(12, 13, "", gameManager.getRandomVillager(name), DIALOGUE_TAG.INTERROGATION, "");
                dialogue.createDialogueOption(13, 14, "", gameManager.getRandomVillager(name), DIALOGUE_TAG.INTERROGATION, "");
                break;

            case VILLAGER_TYPE.Intuit:
                string target = gameManager.getRandomVillager(name);
                //Casual remarks
                dialogue.createDialogueOption(17, 18, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(18, 19, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(19, 20, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                //Interrogation remarks
                dialogue.createDialogueOption(21, 22, "", target, DIALOGUE_TAG.INTERROGATION, mArea.descriptor);
                dialogue.createDialogueOption(22, 23, "", target, DIALOGUE_TAG.INTERROGATION, mArea.descriptor);

                break;

            case VILLAGER_TYPE.Instigator:
                target = gameManager.getRandomVillager(name);
                //Casual remarks
                dialogue.createDialogueOption(35, 36, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(36, 37, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(37, 38, "", target, DIALOGUE_TAG.CASUAL, mArea.descriptor);
                //Interrogation remarks
                dialogue.createDialogueOption(39, 40, "", target, DIALOGUE_TAG.INTERROGATION, mArea.descriptor);
                dialogue.createDialogueOption(40, 41, "", target, DIALOGUE_TAG.INTERROGATION, mArea.descriptor);

                break;

            case VILLAGER_TYPE.Silent:
                //Casual remarks
                dialogue.createDialogueOption(26, 27, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(27, 28, "", "", DIALOGUE_TAG.CASUAL, "");
                //Interrogation remarks
                dialogue.createDialogueOption(30, 31, "", "", DIALOGUE_TAG.INTERROGATION, "");
                dialogue.createDialogueOption(31, 32, "", "", DIALOGUE_TAG.INTERROGATION, "");

                break;

            case VILLAGER_TYPE.Clueless:
                //Casual remarks
                dialogue.createDialogueOption(44, 45, "", "", DIALOGUE_TAG.CASUAL, "");
                dialogue.createDialogueOption(45, 46, "", gameManager.getRandomVillager(name), DIALOGUE_TAG.CASUAL, mArea.descriptor);
                dialogue.createDialogueOption(46, 47, "", "", DIALOGUE_TAG.CASUAL, "");
                //Interrogation remarks
                dialogue.createDialogueOption(48, 49, "", "", DIALOGUE_TAG.INTERROGATION, "");
                dialogue.createDialogueOption(49, 50, "", "", DIALOGUE_TAG.INTERROGATION, "");

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
            //Check if player is Monroe and has collected some evidence
            if (Player.curDoctor == DOCTORS.Monroe && gameManager.getEvidenceAmount() > 0)
            {
                speakingLevel = DIALOGUE_TAG.INTERROGATION;
                string s = gameManager.hasRelevantEvidence(this);
                //Check if evidence is relevant
                if (s == null)
                {
                    //First INTERROGATION reply --Not relevant
                    gameManager.getDialogueBox().activateTextBox(name, dialogue.getOptionByTagAndIndex(speakingLevel, 0));
                }
                else
                {
                    //Check if should become bug
                    if (mType == VILLAGER_TYPE.Bug)
                    {
                        if (timesInterrogated >= 2)
                        {
                            gameManager.exposeVillagerAsBug(this);
                            return;
                        }
                        else
                        {
                            timesInterrogated++;
                        }
                    }
                    //Second INTERROGATION REPLY --Is relevant
                    //Modify line first to contain evidence
                    DialogueOption ogD = dialogue.getOptionByTagAndIndex(speakingLevel, 1);
                    DialogueOption editedD = new DialogueOption(ogD.lineStart, ogD.lineEnd, s, ogD.villagerName, ogD.tag, ogD.area);
                    dialogue.updateDialogueOption(dialogue.getDialogueIndex(ogD), editedD);
                    
                    gameManager.getDialogueBox().activateTextBox(name, dialogue.getOptionByTagAndIndex(speakingLevel, 1));
                }
                return;
            }
            else
            {
                speakingLevel = DIALOGUE_TAG.CASUAL;
            }
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
