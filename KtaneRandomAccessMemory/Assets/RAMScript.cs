using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Random = UnityEngine.Random;
using KModkit;

public class RAMScript : MonoBehaviour
{
    private string[] ignoredModules;
    private int nonIgnoredcount;
    public Material[] colours;
    public Renderer[] buttons;
    public TextMesh[] texts;
    public KMSelectable clear;
    public KMBombInfo Bomb;
    int increasedDuration, increasedPercentage;
    int currentDigit, limitDigit, unitPosition;
    string currentUnit;
    int appclearedcount, clearcount = 1;
    string appclearedtext;
    bool solvedState = false, inputMode = false, safeMode = false, readytoSolve = false;
    int numofbars1, numofbars2;
    string bars1, bars2;
    public Color normal, warning, shutdown, normaltext;
    int resetPercentage;
    int desiredSolvePercentage;
    decimal solvedPercentage, currentPercentage, lifespan;
    private static int moduleIdCounter = 1;
    private int moduleId = 0;
    private string[] unitList = new string[6] { "B", "KB", "MB", "GB", "TB", "PB" };
    // Use this for initialization
    void Awake()
    {
        moduleId = moduleIdCounter++;
        //Clear button interact
        clear.OnInteract += clearRAM;
        //Picking units
        unitPosition = Random.Range(0, 6);
        currentUnit = unitList[unitPosition];
        //Randomizing RAM Limit
        limitDigit = Random.Range(100, 1000);
        //Initial RAM = 16.67%
        currentDigit = limitDigit / 6;
        //Display all of them on the module
        texts[3].text = currentDigit.ToString();
        texts[4].text = currentUnit.ToString();
        texts[5].text = limitDigit.ToString();
        texts[6].text = currentUnit.ToString();
        UpdateProgressBar();
        desiredSolvePercentage = Random.Range(40, 80);
        texts[11].text = string.Empty;
    }
    void Start()
    {
        //Activates the module
        //Getting the Ignore List from Boss Module Manager
        ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("Random Access Memory", new string[]{
            "Random Access Memory",
            "14"
        });
        //Count for non-ignored modules
        nonIgnoredcount = Bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
    	//nonIgnoredcount = nonIgnoredcount + 2; Debug.LogFormat("[Random Access Memory #{0}]: Please notify creator because he forgot he added 2 on non-Ignored modules for debugging!", moduleId); //For debugging 
        StartCoroutine(checkForceSolve());
        StartCoroutine(DisplayTime());
		Debug.LogFormat("[Random Access Memory #{0}]: Number of non-ignored modules = {1}", moduleId, nonIgnoredcount);
		Debug.LogFormat("[Random Access Memory #{0}]: Desired Solve Percentage = {1}%", moduleId, desiredSolvePercentage);
        //StartCoroutine(solveDelay()); //Debugging
    }
    void UpdateProgressBar()
    {
        //27 Bars > 45 (3.333x more)
        if (safeMode == false && solvedState == false) {
        numofbars1 = currentDigit * 45 / limitDigit;
        numofbars2 = (45 - numofbars1);
        decimal percentage = (decimal) currentDigit * (decimal) 100 / (decimal) limitDigit; 
        currentPercentage = Math.Round(percentage, 2);
        texts[14].text = currentPercentage + "%";
        for (int i = 1; i <= numofbars1; i++)
        {
            bars1 += "█";
        }
        for (int i = 1; i <= numofbars2; i++)
        {
            bars2 += "█";
        }
        if (currentDigit * 100 / limitDigit >= 70)
        {
            texts[9].color = warning;
            for (int i = 3; i <= 7; i++)
                texts[i].color = warning;
        }
        else
        {
            texts[9].color = normal;
            for (int i = 3; i <= 7; i++)
                texts[i].color = normaltext;
        }
        texts[9].text = bars1;
        texts[10].text = bars2;
        bars1 = "";
        bars2 = "";
        }
    }
    IEnumerator DisplayTime() {
        while (!solvedState) {
            lifespan = (decimal) 100 - (( (decimal) solvedPercentage) * (decimal) 100 / (decimal) desiredSolvePercentage );
            lifespan = Math.Round(lifespan, 2);
            texts[12].text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            texts[13].text = lifespan + "% // " + DateTime.Now.ToString("HH:mm");
            yield return new WaitForSeconds(0.01f);
        }
    }
    private bool clearRAM()
    //When the clear button is pressed
    {
		if (solvedState == false && readytoSolve == true && inputMode == true)
		{
        solvedState = true;
        GetComponent<KMBombModule>().HandlePass();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        Debug.LogFormat("[Random Access Memory #{0}]: Module Shutdown.", moduleId);
        Debug.LogFormat("[Random Access Memory #{0}]: You have cleared for a total of {1} times.", moduleId, clearcount);
		StartCoroutine(PostSolveAnimation());
		}
        else if (solvedState == false && inputMode == true && (currentDigit * 100 / limitDigit) >= 60)
        {
            StartCoroutine(clearAnimation());
        }
        else if (solvedState == false && inputMode == true)
        {
            StartCoroutine(clearAnimationlite());
        }
        return false;
    }

