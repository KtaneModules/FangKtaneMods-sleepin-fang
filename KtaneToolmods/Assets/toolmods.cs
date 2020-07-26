using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class toolmods : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;
    public KMSelectable SolveButton;
    public TextMesh[] Displays;
    public Color[] Color;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved, bombSolved;
    private float elapsedTime;
    private double startupsec, startupms;
    private string elapsedTimeDisplay, startupmsDisplay;
    void Awake()
    {
        moduleId = moduleIdCounter++;
        SolveButton.OnInteract += delegate () { buttonPress(); return false; };
        moduleSolved = false;

        Displays[0].text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        module.OnActivate += startElapsedTime;

    }

    void Update()
    {
        Displays[2].text = DateTime.Now.ToString("HH:mm:ss");

        elapsedTime += Time.deltaTime;
        double milisecond = Math.Floor(Math.Round(elapsedTime % 60, 2) * 100 % 100);
        string milisecondDisp;
        if (milisecond < 10) milisecondDisp = "0" + milisecond.ToString();
        else milisecondDisp = milisecond.ToString();

        double second = Math.Floor(elapsedTime % 60);
        double minute = Math.Floor(elapsedTime / 60 % 60);
        double hour = Math.Floor(elapsedTime / 3600);

        if (hour != 0 && hour < 10)
        {
            if (minute < 10)
            {
                if (second < 10) elapsedTimeDisplay = "T+0" + hour + ":0" + minute + ":0" + second;
                else elapsedTimeDisplay = "T+0" + hour + ":0" + minute + ":" + second;
            }
            else
            {
                if (second < 10) elapsedTimeDisplay = "T+0" + hour + ":" + minute + ":0" + second;
                else elapsedTimeDisplay = "T+0" + hour + ":" + minute + ":" + second;
            }
        }
        else if (hour != 0)
        {
            if (minute < 10)
            {
                if (second < 10) elapsedTimeDisplay = "T+" + hour + ":0" + minute + ":0" + second;
                else elapsedTimeDisplay = "T+" + hour + ":0" + minute + ":" + second;
            }
            else
            {
                if (second < 10) elapsedTimeDisplay = "T+" + hour + ":" + minute + ":0" + second;
                else elapsedTimeDisplay = "T+" + hour + ":" + minute + ":" + second;
            }
        }
        else
        {
            if (minute < 10)
            {
                if (second < 10) elapsedTimeDisplay = "T+0" + minute + ":0" + second + "." + milisecondDisp;
                else elapsedTimeDisplay = "T+0" + minute + ":" + second + "." + milisecondDisp;
            }
            else
            {
                if (second < 10) elapsedTimeDisplay = "T+" + minute + ":0" + second + "." + milisecondDisp;
                else elapsedTimeDisplay = "T+" + minute + ":" + second + "." + milisecondDisp;
            }
        }
        Displays[1].text = elapsedTimeDisplay + " +" + startupsec + "." + startupmsDisplay + "s";

        decimal solvedPercentage;
        solvedPercentage = (((decimal)bomb.GetSolvedModuleNames().Count) * (decimal)100 / (decimal)bomb.GetSolvableModuleNames().Count);
        solvedPercentage = Math.Round(solvedPercentage, 2);
        Displays[4].text = bomb.GetSolvedModuleNames().Count + "/" + bomb.GetSolvableModuleNames().Count + " - " + solvedPercentage + "%";
        if (moduleSolved)
        {
            Displays[5].text = "Solved";
            Displays[5].color = Color[0];
        }
        else Displays[5].text = "Unsolved";

        if (bomb.GetSolvedModuleNames().Count == bomb.GetSolvableModuleNames().Count)
        {
            bombDefused();
            bombSolved = true;
        }
        double bombRealTime, multiplier;
        switch (bomb.GetStrikes()) {
            case 0:
            multiplier = 1;
            break;

            case 1:
            multiplier = 1.25;
            break;

            case 2:
            multiplier = 1.5;
            break;

            case 3:
            multiplier = 1.75;
            break;

            default:
            multiplier = 2;
            break;
        }
        bombRealTime = bomb.GetTime() / multiplier;

        double bombmilisecond = Math.Floor(Math.Round(bombRealTime % 60, 2) * 100 % 100);
        string bombmilisecondDisp;
        string bombrealtimeDisp;
        if (bombmilisecond < 10) bombmilisecondDisp = "0" + bombmilisecond.ToString();
        else bombmilisecondDisp = bombmilisecond.ToString();

        double bombsecond = Math.Floor(bombRealTime % 60);
        double bombminute = Math.Floor(bombRealTime / 60 % 60);
        double bombhour = Math.Floor(bombRealTime / 3600);

        if (bombhour != 0 && bombhour < 10)
        {
            if (bombminute < 10)
            {
                if (bombsecond < 10) bombrealtimeDisp = "0" + bombhour + ":0" + bombminute + ":0" + bombsecond;
                else bombrealtimeDisp = "0" + bombhour + ":0" + bombminute + ":" + bombsecond;
            }
            else
            {
                if (bombsecond < 10) bombrealtimeDisp = "0" + bombhour + ":" + bombminute + ":0" + bombsecond;
                else bombrealtimeDisp = "0" + bombhour + ":" + bombminute + ":" + bombsecond;
            }
        }
        else if (bombhour != 0)
        {
            if (bombminute < 10)
            {
                if (bombsecond < 10) bombrealtimeDisp = "" + bombhour + ":0" + bombminute + ":0" + bombsecond;
                else bombrealtimeDisp = "" + bombhour + ":0" + bombminute + ":" + bombsecond;
            }
            else
            {
                if (bombsecond < 10) bombrealtimeDisp = "" + bombhour + ":" + bombminute + ":0" + bombsecond;
                else bombrealtimeDisp = "" + bombhour + ":" + bombminute + ":" + bombsecond;
            }
        }
        else
        {
            if (bombminute < 10)
            {
                if (bombsecond < 10) bombrealtimeDisp = "0" + bombminute + ":0" + bombsecond + "." + bombmilisecondDisp;
                else bombrealtimeDisp = "0" + bombminute + ":" + bombsecond + "." + bombmilisecondDisp;
            }
            else
            {
                if (bombsecond < 10) bombrealtimeDisp = "" + bombminute + ":0" + bombsecond + "." + bombmilisecondDisp;
                else bombrealtimeDisp = "" + bombminute + ":" + bombsecond + "." + bombmilisecondDisp;
            }
        }
        Displays[7].text = bombrealtimeDisp + " " + bomb.GetStrikes().ToString() + "X";
    }
    void Start()
    {

    }
    void buttonPress()
    {
        module.HandlePass();
        moduleSolved = true;
    }
    void startElapsedTime()
    {
        startupms = Math.Floor(Math.Round(elapsedTime % 60, 2) * 100 % 100);
        if (startupms < 10) startupmsDisplay = "0" + startupms.ToString();
        else startupmsDisplay = startupms.ToString();
        startupsec = Math.Floor(elapsedTime % 60);
        elapsedTime = 0;
    }
    void bombDefused()
    {
        if (!bombSolved)
        {
            Debug.Log("Solved");
            Debug.LogFormat("[Toolmod #{0}] Elapsed time on bomb solved: {1} after bomb activation.", moduleId, elapsedTimeDisplay);
        }
    }

#pragma warning disable 414
    string TwitchHelpMessage = "Use !{0} s.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:s)$");
        if (m.Success)
        {
            yield return null;
            SolveButton.OnInteract();
            yield break;
        }
        else
            yield return "sendtochaterror Invalid command.";
        yield break;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        SolveButton.OnInteract();
    }
}
