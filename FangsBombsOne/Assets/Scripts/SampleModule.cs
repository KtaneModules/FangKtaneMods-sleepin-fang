using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleModule : MonoBehaviour {

    private static int _moduleIDCounter = 1;
    private int _moduleID;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMAudio Audio;
    private bool active;

    void Start () {
        _moduleID = _moduleIDCounter++;
        Module.OnActivate += delegate () { active = true; };
    }
}
