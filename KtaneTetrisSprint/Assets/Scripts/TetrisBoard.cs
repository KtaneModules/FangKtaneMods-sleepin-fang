using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TetrisBoard {

	private int w;
	private int h;

	private int[] grid;

	public TetrisBoard(int w, int h) {
		this.w = w;
		this.h = h;

		grid = new int[w * h];
		for (int i = 0; i < w * h; i++) {
			grid [i] = 0;
		}

		// Pick a random initial board
		int boardType = Random.Range(0, 4);
		int val1 = Random.Range (1, 4);
		int val2 = Random.Range (1, 4);
		int val3 = Random.Range (1, 4);

		switch (boardType) {
		case 0:
			initColumns (1, 2, val1);
			initColumns (3, 5, val2);
			initColumns (7, 7, val3);
			break;
		case 1:
			initColumns (0, 2, val1);
			initColumns (5, 5, val2);
			initColumns (6, 7, val3);
			break;
		case 2:
			initColumns (0, 1, val1);
			initColumns (3, 5, val2);
			initColumns (6, 6, val3);
			break;
		default:
			initColumns (0, 0, val1);
			initColumns (1, 3, val2);
			initColumns (6, 7, val3);
			break;
		}
	}

	public int get(int x, int y)
	{
		if (y >= h) {
			return 0;
		}
		return grid [y * w + x];
	}

	public void set(int x, int y, int val)
	{
		grid [y * w + x] = val;
	}

	public int getHighest(int x)
	{
		for (int y = 0; y < h; y++) {
			if (get (x, y) != 0) {
				return y;
			}
		}

		return h;
	}

	private void initColumns(int colA, int colB, int height)
	{
		for (int x = colA; x <= colB; x++) {
			for (int y = h - height; y < h; y++) {
				set (x, y, 1);
			}
		}
	}

	public List<int> getCompletedRows()
	{
		List<int> list = new List<int> ();

		for (int y = 0; y < h; y++) {
			bool complete = true;
			for (int x = 0; x < w; x++) {
				if (get (x, y) == 0) {
					complete = false;
					x = w;
				}
			}

			if (complete) {
				Debug.Log ("Row " + y + " completed!");
				list.Add (y);
			}
		}

		return list;
	}

	public void deleteRows(List<int> rows) {
		if (rows.Count == 0) {
			return;
		}

		bool[] available = new bool[h];
		bool hitTop = false;
		//bool[] used = new bool[h];

		for (int y = 0; y < h; y++) {
			//used [y] = false;
			if (rows.Contains (y)) {
				available [y] = false;
			} else {
				available [y] = true;
			}
		}

		for (int y = h - 1; y >= 0; y--) {
			if (!available [y]) {
				if (!hitTop) {
					bool found = false;
					for (int y2 = y - 1; y2 >= 0; y2--) {
						if (available [y2]) {
							found = true;
							available [y2] = false;
							for (int x = 0; x < w; x++) {
								set (x, y, get (x, y2));
							}
							y2 = 0;
						}
					}
					if (!found) {
						hitTop = true;
					}
				}

				if (hitTop) {
					for (int x = 0; x < w; x++) {
						set (x, y, 0);
					}
				}
			}
		}

		//int[] dropHeight = new int[h];

		/*for (int y = 0; y < h; y++) {
			dropHeight [y] = 0;
			if (rows.Contains (y)) {
				for (int i = 0; i <= y; i++) {
					dropHeight [i]++;
				}
			}
		}

		for (int y = h - 1; y >= 0; y--) {
			if (dropHeight [y] != 0) {
				// Move rows down, or set to zero
				int y2 = y - dropHeight [y];
				for (int x = 0; x < w; x++) {
					set (x, y, y2 < 0 ? 0 : get (x, y2));
				}
			}
		}*/
	}
}
