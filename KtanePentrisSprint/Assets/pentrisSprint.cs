using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

public class pentrisSprint : MonoBehaviour {
    public class ModSettingsJSON
    {
        public int linesToClear;
        public string note;
    }
	public GameObject Board;

	public Material BoxFull;
	public Material BoxEmpty;
	public Material BoxError;

	public KMSelectable ModuleSelectable;
	public KMSelectable MoveLeftButton;
	public KMSelectable MoveRightButton;
	public KMSelectable TurnLeftButton;
	public KMSelectable TurnRightButton;
	public KMSelectable DownButton;
	public KMSelectable MuteButton;

	public TextMesh numberDisplay;
	public TextMesh scoreDisplay;
	public TextMesh timeDisplay;
	public TextMesh targetDisplay;
	public KMModSettings modSettings;
	private PentrisBoard GameBoard;

	private const int G_WIDTH = 10; // Width of grid

	private KMBombModule Module;
	public KMAudio Audio;
	private GameObject[,] ObjectGrid;
	public GameObject[] ScreenGrid;
	private Pentomino tetr;
	private int upNext;
	private int Score;
	private int linesLeft;
	private int activation = 0;
	private int moduleId = 0;
	private static int moduleIdCounter = 1;
	private int[] PentoDisplay;
	private float elapsedTime;
	private string elapsedTimeDisplay;
	private bool started = false;
	private bool muted = false;
	private bool moduleSolved = false;
	private bool holdingleft, holdingright;
    private List<int> grabBag = new List<int>();
	KMAudio.KMAudioRef soundEffect;


	void SetMaterial(GameObject go, Material mat)
	{
		go.GetComponent<MeshRenderer> ().material = mat;
	}

