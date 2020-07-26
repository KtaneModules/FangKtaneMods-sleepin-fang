using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class FlavorTextOption
{
    public String text;
    public int number;
}

public class zhStrokes : MonoBehaviour
{
    public TextAsset flavorTextJson;
    public KMSelectable[] Buttons;
    public new KMAudio audio;
    private KMAudio.KMAudioRef soundRef;
    public KMBombInfo bomb;
	public KMBombModule module;
    public TextMesh textDisplay;
    public TextMesh NumDisp, stageDisp;
    private int inputNumber, stageNumber = 1;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved = false, resettable = true, inputMode = true;

    FlavorTextOption textOption;
    List<FlavorTextOption> textOptions;
    void Awake()
    {
    	moduleId = moduleIdCounter++;
        Buttons[0].OnInteract += delegate {shiftleft(); Buttons[0].AddInteractionPunch(0.25f); return false;};
        Buttons[1].OnInteract += delegate {shiftright(); Buttons[1].AddInteractionPunch(0.25f); return false;};
        Buttons[2].OnInteract += delegate {submit(); Buttons[2].AddInteractionPunch(0.25f); return false;};
        Buttons[3].OnInteract += delegate {reset(); Buttons[3].AddInteractionPunch(0.25f); return false;};
        inputNumber = UnityEngine.Random.Range(1, 27);
        if (inputNumber < 10) NumDisp.text = "0" + inputNumber.ToString();
        else NumDisp.text = inputNumber.ToString();
    }

