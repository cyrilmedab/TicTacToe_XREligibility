//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Random = UnityEngine.Random;
//using UnityEngine.Events;

//public enum TicTacToeState { none, cross, circle }

//[System.Serializable]
//public class WinnerEvent : UnityEvent<int>
//{
//}

//public class TicTacToeAI : MonoBehaviour
//{

//	int _aiLevel;
//	int _totalTurns = 0;
//	TicTacToeState[,] boardState;
//	public List<int> easyAiDecide;

//	//made public so ClickTrigger can access it and disable player clicking when not their turn
//	//currently the player always goes first
//	public bool _isPlayerTurn;

//	// took away the serialization because it shouldn't be editable in this state
//	private int _gridSize = 3;

//	//made public so that clickTrigger can access
//	[SerializeField]
//	public TicTacToeState playerState = TicTacToeState.cross;
//	[SerializeField]
//	private TicTacToeState aiState = TicTacToeState.circle;

//	[SerializeField]
//	private GameObject _xPrefab;
//	[SerializeField]
//	private GameObject _oPrefab;
//	[SerializeField]
//	private GameObject _endPanel;

//	public UnityEvent onGameStarted;

//	//Call This event with the player number to denote the winner
//	public WinnerEvent onPlayerWin;

//	ClickTrigger[,] _triggers;

//	private void Awake()
//	{
//		if (onPlayerWin == null)
//		{
//			onPlayerWin = new WinnerEvent();
//		}
//	}

//	public void StartAI(int AILevel)
//	{
//		_aiLevel = AILevel;
//		StartGame();
//	}

//	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
//	{
//		_triggers[myCoordX, myCoordY] = clickTrigger;
//	}

//	private void StartGame()
//	{
//		_triggers = new ClickTrigger[_gridSize, _gridSize]; // changed the "3" to referencing the _gridSize val
//		boardState = new TicTacToeState[_gridSize, _gridSize]; // initializes the size of the board state

//		easyAiDecide = new List<int>(_gridSize * _gridSize);
//		for (int i = 0; i < 9; i++) easyAiDecide.Add(i);

//		onGameStarted.Invoke();
//	}

//	private void SwitchPlayerTurn()
//	{
//		_isPlayerTurn = !_isPlayerTurn;
//	}

//	public void PlayerSelects(int coordX, int coordY)
//	{

//		SetVisual(coordX, coordY, playerState);
//		boardState[coordX, coordY] = playerState; //adds the player's current move to the board state tracker
//		easyAiDecide.Remove(_gridSize * coordX + coordY); //removes the player-selected square from the easy AI's selection list
//		Debug.Log($"Array : {easyAiDecide}");
//		_totalTurns++;

//		if (IsWinningMove(coordX, coordY, playerState)) onPlayerWin.Invoke(0);
//		else if (_totalTurns == 9) onPlayerWin.Invoke(-1);
//		else
//		{
//			SwitchPlayerTurn(); // switches to not the player's turn anymore, as they've already moved
//			StartCoroutine(AiTurn(_xPrefab.GetComponent<GamePiece>()._animTime + 0.25f));
//		}
//	}


//	// coroutine so the AI moves automatically on a delayed schedule after player moves
//	public IEnumerator AiTurn(float delay)
//	{
//		yield return new WaitForSeconds(delay);
//		AiMoveDecision();
//	}

//	// THE AI FUNCTION
//	private void AiMoveDecision()
//	{
//		int randomSelection = easyAiDecide[Random.Range(0, easyAiDecide.Count)];
//		easyAiDecide.Remove(randomSelection);

//		int coordX = randomSelection / 3;
//		int coordY = randomSelection % 3;

//		_triggers[coordX, coordY].canClick = false;
//		AiSelects(coordX, coordY);
//	}

//	public void AiSelects(int coordX, int coordY)
//	{
//		SetVisual(coordX, coordY, aiState);
//		boardState[coordX, coordY] = aiState; //adds the AI's current move to the board state tracker
//		_totalTurns++;

//		if (IsWinningMove(coordX, coordY, aiState)) onPlayerWin.Invoke(1);
//		else if (_totalTurns == 9) onPlayerWin.Invoke(-1); // added in case I decide to add "who goes first" functionality
//		else
//		{
//			SwitchPlayerTurn(); // switches to not the player's turn anymore, as they've already moved
//		}
//	}

