﻿using UnityEngine;
using System.Collections;


using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.UI;                   //Allows us to use UI.

namespace Input
{
	enum Type
	{
		None = 0x0,
		Down = 0x1,
		Up = 0x2
	};

	enum Action
	{
		Action1 = 0,
		Action2 = 1
	}

	struct ActionEntry
	{
		private Action action;
		private KeyCode code;
		private string name;
		private Type type;
		public Action Action
		{
			get { return action; }
			set { }
		}
		public KeyCode Code
		{
			get { return code; } set { }
		}
		public string Name
		{
			get { return name; }
			set { }
		}
		public Type Type
		{
			get { return type; }
			set { type = value; }
		}
		public ActionEntry(Action a, KeyCode c, string s, Type t)
		{
			action = a; code = c; name = s; type = t;
		}
	};

	static class Mapper
	{
		public static ActionEntry[] Actions =
		{
			new ActionEntry(Action.Action1, KeyCode.X, "Action", Type.None),
			new ActionEntry(Action.Action2, KeyCode.C, "Action2", Type.None)
		};

		public static Type IsPressed(Action a)
		{
			return Actions[(int)a].Type;
		}
	}
}

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;
	public enum GameState
	{
		START,
		MENU,
		PLAYING,
		END
	};

	public GameObject m_menu;


	private GameState m_state;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
		InitGame();
	}

	void InitGame()
	{
		m_state = GameState.MENU;
		m_menu.SetActive(true);
	}

	public void StartGame()
	{
		if(m_state == GameState.MENU)
		{
			// start the game here
			Debug.Log("StartGame");
			m_state = GameState.PLAYING;
			if(m_menu)
			{
				m_menu.SetActive(false);
			}
		}
	}
   
	void Update()
	{
		switch(m_state)
		{
			case GameState.MENU:
			{
				break;
			}
			case GameState.PLAYING:
			{
				CaptureKeyboard();
				UpdateGame();
				break;
			}
		}
	}

	void UpdateGame()
	{
		switch (m_state)
		{
			case GameState.MENU:
				{
					if (UnityEngine.Input.anyKey) // special case, start
					{
						Debug.Log("oh");
						m_state = GameState.PLAYING;
					}
					break;
				}
			case GameState.PLAYING:
				{
					if ((Input.Mapper.IsPressed(Input.Action.Action1)&Input.Type.Up)==Input.Type.Up)
					{
						Debug.Log("Action1 pressed.");
					}
					break;
				}
			case GameState.END:
				{
					if(UnityEngine.Input.anyKey) // special case, restart
					{
						m_state = GameState.START;
					}
				}
			break;
		}
	}

	void CaptureKeyboard()
	{
		//foreach (KeyValuePair<Input.Action, Input.ActionEntry> entry in Input.Mapper.Actions)
		//{
			//	for (int i = 0; i < Input.Mapper.Actions.Count; ++i)
			//{
		for (int index = 0; index < Input.Mapper.Actions.Length; index++)
		{
			Input.ActionEntry input = Input.Mapper.Actions[index];
			input.Type = Input.Type.None;
			if(UnityEngine.Input.GetKeyDown(input.Code))
			{
				input.Type |= Input.Type.Down;
			}
			if (UnityEngine.Input.GetKeyUp(input.Code))
			{
				input.Type |= Input.Type.Up;
			}
			Input.Mapper.Actions[index] = input;
		}
	}
}