    IEnumerator clearAnimation()
    {
        if (safeMode == false) {
        inputMode = false;
        appclearedcount = Random.Range(100, 150) * ((currentDigit * 100 / limitDigit) - 45) / 100;
        appclearedtext = appclearedcount + " applications cleared.";
        buttons[10].material = colours[1];
        texts[11].text = appclearedtext;
        resetPercentage = Random.Range(40, 50);
        currentDigit = resetPercentage * limitDigit / 100;
        yield return new WaitForSeconds(1.5f);
        buttons[10].material = colours[2];
        texts[11].text = string.Empty;
        inputMode = true;
        yield return new WaitForSeconds(1f);
        texts[3].text = currentDigit.ToString();
		clearcount += 1;
        UpdateProgressBar();
        }
    }
    IEnumerator clearAnimationlite()
    {
        if (safeMode == false) {
        inputMode = false;
        appclearedtext = "0 applications cleared.";
        buttons[10].material = colours[1];
        texts[11].text = appclearedtext;
        yield return new WaitForSeconds(1.5f);
        buttons[10].material = colours[2];
        texts[11].text = string.Empty;
        inputMode = true;
        yield return new WaitForSeconds(1f);
        }
    }
    IEnumerator checkForceSolve()
    {
        if (nonIgnoredcount == 0)
        {
            yield return new WaitForSeconds(5f);
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Random Access Memory #{0}]: There are no non-ignored modules, forcing module to be solved.", moduleId);
            solvedState = true;
            inputMode = false;
            texts[3].text = "---";
            texts[5].text = "---";
			StartCoroutine(PostSolveAnimation());
        }
        else { StartCoroutine(RAMUsage()); };
    }

