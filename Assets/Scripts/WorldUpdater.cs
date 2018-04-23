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
	public ParticleSystem m_splash = null;

    public float BuoyCatchSqrRange = 1.0f;
    public float FishCatchSqrRange = 1.0f;

    public Object buoyPrefab;
    public Object smallFishPrefab;
    public Object mediumFishPrefab;
    public Object bigFishPrefab;
	public Object aiPrefab;

	private GameObject playerFishNet;
	private GameObject playerFishNetTarget;

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

	class AI
	{
		public GameObject go;
		public Vector3 direction;
	}
	private List<AI> ais = null;

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
			playerFishNetTarget = playerBoat.transform.Find ("FishNetTarget").gameObject;
		}
	}

	public void Reset () {
		boatCurrentSpeed = 0.0f;
		sea.Offset = Vector2.zero;
		MenuManager.instance.RegisterScore (0);
		TimerSecondsLeft = TimerAtTheStart;
		Generate();
		ais = new List<AI>();
	}


	void Generate()
	{
		CurrentBuoy = -1;
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

		if(m_splash!=null)
		{
			var main = m_splash.main;
			main.startLifetimeMultiplier = 2.0f*boatCurrentSpeed;
		}

        Animator boatAnimator = playerBoat.GetComponent<Animator>();
        boatAnimator.SetFloat("Direction", direction);
        boatAnimator.SetFloat("Speed", boatCurrentSpeed);

		Vector3 playerOffset = (playerBoat.transform.rotation * Vector3.forward) * boatCurrentSpeed;

		// Update fishnet location
		playerFishNet.transform.position -= playerOffset;
		float lerpFactor = Mathf.Pow (2.0f, -fishNetRate * Time.deltaTime);
		playerFishNet.transform.position = Vector3.Lerp(playerFishNetTarget.transform.position, playerFishNet.transform.position, lerpFactor);
		playerFishNet.transform.rotation = Quaternion.Lerp(playerFishNetTarget.transform.rotation, playerFishNet.transform.rotation, lerpFactor);

		return playerOffset;
    }

    void StickEverythingToSea()
    {
        Vector3 pos;
        Vector3 normal;

        List<GameObject> objects = new List<GameObject>();
        objects.Add(playerBoat);

		if(buoys!=null)
        foreach (var b in buoys)
            objects.Add(b.go);

        if (fishs != null)
            foreach (var b in fishs)
                objects.Add(b.go);

		if (ais != null)
			foreach (var b in fishs)
				objects.Add(b.go);

		Quaternion fixQuaternion = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 180, 0);

        foreach (var go in objects)
        {
            sea.GetSurfacePosAndNormalForWPos(go.transform.position, out pos, out normal);
            pos.y += stickyOffset;
            if (go.transform.position.y > pos.y)
            {
                go.transform.position = Vector3.Lerp(pos, go.transform.position, Mathf.Pow(2.0f, -stickyDownRate * Time.deltaTime));
            }
            else
            {
                go.transform.position = Vector3.Lerp(pos, go.transform.position, Mathf.Pow(2.0f, -stickyUpRate * Time.deltaTime));
            }

            Quaternion newQuaternion = Quaternion.LookRotation(normal, go.transform.rotation * Vector3.forward) * fixQuaternion;
            go.transform.rotation = Quaternion.Lerp(newQuaternion, go.transform.rotation, Mathf.Pow(2.0f, -stickyTiltRate * Time.deltaTime));
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
    }

    // Update is called once per frame
    void Update () {
        if (!(menu.GetState() == MenuManager.GameState.PLAYING || menu.GetState() == MenuManager.GameState.PLAYINGNOTIMER))
            return;

        Vector3 playerOffset = UpdatePlayer();

        MoveEverythingWithPlayer(playerOffset);
        StickEverythingToSea();

		if(ais.Count<20)
		{
			AI ai = new AI();
			ai.go = Instantiate(aiPrefab) as GameObject;
			ai.go.transform.position = new Vector3(Random.Range(-1000.0f, 1000.0f), 0.0f, Random.Range(-1000.0f, 1000.0f));
			ai.direction = playerBoat.transform.rotation*Vector3.forward;
			ais.Add(ai);
		}
		for(int i = 0;i<ais.Count;++i)
		{
			ais[i].go.transform.position += ais[i].direction * Time.deltaTime * 5.0f;
			if(ais[i].go.transform.position.sqrMagnitude>seaWidth*10.0f)
			{
				ais[i].direction = Vector3.Lerp(ais[i].direction, playerBoat.transform.rotation * Vector3.forward, Time.deltaTime);
				ais[i].go.transform.rotation.SetLookRotation(ais[i].direction, Vector3.up);
				//Destroy(ais[i].go);
				//ais.Remove(ais[i]);
			}
		}

		if((CurrentBuoy+1)>=buoys.Count)
		{
			MenuManager.instance.IncrementScore(computeScore());
			// Restart
			Generate();
			TimerSecondsLeft = TimerAtTheStart;
			CurrentBuoy = -1;
		}
		if(TimerSecondsLeft>0.0f)
		{
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
                }
			}
			// DrawHelp
			if ((CurrentBuoy + 1) < buoys.Count)
			{
				Buoy buoy = buoys[CurrentBuoy + 1];
				if (BuoyHelper != null)
				{
					BuoyHelper.transform.LookAt(buoy.go.transform);
				}
			}

			// Updating timer until the end
			if (MenuManager.instance.GetState()==MenuManager.GameState.PLAYING && (CurrentBuoy + 1) < NumberOfSteps)
			{
				TimerSecondsLeft -= Time.deltaTime;
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
			UINumBuoys.text = (CurrentBuoy+1)+"/"+NumberOfSteps;
		}

		if (UITimer != null)
		{
			UITimer.text = TimerSecondsLeft.ToString("F2")+"s";
		}


		ClipEverythingOutsideSea();
	}

	private float computeScore()
	{
		return CurrentBuoy * 100.0f - (TimerAtTheStart - TimerSecondsLeft) * 10.0f;
	}
}
