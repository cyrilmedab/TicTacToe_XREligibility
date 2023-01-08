using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndMessage : MonoBehaviour
{

	[SerializeField]
	private TMP_Text _playerMessage = null;
    public WinnerEvent onPlayerWin;

    public void Start()
    {
        onPlayerWin.AddListener(OnGameEnded);
    }

    public void OnGameEnded(int winner)
	{
		_playerMessage.text = winner == -1 ? "Tie" : winner == 1 ? "AI wins" : "Player wins";
	}
}
// ad d a listener here??