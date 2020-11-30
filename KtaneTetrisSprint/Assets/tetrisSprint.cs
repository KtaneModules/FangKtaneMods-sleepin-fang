﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class tetrisSprint : MonoBehaviour {

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
	private TetrisBoard GameBoard;

	private const int G_WIDTH = 10; // Width of grid

	private KMBombModule Module;
	public KMAudio Audio;

	private GameObject[,] ObjectGrid;
	public GameObject[] ScreenGrid;
	private Tetromino tetr;
	private int upNext;
	private int Score;
	private int linesLeft = 40;
	private int piecesLeft;
	private int activation = 0;
	private int[] TetroDisplay;
	private float elapsedTime;
	private string elapsedTimeDisplay;
	private bool started = false;
	private bool muted = false;
	private bool moduleSolved = false;
	private bool holdingleft, holdingright;
    private List<int> grabBag = new List<int>();

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

		switch (upNext) {
			case 0:
				TetroDisplay = new int[] {
					0, 1, 1, 0,
					0, 0, 1, 1 
				};
				break;
			case 1:
				TetroDisplay = new int[] {
					0, 0, 1, 1,
					0, 1, 1, 0 
				};
				break;
			case 2:
				TetroDisplay = new int[] {
					0, 1, 0, 0,
					0, 1, 1, 1 
				};
				break;
			case 3:
				TetroDisplay = new int[] {
					0, 0, 0, 1,
					0, 1, 1, 1 
				};
				break;
			case 4:
				TetroDisplay = new int[] {
					0, 0, 1, 0,
					0, 1, 1, 1 
				};
				break;
			case 5:
				TetroDisplay = new int[] {
					0, 0, 0, 0,
					1, 1, 1, 1 
				};
				break;
			case 6:
				TetroDisplay = new int[] {
					0, 1, 1, 0,
					0, 1, 1, 0 
				};
				break;
		}

		for (int i = 0; i < 8; i++) {
			if (TetroDisplay[i] != 0) {
				ScreenGrid[i].SetActive (true);
			} else {
				ScreenGrid[i].SetActive(false);
			}
		}
	}

    int GetPiece()
    {
        if (grabBag.Count == 0)
            grabBag = Enumerable.Range(0, 7).ToList();

        int index = Random.Range(0, grabBag.Count);
        int pieceType = upNext;
		upNext = grabBag[index]; 
        grabBag.RemoveAt(index);
        return pieceType;
    }

	void ApplyTetromino() {
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
				piecesLeft--;
				if (linesLeft > 0) {
					tetr = new Tetromino (G_WIDTH, GameBoard, GetPiece());
				} else {
					Module.HandlePass ();
					moduleSolved = true;
					tetr = null;
					timeDisplay.color = new Color (0, 255, 0);
					Audio.PlaySoundAtTransformWithRef("tetrisTheme", transform).StopSound();

				}
				UpdateGrid ();
			}
            else
            {
                //Module.OnStrike();
                Score -= 200;
				elapsedTime += 20;
				if (Score < 0) Score = 0;
				GameBoard = new TetrisBoard(G_WIDTH, G_WIDTH);
				tetr = new Tetromino (G_WIDTH, GameBoard, GetPiece());
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
				Audio.PlaySoundAtTransformWithRef("tetrisTheme", transform);
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

		GameBoard = new TetrisBoard (G_WIDTH, G_WIDTH);

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
		piecesLeft = 0;
		upNext = Random.Range(0, 7);
		UpdateGrid ();
	}

	void Start()
	{


	}

	protected void OnActivation()
	{
		//piecesLeft = 3;
		tetr = new Tetromino (G_WIDTH, GameBoard, GetPiece());
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

            if (elapsedTime >= .3f)
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

            if (elapsedTime >= .3f)
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
		ApplyTetromino ();
		return false;
	}
	bool Mute()
	{
		if (muted) {
			Audio.PlaySoundAtTransformWithRef("tetrisTheme", transform);
			muted = false;
		}
		else {
			Audio.PlaySoundAtTransformWithRef("tetrisTheme", transform).StopSound();
			muted = true;
		}
		return false;
	}

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
				ApplyTetromino ();
				moved = true;
			}

			if (moved) {
				UpdateGrid ();
			}
		}
	}*/
}
