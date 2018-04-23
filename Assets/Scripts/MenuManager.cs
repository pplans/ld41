using UnityEngine;
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

public class MenuManager : MonoBehaviour
{
	public static MenuManager instance = null;
	public enum GameState
	{
		START,
		MENU,
		PLAYING,
		PLAYINGNOTIMER,
		END
	};

	private GameState m_state;
	public GameObject m_UIMain;
	public GameObject m_UIGameOver;
	public GameObject m_CameraGame;
	public GameObject m_CameraUI;
	public WorldUpdater m_WorldUpdater;
	private float m_score;
	private int m_playerPosition;
	public int PlayerPosition { get { return m_playerPosition; } set{ m_playerPosition = value; } }

	public float m_trialLength = 1000.0f;
	public float TrialLength { get { return m_trialLength; } set { } }
	public float m_trialMinOffsetLength = 10.0f;
	public float TrialMinOffsetLength { get { return m_trialMinOffsetLength; } set { } }
	public float m_trialMaxOffsetLength = 100.0f;
	public float TrialMaxOffsetLength { get { return m_trialMaxOffsetLength; } set { } }
	public float m_bonusTimeBuoy = 10.0f;
	public float BonusTimeBuoy { get { return m_bonusTimeBuoy; } set { } }
	public float m_bonusTimeFish = 2.0f;
	public float BonusTimeFish { get { return m_bonusTimeFish; } set { } }
	public float m_bonusScoreFish = 300.0f;
	public float BonusScoreFish { get { return m_bonusScoreFish; } set { } }
	public uint m_minBuoyNumber = 2;
	public uint MinBuoyNumber { get { return m_minBuoyNumber; } set { } }
	public uint m_maxBuoyNumber = 10;
	public uint MaxBuoyNumber { get { return m_maxBuoyNumber; } set { } }
	public float m_speedWaterClimate = 0.1f;
	public float SpeedWaterClimate { get { return m_speedWaterClimate; } set { } }

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		//DontDestroyOnLoad(gameObject);
		InitGame();
	}

	public void InitGame()
	{
		m_state = GameState.MENU;
		m_CameraUI.tag = "MainCamera";
		m_CameraGame.tag = "Untagged";
		m_CameraUI.SetActive(true);
		m_CameraGame.SetActive(false);

		m_UIGameOver.SetActive(false);
		m_UIMain.SetActive(true);
	}

	public void StartGame()
	{
		if(m_state == GameState.MENU)
		{
			m_CameraGame.tag = "MainCamera";
			m_CameraUI.tag = "Untagged";
			m_CameraGame.SetActive(true);
			m_CameraUI.SetActive(false);

			m_UIMain.SetActive(false);
			m_UIGameOver.SetActive(false);
			// start the game here
			Debug.Log("StartGame");
			m_state = GameState.PLAYING;

			m_WorldUpdater.Reset ();
		}
	}

	public void StartGameNoTimer()
	{
		if (m_state == GameState.MENU)
		{
			m_CameraGame.tag = "MainCamera";
			m_CameraUI.tag = "Untagged";
			m_CameraGame.SetActive(true);
			m_CameraUI.SetActive(false);

			m_UIMain.SetActive(false);
			m_UIGameOver.SetActive(false);
			// start the game here
			Debug.Log("StartGame");
			m_state = GameState.PLAYINGNOTIMER;

			m_WorldUpdater.Reset ();
		}
	}

	public void EndGame()
	{
		if (m_state == GameState.PLAYING)
		{
			m_state = GameState.END;
			m_CameraUI.tag = "MainCamera";
			m_CameraGame.tag = "Untagged";
			m_CameraUI.SetActive(true);
			m_CameraGame.SetActive(false);

			m_UIMain.SetActive(false);
			m_UIGameOver.SetActive(true);
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
			case GameState.PLAYINGNOTIMER:
			case GameState.PLAYING:
			{
				CaptureKeyboard();
				UpdateGame();
				break;
			}
		}
	}

    public GameState GetState()
    {
        return m_state;
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
			case GameState.PLAYINGNOTIMER:
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

	public void RegisterScore(float score)
	{
		m_score = score;
	}

	public void IncrementScore(float score)
	{
		m_score += score;
	}

	public float GetScore()
	{
		return m_score;
	}
}