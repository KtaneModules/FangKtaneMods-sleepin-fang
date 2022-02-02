using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable IDE1006
public class pwMutilatorEX : MonoBehaviour
#pragma warning restore IDE1006
{
    public KMBombInfo bomb;
    public KMBombModule module;
    public KMBossModule boss;

    public KMSelectable[] keyboard;
    public KMSelectable[] functionKeys;
    public KMSelectable[] timeBtns;
    public TextMesh[] displayTextsLeft;
    public TextMesh[] displayTextsRight;
    public TextMesh[] timeTexts;
    public Renderer[] modFrames;
    public Renderer   modBackground;
    public Material[] Materials;


    static string[] ignoredModules;
    bool moduleSolved = false;
    bool moduleActivated = false;
    bool moduleWaiting = false;
    bool nextPhase = false;

    bool[] phases = new bool[4];
        
    int currStage;
    int totalStage;
    int currSolved;

    char[] finalAnswer;
    string finalAnswerString;
    string inputAnswerString;

    int[] stageInformation;
    int[] stageAnswer;

    float h;
    float v;

    private Dictionary<string, string> altKeysDict = new Dictionary<string, string>();
    private Dictionary<int, char> htmlDict = new Dictionary<int, char>();

    // Logging
    static int moduleIdCounter = 1;
    int moduleId;

    float[] times = { 0.00f, 0.00f, 10.00f }; //Elapsed, Bomb RT, Countdown timer
    float[] countdownTimer = { 2.00f, 10.00f, 5.00f }; //Delayer, or Reset timer or Input phase >> times[2];
    float[] defaultTimes = { 2.00f, 10.00f };
    

    void Awake()
        //Initializations
    {
        h = Random.Range(0f, 1f);
        v = Random.Range(-0.5f, 0.5f);
        altKeysDict = new Dictionary<string, string>()
        {
            {"``","~"},
            {"11","!"},
            {"22","@"},
            {"33","#"},
            {"44","$"},
            {"55","%"},
            {"66","^"},
            {"77","&"},
            {"88","*"},
            {"99","("},
            {"00",")"},
            {"--","_"},
            {"==","+"},
            {"QQ","q"},
            {"WW","w"},
            {"EE","e"},
            {"RR","r"},
            {"TT","t"},
            {"YY","y"},
            {"UU","u"},
            {"II","i"},
            {"OO","o"},
            {"PP","p"},
            {"AA","a"},
            {"SS","s"},
            {"DD","d"},
            {"FF","f"},
            {"GG","g"},
            {"HH","h"},
            {"JJ","j"},
            {"KK","k"},
            {"LL","l"},
            {"ZZ","z"},
            {"XX","x"},
            {"CC","c"},
            {"VV","v"},
            {"BB","b"},
            {"NN","n"},
            {"MM","m"},
            {"[[","{"},
            {"]]","}"},
            {"\\\\","|"},
            {";;",":"},
            {"\'\'","\""},
            {",,","<"},
            {"..",">"},
            {"//","?"},
        };
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
        
        module.OnActivate += Activate;
    }

    void Start()
    {
        StartCoroutine(ColorCycle());
        if (ignoredModules == null)
            ignoredModules = boss.GetIgnoredModules("Password Mutilator EX", new string[] // Default Ignore List
            {
				"Password Mutilator EX",
                "Alchemy",
                "Forget Everything",
                "Forget Infinity",
                "Forget Me Not",
                "Forget This",
                "Purgatory",
                "Souvenir",
                "Cookie Jars",
                "Divided Squares",
                "Hogwarts",
                "The Swan",
                "Turn the Keys",
                "The Time Keeper",
                "Timing is Everything",
                "Turn the Key"
            });

    }

    void Activate()
    {
        totalStage = bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
        stageAnswer = new int[totalStage];
        finalAnswer = new char[totalStage];

        InitModule();
    }
    private IEnumerator ColorCycle()
    {
        Material tempMat = Materials[0];

        while (true)
        {
            h = (h + 0.0005f) % 1f;
            v = (v + 0.0005f) % 1f;
            tempMat.color = Color.HSVToRGB(h, 1f, Mathf.Abs(v - 0.5f));
            foreach (Renderer modFrame in modFrames)
            {
                if (moduleSolved) modFrame.material = Materials[1];
                else modFrame.material = tempMat;
            }
            modBackground.material = tempMat;
            yield return new WaitForSeconds(.01f);
        }
    }
 
    void InitModule()
        //Initiate stage phase. (Pre-stage phase not yet implemented)
    {
        moduleActivated = true;
        phases[0] = true;
    }


    bool HandlePress(KMSelectable key)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
        key.AddInteractionPunch(0.25f);
        switch (key.GetComponentInChildren<TextMesh>().text)
        { 
            case "F1": ; break;
            case "F2": ; break;
            case "F3": ; break;
            case "F4": if (phases[3]) SubmitStage(); break;
            case "<": ; break;
            case "v": ; break;
            case "^": ; break;
            case ">": ; break;
            case "Backspace": if (inputAnswerString.Length > 0) inputAnswerString = inputAnswerString.Remove(inputAnswerString.Length-1, 1); break;
            default: if (phases[3]) inputAnswerString += key.GetComponentInChildren<TextMesh>().text; break;
        }

        return false;   
    } 
    void Update()
    {
        if (!moduleActivated || moduleSolved) return;

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

        currSolved = bomb.GetSolvedModuleNames().Count();
        if (phases[3])
        {   //Input phase, receiving inputs to solve answer.
            try {foreach (string key in altKeysDict.Keys) if (inputAnswerString.Contains(key)) inputAnswerString = inputAnswerString.Replace(key, altKeysDict[key]);}
            catch (NullReferenceException) { };
            displayTextsLeft[1].text = inputAnswerString;
        } 
        else if (phases[2] && currStage == totalStage && countdownTimer[2]<= 0)
            //Input phase commenced: note down elpTime, calculate final answer.
        {
            phases[2] = false;
            phases[3] = true;
            InputStage();
        }
        else if (currStage == totalStage && countdownTimer[2] > 0 )
        {   //Input phase commencing.
            phases[1] = false;
            phases[2] = true;
            displayTextsLeft[0].text = "--- ---";
            displayTextsLeft[1].text = "----";
            displayTextsRight[0].text = "Next phase:";
            displayTextsRight[0].color = Color.yellow;
            countdownTimer[2] -= Time.deltaTime;
        }
        else if (phases[1] && countdownTimer[0] <= 0f && currSolved > currStage)
        {   //Stage phase - Solved something AND time depleted.
            CalStage();
            currStage++;
            GenStage(currStage);
            countdownTimer[0] = defaultTimes[0];
            countdownTimer[1] = defaultTimes[1];
        }
        else if (phases[1] && currSolved > currStage)
        {   //Stage phase - Solved something before time depleted.
            if (countdownTimer[0] <= 0f) countdownTimer[0] = defaultTimes[0];
            moduleWaiting = true;
            countdownTimer[0] -= Time.deltaTime;
            countdownTimer[1] = defaultTimes[1];
            displayTextsRight[0].text = "Next in:";
            displayTextsRight[0].color = Color.yellow;
        }
        else if (phases[1])
        {   //Stage phase - Not yet solved.
            moduleWaiting = false;
            if (countdownTimer[0] > 0f) countdownTimer[0] -= Time.deltaTime;
            countdownTimer[1] -= Time.deltaTime;
            displayTextsRight[0].text = "Reset in:";
            displayTextsRight[0].color = Color.white;
            if (countdownTimer[1] <= 0f)
            {
                GenStage(currStage);
                countdownTimer[1] = defaultTimes[1];
            }
        }
        else if (phases[0])
        {   //Pre-stage phase, transition.
            phases[0] = false;
            phases[1] = true;
            GenStage(1);
        }
        else if (phases[0] && currSolved > 0)
        {
            float countdown = 30f;
            countdown -= Time.deltaTime;
            if (countdown <= 0f) { module.HandleStrike(); countdown = 30f; } ;
        }

        //Displaying times
        times[0] += Time.deltaTime; 
        times[1]  = Convert.ToSingle(bomb.GetTime() / multiplier);
        times[2]  = phases[2] ? countdownTimer[2]: moduleWaiting ? countdownTimer[0] : countdownTimer[1];
        for (int i = (phases[3] ? 1 : 2); i >= 0; i--)
        {
            if (defaultTimes[0] - countdownTimer[0] >= 5f && i != 2 ) return;
            double second = Math.Round(times[i] % 60, 2);
            double minute = Math.Floor(times[i] / 60 % 60);
            double hour = Math.Floor(times[i] / 3600);

            timeTexts[i].text = (hour > 0 ? hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00")
                                     : minute.ToString("00") + ":" + (second).ToString("00.00"));
            if (i == 1) timeTexts[i].text = "-" + timeTexts[i].text;
        }

        

    }
    
    void GenStage(int stageNumber)
        //Generate stage when next stage/timer runs out.
    {
        StopAllCoroutines();
        StartCoroutine(ColorCycle());
        int twoFactor, startingNumber, increaseFactorAverage, radix;
        
        twoFactor = Random.Range(100000, 1000000);
        radix = Random.Range(5, 17);
        startingNumber          = Random.Range(radix*radix*radix*radix, radix*radix*radix*radix*radix);
        increaseFactorAverage   = Random.Range((radix*radix)+2, (radix*radix*radix)-2);
        
        stageInformation = new int[] { stageNumber, twoFactor, increaseFactorAverage, startingNumber, radix };
        StartCoroutine(DisplayStage());
    }
                                                
    IEnumerator DisplayStage()
        //Display the stage after generating its information.
    {
        if (!phases[1]) yield break;
             
        displayTextsLeft[0].text = (stageInformation[1] / 1000).ToString("000") + " " + (stageInformation[1] % 1000).ToString("000");
        displayTextsLeft[3].text = "  Stage  " + stageInformation[0].ToString("000") + " / " + (totalStage-1).ToString("000");     //"  Stage  000 / 000"
        
        List<int> increaseFactorPool = new int[] { stageInformation[2] - 2, stageInformation[2] - 1, stageInformation[2], stageInformation[2] + 1, stageInformation[2] + 2 }.ToList();
        int rawValue = stageInformation[3];
        int radix = stageInformation[4];
        int rng = 0;
        while (phases[1])
        {
            if (increaseFactorPool.Count() == 0)
                increaseFactorPool =  new int[] { stageInformation[2] - 2, stageInformation[2] - 1, stageInformation[2], stageInformation[2] + 1, stageInformation[2] + 2 }.ToList();
            else rng = Random.Range(0, increaseFactorPool.Count());
            rawValue = (rawValue + increaseFactorPool[rng]) % (radix * radix * radix * radix);
            displayTextsLeft[1].text = DecimalToArbitrarySystem(rawValue, radix);
            increaseFactorPool.RemoveAt(rng);
            yield return new WaitForSeconds(.5f);
        }
    }

    public static string DecimalToArbitrarySystem(int decimalNumber, int radix)
    {
        const int BitsInLong = 64;
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        if (decimalNumber == 0)
            return "0";

        int index = BitsInLong - 1;
        long currentNumber = Math.Abs(decimalNumber);
        char[] charArray = new char[BitsInLong];

        while (currentNumber != 0)
        {
            int remainder = (int)(currentNumber % radix);
            charArray[index--] = Digits[remainder];
            currentNumber = currentNumber / radix;
        }

        string result = new String(charArray, index + 1, BitsInLong - index - 1);
        if (decimalNumber < 0)
        {
            result = "-" + result;
        }

        return result;
    }

    void CalStage()
        //Calculate stage when next stage is shown, but prior to input phase.
    {
        stageAnswer[stageInformation[0]] = stageInformation[1] % 9 + stageInformation[2] + Convert.ToInt32(Math.Floor(times[0] % 60)) + Convert.ToInt32(Math.Floor(times[1] / 60 % 60));
        
        Debug.LogFormat("Stage {0}: 2FAST: {1}, If = {2}, CT = {3}+{4}, Cv = {5}", 
            stageInformation[0], stageInformation[1], stageInformation[2], Convert.ToInt32(Math.Floor(times[0] % 60)), Convert.ToInt32(Math.Floor(times[1] / 60 % 60)), 
            stageAnswer[stageInformation[0]]);
    }
    
    void InputStage()
    {   //Calculate final answer, note down phase start time.
        Debug.Log(times[0]);
        for (int i = 0; i < stageAnswer.Length; i++)
        {
            stageAnswer[i] = (((stageAnswer[i] + Convert.ToInt32(Math.Floor(times[0]))) % 94) + 33);
            if (i != 0 && stageAnswer[i] == stageAnswer[i - 1]) stageAnswer[i] = stageAnswer[i] == 126 ? stageAnswer[i] - 1 : stageAnswer[i] + 1;
            finalAnswer[i] = Convert.ToChar(stageAnswer[i]);
        }
        finalAnswerString = new string(finalAnswer);
        Debug.LogFormat("Input phase started at {0} seconds.", Math.Round(times[0], 2));
        Debug.LogFormat("Final answer string: {0} ", finalAnswerString);
        displayTextsLeft[0].text = "Input password:";
        displayTextsRight[0].text = "";
        displayTextsRight[1].text = ""; 
    }
    
    void SubmitStage()
    {
        if (finalAnswerString == inputAnswerString)
        {
            module.HandlePass();
            moduleSolved = true;
        }
        else module.HandleStrike();
    }
#pragma warning disable 414
    readonly string TwitchHelpMessage = "Use !{0} press <number> // toggle <switches position> // time // clear // split // split/submit (at/on/-) <time> // s <number> <switches (1 as up, 0 as down)> <time>.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
        //Twitch Plays.
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:(press ([0-9]+))|(toggle ([1-3]+))|(time|clear|split)|((submit|split)\s*(?:at|on)?\s*([0-9]+:)?([0-9]+):([0-5][0-9]))|(s\s*([0-9]{7})\s*([01]{3})\s*([0-9]+:)?([0-9]+):([0-5][0-9])))$");

        if (!m.Success || (m.Groups[6].Success && m.Groups[8].Success && int.Parse(m.Groups[9].Value)> 59))
            yield break;
        yield return null;
    }

}
