using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class TetType {
	public int[] map;
	public int width;

	public TetType(int[] map, int width)
	{
		this.map = map;
		this.width = width;
	}
}

public class Tetromino {

	private static int[] S_LEFT_MAP = {
		1, 1, 0,
		0, 1, 1,
		0, 0, 0
	};

	private static int[] S_RIGHT_MAP = {
		0, 1, 1,
		1, 1, 0,
		0, 0, 0
	};

	private static int[] L_LEFT_MAP = {
		1, 0, 0,
		1, 1, 1,
		0, 0, 0
	};

	private static int[] L_RIGHT_MAP = {
		0, 0, 1,
		1, 1, 1,
		0, 0, 0
	};

	private static int[] CROSS_MAP = {
		0, 1, 0,
		1, 1, 1,
		0, 0, 0
	};

	private static int[] STRAIGHT_MAP = {
		0, 0, 0, 0,
		1, 1, 1, 1,
		0, 0, 0, 0,
		0, 0, 0, 0
	};

	private static int[] SQUARE_MAP = {
		1, 1,
		1, 1
	};

	private static TetType S_LEFT = new TetType (S_LEFT_MAP, 3);
	private static TetType S_RIGHT = new TetType (S_RIGHT_MAP, 3);
	private static TetType L_LEFT = new TetType (L_LEFT_MAP, 3);
	private static TetType L_RIGHT = new TetType (L_RIGHT_MAP, 3);
	private static TetType CROSS = new TetType (CROSS_MAP, 3);
	private static TetType STRAIGHT = new TetType (STRAIGHT_MAP, 4);
	private static TetType SQUARE = new TetType (SQUARE_MAP, 2);

	private TetType type;
	private int rotation = 0;
	private int column = 0;
	private int height;
	private int gridWidth;
	private TetrisBoard board;
	private bool valid;

	public Tetromino(int gridWidth, TetrisBoard board, int i)
	{
		switch (i) {
		case 0:
			type = S_LEFT;
			break;
		case 1:
			type = S_RIGHT;
			break;
		case 2:
			type = L_LEFT;
			break;
		case 3:
			type = L_RIGHT;
			break;
		case 4:
			type = CROSS;
			break;
		case 5:
			type = STRAIGHT;
			break;
		default:
			type = SQUARE;
			break;
		}

		//rotation = Random.Range (0, 4);
		column = 3;
		this.gridWidth = gridWidth;
		this.board = board;

		ResolvePosition ();
	}

	private void ResolvePosition()
	{
		while (HasColumn (-1)) {
			column++;
		}

		while (HasColumn (gridWidth)) {
			column--;
		}

		height = gridWidth;
		int thisHeight;
		for (int x = column; x < column + type.width; x++) {
			if (!HasColumn (x)) {
				continue;
			}

			thisHeight = board.getHighest (x) - GetLowest (x) - 1;
			if (thisHeight < height) {
				height = thisHeight;
			}
		}
	}

	public bool HasColumn(int c)
	{
		if(c < column || c >= column + type.width) {
			return false;
		}

		int x = c - column;
		for(int y = 0; y < type.width; y++) {
			if (getMap (x, y) != 0) {
				return true;
			}
		}

		return false;
	}

	public int GetLowest(int c)
	{
		if (!HasColumn (c)) {
			return 0;
		}

		int x = c - column;
		for (int y = type.width - 1; y >= 0; y--) {
			if (getMap (x, y) != 0) {
				return y;
			}
		}

		return 0;
	}

	public void MoveLeft()
	{
		column--;

		ResolvePosition ();
	}

	public void MoveRight()
	{
		column++;

		ResolvePosition ();
	}

	public void TurnLeft()
	{
		rotation--;
		if (rotation < 0) {
			rotation = 3;
		}

		ResolvePosition ();
	}

	public void TurnRight()
	{
		rotation++;
		if (rotation > 3) {
			rotation = 0;
		}

		ResolvePosition ();
	}

	private int index(int x, int y)
	{
		return type.map[x + y * type.width];
	}

	public int getMap(int x, int y)
	{
		int x2;
		int y2;

		switch (rotation)
		{
			case 1:
				x2 = y;
				y2 = type.width - x - 1;
				break;
			case 2:
				x2 = type.width - x - 1;
				y2 = type.width - y - 1;
				break;
			case 3:
				x2 = type.width - y - 1;
				y2 = x;
				break;
			default:
				x2 = x;
				y2 = y;
				break;
		}

		return index(x2, y2);
	}

	public List<IntPair> GetTileCoordinates()
	{
		valid = true;
		List<IntPair> list = new List<IntPair> ();
		for (int y = 0; y < type.width; y++) {
			for (int x = 0; x < type.width; x++) {
				if (getMap(x, y) != 0) {
					if (height + y < 0) {
						valid = false;
					} else {
						list.Add (new IntPair (x + column, y + height));
					}
				}
			}
		}
		return list;
	}

	public bool isValid()
	{
		return valid;
	}
}