	IEnumerator PostSolveAnimation()
	{
        yield return new WaitForSeconds(1.5f);
        foreach (Renderer b in buttons)
        {
        b.material = colours[0];
        }
    	foreach (TextMesh thing in texts)
        {
		thing.text = string.Empty;
        }
	}
    IEnumerator RAMUsage()
    {
        if (solvedState == false && safeMode == false)
        {
            inputMode = true;
            increasedDuration = Random.Range(3, 8);
            for (int i = 1; i < increasedDuration; i++)
            {
                if (safeMode == false){
				yield return new WaitForSeconds(1f);
				}
            }
            increasedPercentage = Random.Range(2, 6);
            currentDigit = currentDigit + (increasedPercentage * limitDigit / 100);
            texts[3].text = currentDigit.ToString();
            UpdateProgressBar();
            StartCoroutine(checkSolveStrike());
        }
    }
    IEnumerator checkSolveStrike()
    {   
        solvedPercentage = ((decimal) Bomb.GetSolvedModuleNames().Count() * (decimal) 100 / (decimal) nonIgnoredcount) ;
        solvedPercentage = Math.Round(solvedPercentage, 2);
        if ((solvedPercentage > desiredSolvePercentage) && safeMode == false) {
            solvedPercentage = desiredSolvePercentage;
            StartCoroutine(solveDelay());
        }
        else if (currentDigit > limitDigit && safeMode == false)
        {
            inputMode = false;
            currentDigit = limitDigit;
            texts[3].text = currentDigit.ToString();
            UpdateProgressBar();
            //*Add bomb freeze here
            yield return new WaitForSeconds(3f);
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Random Access Memory #{0}]: RAM Exceeded Maximum, module striked.", moduleId);
            currentDigit = limitDigit * 4 / 10;
            yield return new WaitForSeconds(0.01f);
			UpdateProgressBar();
            StartCoroutine(RAMUsage());
            inputMode = true;
        }
        else StartCoroutine(RAMUsage());
    }

    IEnumerator solveDelay()
    {
        //sound = 7.039s
		safeMode = true; 
        yield return new WaitForSeconds(3f);
        texts[14].text = "";
		texts[1].text = "Safe Mode";
		texts[2].text = "Safe Mode has been activated,\nfor safety reasons.\n\nThose includes risk of fire, explosion,\nor injury.";
        currentDigit = 0;
        GetComponent<KMAudio>().PlaySoundAtTransform("4beeps", transform);
        inputMode = false;
        buttons[10].material = colours[1];
        Debug.LogFormat("[Random Access Memory #{0}]: Initiated safe mode!", moduleId);
        texts[9].color = shutdown;
        bars1 = "";
        bars2 = "█████████████████████████████████████████████";
        numofbars1 = 44;
        numofbars2 = 44;
        texts[9].text = bars1;
        texts[10].text = bars2;
        texts[3].text = "---";
        texts[5].text = "---";
        for (int i = 1; i <= 45; i++)
        {
            yield return new WaitForSeconds(0.15642f);
            bars1 += "█";
            bars2 = bars2.Remove(numofbars2);
            numofbars2--;
            texts[9].text = bars1;
            texts[10].text = bars2;
        }
        yield return new WaitForSeconds(1.33333333f);
        for (int i = 1; i <= 45; i++)
        {
            yield return new WaitForSeconds(1.33333333f); //3.333s(100s) > 2.222(60s) is normal, lesser is for debug
            bars1 = bars1.Remove(numofbars1);
            bars2 += "█";
            numofbars1--;   
            texts[9].text = bars1;
            texts[10].text = bars2;
        }
		yield return new WaitForSeconds(3f);
		inputMode = true;
		buttons[10].material = colours[2];
		texts[8].text = "Shut down";
		readytoSolve = true;
	}

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} clear [Clears the memory] | !{0} shut down [Presses the shutdown button]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) 
		|| Regex.IsMatch(command, @"^\s*slap clear now\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
		|| Regex.IsMatch(command, @"^\s*clear memory\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
		|| Regex.IsMatch(command, @"^\s*shutdown\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
		|| Regex.IsMatch(command, @"^\s*shut down\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            clear.OnInteract();
            yield break;
        }
    }

    void TwitchHandleForcedSolve()
    {
        StartCoroutine(HandleSolve());
    }

    IEnumerator HandleSolve()
    {
        while (!safeMode)
        {
            while ((currentDigit * 100 / limitDigit) < 60) { yield return new WaitForSeconds(0.1f); }
            clear.OnInteract();
            yield return new WaitForSeconds(0.001f);
        }
        while (!readytoSolve) { yield return new WaitForSeconds (0.1f);}
        clear.OnInteract();
        yield return new WaitForSeconds(0.001f);
    }
}
