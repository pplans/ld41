using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldUpdater : MonoBehaviour {

    public GameObject playerBoat;
    public MenuManager menu;
    public Water sea;
	public GameObject BuoyHelper = null;
	public Text UIScoreValue = null;
	public Text UIScoreValue2 = null;
	public Text UINumBuoys = null;
	public Text UITimer = null;
    public Image UIClock = null;
    public Text UIPlayerPosition = null;
	public Text UIPlayerPositionInGame = null;
	public ParticleSystem m_splash = null;

    public float BuoyCatchSqrRange = 1.0f;
	public float FishCatchSqrRange = 1.0f;
	public float BuoyCatchSqrRangeAI = 2.0f;

	public Object buoyPrefab;
    public Object smallFishPrefab;
    public Object mediumFishPrefab;
    public Object bigFishPrefab;
	public Object aiPrefab;

    private GameObject playerFishNet;
    private GameObject playerFishNetAnchor;
    private GameObject playerFishNetTarget;
    private GameObject playerFishNetAnchorTarget;
	public float boatRotationSpeed = 200.0f;
    public float boatAcceleration = 2.0f;
    public float boatMaxSpeed = 0.5f;
    public float boatDrag = 1.0f;
    private float boatCurrentSpeed = 0.0f;

    public float stickyUpRate = 30.0f;
    public float stickyDownRate = 9.8f;
    public float stickyTiltRate = 9.8f;
    public float stickyOffset = -0.05f;
	public float fishNetRate = 9.8f;

    public float seaWidth = 12.0f; // supposed to be 10 but we had a margin

	// path
	public Vector3	StartRun;
	public Vector3	EndRun;
	public uint		NumberOfSteps; // buoys

	// Timer
	public float TimerAtTheStart = 60.0f;
	private float TimerSecondsLeft;

    private bool ArrowReachedBuoy = false;

	class Buoy
	{
		public GameObject go;
		public int id;
		public Buoy(int _id, GameObject _go)
		{
			id = _id;
			go = _go;
		}
	}
	private List<Buoy> buoys;
	private int CurrentBuoy = -1;

    class Fish
    {
        public GameObject go;
        public enum Type
        {
            SMALL,
            MEDIUM,
            LARGE
        };
        Type type = Type.SMALL;

        public Fish(Type _type, GameObject _go)
        {
            type = _type;
            go = _go;
        }
    }
    private List<Fish> fishs;

	public int numberStaticObject = 10;
	class StaticObject
	{
		public GameObject go;
		public float radius;
	}
	private List<StaticObject> staticObjects;
    private List<StaticObject> staticObjectslm1; // staticObjects from previous leg, we keep them a bit so they don't vanish when we reach a checkpoint

    public List<GameObject> staticObjectPrefabs;

	public uint numberIAs = 3;
	class AI
	{
		public GameObject go;
		public Vector3 direction;
		public int CurrentBuoy;
		public float currentSpeed;
		public float score;
	}
	private List<AI> ais = null;

	public List<AudioClip> backgroundMusic;
	private int CurrentMusic = -1;

	const int SFX1 = 0;
	const int SFX2 = 1;
	const int SFX3 = 2;
	const int SFX4 = 3;
    const int SFX5 = 4;
    const int SFX6 = 5;
    const int SFX7 = 6;
    public List<AudioClip> soundBank;
	public AudioSource audioPlayer;

    private Color defaultUITimerColor;

    class BezierCurve
	{
		public Vector3 P1;
		public Vector3 P2;
		public Vector3 P3;
		public Vector3 P4;
		public Vector3 Get2(float t)
		{
			return (1.0f - t) * (1.0f - t) * P1 + 2.0f * (1.0f - t) * t * P2 + t * t * P3;
		}
		public Vector3 Get3(float t)
		{
			return (1.0f - t) * (1.0f - t) * (1.0f -t) * P1
			+ 3.0f * (1.0f - t) * (1.0f - t) * t * P2
			+ 3.0f * (1.0f - t) * (1.0f - t) * t * P3
			+ t * t * t * P4;
		}
	}

    // Use this for initialization
    void Start () {
        boatCurrentSpeed = 0.0f;
		TimerSecondsLeft = TimerAtTheStart;
		Generate();
		ais = new List<AI>();

		if (playerBoat) {
			playerFishNet = playerBoat.transform.parent.Find ("FishNet").gameObject;
            playerFishNetAnchor = playerBoat.transform.parent.Find("FishNet/Armature/Base/Anchor").gameObject;
            playerFishNetTarget = playerBoat.transform.Find ("FishNetTarget").gameObject;
            playerFishNetAnchorTarget = playerBoat.transform.Find("Armature/Base/FishNetAnchorTarget").gameObject;
		}

        defaultUITimerColor = UITimer.color;

    }

	public void Reset () {
		boatCurrentSpeed = 0.0f;
		sea.Offset = Vector2.zero;
		MenuManager.instance.RegisterScore (0);
		TimerSecondsLeft = TimerAtTheStart;

        if (staticObjects != null)
            foreach (StaticObject s in staticObjects)
                Destroy(s.go);
        staticObjects = null;
        if (staticObjectslm1 != null)
            foreach (StaticObject s in staticObjectslm1)
                Destroy(s.go);
        staticObjectslm1 = null;

        Generate();

        if (ais != null)
            foreach (AI a in ais)
                Destroy(a.go);

		ais = new List<AI>();
	}


	void Generate()
	{
		CurrentBuoy = -1;
		if(ais!=null)
		foreach(AI a in ais)
			a.CurrentBuoy = -1;
		NumberOfSteps = (uint)Random.Range(MenuManager.instance.MinBuoyNumber, MenuManager.instance.MaxBuoyNumber);
		if(buoys!=null)
		foreach(Buoy b in buoys)
		{
			Destroy(b.go);
		}
		buoys = new List<Buoy>();

		// Generate some buoys
		BezierCurve path = new BezierCurve();
		float randomAngle = Random.Range(-3.14f, 3.14f);
		path.P1 = playerBoat.transform.position+ (new Vector3(Mathf.Cos(Random.Range(-3.14f, 3.14f)), 0.0f, Mathf.Sin(Random.Range(-3.14f, 3.14f))))*5.0f;
		path.P4 = MenuManager.instance.TrialLength*(new Vector3(Mathf.Cos(randomAngle), 0.0f, Mathf.Sin(randomAngle)));
		Vector3 perp = Vector3.Cross((path.P4 - path.P1).normalized, Vector3.up);
		path.P2 = path.P1 + 0.5f * (path.P4 - path.P1) + perp
		* Random.Range(MenuManager.instance.TrialMinOffsetLength, MenuManager.instance.MaxBuoyNumber);
		path.P3 = path.P1 + 0.5f * (path.P4 - path.P1) - perp
		* Random.Range(MenuManager.instance.TrialMinOffsetLength, MenuManager.instance.MaxBuoyNumber);

		for (int i = 0; i < NumberOfSteps; i++)
		{
			GameObject newObject = Instantiate(buoyPrefab) as GameObject;
			Vector3 newObjectPos = Random.onUnitSphere * 100;
			newObject.transform.position = path.Get3((float)i / NumberOfSteps);
			buoys.Add(new Buoy(i, newObject));
		}

        // Generate some fishes
        if (fishs != null)
        foreach (Fish f in fishs)
        {
                Destroy(f.go);
        }
        fishs = new List<Fish>();
        for (int i = 0; i < NumberOfSteps; i++)
        {
            GameObject newObject;
            int type = Random.Range(0, 3);

            if (type == 0)
                newObject = Instantiate(smallFishPrefab) as GameObject;
            else if(type == 1)
                newObject = Instantiate(mediumFishPrefab) as GameObject;
            else
                newObject = Instantiate(bigFishPrefab) as GameObject;

            Vector3 newObjectPos = Random.onUnitSphere * 5;
            newObject.transform.position = newObjectPos;
            //newObject.transform.position = path.Get2((float)i / NumberOfSteps) + newObjectPos;

            fishs.Add(new Fish((Fish.Type)type, newObject));
        }

        if (staticObjectslm1 != null)
        {
            foreach (StaticObject s in staticObjectslm1)
            {
                Destroy(s.go);
            }
        }
        staticObjectslm1 = staticObjects;
		staticObjects = new List<StaticObject>();
		// generate static objects
		if (staticObjectPrefabs.Count > 0)
		{
			for (int i = 0; i < numberStaticObject; ++i)
			{
				Vector3 pos = path.Get3(((float)i+1) / (numberStaticObject+1));
				pos += Vector3.Cross((pos - path.P1).normalized, Vector3.up) * Random.Range(3.0f, 6.0f);
				StaticObject so = new StaticObject();
				so.go = Instantiate(staticObjectPrefabs[Random.Range(0, staticObjectPrefabs.Count)]) as GameObject;
                so.radius = so.go.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 0.5f; //so.go.transform.localScale.magnitude * 0.5f;
				//so.go.transform.position = new Vector3(Random.Range(-MenuManager.instance.TrialLength, MenuManager.instance.TrialLength), 0.0f, Random.Range(-MenuManager.instance.TrialLength, MenuManager.instance.TrialLength));
				so.go.transform.position = pos;
                so.go.transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));

                bool locationOk = true;

				if ((playerBoat.transform.position - so.go.transform.position).magnitude < (so.radius + 1.5f))
                    locationOk = false;

                foreach (Buoy b in buoys)
                {
                    if ((b.go.transform.position - so.go.transform.position).magnitude < (so.radius + 1.5f))
                        locationOk = false;
                }

                if (locationOk)
                    staticObjects.Add(so);
                else
                    Destroy(so.go); // forget it
			}
		}
    }

    // Update player, returns the sea offset
    Vector3 UpdatePlayer()
    {
        float direction = 0.0f;

        if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
        {
            playerBoat.transform.Rotate(Vector3.down * boatRotationSpeed * Time.deltaTime);
            direction -= 2.0f;
        }
        if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
        {
            playerBoat.transform.Rotate(Vector3.up * boatRotationSpeed * Time.deltaTime);
            direction += 2.0f;
        }

        if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
        {
            boatCurrentSpeed = Mathf.Lerp(boatMaxSpeed, boatCurrentSpeed, Mathf.Pow(2.0f, -boatAcceleration * Time.deltaTime));
        }
        else
        {
            boatCurrentSpeed = Mathf.Lerp(0.0f, boatCurrentSpeed, Mathf.Pow(2.0f, -boatDrag * Time.deltaTime));
		}
		// ENGINE SOUND
		playerBoat.GetComponent<Boat>().UpdateAudio(boatCurrentSpeed*4.0f);

		if (m_splash!=null)
		{
			var main = m_splash.main;
			main.startLifetimeMultiplier = 2.0f*boatCurrentSpeed;
		}

        Animator boatAnimator = playerBoat.GetComponent<Animator>();
        boatAnimator.SetFloat("Direction", direction);
        boatAnimator.SetFloat("Speed", boatCurrentSpeed);

		Vector3 playerOffset = (playerBoat.transform.rotation * Vector3.forward) * boatCurrentSpeed;
        // collision
        if (staticObjects != null)
        {
            foreach (StaticObject s in staticObjects)
            {
                if ((playerBoat.transform.position - s.go.transform.position).magnitude < (s.radius + 0.5f))
                {
                    playerOffset = -1.2f * playerOffset;
                    boatCurrentSpeed *= -1.0f;
                    PlaySFX(SFX6);
                }
            }
        }

        // collision
        if (staticObjectslm1 != null)
        {
            foreach (StaticObject s in staticObjectslm1)
            {
                if ((playerBoat.transform.position - s.go.transform.position).magnitude < (s.radius + 0.5f))
                {
                    playerOffset = -playerOffset;
                    boatCurrentSpeed *= -1.0f;
                    PlaySFX(SFX6);
                }
            }
        }

        // Update fishnet location
        playerFishNet.transform.position -= playerOffset;
		float lerpFactor = Mathf.Pow (2.0f, -fishNetRate * Time.deltaTime);
		playerFishNet.transform.position = Vector3.Lerp(playerFishNetTarget.transform.position, playerFishNet.transform.position, lerpFactor);
		playerFishNet.transform.rotation = Quaternion.Lerp(playerFishNetTarget.transform.rotation, playerFishNet.transform.rotation, lerpFactor);
		
        playerFishNetAnchor.transform.position = playerFishNetAnchorTarget.transform.position;
        playerFishNetAnchor.transform.rotation = playerFishNetAnchorTarget.transform.rotation;
        return playerOffset;
    }

	private struct Sticker {
		public GameObject go;
		public bool lerp;
		public Sticker(GameObject _go, bool _lerp) {go=_go; lerp=_lerp;}
	};

    void StickEverythingToSea()
    {
        Vector3 pos;
        Vector3 normal;

        List<Sticker> objects = new List<Sticker>();
		objects.Add(new Sticker(playerBoat, true));

		if(buoys!=null)
        	foreach (var b in buoys)
				objects.Add(new Sticker(b.go, true));

        if (fishs != null)
            foreach (var b in fishs)
				objects.Add(new Sticker(b.go, false));

		if (ais != null)
			foreach (var b in ais)
				objects.Add(new Sticker(b.go, true));

		Quaternion fixQuaternion = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 180, 0);

        foreach (var o in objects)
        {
			var go = o.go;
            sea.GetSurfacePosAndNormalForWPos(go.transform.position, out pos, out normal);
            
			pos.y += stickyOffset;
			Quaternion newQuaternion = Quaternion.LookRotation (normal, go.transform.rotation * Vector3.forward) * fixQuaternion;

			if (o.lerp) {
				if (go.transform.position.y > pos.y) {
					go.transform.position = Vector3.Lerp (pos, go.transform.position, Mathf.Pow (2.0f, -stickyDownRate * Time.deltaTime));
				} else {
					go.transform.position = Vector3.Lerp (pos, go.transform.position, Mathf.Pow (2.0f, -stickyUpRate * Time.deltaTime));
				}
				go.transform.rotation = Quaternion.Lerp (newQuaternion, go.transform.rotation, Mathf.Pow (2.0f, -stickyTiltRate * Time.deltaTime));
			} else {
				go.transform.position = pos;
				go.transform.rotation = newQuaternion;
			}
        }
    }

	void PlaySFX(int sfx)
	{
		if (sfx < soundBank.Count)
		{
			audioPlayer.clip = soundBank[sfx];
			audioPlayer.Play();
		}
	}


	void MoveEverythingWithPlayer(Vector3 deltaPlayerPos)
    {
        deltaPlayerPos.y = 0.0f;

        sea.Offset += deltaPlayerPos;

        if(buoys!=null)
        foreach (var b in buoys)
            b.go.transform.position -= deltaPlayerPos;

        if (fishs != null)
            foreach (var f in fishs)
                f.go.transform.position -= deltaPlayerPos;

		if (ais != null)
			foreach (var a in ais)
				a.go.transform.position -= deltaPlayerPos - a.direction*a.currentSpeed;

		if (staticObjects != null)
			foreach (var s in staticObjects)
				s.go.transform.position -= deltaPlayerPos;

        if (staticObjectslm1 != null)
            foreach (var s in staticObjectslm1)
                s.go.transform.position -= deltaPlayerPos;
    }

    bool IsInsideSea(Vector3 pos)
    {
        Rect seaRect = new Rect(Vector2.one * -seaWidth / 2, Vector2.one * seaWidth);
        return seaRect.Contains(new Vector2(pos.x, pos.z));
    }

    void ClipEverythingOutsideSea()
    {
        foreach (Buoy b in buoys)
            foreach (var comp in b.go.GetComponentsInChildren<Renderer>())
                comp.enabled = IsInsideSea(b.go.transform.position);

        foreach (Fish f in fishs)
            foreach (var comp in f.go.GetComponentsInChildren<Renderer>())
                comp.enabled = IsInsideSea(f.go.transform.position);

		foreach (AI a in ais)
			foreach (var comp in a.go.GetComponentsInChildren<Renderer>())
				comp.enabled = IsInsideSea(a.go.transform.position);

        if (staticObjects != null)
        {
            foreach (StaticObject s in staticObjects)
                foreach (var comp in s.go.GetComponentsInChildren<Renderer>())
                    comp.enabled = IsInsideSea(s.go.transform.position);
        }

        if (staticObjectslm1 != null)
        {
            foreach (StaticObject s in staticObjectslm1)
                foreach (var comp in s.go.GetComponentsInChildren<Renderer>())
                    comp.enabled = IsInsideSea(s.go.transform.position);
        }
    }

    // Update is called once per frame
    void Update () {
		// MUSIC
		AudioSource asrc = gameObject.GetComponent<AudioSource>();
		if(!asrc.isPlaying)
		{
			asrc.clip = backgroundMusic[CurrentMusic+1];
			asrc.Play();
			CurrentMusic = (CurrentMusic+1)<backgroundMusic.Count-1?CurrentMusic+1:0;
		}
		// MUSIC

		if (!(menu.GetState() == MenuManager.GameState.PLAYING || menu.GetState() == MenuManager.GameState.PLAYINGNOTIMER))
            return;

		int playerPosition = ais.Count + 1;
		foreach (AI a in ais)
		{
			if (CurrentBuoy > a.CurrentBuoy)
				playerPosition--;
		}
		MenuManager.instance.PlayerPosition = playerPosition;

		Vector3 playerOffset = UpdatePlayer();

        MoveEverythingWithPlayer(playerOffset);
        StickEverythingToSea();
		for (int i = 0; i < ais.Count; ++i)
		{
			Vector3 aiRandomOffset = new Vector3(Random.Range(-BuoyCatchSqrRangeAI, BuoyCatchSqrRangeAI), 0.0f, Random.Range(-BuoyCatchSqrRangeAI, BuoyCatchSqrRangeAI));
			if ((ais[i].CurrentBuoy + 1) < buoys.Count)
			{
				Buoy buoy = buoys[ais[i].CurrentBuoy + 1];
				ais[i].direction = (buoy.go.transform.position + aiRandomOffset - ais[i].go.transform.position).normalized;
				ais[i].go.transform.LookAt(buoy.go.transform.position + aiRandomOffset);
				if ((ais[i].go.transform.position - buoy.go.transform.position).sqrMagnitude < BuoyCatchSqrRangeAI)
				{
					ais[i].CurrentBuoy = (ais[i].CurrentBuoy + 1) < NumberOfSteps ? ais[i].CurrentBuoy + 1 : ais[i].CurrentBuoy;
					if(menu.GetState() == MenuManager.GameState.PLAYING && (ais[i].CurrentBuoy + 1) >= buoys.Count)
					{
						// AI arrived before you
						TimerSecondsLeft = TimerAtTheStart;
						CurrentBuoy = -1;
						MenuManager.instance.EndGame();
					}
				}
			}
			else
			{
				ais[i].direction = (aiRandomOffset*10.0f - ais[i].go.transform.position).normalized;
			}
			//ais[i].currentSpeed = Mathf.Lerp(boatMaxSpeed, ais[i].currentSpeed, Mathf.Pow(2.0f, -boatAcceleration * Time.deltaTime));
		}

		if((CurrentBuoy+1)>=buoys.Count)
		{
			MenuManager.instance.IncrementScore(computeScore());
			// Restart
			Generate();
			TimerSecondsLeft = TimerAtTheStart;
			CurrentBuoy = -1;
			PlaySFX(SFX4);
			foreach(AI a in ais)
			{
				a.currentSpeed = Random.Range(0.05f, boatMaxSpeed * 0.50f);
			}
		}
		if(TimerSecondsLeft>0.0f)
		{
			if (ais.Count < numberIAs)
			{
				AI ai = new AI();
				ai.go = Instantiate(aiPrefab) as GameObject;
				Vector3 aiRandomOffset = new Vector3(Random.Range(-BuoyCatchSqrRangeAI, BuoyCatchSqrRangeAI), 0.0f, Random.Range(-BuoyCatchSqrRangeAI, BuoyCatchSqrRangeAI));
				ai.go.transform.position = playerBoat.transform.position + aiRandomOffset;
				ai.CurrentBuoy = -1;
				ai.currentSpeed = Random.Range(0.05f, boatMaxSpeed*0.50f);
				if ((ai.CurrentBuoy + 1) < buoys.Count)
				{
					Buoy buoy = buoys[ai.CurrentBuoy + 1];
					ai.direction = (buoy.go.transform.position+ aiRandomOffset - ai.go.transform.position).normalized;
				}
				else
				{
					ai.direction = (aiRandomOffset - ai.go.transform.position).normalized;
				}
				ais.Add(ai);
			}

			// Catch fish
			List<Fish> fishesToDelete = new List<Fish>();
            foreach (Fish f in fishs)
            {
				if ((playerFishNet.transform.position - f.go.transform.position).sqrMagnitude < FishCatchSqrRange)
                {
                    TimerSecondsLeft += MenuManager.instance.BonusTimeFish;
					MenuManager.instance.IncrementScore (MenuManager.instance.BonusScoreFish);
                    // TODO : juice it up, display some FX
                    fishesToDelete.Add(f);
					PlaySFX(SFX1); // sound
                }
            }
            foreach (Fish f in fishesToDelete)
            {
                DestroyObject(f.go);
                fishs.Remove(f);
            }

            // Next Buoy
            if ((CurrentBuoy + 1) < buoys.Count)
			{
				Buoy buoy = buoys[CurrentBuoy + 1];
				if ((playerBoat.transform.position - buoy.go.transform.position).sqrMagnitude < BuoyCatchSqrRange)
				{
					CurrentBuoy = (CurrentBuoy + 1) < NumberOfSteps ? CurrentBuoy + 1 : CurrentBuoy;
					TimerSecondsLeft += MenuManager.instance.BonusTimeBuoy;
					ParticleSystem ps = buoy.go.GetComponentInChildren<ParticleSystem>();
					ps.Play();
                    Animator animator = buoy.go.GetComponent<Animator>();
                    animator.SetBool("Triggered", true);
					PlaySFX(SFX3);
				}
			}
			// DrawHelp
			if ((CurrentBuoy + 1) < buoys.Count)
			{
				Buoy buoy = buoys[CurrentBuoy + 1];
				if (BuoyHelper != null)
				{
                    float lerpFactor = Mathf.Pow(2.0f, -30.0f * Time.deltaTime);
                    if ((playerBoat.transform.position-buoy.go.transform.position).sqrMagnitude < (9.0f*9.0f))
                    {
                        Transform arrow = BuoyHelper.transform.Find("arrow").transform;
                        Transform anchor = buoy.go.transform.Find("ArrowAttachPoint").transform;

                        if (ArrowReachedBuoy)
                        {
                            arrow.position = anchor.position;
                            arrow.rotation = anchor.rotation;
                        }
                        else
                        {
                            arrow.position = Vector3.Lerp(anchor.position, arrow.position, lerpFactor);
                            arrow.rotation = Quaternion.Lerp(anchor.rotation, arrow.rotation, lerpFactor);

                            ArrowReachedBuoy = (arrow.position - anchor.position).sqrMagnitude < (1.0f * 1.0f);
                        }
                    }
                    else
                    {
                        ArrowReachedBuoy = false;
                        BuoyHelper.transform.LookAt(buoy.go.transform);

                        Transform arrow = BuoyHelper.transform.Find("arrow").transform;
                        Transform anchor = BuoyHelper.transform.Find("HelperAttachPoint").transform;
                        arrow.position = Vector3.Lerp(anchor.position, arrow.position, lerpFactor);
                        arrow.rotation = Quaternion.Lerp(anchor.rotation, arrow.rotation, lerpFactor);
                    }
                    BuoyHelper.transform.Find("arrow").GetComponent<Animator>().SetBool("Point", ArrowReachedBuoy);

                }
			}

			// Updating timer until the end
			if (MenuManager.instance.GetState()==MenuManager.GameState.PLAYING && (CurrentBuoy + 1) < NumberOfSteps)
			{
                int IntTimerBefore = (int)TimerSecondsLeft;
				TimerSecondsLeft -= Time.deltaTime;
                int IntTimerAfter = (int)TimerSecondsLeft;

                if (IntTimerAfter < 10 && IntTimerBefore != IntTimerAfter)
                    PlaySFX(SFX7);
            }
		}
		else
		{
			/* Ultimate End*/
			TimerSecondsLeft = TimerAtTheStart;
			CurrentBuoy = -1;
			MenuManager.instance.EndGame();
		}

		// UI
		if (UIScoreValue!=null)
		{
			UIScoreValue.text = MenuManager.instance.GetScore().ToString();
		}
		if (UIScoreValue2!=null)
		{
			UIScoreValue2.text = MenuManager.instance.GetScore().ToString();
		}

		if (UINumBuoys != null)
		{
			UINumBuoys.text = (CurrentBuoy+1)+" OVER "+NumberOfSteps;
		}

		if(UITimer.gameObject.activeSelf && menu.GetState() == MenuManager.GameState.PLAYINGNOTIMER)
		{
			UITimer.gameObject.SetActive(false);
            UIClock.gameObject.SetActive(false);
        }
		else if(!UITimer.gameObject.activeSelf && menu.GetState() != MenuManager.GameState.PLAYINGNOTIMER)
		{
			UITimer.gameObject.SetActive(true);
            UIClock.gameObject.SetActive(true);
        }
		if (UITimer != null)
		{
			UITimer.text = ((int)TimerSecondsLeft).ToString();
            if (TimerSecondsLeft <= 10.0f)
                UITimer.color = Color.red;
            else
                UITimer.color = defaultUITimerColor;

        }

		if (UIPlayerPosition != null || UIPlayerPositionInGame != null)
		{
			string rank = MenuManager.instance.PlayerPosition.ToString();
			switch (MenuManager.instance.PlayerPosition)
			{
				case 1:
					rank = MenuManager.instance.PlayerPosition.ToString() + "ST";
					break;
				case 2:
					rank = MenuManager.instance.PlayerPosition.ToString() + "ND";
					break;
				case 3:
					rank = MenuManager.instance.PlayerPosition.ToString() + "RD";
					break;
				default:
					rank = MenuManager.instance.PlayerPosition.ToString() + "TH";
					break;
			}

			if (UIPlayerPosition != null)
			{
				UIPlayerPosition.text = MenuManager.instance.PlayerPosition.ToString() + " / " + (numberIAs + 1);
			}

			if (UIPlayerPositionInGame != null)
			{
				UIPlayerPositionInGame.text = rank + " OVER " + (numberIAs + 1);
			}
		}

		ClipEverythingOutsideSea();
	}

	private float computeScore()
	{
		return CurrentBuoy * 100.0f - (TimerAtTheStart - TimerSecondsLeft) * 10.0f;
	}
}
