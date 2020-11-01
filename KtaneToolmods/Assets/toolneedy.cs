    using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class toolneedy : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;
    public TextMesh[] Displays;
    public Color[] Color;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved, bombSolved;
    private float elapsedTime;
    private double startupsec, startupms;
    private string elapsedTimeDisplay, startupmsDisplay;
    private int solvedPercentage, solveCheck;
    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;

        Displays[0].text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        module.OnActivate += startElapsedTime;
        StartCoroutine(timeGlitchiness());
    }

    void Update ()
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
                if (second < 10) elapsedTimeDisplay = "0" + hour + ":0" + minute + ":0" + second;
                else elapsedTimeDisplay = "0" + hour + ":0" + minute + ":" + second;
            }
            else
            {
                if (second < 10) elapsedTimeDisplay = "0" + hour + ":" + minute + ":0" + second;
                else elapsedTimeDisplay = "0" + hour + ":" + minute + ":" + second;
            }
        }
        else if (hour != 0)
        {
            if (minute < 10)
            {
                if (second < 10) elapsedTimeDisplay = hour + ":0" + minute + ":0" + second;
                else elapsedTimeDisplay = hour + ":0" + minute + ":" + second;
            }
            else
            {
                if (second < 10) elapsedTimeDisplay = hour + ":" + minute + ":0" + second;
                else elapsedTimeDisplay = hour + ":" + minute + ":" + second;
            }
        }
        else
        {
            if (minute < 10)
            {
                if (second < 10) elapsedTimeDisplay = "0" + minute + ":0" + second + "." + milisecondDisp;
                else elapsedTimeDisplay = "0" + minute + ":" + second + "." + milisecondDisp;
            }
            else
            {
                if (second < 10) elapsedTimeDisplay = minute + ":0" + second + "." + milisecondDisp;
                else elapsedTimeDisplay = minute + ":" + second + "." + milisecondDisp;
            }
        }
        Displays[1].text = "T+" + elapsedTimeDisplay + " +" + startupsec + "." + startupmsDisplay + "s";

        solvedPercentage = ((bomb.GetSolvedModuleNames().Count) * 100 / bomb.GetSolvableModuleNames().Count);
        Displays[4].text = bomb.GetSolvedModuleNames().Count + "/" + bomb.GetSolvableModuleNames().Count;

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
        if (TimeModeActive) Displays[6].text = bombrealtimeDisp;
        else if (ZenModeActive) Displays[6].text = "N/A " + bomb.GetStrikes().ToString() + "X";
        else Displays[6].text = bombrealtimeDisp + " " + bomb.GetStrikes().ToString() + "X";

        if (solveCheck != bomb.GetSolvedModuleNames().Count) 
        {
            Debug.LogFormat("[Toolneedy #{0}] Solve #{1}: {2} #{4} - {3}", moduleId, bomb.GetSolvedModuleNames().Count, bomb.GetSolvedModuleNames().Last(), elapsedTimeDisplay, bomb.GetSolvedModuleNames().Where(x => x.Equals(bomb.GetSolvedModuleNames().Last())).Count() );
            solveCheck++;
        }
    }
    void Start()
    {

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
    IEnumerator timeGlitchiness() {
        while (!moduleSolved) {
            int displayedtimeremaining = Convert.ToInt32(Math.Floor(module.GetNeedyTimeRemaining()));
            yield return new WaitForSeconds(0.25f);
            while (solvedPercentage - module.GetNeedyTimeRemaining() >= 2 ) {
                yield return null;
                module.SetNeedyTimeRemaining(module.GetNeedyTimeRemaining() + 1);
                yield return new WaitForSeconds(0.25f);
            }
            module.SetNeedyTimeRemaining(solvedPercentage);
        }
    }
#pragma warning disable 414
	bool TimeModeActive;
	bool ZenModeActive;
    string TwitchHelpMessage = "Do nothing.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(.+)$");
        if (m.Success)
        {
            yield return null;
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }
}
