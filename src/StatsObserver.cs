using Godot;
using System;
using System.Collections.Generic;
/*
 * The StatsObserver translates game information to the End Screen, mainly the identity of the bugs and how things went down
 */
public class StatsObserver : Node2D
{
    private List<string> bugNames = new List<string>();
    private int deadVillagers = 0;
    /*
     * An Integer determining in what way we lost/won for the statsLabel thing in the EndScreen
     */
    int endScenario = 0;

    public void addBugName(string s)
    {
        bugNames.Add(s);
    }

    public void addDeadVillager()
    {
        deadVillagers++;
    }

    public void setEndScenario(int es)
    {
        endScenario = es;
    }

    //Gets
    public string getBugNames()
    {
        string toRet = "";
        foreach(String s in bugNames)
        {
            toRet += s + "-";
        }
        toRet = toRet.Substring(0, toRet.Length - 1);
        toRet = toRet.Replace("-", ", ");
        return toRet;
    }

    public int getDead()
    {
        return deadVillagers;
    }

    public int getScenario()
    {
        return endScenario;
    }

    public void clear()
    {
        bugNames = new List<string>();
        deadVillagers = 0;
        endScenario = 0;
    }
}
