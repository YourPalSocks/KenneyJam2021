using Godot;
using System;
using System.Collections.Generic;

public class DialogueBox : TextureRect
{
    //Componenets
    private Label nameArea;
    private Label textArea;
    private List<string> genericResponses = new List<string>();
    private List<string> villagerLines = new List<string>();
    private AudioStreamPlayer audio;

    //Controls
    private bool isTyping = false;
    private int curLine = 0;
    private int curChar;
    private string[] linesToRead;
    private bool oneTime = false;

    public static bool textboxActive = false;


    public override void _Ready()
    {
        //Get Componenets
        nameArea = GetNode<Label>("Name Area");
        textArea = GetNode<Label>("CenterContainer/Text Area");
        audio = GetNode<AudioStreamPlayer>("Chat Sound");

        /*
         * Build genericRespones -- These are basic prompts not requiring seperate text files
         * Things like "I can't pass" or "No one's home" or junk like that
         * Villager dialogue is stored in its own text file
         */
        genericResponses.Add("Found ");
        genericResponses.Add("Nothing to do now...");

        /*
         * Build villagerLines -- Open text file and read it to load into the list
         */
        File f = new File();
        f.Open("res://Assets/Villager_Dialogue.res", File.ModeFlags.Read);
        string[] lines = f.GetAsText().Split('\n');
        foreach (string l in lines)
        {
            villagerLines.Add(l);
        }
        f.Close();
    }

    public override void _Process(float delta)
    {
        if (!Visible)
        {
            return;
        }

        if (isTyping)
        {
            //Check if theres at least 2 characters, then check for interaction
            if (curChar >= 2 && Input.IsActionJustPressed("Interact"))
            {
                //Print out whole line and stop typing
                textArea.Text = linesToRead[curLine];
                isTyping = false;
                return;
            }

            //Check if done
            if (curChar < linesToRead[curLine].ToCharArray().Length)
            {
                textArea.Text += linesToRead[curLine].ToCharArray()[curChar];
                curChar++;
                audio.Play();
            }
            else
            {
                isTyping = false;
                curChar = 0;
            }
        }
        else
        {
            //Move to next line if done typing
            if (Input.IsActionJustPressed("Interact"))
            {
                //Check if at end
                curLine++;
                if (curLine >= linesToRead.Length)
                {
                    deactivateTextBox();
                    return;
                }
                textArea.Text = "";
                isTyping = true;
            }
        }
    }

    /*
     * activateTextBox initiates interaction stuff, reading from Villager_Dialogue.tres
     */
    public void activateTextBox(string speaker, DialogueOption option)
    {
        Player.inInteraction = true;
        nameArea.Text = speaker;
        Visible = true;
        textArea.Text = "";
        //Get lines into linesToRead
        linesToRead = new string[option.lineEnd - option.lineStart];
        int lineCount = 0;
        for (int i = option.lineStart; i < option.lineEnd; i++)
        {
            linesToRead[lineCount] = villagerLines[i];
            lineCount++;
        }
        //Filter linesToRead
        filterDialogueLines(speaker, option.evidenceName, option.villagerName, ref linesToRead, option.area);
        oneTime = false;
        Player.showInteractionIcon = false;
        textboxActive = true;
        isTyping = true;
    }

    public void activateTextBox(int index, string additions, bool playsOnce)
    {
        Player.inInteraction = true;
        textArea.Text = "";
        Visible = true;
        nameArea.Text = "";
        linesToRead = new string[1];
        linesToRead[0] = genericResponses[index] + additions;

        Player.showInteractionIcon = false;
        oneTime = playsOnce;
        textboxActive = true;
        isTyping = true;
    }

    public void deactivateTextBox()
    {
        Visible = false;
        //Reset
        isTyping = false;
        curLine = 0;
        curChar = 0;
        Player.inInteraction = false;
        if (!oneTime)
        {
            Player.showInteractionIcon = true;
        }
        else 
        {
            Player.showInteractionIcon = false;
        }
        textboxActive = false;
    }

    /*
     * This method "filters" the various lines of dialogue adding details like the name of the villager, evidence, other villagers, and the like 
     * 
     * TAGS: -e -> Evidence
     *       -v -> Villager name
     *       -j -> Job title
     *       -n -> My name
     *       -p -> Doctor's name
     *       -a -> Clue Area
     */
    private void filterDialogueLines(string name, string evidence, string villagerName, ref string[] lines, string area)
    {
        //Split speaker name into [0] -> Job and [1] -> Name
        string[] splitSpeaker = name.Split(" ");
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("-e") && evidence != null)
            {
                lines[i] = lines[i].Replace("-e", evidence);
            }

            if (lines[i].Contains("-v") && villagerName != null)
            {
                lines[i] = lines[i].Replace("-v", villagerName);
            }

            if (lines[i].Contains("-j"))
            {
                lines[i] = lines[i].Replace("-j", splitSpeaker[0]);
            }

            if (lines[i].Contains("-n"))
            {
                lines[i] = lines[i].Replace("-n", splitSpeaker[1]);
            }

            if (lines[i].Contains("-p"))
            {
                lines[i] = lines[i].Replace("-p", Player.curDoctor.ToString());
            }

            if (lines[i].Contains("-a"))
            {
                lines[i] = lines[i].Replace("-a", area);
            }
        }
    }
}
