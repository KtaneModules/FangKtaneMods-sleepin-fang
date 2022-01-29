using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Text;
using System.Text.RegularExpressions;

public class pwMutilatorEX : MonoBehaviour
{
    public KMBombInfo bomb;
    public KMBombModule module;
    public KMSelectable[] keyboard;
    public KMSelectable[] timeBtns;
    public TextMesh[] displayTextsLeft;
    public TextMesh[] displayTextsRight;
    public TextMesh[] timeTexts;
    public Renderer[] modFrames;
    public Renderer   modBackground;
    public Material[] Materials;

    bool moduleSolved = false;
    bool moduleActivated = false;

    int[] stageAnswer; 

    private readonly Dictionary<char, int> HTMLCharCodes = new Dictionary<char, int>();

    // Logging
    static int moduleIdCounter = 1;
    int moduleId;

    double[] times = { 0.00, 0.00, 60.00, 0.00 }; //Elapsed, Bomb RT, Countdown timer, Internal Buffer

    // Use this for initialization
    void Awake()
    {
        // Assigning buttons
        foreach (KMSelectable key in keyboard)
        {
            KMSelectable pressedKey = key;
            key.OnInteract += () => HandlePress(pressedKey);
        }

        //Module ID
        moduleId = moduleIdCounter++;
        //
        //
        //
        StartCoroutine(ColorCycle());
        module.OnActivate += delegate { moduleActivated = true; InitModule(); };
    }

    private IEnumerator ColorCycle()
    {
        Material tempMat = Materials[0];
        float hue = 0;
        while (true)
        {
            hue = (hue + 0.005f) % 1f;
            tempMat.color = Color.HSVToRGB(hue, 1f, 0.5f);
            foreach (Renderer modFrame in modFrames)
            {
                if (moduleSolved) modFrame.material = Materials[1];
                modFrame.material = tempMat;
            }
            modBackground.material = tempMat;
            yield return new WaitForSeconds(.1f);
        }
    }

    void InitModule()
    {

    }
    bool HandlePress(KMSelectable key)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
        key.AddInteractionPunch(0.25f);
        Debug.Log(key.GetComponentInChildren<TextMesh>().text.ToString());
        return false;
    } 
    void Update()
    {
        if (!moduleActivated) return;
        //Timer Multiplier for bomb RT
        double multiplier;
        switch (bomb.GetStrikes())
        {
            case 0:  multiplier = 1; break;
            case 1:  multiplier = 1.25; break;
            case 2:  multiplier = 1.5; break;
            case 3:  multiplier = 1.75; break;
            default: multiplier = 2; break;
        }
        //Displaying times
        times[0] += Time.deltaTime; 
        times[1]  = bomb.GetTime() / multiplier;
        times[2] -= Time.deltaTime;

        for (int i = 0; i < 3; i++) 
        {
            double second = Math.Round(times[i] % 60, 2);
            double minute = Math.Floor(times[i] / 60 % 60);
            double hour = Math.Floor(times[i] / 3600);

            timeTexts[i].text = (hour > 0 ? hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00")
                                     : minute.ToString("00") + ":" + (times[i] % 60).ToString("00.00"));
        }
    }
    
    void GenStage(int stageNumber, out int[] stageInformation)
    {
        int twoFactor = Random.Range(100000, 1000000);
        int baseNumber = Random.Range(5, 17);

        int increaseFactorAverage = Random.Range(baseNumber^1, baseNumber^2);

        displayTextsLeft[0].text = (twoFactor / 1000).ToString() + " " + (twoFactor % 1000).ToString();
        stageInformation = [stageNumber, twoFactor, increaseFactorAverage];
    }

    void CalStage(int stageNumber)
    {

        stageAnswer[stageNumber - 1] = 0;
    }

    #pragma warning disable 414
    readonly string TwitchHelpMessage = "Use !{0} press <number> // toggle <switches position> // time // clear // split // split/submit (at/on/-) <time> // s <number> <switches (1 as up, 0 as down)> <time>.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:(press ([0-9]+))|(toggle ([1-3]+))|(time|clear|split)|((submit|split)\s*(?:at|on)?\s*([0-9]+:)?([0-9]+):([0-5][0-9]))|(s\s*([0-9]{7})\s*([01]{3})\s*([0-9]+:)?([0-9]+):([0-5][0-9])))$");

        if (!m.Success || (m.Groups[6].Success && m.Groups[8].Success && int.Parse(m.Groups[9].Value)> 59))
            yield break;
        yield return null;
    }

}