	void UpdateGrid()
	{
		for (int y = 0; y < G_WIDTH; y++) {
			for (int x = 0; x < G_WIDTH; x++) {
				GameObject go = ObjectGrid [x, y];

				if (GameBoard.get (x, y) != 0) {
					go.SetActive (true);
					SetMaterial (go, BoxFull);
				} else {
					go.SetActive (false);
				}
			}
		}

		if (tetr != null) {
			List<IntPair> list = tetr.GetTileCoordinates ();
			foreach (IntPair p in list) {
				GameObject go = ObjectGrid [p.x, p.y];
				go.SetActive (true);
				if (tetr.isValid ()) {
					SetMaterial (go, BoxEmpty);
				} else {
					SetMaterial (go, BoxError);
				}
			}
		}
		scoreDisplay.text = Score.ToString();
		numberDisplay.text = linesLeft.ToString();
		if (moduleSolved) for (int i = 0; i < 17; i++) ScreenGrid[i].SetActive(false);
		else 
		{
			switch (upNext) {
				case 0:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 1, 0,
						1, 1, 0, 0,
						0, 1, 0, 0, 0
					};
					break;
				case 1:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						1, 1, 0, 0,
						0, 1, 1, 0,
						0, 1, 0, 0, 0
					};
					break;
				case 2:
					PentoDisplay = new int[] {
						0, 0, 1, 0, 
						0, 0, 1, 0, 
						0, 0, 1, 0,
						0, 1, 1, 0, 0
					};
					break;
				case 3:
					PentoDisplay = new int[] {
						0, 1, 0, 0, 
						0, 1, 0, 0,
						0, 1, 0, 0,
						0, 1, 1, 0, 0
					};
					break;
				case 4:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 1, 1, 0,
						0, 1, 1, 0, 
						0, 0, 1, 0, 0
					};
					break;
				case 5:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 1, 0,
						0, 1, 1, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 6:
					PentoDisplay = new int[] {
						0, 0, 1, 0, 
						0, 0, 1, 0,  
						0, 1, 1, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 7:
					PentoDisplay = new int[] {
						0, 1, 0, 0,
						0, 1, 0, 0, 
						0, 1, 1, 0,
						0, 0, 1, 0, 0
					};
					break;
				case 8:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 0, 0, 
						1, 1, 1, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 9:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						1, 1, 1, 0,
						0, 1, 0, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 10:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 0, 0, 0,
						1, 0, 1, 0,
						1, 1, 1, 0, 0 
					};
					break;
				case 11:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 0, 1, 0, 
						0, 0, 1, 0,
						1, 1, 1, 0, 0
					};
					break;
				case 12:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 0, 1, 0,
						0, 1, 1, 0,
						1, 1, 0, 0, 0
					};
					break;
				case 13:
					PentoDisplay = new int[] {
						0, 0, 1, 0,
						0, 1, 1, 0, 
						0, 0, 1, 0,
						0, 0, 1, 0, 0
					};
					break;
				case 14:
					PentoDisplay = new int[] {
						0, 1, 0, 0, 
						0, 1, 1, 0,  
						0, 1, 0, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 15:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 1, 0, 
						0, 1, 0, 0, 
						1, 1, 0, 0, 0
					};
					break;
				case 16:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						1, 1, 0, 0,
						0, 1, 0, 0,
						0, 1, 1, 0, 0
					};
					break;
				case 17:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 0, 0, 0,
						1, 1, 1, 1,
						0, 0, 0, 0, 1
					};
					break;
			}
			for (int i = 0; i < 17; i++) {
				if (PentoDisplay[i] != 0) {
					ScreenGrid[i].SetActive (true);
				} else {
					ScreenGrid[i].SetActive(false);
				}
			}
		}
	}

    int GetPiece()
    {
        if (grabBag.Count == 0)
            grabBag = Enumerable.Range(0, 18).ToList();

        int index = Random.Range(0, grabBag.Count);
        int pieceType = upNext;
		upNext = grabBag[index]; 
        grabBag.RemoveAt(index);
        return pieceType;
    }

	void ApplyPentomino() {
		if (tetr != null) {
			List<IntPair> list = tetr.GetTileCoordinates ();
			if (tetr.isValid ()) {
				Score += 10;
				foreach (IntPair p in list) {
					GameBoard.set (p.x, p.y, 1);
				}

				List<int> rows = GameBoard.getCompletedRows ();
				if (rows.Count > 0) {
					GameBoard.deleteRows (rows);
				}
				linesLeft = linesLeft - rows.Count;
				if (linesLeft < 0) linesLeft = 0;
				switch (rows.Count) {
					case 1:
						Score += 50;
						break;
					case 2:
						Score += 150;
						break;
					case 3:
						Score += 350;
						break;
					case 4:
						Score += 1000;
						break;
				}
				if (linesLeft > 0) {
					tetr = new Pentomino (G_WIDTH, GameBoard, GetPiece());
				} else {
					UpdateGrid ();
					Module.HandlePass ();
					moduleSolved = true;
					tetr = null;
					timeDisplay.color = new Color (0, 255, 0);
					if (soundEffect != null) soundEffect.StopSound();
					Debug.LogFormat("[Pentris Sprint #{0}] {1} is completed with a score of {2}, in {3}.", moduleId, targetDisplay.text, Score, elapsedTimeDisplay);				}
				UpdateGrid ();
			}
            else
            {
                //Module.OnStrike();
                Score -= 200;
				elapsedTime += 20;
				if (Score < 0) Score = 0;
				GameBoard = new PentrisBoard(G_WIDTH, G_WIDTH);
				tetr = new Pentomino (G_WIDTH, GameBoard, GetPiece());
                UpdateGrid();
            }
		}
	}

	void Awake()
	{
		Module = GetComponent<KMBombModule> ();

		this.ModuleSelectable.OnInteract += delegate
        {
            if (!started)
            {
                OnActivation();
				if (soundEffect == null) soundEffect = Audio.PlaySoundAtTransformWithRef("tetrisTheme", transform);
            }
            started = true;
            return true;
        };
        MoveLeftButton.OnInteract += delegate 		{ MoveLeft(); holdingleft = true; StartCoroutine(MoveLeftnew()); GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform); return false; };
        MoveLeftButton.OnInteractEnded += delegate 	{ holdingleft = false; };
        MoveRightButton.OnInteract += delegate 		{ MoveRight(); holdingright = true; StartCoroutine(MoveRightnew()); GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform); return false; };
        MoveRightButton.OnInteractEnded += delegate { holdingright = false; };

		TurnLeftButton.OnInteract += delegate() { return TurnLeft (); };
		TurnRightButton.OnInteract += delegate() { return TurnRight (); };
		DownButton.OnInteract += delegate() { return Down (); };
		MuteButton.OnInteract += delegate() { return Mute (); };

		//Module.OnActivate += OnActivation;

		/*MoveLeftButton.OnInteract += delegate() { return MoveLeft (); };
		MoveRightButton.OnInteract += delegate() { return MoveRight (); };
		TurnLeftButton.OnInteract += delegate() { return TurnLeft (); };
		TurnRightButton.OnInteract += delegate() { return TurnRight (); };
		DownButton.OnInteract += delegate() { return Down (); };*/

		ObjectGrid = new GameObject[G_WIDTH, G_WIDTH];

		GameBoard = new PentrisBoard (G_WIDTH, G_WIDTH);

		// Populate the grid
		for (int x = 0; x < G_WIDTH; x++) {
			GameObject col = Board.transform.Find ("Col" + x).gameObject;
			for (int y = 0; y < G_WIDTH; y++) {
				GameObject go = col.transform.Find ("Quad" + y).gameObject;
				go.SetActive (false);

				ObjectGrid [x, G_WIDTH - y - 1] = go;
			}
		}

		tetr = null;
		upNext = Random.Range(0, 7);
		

		UpdateGrid ();

	}

	void Start()
	{
		moduleId = moduleIdCounter++;
		Module.OnActivate += delegate { linesLeft = FindThreshold(); targetDisplay.text = linesLeft.ToString() + "L";}; 
	}

	protected void OnActivation()
	{
		tetr = new Pentomino (G_WIDTH, GameBoard, GetPiece());
		UpdateGrid ();
	}

	protected void OnDeactivation()
	{

	}



	void MoveLeft()
	{
		if (tetr != null) {
			tetr.MoveLeft ();
			UpdateGrid ();
		}
	}
	void MoveRight()
	{
		if (tetr != null) {
			tetr.MoveRight ();
			UpdateGrid ();
		}
	}
	IEnumerator MoveLeftnew()
	{
		//GetComponent<KMAudio> ().PlaySoundAtTransform ("part1.wav", transform);
		float elapsedTime = 0f;
        while (holdingleft)
        {
            elapsedTime += Time.deltaTime;

		if (elapsedTime >= .3f && !TwitchPlaysActive)
            {
				if (tetr != null) {
					tetr.MoveLeft ();
					UpdateGrid ();
				}
            }
            yield return null;
        }
	}
	IEnumerator MoveRightnew()
	{
		float elapsedTime = 0f;
        while (holdingright)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= .3f && !TwitchPlaysActive)
            {
				if (tetr != null) {
					tetr.MoveRight ();
					UpdateGrid ();
				}
            }
            yield return null;
        }
	}

	bool TurnLeft()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (tetr != null) {
			tetr.TurnLeft ();
			UpdateGrid ();
		}
		return false;
	}

	bool TurnRight()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (tetr != null) {
			tetr.TurnRight ();
			UpdateGrid ();
		}
		return false;
	}

	bool Down()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		ApplyPentomino ();
		return false;
	}
	bool Mute()
	{
		if (soundEffect != null) {
			soundEffect.StopSound();
			soundEffect = null;
		}
		else soundEffect = Audio.PlaySoundAtTransformWithRef("tetrisTheme", transform);

		return false;
	}
	bool TwitchPlaysActive;
    public readonly string TwitchHelpMessage = "Each command is a letter which can be stringed together. a: move left, d: move right, q: turn left, e: turn right, s: drop, m: mute.";

    public IEnumerator ProcessTwitchCommand(string command)
    {
        Dictionary<char, KMSelectable> buttonMap = new Dictionary<char, KMSelectable>()
        {
            { 'a', MoveLeftButton },
            { 'd', MoveRightButton },
            { 'q', TurnLeftButton },
            { 'e', TurnRightButton },
            { 's', DownButton },
			{ 'm', MuteButton}
        };

        var buttons = command.ToLowerInvariant().Replace(" ", "").Select(character =>
        {
            KMSelectable button;
            if (buttonMap.TryGetValue(character, out button))
            {
                return button;
            }

            return null;
        });

        if (buttons.Contains(null))
            yield break;

        yield return null;
        foreach (KMSelectable selectable in buttons)
        {
            selectable.OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
    }
	void Update() {
		if (started && !moduleSolved) {
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
			timeDisplay.text = "T+" + elapsedTimeDisplay;
		}
		if (moduleSolved) {
			if (soundEffect != null) {
				soundEffect.StopSound();
				soundEffect = null;
			}
		}
	}

	void OnExplode() {
		if (soundEffect != null)
        {
            soundEffect.StopSound();
            soundEffect = null;
        }
	}
	
    int FindThreshold()
    {
        try
        {
            ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(modSettings.Settings);
            if (settings != null)
            {
                if (settings.linesToClear < 10)
                    return 10;
                else if (settings.linesToClear > 10000)
                    return 10000;
                else return settings.linesToClear;
            }
            else return 40;
        }
        catch (JsonReaderException e)
        {
            Debug.LogFormat("[Pentris Sprint #{0}] JSON reading failed with error {1}, using default number.", moduleId, e.Message);
            return 40;
        }
    }

	/*void Update()
	{
		if (tetr != null) {
			bool moved = false;
			if (Input.GetKeyDown ("q")) {
				tetr.MoveLeft ();
				moved = true;
			}
			if (Input.GetKeyDown ("e")) {
				tetr.MoveRight ();
				moved = true;
			}
			if (Input.GetKeyDown ("a")) {
				tetr.TurnLeft ();
				moved = true;
			}
			if (Input.GetKeyDown ("d")) {
				tetr.TurnRight ();
				moved = true;
			}
			if (Input.GetKeyDown ("s")) {
				ApplyPentomino ();
				moved = true;
			}

			if (moved) {
				UpdateGrid ();
			}
		}
	}*/
}