//	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
//	{
//		Instantiate(
//			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
//			_triggers[coordX, coordY].transform.position,
//			Quaternion.identity
//		);
//	}

//	//TODO: brute force check, could be made more efficient by not checking diagonals for 4 selections, and removing the hard coding
//	private bool IsWinningMove(int coordX, int coordY, TicTacToeState symbol)
//	{
//		TicTacToeState[] horzCheck = { boardState[coordX, 0], boardState[coordX, 1], boardState[coordX, 2] };
//		TicTacToeState[] vertCheck = { boardState[0, coordY], boardState[1, coordY], boardState[2, coordY] };
//		TicTacToeState[] diag1 = new TicTacToeState[] { boardState[0, 0], boardState[1, 1], boardState[2, 2] }; //temp hard coding for game state checking
//		TicTacToeState[] diag2 = new TicTacToeState[] { boardState[2, 0], boardState[1, 1], boardState[0, 2] };

//		return (horzCheck[0].Equals(horzCheck[1]) && horzCheck[0].Equals(horzCheck[2]))
//				|| (vertCheck[0].Equals(vertCheck[1]) && vertCheck[0].Equals(vertCheck[2]))
//				|| (symbol.Equals(diag1[0]) && diag1[0].Equals(diag1[1]) && diag1[0].Equals(diag1[2]))
//				|| (symbol.Equals(diag2[0]) && diag2[0].Equals(diag2[1]) && diag2[0].Equals(diag2[2]));
//	}

//	private int CheckHorz(int targetSymbolCount, TicTacToeState symbol)
//	{
//		for (int row = 0; row < _gridSize; row++)
//		{
//			int currSymbolCount = 0;
//			for (int col = 0; col < _gridSize; col++)
//			{
//				if (boardState[row, col].Equals(symbol)) currSymbolCount++;
//				else if (boardState[row, col].Equals(TicTacToeState.none)) {; }
//				else currSymbolCount--;
//			}
//			if (currSymbolCount == targetSymbolCount) return row;
//		}
//		return -1;
//	}

//	private int CheckDiagLines(int targetSymbolCount, TicTacToeState symbol)
//	{
//		string dir = " ";
//		int[] axis1 = new int[3] { 0, 1, 2 };
//		if (dir == "diag2")

//			for (int col = 0; col < _gridSize; col++)
//			{
//				int currSymbolCount = 0;
//				for (int col = 0; col < _gridSize; col++)
//				{
//					if (boardState[row, col].Equals(symbol)) currSymbolCount++;
//					else if (boardState[row, col].Equals(TicTacToeState.none)) {; }
//					else currSymbolCount--;
//				}
//				if (currSymbolCount == targetSymbolCount) return col;
//			}
//		return -1;
//	}


//	// This method currently checks either the horizontal lines or vertical lines depending on string input dir being "hor"
//	// The TicTacToeState allows the function to be used by the AI for both deciding if it has a winning move and if the player does
//	// the targetSymbolCount allows the method to be called by the AI to see if there's a winning move or by the player if a winning move has been made
//	private int CheckPerpLines(TicTacToeState symbol, string dir, int targetSymbolCount)
//	{
//		for (int axis1 = 0; axis1 < _gridSize; axis1++)
//		{
//			int currSymbolCount = 0;
//			for (int axis2 = 0; axis2 < _gridSize; axis2++)
//			{
//				if (dir == "hor") currSymbolCount += CountSymbolInSquare(symbol, axis1, axis2);
//				else currSymbolCount += CountSymbolInSquare(symbol, axis2, axis1);
//			}
//			if (currSymbolCount == targetSymbolCount) return axis1;
//		}
//		return -1;
//	}

//	// Method returns an integer value if the given TicTacToeState matches the TicTacToeState on the board at the given coordinates
//	private int CountSymbolInSquare(TicTacToeState symbol, int axis1, int axis2)
//	{
//		if (boardState[axis1, axis2].Equals(symbol)) return 1;
//		else if (boardState[axis1, axis2].Equals(TicTacToeState.none)) return 0;
//		else return -1;
//	}
//}
