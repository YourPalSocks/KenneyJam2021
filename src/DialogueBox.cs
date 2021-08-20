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
    private string[] linesToRead;

    //Controls
    private bool isTyping = false;
    private int curLine = 0;
    private int curChar;


    public override void _Ready()
    {
        //Get Componenets
        nameArea = GetNode<Label>("Name Area");
        textArea = GetNode<Label>("CenterContainer/Text Area");

        /*
         * Build genericRespones -- These are basic prompts not requiring seperate text files
         * Things like "I can't pass" or "No one's home" or junk like that
         * Villager dialogue is stored in its own text file
         */


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
            //Check if done
            if (curChar < linesToRead[curLine].ToCharArray().Length)
            {
                textArea.Text += linesToRead[curLine].ToCharArray()[curChar];
                curChar++;
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
    public void activateTextBox(string speaker, int lineStart, int lineEnd)
    {
        Player.inInteraction = true;
        nameArea.Text = speaker;
        Visible = true;
        textArea.Text = "";
        //Get lines into linesToRead
        linesToRead = new string[lineEnd - lineStart];
        int lineCount = 0;
        for (int i = lineStart; i < lineEnd; i++)
        {
            linesToRead[lineCount] = villagerLines[i];
            lineCount++;
        }

        isTyping = true;
    }

    public void activateTextBox(int genericLineIndex)
    {
        Player.inInteraction = true;
        nameArea.Text = "";

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
    }
}