    void Start()
    {
        textOptions = JsonConvert.DeserializeObject<List<FlavorTextOption>>(flavorTextJson.text);
        Randomize();
    }
    void Randomize() {
        int index;
        //
        if (stageNumber == 1) index = UnityEngine.Random.Range(0, 694);
        else if (stageNumber == 2) index = UnityEngine.Random.Range(694, 5440); //12
        else if (stageNumber == 3) index = UnityEngine.Random.Range(5440, 7629); //12
        else index = UnityEngine.Random.Range(7629, 7893); //13+
        textOption = textOptions[index];
        textDisplay.text = textOption.text.ToString();
        //Randomize();
        Debug.LogFormat("[Chinese Strokes #{0}]: Selected character: {1} (index {3}), Number of strokes: {2}", moduleId, textOption.text, textOption.number, index);
    }
    void shiftleft() {
        if (inputMode) {
            if (!(moduleSolved || inputNumber <= 1)) {
                StopAllCoroutines();
                if (soundRef != null) soundRef.StopSound();
                inputNumber--;
                StartCoroutine(playSound());
            if (inputNumber < 10) NumDisp.text = "0" + inputNumber.ToString();
            else NumDisp.text = inputNumber.ToString();
            }
        }
    }
    void shiftright() { 
        if (inputMode) {
            if (!(moduleSolved || inputNumber >= 36)) {
                StopAllCoroutines();
                if (soundRef != null) soundRef.StopSound();
                inputNumber++;
                StartCoroutine(playSound());
            if (inputNumber < 10) NumDisp.text = "0" + inputNumber.ToString();
            else NumDisp.text = inputNumber.ToString();
            }
        }
    }
    void reset() {
        if (resettable && inputMode) {
            Debug.LogFormat("[Chinese Strokes #{0}]: Stage reset.", moduleId, inputNumber, textOption.number);
            Randomize();
            resettable = false;
        }
    }
    void submit() {
        if (inputMode) {
            if (inputNumber == textOption.number) {
                if (stageNumber == 4) {
                    module.HandlePass();
                    textDisplay.text = "赞";
                    stageDisp.text = "-";
                    NumDisp.text = "GG";
                    Debug.LogFormat("[Chinese Strokes #{0}]: Stage 3: You inputted {1}, expected {2}. Module solved.", moduleId, inputNumber, textOption.number);
                    inputMode = false;
                }
                else {
                    stageNumber++;
                    stageDisp.text = stageNumber.ToString();
                    Debug.LogFormat("[Chinese Strokes #{0}]: Stage {3}: You inputted {1}, expected {2}. Proceeding to next stage.", moduleId, inputNumber, textOption.number, stageNumber);
                    Randomize();
                }
                StopAllCoroutines();
                if (soundRef != null) soundRef.StopSound();
                audio.PlaySoundAtTransform("corr", transform);
            }
            else {
                module.HandleStrike();
                StopAllCoroutines();
                if (soundRef != null) soundRef.StopSound();
                audio.PlaySoundAtTransform("str", transform);
                Debug.LogFormat("[Chinese Strokes #{0}]: You inputted {1}, expected {2}. Module striked and reset.", moduleId, inputNumber, textOption.number);
                Randomize();
            }
        }
    }
    IEnumerator playSound() {
        switch (inputNumber) {
            case 1:
                soundRef = audio.PlaySoundAtTransformWithRef("1", transform);
                yield return new WaitForSeconds(0.757f);
                soundRef.StopSound();
                break;
            case 2:
                soundRef = audio.PlaySoundAtTransformWithRef("2", transform);
                yield return new WaitForSeconds(0.574f);
                soundRef.StopSound();
                break;
            case 3:
                soundRef = audio.PlaySoundAtTransformWithRef("3", transform);
                yield return new WaitForSeconds(0.731f);
                soundRef.StopSound();
                break;
            case 4:
                soundRef = audio.PlaySoundAtTransformWithRef("4", transform);
                yield return new WaitForSeconds(0.783f);
                soundRef.StopSound();
                break;
            case 5:
                soundRef = audio.PlaySoundAtTransformWithRef("5", transform);
                yield return new WaitForSeconds(0.574f);
                soundRef.StopSound();
                break;
            case 6:
                soundRef = audio.PlaySoundAtTransformWithRef("6", transform);
                yield return new WaitForSeconds(0.757f);
                soundRef.StopSound();
                break;
            case 7:
                soundRef = audio.PlaySoundAtTransformWithRef("7", transform);
                yield return new WaitForSeconds(0.653f);
                soundRef.StopSound();
                break;
            case 8:
                soundRef = audio.PlaySoundAtTransformWithRef("8", transform);
                yield return new WaitForSeconds(0.574f);
                soundRef.StopSound();
                break;
            case 9:
                soundRef = audio.PlaySoundAtTransformWithRef("9", transform);
                yield return new WaitForSeconds(0.757f);
                soundRef.StopSound();
                break;
            case 10:
                soundRef = audio.PlaySoundAtTransformWithRef("10", transform);
                yield return new WaitForSeconds(0.862f);
                soundRef.StopSound();
                break;
            case 11:
                soundRef = audio.PlaySoundAtTransformWithRef("11", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 12:
                soundRef = audio.PlaySoundAtTransformWithRef("12", transform);
                yield return new WaitForSeconds(0.992f);
                soundRef.StopSound();
                break;
            case 13:
                soundRef = audio.PlaySoundAtTransformWithRef("13", transform);
                yield return new WaitForSeconds(1.149f);
                soundRef.StopSound();
                break;
            case 14:
                soundRef = audio.PlaySoundAtTransformWithRef("14", transform);
                yield return new WaitForSeconds(1.097f);
                soundRef.StopSound();
                break;
            case 15:
                soundRef = audio.PlaySoundAtTransformWithRef("15", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 16:
                soundRef = audio.PlaySoundAtTransformWithRef("16", transform);
                yield return new WaitForSeconds(1.018f);
                soundRef.StopSound();
                break;
            case 17:
                soundRef = audio.PlaySoundAtTransformWithRef("17", transform);
                yield return new WaitForSeconds(1.149f);
                soundRef.StopSound();
                break;
            case 18:
                soundRef = audio.PlaySoundAtTransformWithRef("18", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 19:
                soundRef = audio.PlaySoundAtTransformWithRef("19", transform);
                yield return new WaitForSeconds(0.914f);
                soundRef.StopSound();
                break;
            case 20:
                soundRef = audio.PlaySoundAtTransformWithRef("20", transform);
                yield return new WaitForSeconds(1.044f);
                soundRef.StopSound();
                break;
            case 21:
                soundRef = audio.PlaySoundAtTransformWithRef("21", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 22:
                soundRef = audio.PlaySoundAtTransformWithRef("22", transform);
                yield return new WaitForSeconds(1.123f);
                soundRef.StopSound();
                break;
            case 23:
                soundRef = audio.PlaySoundAtTransformWithRef("23", transform);
                yield return new WaitForSeconds(1.175f);
                soundRef.StopSound();
                break;
            case 24:
                soundRef = audio.PlaySoundAtTransformWithRef("24", transform);
                yield return new WaitForSeconds(1.097f);
                soundRef.StopSound();
                break;
            case 25:
                soundRef = audio.PlaySoundAtTransformWithRef("25", transform);
                yield return new WaitForSeconds(1.175f);
                soundRef.StopSound();
                break;
            case 26:
                soundRef = audio.PlaySoundAtTransformWithRef("26", transform);
                yield return new WaitForSeconds(0.992f);
                soundRef.StopSound();
                break;
            case 27:
                soundRef = audio.PlaySoundAtTransformWithRef("27", transform);
                yield return new WaitForSeconds(1.149f);
                soundRef.StopSound();
                break;
            case 28:
                soundRef = audio.PlaySoundAtTransformWithRef("28", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 29:
                soundRef = audio.PlaySoundAtTransformWithRef("29", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 30:
                soundRef = audio.PlaySoundAtTransformWithRef("30", transform);
                yield return new WaitForSeconds(1.071f);
                soundRef.StopSound();
                break;
            case 31:
                soundRef = audio.PlaySoundAtTransformWithRef("31", transform);
                yield return new WaitForSeconds(1.384f);
                soundRef.StopSound();
                break;
            case 32:
                soundRef = audio.PlaySoundAtTransformWithRef("32", transform);
                yield return new WaitForSeconds(1.253f);
                soundRef.StopSound();
                break;
            case 33:
                soundRef = audio.PlaySoundAtTransformWithRef("33", transform);
                yield return new WaitForSeconds(1.306f);
                soundRef.StopSound();
                break;
            case 34:
                soundRef = audio.PlaySoundAtTransformWithRef("34", transform);
                yield return new WaitForSeconds(1.227f);
                soundRef.StopSound();
                break;
            case 35:
                soundRef = audio.PlaySoundAtTransformWithRef("35", transform);
                yield return new WaitForSeconds(1.227f);
                soundRef.StopSound();
                break;
            default:
                soundRef = audio.PlaySoundAtTransformWithRef("36", transform);
                yield return new WaitForSeconds(1.149f);
                soundRef.StopSound();
                break;

        }
    }
    #pragma warning disable 414
    string TwitchHelpMessage = "Use !{0} submit <#> / reset.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(command, @"^(?:reset|submit (\d{1,2}))$");
        if (m.Success)  
        {
            if (m.Groups[0].Value == "reset") {
                yield return null;
                Buttons[3].OnInteract();
                yield break;
            }
            else {
                yield return null;
                int tpNum = int.Parse(m.Groups[1].Value);
                if (tpNum < 1 || tpNum > 36)
                {
                    yield return "sendtochaterror Invalid digit.";
                    yield break;
                }
                KMSelectable button;
                while (tpNum != inputNumber) {
                button = tpNum <= inputNumber ? Buttons[0] : Buttons[1];
                    button.OnInteract();
                    yield return new WaitForSeconds(.2f);
                }
            }
            Buttons[2].OnInteract();
        }
        else
            yield return "sendtochaterror Invalid command.";
            yield break;
    }    
    IEnumerator TwitchHandleForcedSolve()
    {
        KMSelectable button;
        for (int i = 0; i < 3; i++) {
            while (textOption.number != inputNumber) {
                button = textOption.number <= inputNumber ? Buttons[0] : Buttons[1];
                button.OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            Buttons[2].OnInteract();
        }
     }
}
