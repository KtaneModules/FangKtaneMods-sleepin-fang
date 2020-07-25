using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class passwordGenerator : MonoBehaviour {
	public KMAudio Audio;
	public KMBombInfo Bomb;
	public KMSelectable[] keypad;
	public KMSelectable clearButton;
	public KMSelectable submitButton;
	public TextMesh Screen;
	
	private int pressedNumber = 0;
	private bool inputMode = false;
	private string submitKey = "";
	private string generatedInput = "";
	private string convertedPorts = "";
	private string convertedBatteries = "";
	private string convertedIndicators = "";
	private string convertedLetter = "";
	private string symbol ="";
    private readonly string[] abc = new[] { "A", "B", "C", "D", "E", "F" };
    private Dictionary<string, KMSelectable> btnDict = new Dictionary<string, KMSelectable>();
    // Logging
    static int moduleIdCounter = 1;
	int moduleId;
	string currentHour;
	string currentMin;
	

	// Use this for initialization
	void Awake () {
        btnDict = new Dictionary<string, KMSelectable>()
        {
            {"@",keypad[0]},
            {"&",keypad[1]},
            {"?",keypad[2]},
            {"*",keypad[3]},
            {"/",keypad[4]},
            {"-",keypad[5]},
            {"1",keypad[6]},
            {"2",keypad[7]},
            {"3",keypad[8]},
            {"4",keypad[9]},
            {"5",keypad[10]},
            {"6",keypad[11]},
            {"7",keypad[12]},
            {"8",keypad[13]},
            {"9",keypad[14]},
            {"0",keypad[15]},
            {"a",keypad[16]},
            {"b",keypad[17]},
            {"c",keypad[18]},
            {"d",keypad[19]},
            {"e",keypad[20]},
            {"f",keypad[21]},
            {"r",clearButton},
            {"s",submitButton}
        };

        moduleId = moduleIdCounter++;

		// Assigning buttons
		foreach (KMSelectable key in keypad)
		{
			KMSelectable pressedKey = key;
            key.OnInteract += () => PressKey(pressedKey);
		}
        clearButton.OnInteract += resetInput;
        submitButton.OnInteract += submitInput;
		ScreenDisplay(submitKey);
		inputMode = true;
		StartCoroutine(TimeDisplay());
		Debug.LogFormat("[Password Generator #{0}]: The module is waiting for submit button press to start the calculation.", moduleId);
	}
	// When a key is pressed
	bool PressKey (KMSelectable key)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
		key.AddInteractionPunch(0.25f);
		if (inputMode == true)
		{
			submitKey += key.GetComponentInChildren<TextMesh>().text.ToString();
			ScreenDisplay(submitKey);
			pressedNumber = submitKey.Length;	
			checkInput(pressedNumber);
		}
        return false;
	}
    IEnumerator TimeDisplay() {
			while (pressedNumber == 0 && inputMode == true) {
				currentHour = DateTime.Now.ToString("HH");
				currentMin = DateTime.Now.ToString("mm");
				if (pressedNumber == 0 && inputMode == true) {Screen.text = currentHour + ":" + currentMin + "      ";};
				yield return new WaitForSeconds(0.5f);
				if (pressedNumber == 0 && inputMode == true) {Screen.text = currentHour + " " + currentMin + "      ";};
				yield return new WaitForSeconds(0.5f);
			}	
		}
	
	// Displays the screen
	private void ScreenDisplay(string str) 
	{
		Screen.text = str;
	}
	// Reset the input
	private bool resetInput ()
		{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, clearButton.transform);
		clearButton.AddInteractionPunch(0.25f);
		if (inputMode == true && pressedNumber != 0)
			{	
			submitKey = "";
			pressedNumber = 0;
			ScreenDisplay(submitKey);
			StartCoroutine(TimeDisplay());
			};
        return false;
		}
	// Submit the number
	private bool submitInput ()
		{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitButton.transform);
		submitButton.AddInteractionPunch(0.25f);
		if (inputMode == true)
			{
			if (Bomb.GetModuleNames().Contains("Bamboozled Again") || Bomb.GetModuleNames().Contains("Ultimate Cycle") || Bomb.GetModuleNames().Contains("UltraStores"))
			{
				generatedInput = "*DEAD*";
				Debug.LogFormat("[Password Generator #{0}]: Bamboozled Again, UltraStores or Ultimate Cycle present, ignoring all other rules.", moduleId);
				Debug.LogFormat("[Password Generator #{0}]: The correct input is *DEAD*.", moduleId);
			}
			else 
			{
				Debug.LogFormat("[Password Generator #{0}]: Submit button was pressed! Generating input..", moduleId);
				//Part I: First letter of SN
				char firstletter = Bomb.GetSerialNumberLetters().First();
				var firstCharPos = char.ToUpperInvariant(firstletter) - 'A' + 1;
				Debug.LogFormat("[Password Generator #{0}]: The numerical position of the first character of serial number is {1}", moduleId, firstCharPos);

                convertedLetter = abc[firstCharPos % 6];
				Debug.LogFormat("[Password Generator #{0}]: The calculated answer for Part I is {1}.", moduleId, convertedLetter);

                //Part II: Indicators, Batt, Ports
                convertedBatteries = abc[Bomb.GetBatteryCount() % 6];
                convertedPorts = abc[Bomb.GetPortCount() % 6];
                convertedIndicators = abc[Bomb.GetIndicators().Count() % 6];

				Debug.LogFormat("[Password Generator #{0}]: Number of batteries = {1}, indicators = {2}, and ports = {3}", moduleId, Bomb.GetBatteryCount(), Bomb.GetIndicators().Count(), Bomb.GetPortCount());
				
				string correctPart2 = "";
				if ( Bomb.GetIndicators().Count() == Bomb.GetBatteryCount() || Bomb.GetIndicators().Count() == Bomb.GetPortCount() || Bomb.GetBatteryCount() == Bomb.GetPortCount()  )
				{
				Debug.LogFormat("[Password Generator #{0}]: There are equal number of batteries, indicators or ports, reversing the order", moduleId);
				correctPart2 = convertedPorts + convertedIndicators + convertedBatteries;
				Debug.LogFormat("[Password Generator #{0}]: The calculated answer for Part II is {1}", moduleId, correctPart2);
				}
				else
				{
				Debug.LogFormat("[Password Generator #{0}]: There are different number of batteries, indicators and ports.", moduleId);
				correctPart2 = convertedBatteries + convertedIndicators + convertedPorts;

				Debug.LogFormat("[Password Generator #{0}]: The calculated answer for Part II is {1}", moduleId, correctPart2);
				}
				// Part III: Symbols
				if (containsModule("Question Mark", true))
					{
						symbol = "?"; 
						Debug.LogFormat("[Password Generator #{0}]: Rule 1 applied: There are Question Mark module on the bomb.", moduleId);
					}
				else if (containsModule("Astrology", true))
					{	
						symbol = "*";
						Debug.LogFormat("[Password Generator #{0}]: Rule 2 applied: There are Astrology module on the bomb.", moduleId);
					}
				else if (containsModule(new[] { "logic", "boolean" }, false))
					{	
						symbol = "&";
						Debug.LogFormat("[Password Generator #{0}]: Rule 3 applied: There are at least one module with 'Logic' or 'Boolean' in its name on the bomb.", moduleId);
					}
				else if (containsModule("code", false))
					{	
						symbol = "/" ;
						Debug.LogFormat("[Password Generator #{0}]: Rule 4 applied: There are at least one module with 'Code' in its name on the bomb.", moduleId);
					}
				else if (containsModule("alphabet", false))
					{	
						symbol = "@";
						Debug.LogFormat("[Password Generator #{0}]: Rule 5 applied: There are at least one module with 'Alphabet' in its name on the bomb.", moduleId);
					}
				else
					{	
						symbol = "-";
						Debug.LogFormat("[Password Generator #{0}]: Otherwise rule.", moduleId);
					}
				// Part IV: Solved, Unsolved, Minutes Remaining
				int solvedCount = Bomb.GetSolvedModuleNames().Count;
				int unsolvedCount = Bomb.GetSolvableModuleNames().Count - Bomb.GetSolvedModuleNames().Count;
				int minutesRemaining = (int) Bomb.GetTime() / 60;

				Debug.LogFormat("[Password Generator #{0}] The submit button was pressed at {1} solve(s), {2} unsolved and {3} min(s) remaining.", moduleId, solvedCount, unsolvedCount, minutesRemaining);
				int lastDigit = Bomb.GetSerialNumberNumbers().Last();
				Debug.LogFormat("[Password Generator #{0}]: The last digit of the Serial Number is {1}.", moduleId, lastDigit);
				int correctPart4 = ( (solvedCount * unsolvedCount * minutesRemaining) + lastDigit ) % 100;
				Debug.LogFormat("[Password Generator #{0}] The calculated answer for Part IV is {1}.", moduleId, correctPart4);


				//Contenating the calculated answer
				generatedInput = convertedLetter + correctPart2 + symbol + correctPart4;
				Debug.LogFormat("[Password Generator #{0}]: The final input is {1}.", moduleId, generatedInput);
			}
			//Whether is the input correct
			if (submitKey == generatedInput)
			{
				GetComponent<KMBombModule>().HandlePass();
				Debug.LogFormat("[Password Generator #{0}]: You have inputted correct answer. Module solved.", moduleId);
				submitKey = "SOLVED";
				ScreenDisplay(submitKey);
				inputMode = false;
				Audio.PlaySoundAtTransform("win95start", transform);
			}
			else 
			{
				GetComponent<KMBombModule>().HandleStrike();
				Debug.LogFormat("[Password Generator #{0}]: You have inputted {1}, which is a wrong answer. Module striked and reset.", moduleId, submitKey);
				StartCoroutine(DisplayError(submitKey));
				pressedNumber = 0;
				ScreenDisplay(submitKey);
				generatedInput = "";
			}
		}
        return false;
	}

    private bool containsModule(object Module, bool Exact)
    {
        return Exact ? (Module.GetType().IsArray ? Bomb.GetModuleNames().Any(mod => ((string[])Module).Contains(mod)) : Bomb.GetModuleNames().Contains((string)Module)) : (Module.GetType().IsArray ? Bomb.GetModuleNames().Any(mod => ((string[])Module).Any(param => mod.ToLowerInvariant().Contains(param))) : Bomb.GetModuleNames().Any(mod => mod.ToLowerInvariant().Contains((string)Module)));
    }

    // Strike if received too many inputs
    void checkInput(int pressedNumber)
	{
		if (pressedNumber > 7)
		{
			GetComponent<KMBombModule>().HandleStrike();
			StartCoroutine(DisplayBlink(submitKey));	
			Debug.LogFormat("[Password Generator #{0}]: You have inputted too many characters. Module striked and reset.", moduleId);
		}
	}
    	IEnumerator DisplayBlink(string checkInput) {
		inputMode = false;
		for (var i = 0; i < 3; i++) {
			submitKey = "-------";
			ScreenDisplay(submitKey);
            yield return new WaitForSeconds(0.5f);
			submitKey = "";
			ScreenDisplay(submitKey);
            yield return new WaitForSeconds(0.5f);
		}
		pressedNumber = 0;
		inputMode = true;
		StartCoroutine(TimeDisplay());
		}
		IEnumerator DisplayError(string submitInput) 
		{
			inputMode = false;
			StopCoroutine(TimeDisplay());
			yield return new WaitForSeconds(0.1f);
			submitKey = "-WRONG-";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(3f);
			submitKey = "-WRONG";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			submitKey = "-WRON";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			submitKey = "-WRO";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			submitKey = "-WR";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			submitKey = "-W";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			submitKey = "-";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			submitKey = "";
			ScreenDisplay(submitKey);
			yield return new WaitForSeconds(0.5f);
			inputMode = true;
			StartCoroutine(TimeDisplay()); 
		}
    #pragma warning disable 414
    string TwitchHelpMessage = "Use '!{0} press <button>'. Buttons include (S)ubmit, clea(R), or any other on the keypad.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if(command.StartsWith("press "))
        {
            string btns = command.Replace("press ", "").Replace(" ", "");
            char[] numbers = btns.ToCharArray();
            List<KMSelectable> btnsToPress = new List<KMSelectable>();
            foreach (char number in numbers)
            {
                if(!btnDict.ContainsKey(number.ToString()))
                {
                    yield return null;
                    yield return "sendtochaterror Invalid button.";
                    yield break;
                }
                btnsToPress.Add(btnDict[number.ToString()]);
            }
            yield return null;
            yield return btnsToPress.ToArray();
        }
    	else {
            yield return null;
            yield return "sendtochaterror Unrecognised or invalid command.";
            yield break;
        }
    }
}
