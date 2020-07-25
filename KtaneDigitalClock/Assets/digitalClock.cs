using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class digitalClock : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
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
    string answerMinuteDisplay;
    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    char alarmclock = '\u23F0';
    void Awake () {
        moduleId = moduleIdCounter++;
        tempunit = 0;

        Buttons[0].OnInteract += delegate {btn1Press(); Buttons[0].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform);return false;};
        Buttons[1].OnInteract += delegate {btn2Press(); Buttons[1].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform);return false;};
        Buttons[2].OnInteract += delegate {btn3Press(); Buttons[2].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform);return false;};
        Buttons[3].OnInteract += delegate {btn4Press(); Buttons[3].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform);return false;};
        Buttons[4].OnInteract += delegate {btn5Press(); Buttons[4].AddInteractionPunch(0.25f); Audio.PlaySoundAtTransform("click", transform);return false;};
        Displays[17].text = alarmclock.ToString();
    }

    // Use this for initialization
    void Start () {
        clockColorIndex = Random.Range(0, 14);
        timeformat = Random.Range(0, 2);
        blinking = Random.Range(0, 2);
        generateClock();
        StartCoroutine(BlinkAnim());
    }

    // Update is called once per frame
    void Update () {
        if (!moduleSolved && mode == 1) Displays[16].text = alarmclock.ToString();
        else Displays[16].text = "";
        //Display clock
        if (moduleSolved && timeformat == 1) {
            Displays[0].text = (int.Parse(DateTime.Now.ToString("hh")) / 10).ToString();
            Displays[14].text = (int.Parse(DateTime.Now.ToString("hh")) % 10).ToString();
            Displays[5].text = DateTime.Now.ToString("ss");
            Displays[6].text = ""; 
            if ((int.Parse((DateTime.Now.ToString("hh"))) / 10) == 0) {
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
        else if (moduleSolved && timeformat == 0) {
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
        Background.material = clockBackgroundColor[clockColorIndex];
        if (moduleSolved == false && mode == 0) {
            if (timeformat == 1) {
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
            else {
                Displays[0].text = initialHourDisplay;
                Displays[14].text = initialHourDisplay2.ToString();
                Displays[1].text = initialMinuteDisplay;
                Displays[3].text = ""; 
                Displays[4].text = "";
            }
            
        }
        else if (moduleSolved == false && mode == 1) {
            Displays[0].text = alarmHourDisplay;
            Displays[14].text = alarmHourDisplay2;
            Displays[1].text = alarmMinuteDisplay;
        }
        if (!moduleSolved && tempunit == 1) {
            Displays[5].text = tempf.ToString();
            Displays[6].text = "°F";
        }

        else if (!moduleSolved) {
            Displays[5].text = temp.ToString();
            Displays[6].text = "°C"; 
        }
        //Alarm
        if (alarmHour < 12 && timeformat == 1) {
            if (alarmHour == 0) {
                alarmHourDisplay = "1";
                alarmHourDisplay2 = "2";
            }
            else {
            alarmHourDisplay = (alarmHour / 10).ToString();
            if (alarmHourDisplay == "0") alarmHourDisplay = "";
            alarmHourDisplay2 = (alarmHour % 10).ToString();
            }    
            if (mode == 1) {
            Displays[3].text = "A"; 
            Displays[4].text = "";
            }
        }
        else if (timeformat == 1 && mode == 1) {
            if (alarmHour == 12) {
                alarmHourDisplay = "1";
                alarmHourDisplay2 = "2";
            }
            else {
                alarmHourDisplay = (alarmHour % 12 / 10).ToString();
                if (alarmHourDisplay == "0") alarmHourDisplay = "";
                alarmHourDisplay2 = (alarmHour % 12 % 10).ToString();
            }
            if (mode == 1) {
            Displays[3].text = ""; 
            Displays[4].text = "P";
            }
        }
        else if (mode == 1) {
            if (alarmHour < 10) {
                alarmHourDisplay = "0";
                alarmHourDisplay2 = (alarmHour % 10).ToString();
            }
            else {
                alarmHourDisplay = (alarmHour / 10).ToString();
                alarmHourDisplay2 = (alarmHour % 10).ToString();
            }
        }

        if (alarmMinute < 10) alarmMinuteDisplay = "0" + alarmMinute.ToString();
        else alarmMinuteDisplay = alarmMinute.ToString();

    }

    void generateClock() {
        temp = Random.Range(10, 38);
        //C to F conversion: (0°C × 9/5) + 32 = 32°F
        tempf = temp * 9/5 + 32;
        Debug.LogFormat("[Digital Clock #{0}]: Temperature = {1}°C / {2}°F", moduleId, temp,  tempf);

    //Red, Cyan, Orange(text, in black background) or Orange, White (background, with black text) 
        initialHour = Random.Range(0, 24);
        initialMinute = Random.Range(0, 60);
        alarmHour = 0;
        alarmMinute = 0;
        if (initialMinute < 10) initialMinuteDisplay = "0" + initialMinute.ToString();
        else initialMinuteDisplay = initialMinute.ToString();
        
        if (initialHour < 12 && timeformat == 1) {
            if (initialHour == 0) {
                initialHourDisplay = "1";
                initialHourDisplay2 = "2";
            }
            else {
                initialHourDisplay = (initialHour / 10).ToString();
                initialHourDisplay2 = (initialHour % 10).ToString();
            }
            if (mode == 1) {
            Displays[3].text = "A"; 
            Displays[4].text = "";
            }
        }
        else if (timeformat == 1) {
            if (initialHour == 12) {
                initialHourDisplay = "1";
                initialHourDisplay2 = "2";
            }
            else {
                initialHourDisplay = (initialHour % 12 / 10).ToString();
                initialHourDisplay2 = (initialHour % 12 % 10).ToString();
            }
            if (mode == 1) {
            Displays[3].text = ""; 
            Displays[4].text = "P";
            }
        }
        else {
            if (initialHour < 10) {
                initialHourDisplay = "0";
                initialHourDisplay2 = initialHour.ToString();
            }
            else { 
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

    void btn1Press() {
        //Mode button
        if (!moduleSolved) {
            if (mode == 1) checkAnswer();
            else mode = 1;
        }    
        //StartCoroutine(ChangeModes());
    }
    void btn2Press() {
        //Hour button
        if (mode == 1 && !moduleSolved) {
            alarmHour = (alarmHour + 1) % 24;
        }
    }
    void btn3Press() {
        if (mode == 1  && !moduleSolved) {
            alarmMinute = (alarmMinute + 1) % 60;
        }
        //Minute button
    }   
    void btn4Press() {
        //Temp button
        if (tempunit == 1) tempunit = 0;
        else tempunit = 1;
    }
    void btn5Press() {
        mode = 0;
        alarmHour = 0;
        alarmMinute = 0;
        //Reset button
    }

    void generateAnswer() {
        //24h
        answer = 0;
        if (timeformat == 0) {
        answer += 180;
        Debug.LogFormat("[Digital Clock #{0}]: Rule 1 is true, +180 min.", moduleId);
        }
        //bgcolor/txcolor
        if (clockColorIndex % 7 == 1) {
            answer += 765;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +765 min.", moduleId);
        }
        else if (clockColorIndex % 7 == 0 || clockColorIndex % 7 == 4 ) {
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +383 min.", moduleId);
            answer += 383;
        }
        else if (clockColorIndex % 7 == 2 || clockColorIndex % 7 == 5 ) {
            answer += 255;
            Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +255 min.", moduleId);
        }
        else if (clockColorIndex % 7 == 3 || clockColorIndex % 7 == 6 ) {
        answer += 510;
        Debug.LogFormat("[Digital Clock #{0}]: Rule 2/3, +510 min.", moduleId);
        }
        //blinking
        if (blinking == 1) {
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
    void checkAnswer() {
        if (answerHour == alarmHour && answerMinute == alarmMinute) {
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
    
    IEnumerator BlinkAnim() {
        while (alwaysOn) {
        blinkOn = true;
        yield return new WaitForSeconds(0.5f);
        blinkOn = false;
        yield return new WaitForSeconds(0.5f);
        }
    }
    /*private IEnumerator ChangeModes()
    {
        Debug.LogFormat("[M9 Holdable #1] Slide Coroutine Started!");
        float elapsedTime = 0f;
        Debug.LogFormat("[M9 Holdable #1] Counting Time...");
        while (holding)
        {
            elapsedTime += Time.deltaTime;
            Debug.LogFormat("[M9 Holdable #1] {0} second passed...", elapsedTime);

            if (elapsedTime >= 1f)
            {
                modeChange = true;
                Debug.LogFormat("[M9 Holdable #1] {0} second passed. Mode changed.", elapsedTime);
                yield break;
            }
            yield return null;
        }
    }*/

    #pragma warning disable 414
    string TwitchHelpMessage = "Use !{0} hours/minutes add <#> // set <##:##> // mode // reset // temp. Time specified are to be 24 hour format.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:mode|temp|reset|(hours|minutes) (add) (\d{1,2})|set (\d{1,2}):(\d{2}))$");
        if (m.Success)  
        {
            if (m.Groups[3].Success)
            {
                yield return null;
                if (mode != 1) {
                    yield return "sendtochaterror Digital Clock is not in alarm mode.";
                    yield break;
                }
                KMSelectable button;
                button = m.Groups[1].Value == "hours" ? Buttons[1]: Buttons[2];
                int count = m.Groups[1].Value == "hours" ? int.Parse(m.Groups[3].Value) % 24 : int.Parse(m.Groups[3].Value) % 60;
                for (int i = 0; i < count; i++)
                {
                    button.OnInteract();
                    yield return new WaitForSeconds(.1f);
                    yield return "trycancel";
                }
            }
            else if (m.Groups[4].Success)
            {
                if (mode != 1) Buttons[0].OnInteract();
                int tpHours = int.Parse(m.Groups[4].Value);
                int tpMins = int.Parse(m.Groups[5].Value);
                if (tpHours < 0 || tpHours > 24 || tpMins < 0 || tpMins > 59)
                {
                    yield return "sendtochaterror Invalid time! Hours must be in between 0-23 and minutes must be in between 0-59";
                    yield break;
                }
                yield return null;
                while (alarmHour != tpHours)
                {
                    Buttons[1].OnInteract();
                    yield return new WaitForSeconds(.1f);
                    yield return "trycancel";
                }
                yield return null;
                while (alarmMinute != tpMins)
                {
                    Buttons[2].OnInteract();
                    yield return new WaitForSeconds(.1f);
                    yield return "trycancel";
                }
                yield return null;
                Buttons[0].OnInteract();
            }
            else
            {
                yield return null;
                KMSelectable button;
                button = m.Groups[0].Value == "mode" ? Buttons[0] : m.Groups[0].Value == "temp" ? Buttons[3] : Buttons[4];
                button.OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
        else
            yield return "sendtochaterror Invalid command! Please use !{1} help to see full command.";
        yield break;
    }    
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        if (mode != 1) {
            Buttons[0].OnInteract();
            yield return new WaitForSeconds(.03f);
        }
        while (answerHour != alarmHour) {
            Buttons[1].OnInteract();
            yield return new WaitForSeconds(.03f);
            yield return true;
        }
        while (answerMinute != alarmMinute) {
            Buttons[2].OnInteract();
            yield return new WaitForSeconds(.03f);
            yield return true;
        }
        Buttons[0].OnInteract();
     }
}