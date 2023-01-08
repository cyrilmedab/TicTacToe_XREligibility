using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
	[SerializeField]
	int _aiLevel;
	int _totalTurns = 0;
	TicTacToeState[,] boardState;
	ClickTrigger[,] _triggers;

	public List<int> availableSquares;
	[HideInInspector] public List<int> availableCorners;

	// Made public so ClickTrigger can access it to prevent clicking when not the Player's turn
	// Currently the player always goes first
	public bool _isPlayerTurn;

	// Took away the serialization because it shouldn't be editable in this state
	private int _gridSize = 3;
	private int _totalBoxCount = 9;


	[SerializeField]
	private TicTacToeState _playerState = TicTacToeState.cross; 
	[SerializeField]
	private TicTacToeState _aiState = TicTacToeState.circle;
	private TicTacToeState _currSymbol;

	[SerializeField]
	private GameObject _xPrefab;
	[SerializeField]
	private GameObject _oPrefab;
	[SerializeField]
	private GameObject _endPanel;

	public UnityEvent onGameStarted;
	public WinnerEvent onPlayerWin;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[_gridSize, _gridSize]; // changed the "3" to referencing the _gridSize val
		boardState = new TicTacToeState[_gridSize, _gridSize]; // initializes the size of the board state

		availableSquares = new List<int>(_totalBoxCount);
		for (int i = 0; i < _totalBoxCount; i++) availableSquares.Add(i);
		availableCorners = new List<int> { 0, 2, 6, 8 };

		_currSymbol = _isPlayerTurn ? _playerState : _aiState;
		onGameStarted.Invoke();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	// Modified because the targetSymbol parameter is no longer necessary
	private void SetVisual(int coordX, int coordY)
	{
		Instantiate(
			_currSymbol == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}


	// Changes whose turn it is to move and the Symbol as well, and starts the AI coroutine if it's the AI's turn
	private void SwitchPlayerTurn()
	{
		_isPlayerTurn = !_isPlayerTurn;
		_currSymbol = _isPlayerTurn ? _playerState : _aiState;

		if (!_isPlayerTurn) StartCoroutine(AiTurn(_xPrefab.GetComponent<GamePiece>()._animTime + 0.1f));
	}

	// combines the initial PlayerSelects and AISelects Methods since they ended up being very similar
	// doesn't need to be passed the symbol to be used since the current symbol is publicy declared
	public void MoveSelects(int coordX, int coordY)
    {
		_totalTurns++; //Increases Turn counter

		RecordMove(coordX, coordY); //Updates all the trackers

		SetVisual(coordX, coordY); //Updates the UI

		IsGameOver(); //Continues or ends the game
	}

	// Updates the different lists and the trigger array to keep track of the boxes that have been filled
    public void RecordMove(int coordX, int coordY)
    {
		//Adds the current symbol to the board state tracker
		boardState[coordX, coordY] = _currSymbol; 
		
		//Prevents the box from being selected again
		_triggers[coordX, coordY].canClick = false;

		//Keeps track of the remaining boxes that can be selected
		int squareNumber = _gridSize * coordX + coordY;
		availableSquares.Remove(squareNumber); //removes the player-selected square from the easy AI's selection list
		if (availableCorners.Contains(squareNumber)) availableCorners.Remove(squareNumber);
	}

 	// Coroutine so the AI moves automatically on a delayed schedule after player moves
	public IEnumerator AiTurn(float delay)
    {
		yield return new WaitForSeconds(delay);
		if (_aiLevel == 0) EasyAIMoveDecision(availableSquares);
		else HardAIMoveDecision();
    }

	//Easy aka RandomSelection AI
	//Takes in a list that represents the boxes that can be selected, so that it can be utilized
	//by the Hard AI as well, to prioritize corners from a list of remaining corners
    private void EasyAIMoveDecision(List<int> selectionList)
    {
		int randomSelection = selectionList[Random.Range(0, selectionList.Count)];

		int coordX = randomSelection / 3;
		int coordY = randomSelection % 3;

		MoveSelects(coordX, coordY);
    }

	// Hard AI simple, brute-force algorithm.
	// Prioritizes winning for itself, then blocking the player's win, then goes for the center, then the corners, and then random
	// Uses the Easy AI algorithm for random selection from the corners and also from remaining boxes
	private void HardAIMoveDecision()
	{
		int coordX, coordY;

		if (WinCheckLines(_aiState, 2, out coordX, out coordY))
		{
			MoveSelects(coordX, coordY); //ai trying to see winning move
		}
		else if (WinCheckLines(_playerState, 2, out coordX, out coordY))
		{
			MoveSelects(coordX, coordY); //ai trying to see human player's winning move
		}
		else if (availableSquares.Contains(4)) MoveSelects(1, 1);
		else if (availableCorners.Count != 0) EasyAIMoveDecision(availableCorners);
		else EasyAIMoveDecision(availableSquares); //select center, then corners, then random
	}

	// Determines if the game is over and who won, or if it's the next player's turn
	private void IsGameOver()
	{
		int x, y; //throwaway variables because the coordinates aren't needed, but this way we can reuse the AI's board-checking methods
		int winner = _isPlayerTurn ? 0 : 1; //determines if it's the AI or player winning

		if (_totalTurns == _totalBoxCount) onPlayerWin.Invoke(-1);
		else if (WinCheckLines(_currSymbol, _gridSize, out x, out y)) onPlayerWin.Invoke(winner);
		else SwitchPlayerTurn();
	}

	// Checks all 8 Win Lines for a 3x3 TicTacToe grid, for lines containing only the given target number of the target symbol and no other symbol
	// The TicTacToeState is included so that the AI can use it to determine both if it has a winning move or the player does
	// The targetSymbolCount allows the method to be called by the AI to see if there's a winning move or by the player if a winning move has been made
	private bool WinCheckLines(TicTacToeState symbol, int targetsymbolCount, out int coordX, out int coordY)
    {
		if (WinCheckPerpLines(symbol, targetsymbolCount, true, out coordX, out coordY)) return true;
		else if (WinCheckPerpLines(symbol, targetsymbolCount, false, out coordY, out coordX)) return true;
		else if (WinCheckDiagLine(symbol, targetsymbolCount, true, out coordX, out coordY)) return true;
		else if (WinCheckDiagLine(symbol, targetsymbolCount, false, out coordX, out coordY)) return true;
		else
		{
			coordX = -1; coordY = -1; return false;
		}

	}

	// Checks both Diagonal Win Lines, with the same functionality as CheckPerpLines
	private bool WinCheckDiagLine(TicTacToeState symbol, int targetSymbolCount, bool isDiagTopLeft, out int primeInd, out int secInd)
	{
		//Default index value if the sought after move or win condition isn't present
		primeInd = -1;
		secInd = -1;
		//array that will determine the direction of progression for coodinate checking
		int[] row = new int[3] { 0, 1, 2 };
		//reverses the order of progression to check the backwards diagonal line (bottom left to top right)
		if (!isDiagTopLeft) (row[0], row[2]) = (row[2], row[0]);

		int coordY = 0;
		int currSymbolCount = 0;
		foreach (int coordX in row)
		{
			int score = CountSymbolInSquare(symbol, coordX, coordY);
			if (score == 0)
			{
				primeInd = coordX;
				secInd = coordY;
			}
			else currSymbolCount += score;
			coordY++;
		}
		return currSymbolCount == targetSymbolCount;
	}

	// This method checks either the horizontal lines or vertical lines depending on boolean input isHorizontal (true => horizontal lines)
	// The TicTacToeState is included so that the AI can use it to determine both if it has a winning move or the player does
	// The targetSymbolCount allows the method to be called by the AI to see if there's a winning move or by the player if a winning move has been made
	private bool WinCheckPerpLines(TicTacToeState symbol, int targetSymbolCount, bool isHorizontal, out int primeInd, out int secInd)
	{
		// default index value if the sought after move or win condition isn't present
		primeInd = -1;
		secInd = -1;

		for (int coordPrime = 0; coordPrime < _gridSize; coordPrime++)
		{
			int currSymbolCount = 0;
			for (int coordSec = 0; coordSec < _gridSize; coordSec++)
			{
				int score = isHorizontal ? CountSymbolInSquare(symbol, coordPrime, coordSec) : CountSymbolInSquare(symbol, coordSec, coordPrime);

				if (score == 0) secInd = coordSec;
				else currSymbolCount += score;
			}
			if (currSymbolCount == targetSymbolCount)
			{
				primeInd = coordPrime;
				return true;
			}
		}
		return false;
	}

	// Method returns an integer value if the current TicTacToeState matches the TicTacToeState on the board at the given coordinates
	private int CountSymbolInSquare(TicTacToeState symbol, int axis1, int axis2)
	{
		if (boardState[axis1, axis2].Equals(symbol)) return 1;
		else if (boardState[axis1, axis2].Equals(TicTacToeState.none)) return 0;
		else return -1;
	}
}
