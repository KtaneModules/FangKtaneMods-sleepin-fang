using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Text;
using System.Text.RegularExpressions;

public class pwMutilator : MonoBehaviour
{

    private const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    // Logging
    static int moduleIdCounter = 1;
    int moduleId;

    // Use this for initialization
    void Awake()
    {
        // Assigning buttons
        foreach (KMSelectable key in keypad)
        {
            KMSelectable pressedKey = key;
        }        //
        //Module ID
        moduleId = moduleIdCounter++;
        //
        //
        //
    }
    void Update() {
    }
    #pragma warning disable 414
    string TwitchHelpMessage = "Use !{0} press <number> // toggle <switches position> // time // clear // split // split/submit (at/on/-) <time> // s <number> <switches (1 as up, 0 as down)> <time>.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:(press ([0-9]+))|(toggle ([1-3]+))|(time|clear|split)|((submit|split)\s*(?:at|on)?\s*([0-9]+:)?([0-9]+):([0-5][0-9]))|(s\s*([0-9]{7})\s*([01]{3})\s*([0-9]+:)?([0-9]+):([0-5][0-9])))$");

        if (!m.Success || (m.Groups[6].Success && m.Groups[8].Success && int.Parse(m.Groups[9].Value)> 59))
            yield break;
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
    }
}
