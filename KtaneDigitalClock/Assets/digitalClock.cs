using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class digitalClock : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMColorblindMode ColorblindMode;    
    public TextMesh[] Displays;
    public Renderer Background;
    public KMSelectable[] Buttons;
    public Color[] digitColor;
    public Color[] digitBackgroundColor;
    public Material[] clockBackgroundColor;
    private int temp, tempf, mode = 0, blinking = 0, answer;
    private int tempunit, timeformat, clockColorIndex;
    private int initialHour, initialMinute, alarmHour = 0, alarmMinute = 0, answerHour, answerMinute;
    private string initialHourDisplay, initialHourDisplay2, initialMinuteDisplay, alarmHourDisplay, alarmHourDisplay2, alarmMinuteDisplay;
    private bool blinkOn = true, alwaysOn = true;
    private bool holdingbtn1, holdingbtn2, holdingbtn3;
    private bool colorblindMode = false;
    string answerMinuteDisplay;
    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    char alarmclock = '\u23F0';
    float modeElapsedTime = 0f;
    void Awake()
    {
        moduleId = moduleIdCounter++;
        tempunit = 0;

        Buttons[0].OnInteract += delegate { holdingbtn1 = true; StartCoroutine(btn1New()); Buttons[0].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform); return false; };
        Buttons[0].OnInteractEnded += delegate { holdingbtn1 = false; };
        Buttons[1].OnInteract += delegate { btn2Press(); holdingbtn2 = true; StartCoroutine(btn2New()); Buttons[1].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform); return false; };
        Buttons[1].OnInteractEnded += delegate { holdingbtn2 = false; };
        Buttons[2].OnInteract += delegate { btn3Press(); holdingbtn3 = true; StartCoroutine(btn3New()); Buttons[1].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform); return false; };
        Buttons[2].OnInteractEnded += delegate { holdingbtn3 = false; };
        Buttons[3].OnInteract += delegate { btn4Press(); Buttons[3].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform); return false; };
        Buttons[4].OnInteract += delegate { btn5Press(); Buttons[4].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform); return false; };
        Displays[17].text = alarmclock.ToString();
    }

    // Use this for initialization
    void Start()
    {
        clockColorIndex = Random.Range(0, 14);
        timeformat = Random.Range(0, 2);
        blinking = Random.Range(0, 2);
        generateClock();

        colorblindMode = ColorblindMode.ColorblindModeActive;
        if (colorblindMode) { 
            if      (clockColorIndex == 0) Displays[18].text = "Orange";
            else if (clockColorIndex == 1) Displays[18].text = "White"; 
            else if (clockColorIndex == 2) Displays[18].text = "Blue";
            else if (clockColorIndex == 3) Displays[18].text = "Cyan";
            else if (clockColorIndex == 4) Displays[18].text = "Lime"; 
            else if (clockColorIndex == 5) Displays[18].text = "Red";
            else if (clockColorIndex == 6) Displays[18].text = "Yellow";
            else if (clockColorIndex == 7) Displays[18].text = "Orange";
            else if (clockColorIndex == 8) Displays[18].text = "White"; 
            else if (clockColorIndex == 9) Displays[18].text = "Blue";
            else if (clockColorIndex == 10) Displays[18].text = "Lime"; 
            else if (clockColorIndex == 11) Displays[18].text = "Purple";
            else if (clockColorIndex == 12) Displays[18].text = "Red";
            else if (clockColorIndex == 13) Displays[18].text = "Yellow";
        }
        else Displays[18].text = "";
        

        StartCoroutine(BlinkAnim());
    }

    // Update is called once per frame
    void Update()
    {

        //Display clock
        if (moduleSolved && timeformat == 1)
        {
            Displays[0].text = (int.Parse(DateTime.Now.ToString("hh")) / 10).ToString();
            Displays[14].text = (int.Parse(DateTime.Now.ToString("hh")) % 10).ToString();
            Displays[5].text = DateTime.Now.ToString("ss");
            Displays[6].text = "";
            if ((int.Parse((DateTime.Now.ToString("hh"))) / 10) == 0)
            {
                Displays[0].text = "";
            }
            Displays[1].text = DateTime.Now.ToString("mm");
            if (Convert.ToInt32(DateTime.Now.ToString("HH")) < 12 || DateTime.Now.ToString("tt") == "AM")
            {
                Displays[3].text = "A";
                Displays[4].text = "";
            }
            else
            {
                Displays[3].text = "";
                Displays[4].text = "P";
            }
        }
        else if (moduleSolved && timeformat == 0)
        {
            Displays[0].text = (int.Parse(DateTime.Now.ToString("HH")) / 10).ToString();
            Displays[14].text = (int.Parse(DateTime.Now.ToString("HH")) % 10).ToString();
            Displays[1].text = DateTime.Now.ToString("mm");
            Displays[3].text = "";
            Displays[4].text = "";
            Displays[5].text = DateTime.Now.ToString("ss");
            Displays[6].text = "";
        }
        if (mode == 1 || blinkOn || blinking == 0) Displays[2].text = ":";
        else Displays[2].text = "";
        for (int i = 0; i < 7; i++) Displays[i].color = digitColor[clockColorIndex];
        for (int i = 7; i < 14; i++) Displays[i].color = digitBackgroundColor[clockColorIndex];
        Displays[14].color = digitColor[clockColorIndex];
        Displays[15].color = digitBackgroundColor[clockColorIndex];
        Displays[16].color = digitColor[clockColorIndex];
        Displays[17].color = digitBackgroundColor[clockColorIndex];
        Displays[18].color = digitColor[clockColorIndex];
        Background.material = clockBackgroundColor[clockColorIndex];
        if (moduleSolved == false && mode == 0)
        {
            if (timeformat == 1)
            {
                //12h 
                Displays[0].text = initialHourDisplay.ToString();
                Displays[14].text = initialHourDisplay2.ToString();
                if (initialHourDisplay == "0") Displays[0].text = "";
                Displays[1].text = initialMinuteDisplay.ToString();
                if (initialHour < 12)
                {
                    Displays[3].text = "A";
                    Displays[4].text = "";
                }
                else
                {
                    Displays[3].text = "";
                    Displays[4].text = "P";
                }
            }
            else
            {
                Displays[0].text = initialHourDisplay;
                Displays[14].text = initialHourDisplay2.ToString();
                Displays[1].text = initialMinuteDisplay;
                Displays[3].text = "";
                Displays[4].text = "";
            }

        }
        else if (moduleSolved == false && mode == 1)
        {
            Displays[0].text = alarmHourDisplay;
            Displays[14].text = alarmHourDisplay2;
            Displays[1].text = alarmMinuteDisplay;
        }
        if (!moduleSolved && tempunit == 1)
        {
            Displays[5].text = tempf.ToString();
            Displays[6].text = "°F";
        }

        else if (!moduleSolved)
        {
            Displays[5].text = temp.ToString();
            Displays[6].text = "°C";
        }
        //Alarm
        if (alarmHour < 12 && timeformat == 1)
        {
            if (alarmHour == 0)
            {
                alarmHourDisplay = "1";
                alarmHourDisplay2 = "2";
            }
            else
            {
                alarmHourDisplay = (alarmHour / 10).ToString();
                if (alarmHourDisplay == "0") alarmHourDisplay = "";
                alarmHourDisplay2 = (alarmHour % 10).ToString();
            }
            if (mode == 1)
            {
                Displays[3].text = "A";
                Displays[4].text = "";
            }
        }
        else if (timeformat == 1 && mode == 1)
        {
            if (alarmHour == 12)
            {
                alarmHourDisplay = "1";
                alarmHourDisplay2 = "2";
            }
            else
            {
                alarmHourDisplay = (alarmHour % 12 / 10).ToString();
                if (alarmHourDisplay == "0") alarmHourDisplay = "";
                alarmHourDisplay2 = (alarmHour % 12 % 10).ToString();
            }
            if (mode == 1)
            {
                Displays[3].text = "";
                Displays[4].text = "P";
            }
        }
        else if (mode == 1)
        {
            if (alarmHour < 10)
            {
                alarmHourDisplay = "0";
                alarmHourDisplay2 = (alarmHour % 10).ToString();
            }
            else
            {
                alarmHourDisplay = (alarmHour / 10).ToString();
                alarmHourDisplay2 = (alarmHour % 10).ToString();
            }
        }

        if (alarmMinute < 10) alarmMinuteDisplay = "0" + alarmMinute.ToString();
        else alarmMinuteDisplay = alarmMinute.ToString();

    }

    void generateClock()
    {
        temp = Random.Range(10, 38);
        //C to F conversion: (0°C × 9/5) + 32 = 32°F
        tempf = temp * 9 / 5 + 32;
        Debug.LogFormat("[Digital Clock #{0}]: Temperature = {1}°C / {2}°F", moduleId, temp, tempf);

        //Red, Cyan, Orange(text, in black background) or Orange, White (background, with black text) 
        initialHour = Random.Range(0, 24);
        initialMinute = Random.Range(0, 60);
        alarmHour = 0;
        alarmMinute = 0;
        if (initialMinute < 10) initialMinuteDisplay = "0" + initialMinute.ToString();
        else initialMinuteDisplay = initialMinute.ToString();

        if (initialHour < 12 && timeformat == 1)
        {
            if (initialHour == 0)
            {
                initialHourDisplay = "1";
                initialHourDisplay2 = "2";
            }
            else
            {
                initialHourDisplay = (initialHour / 10).ToString();
                initialHourDisplay2 = (initialHour % 10).ToString();
            }
            if (mode == 1)
            {
                Displays[3].text = "A";
                Displays[4].text = "";
            }
        }
        else if (timeformat == 1)
        {
            if (initialHour == 12)
            {
                initialHourDisplay = "1";
                initialHourDisplay2 = "2";
            }
            else
            {
                initialHourDisplay = (initialHour % 12 / 10).ToString();
                initialHourDisplay2 = (initialHour % 12 % 10).ToString();
            }
            if (mode == 1)
            {
                Displays[3].text = "";
                Displays[4].text = "P";
            }
        }
        else
        {
            if (initialHour < 10)
            {
                initialHourDisplay = "0";
                initialHourDisplay2 = initialHour.ToString();
            }
            else
            {
                initialHourDisplay = (initialHour / 10).ToString();
                initialHourDisplay2 = (initialHour % 10).ToString();
            }
        }
        if (timeformat == 1) Debug.LogFormat("[Digital Clock #{0}]: Generated time in 24H = {1}:{2}, displaying 12H format.", moduleId, initialHour, initialMinuteDisplay);
        else Debug.LogFormat("[Digital Clock #{0}]: Generated time in 24H = {1}:{2}, displaying 24H format.", moduleId, initialHour, initialMinuteDisplay);

        generateAnswer();
    }
    /*
    bool keypadPress(KMSelectable object) {
        return false;
    }
    */
    /*
    bool buttonPress() {
	return false;
    }
    */

    void btn2Press()
    {
        //Hour button
        if (mode == 1 && !moduleSolved)
        {
            alarmHour = (alarmHour + 1) % 24;
            modeElapsedTime = 0f;
        }
    }
    void btn3Press()
    {
        if (mode == 1 && !moduleSolved)
        {
            alarmMinute = (alarmMinute + 1) % 60;
            modeElapsedTime = 0f;
        }
        //Minute button
    }
    void btn4Press()
    {
        //Temp button
        if (tempunit == 1) tempunit = 0;
        else tempunit = 1;
    }
    void btn5Press()
    {
        modeElapsedTime = 0f;
        mode = 0;
        alarmHour = 0;
        alarmMinute = 0;
        //Reset button
    }
    private IEnumerator btn1New()
    {
        float elapsedTime;
        elapsedTime = 0f;
        while (holdingbtn1)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 2f)
            {
                if (!moduleSolved && mode != 1)
                {
                    mode = 1;
                    StartCoroutine(AlarmSymbolFlash());
                    StartCoroutine(modeCounter());
                };
                elapsedTime = 0f;
                yield break;
            }
            yield return null;
        }
    }
    private IEnumerator btn2New()
    {
        float elapsedTime = 0f;
        while (holdingbtn2)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 1f)
            {
                if (mode == 1 && !moduleSolved)
                {
                    alarmHour = (alarmHour + 1) % 24;
                    modeElapsedTime = 0f;
                    yield return new WaitForSeconds(0.1f);
                };
            }
            yield return null;
        }
    }
    private IEnumerator btn3New()
    {
        float elapsedTime = 0f;
        while (holdingbtn3)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 1f)
            {
                if (mode == 1 && !moduleSolved)
                {
                    alarmMinute = (alarmMinute + 1) % 60;
                    modeElapsedTime = 0f;
                    yield return new WaitForSeconds(0.1f);
                };
            };
            yield return null;
        }
    }
    private IEnumerator AlarmSymbolFlash()
    {
        while (!moduleSolved && mode == 1)
        {
            yield return new WaitForSeconds(0.5f);
            Displays[16].text = alarmclock.ToString();
            yield return new WaitForSeconds(0.5f);
            Displays[16].text = "";
            yield return null;
        }
    }
    private IEnumerator modeCounter()
    {

        while (!moduleSolved)
        {
            if (mode == 1)
            {
                modeElapsedTime += Time.deltaTime;

                if (modeElapsedTime >= 5f)
                {
                    if (!moduleSolved) checkAnswer();
                    modeElapsedTime = 0f;
                    yield break;
                }
            }
            else modeElapsedTime = 0f;
            yield return null;
        }
    }
    void generateAnswer()
    {
        //24h
        answer = 0;
        if (timeformat == 0)
        {
            answer += 180;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 1 is true, +180 min.", moduleId);
        }
        //bgcolor/txcolor
        if (clockColorIndex % 7 == 1)
        {
            answer += 765;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +765 min.", moduleId);
        }
        else if (clockColorIndex % 7 == 0 || clockColorIndex % 7 == 4)
        {
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +383 min.", moduleId);
            answer += 383;
        }
        else if (clockColorIndex % 7 == 2 || clockColorIndex % 7 == 5)
        {
            answer += 255;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +255 min.", moduleId);
        }
        else if (clockColorIndex % 7 == 3 || clockColorIndex % 7 == 6)
        {
            answer += 510;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +510 min.", moduleId);
        }
        //blinking
        if (blinking == 1)
        {
            answer += 120;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 4 is true, +120 min.", moduleId);
        }
        //temp
        answer += temp;
        answer -= tempf;
        Debug.LogFormat("[Digital Clock #{0}]: Rule 5, +{1}-{2} min.", moduleId, temp, tempf);

        int hourtoAdd = answer / 60;
        int minutetoAdd = answer % 60;
        Debug.LogFormat("[Digital Clock #{0}]: Result = +{1} min (+{2}:{3}).", moduleId, answer, hourtoAdd, minutetoAdd);
        if (initialMinute + minutetoAdd > 59) hourtoAdd += 1;

        answerHour = (initialHour + hourtoAdd) % 24;
        answerMinute = (initialMinute + minutetoAdd) % 60;

        if (answerMinute < 10) answerMinuteDisplay = "0" + answerMinute.ToString();
        else answerMinuteDisplay = answerMinute.ToString();

        Debug.LogFormat("[Digital Clock #{0}]: Answer in 24H = {1}:{2}.", moduleId, answerHour, answerMinuteDisplay);
    }
    void checkAnswer()
    {
        modeElapsedTime = 0f;
        if (answerHour == alarmHour && answerMinute == alarmMinute)
        {
            mode = 2;
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            Audio.PlaySoundAtTransform("ring", transform);
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            generateClock();
            mode = 0;
            Displays[3].text = "";
            Displays[4].text = "";
        }
    }

    IEnumerator BlinkAnim()
    {
        while (alwaysOn)
        {
            blinkOn = true;
            yield return new WaitForSeconds(0.5f);
            blinkOn = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

#pragma warning disable 414
    bool TwitchShouldCancelCommand;
    string TwitchHelpMessage = "Use !{0} set <##:##> // reset // temp // colorblind. Time specified are to be 24 hour format.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:colorblind|temp|reset|set (\d{1,2}):(\d{2}))$");
        if (m.Success)
        {
            if (m.Groups[2].Success)
            {
                yield return null;
                int tpHours = int.Parse(m.Groups[1].Value);
                int tpMins = int.Parse(m.Groups[2].Value);
                if (tpHours < 0 || tpHours > 23 || tpMins < 0 || tpMins > 59)
                {
                    yield return "sendtochaterror Invalid time.";
                    yield break;
                }
                if (mode != 1)
                {
                    Buttons[0].OnInteract();
                    yield return new WaitForSeconds(2.1f);
                    Buttons[0].OnInteractEnded();
                }
                yield return "strike";
                yield return "solve";
                yield return null;
                if (alarmHour != tpHours)
                {
                    Buttons[1].OnInteract();
                    while (alarmHour != tpHours)
                    {
                        yield return new WaitForSeconds(.1f);
                        yield return "trycancel";
                    }
                    Buttons[1].OnInteractEnded();
                }
                yield return null;
                if (alarmMinute != tpMins)
                {
                    Buttons[2].OnInteract();
                    while (alarmMinute != tpMins)
                    {
                        yield return new WaitForSeconds(.1f);
                        yield return "trycancel";
                    }
                    Buttons[2].OnInteractEnded();
                }
                yield return null;
            }
            else if (m.Groups[0].Value == "colorblind") {
                yield return null;
                if      (clockColorIndex == 0) Displays[18].text = "Orange";
                else if (clockColorIndex == 1) Displays[18].text = "White"; 
                else if (clockColorIndex == 2) Displays[18].text = "Blue";
                else if (clockColorIndex == 3) Displays[18].text = "Cyan";
                else if (clockColorIndex == 4) Displays[18].text = "Lime"; 
                else if (clockColorIndex == 5) Displays[18].text = "Red";
                else if (clockColorIndex == 6) Displays[18].text = "Yellow";
                else if (clockColorIndex == 7) Displays[18].text = "Orange";
                else if (clockColorIndex == 8) Displays[18].text = "White"; 
                else if (clockColorIndex == 9) Displays[18].text = "Blue";
                else if (clockColorIndex == 10) Displays[18].text = "Lime"; 
                else if (clockColorIndex == 11) Displays[18].text = "Purple";
                else if (clockColorIndex == 12) Displays[18].text = "Red";
                else if (clockColorIndex == 13) Displays[18].text = "Yellow";
                yield break;
            }
            else
            {
                yield return null;
                KMSelectable button;
                button = m.Groups[0].Value == "temp" ? Buttons[3] : Buttons[4];
                button.OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
        else
            yield return "sendtochaterror Invalid command.";

        if (TwitchShouldCancelCommand) {
                Buttons[4].OnInteract();
                yield return "cancelled";
            }
        yield break;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        if (mode != 1)
        {
            Buttons[0].OnInteract();
            yield return new WaitForSeconds(2.1f);
            Buttons[0].OnInteractEnded();
        }
        yield return null;
        if (alarmHour != answerHour)
        {
            Buttons[1].OnInteract();
            while (alarmHour != answerHour)
            {
                yield return new WaitForSeconds(.1f);
            }
            Buttons[1].OnInteractEnded();
        }
        yield return null;
        if (alarmMinute != answerMinute)
        {
            Buttons[2].OnInteract();
            while (alarmMinute != answerMinute)
            {
                yield return new WaitForSeconds(.1f);
            }
            Buttons[2].OnInteractEnded();
        }
    }
}