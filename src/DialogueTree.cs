using Godot;
using System;
using System.Collections.Generic;

/*
 * The DialogueTree is used to dynamically create/adjust dialogue options for various characters depending on personality and to whom they are speaking to.
 * The struct below is one particular line of dialogue, possibly involving the name of a piece of evidence or villager given by the tags "-e" or "-v" in 
 * the "Villager_Dialogue.res" file.
 */
public enum DIALOGUE_TAG { CASUAL, INTERROGATION }

public struct DialogueOption 
{
    public DialogueOption(int s, int e, string ev, string vn, DIALOGUE_TAG t, string a)
    {
        lineStart = s;
        lineEnd = e;
        evidenceName = ev;
        villagerName = vn;
        tag = t;
        area = a;
    }

    public int lineStart;
    public int lineEnd;
    public string evidenceName;
    public string villagerName;
    public DIALOGUE_TAG tag;
    public string area;
}

public class DialogueTree
{
    private List<DialogueOption> lines = new List<DialogueOption>();
    //Methods

    public void createDialogueOption(int s, int e, string ev, string vn, DIALOGUE_TAG t, string a)
    {
        lines.Add(new DialogueOption(s, e, ev, vn, t, a));
    }

    public DialogueOption getDialogueOption(int index)
    {
        return lines[index];
    }

    public List<DialogueOption> getOptionsByTag(DIALOGUE_TAG t)
    {
        List<DialogueOption> ret = new List<DialogueOption>();
        foreach (DialogueOption o in lines)
        {
            if (o.tag == t)
            {
                ret.Add(o);
            }
        }
        return ret;
    }

    public DialogueOption getRandomOptionByTag(DIALOGUE_TAG t)
    {
        List<DialogueOption> d = getOptionsByTag(t);
        RandomNumberGenerator rand = new RandomNumberGenerator();
        rand.Randomize(); //Pseudorandom my darling
        return d[rand.RandiRange(0, d.Count - 1)];
    }

}
