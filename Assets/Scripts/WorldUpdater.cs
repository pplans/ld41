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
	public Text UINumBuoys = null;
	public Text UITimer = null;
	public float TimerAtTheStart = 60.0f;
    public float ExtraTimeForSmallFish = 2.0f;

    public float BuoyCatchSqrRange = 1.0f;
    public float FishCatchSqrRange = 1.0f;

    public Object buoyPrefab;
    public Object smallFishPrefab;

    public float boatRotationSpeed = 200.0f;
    public float boatAcceleration = 2.0f;
    public float boatMaxSpeed = 0.5f;
    public float boatDrag = 1.0f;
    private float boatCurrentSpeed = 0.0f;

    public float stickyUpRate = 30.0f;
    public float stickyDownRate = 9.8f;
    public float stickyTiltRate = 9.8f;
    public float stickyOffset = -0.05f;

    public float seaWidth = 12.0f; // supposed to be 10 but we had a margin

	public Vector3	StartRun;
	public Vector3	EndRun;
	public uint		NumberOfSteps; // buoys

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
        Type type;

        public Fish(Type _type, GameObject _go)
        {
            type = _type;
            go = _go;
        }
    }
    private List<Fish> fishs;

    class BezierCurve
	{
		public Vector3 P1;
		public Vector3 P2;
		public Vector3 P3;
		public Vector3 Get(float t)
		{
			return (1.0f - t) * (1.0f - t) * P1 + 2.0f * (1.0f - t) * t * P2 + t * t * P3;
		}
	}

    // Use this for initialization
    void Start () {
        boatCurrentSpeed = 0.0f;
		TimerSecondsLeft = TimerAtTheStart;
		buoys = new List<Buoy>();

		// Generate some buoys
		BezierCurve path = new BezierCurve();
		path.P1 = StartRun;
		path.P3 = EndRun;
		path.P2 = StartRun+0.5f*(EndRun - StartRun)+Vector3.Cross((EndRun-StartRun).normalized, Vector3.up)*4.0f;
        for (int i=0; i < NumberOfSteps; i++)
        {
            GameObject newObject = Instantiate(buoyPrefab) as GameObject;
            Vector3 newObjectPos = Random.onUnitSphere * 100;
            newObject.transform.position = path.Get((float)i/NumberOfSteps);
            buoys.Add(new Buoy(i, newObject));
		}

        // Generate some fishes
        BezierCurve path2 = new BezierCurve();
        path.P1 = StartRun;
        path.P3 = EndRun;
        path.P2 = StartRun + 0.5f * (EndRun - StartRun) + Vector3.Cross((EndRun - StartRun).normalized, Vector3.up) * 4.0f;
        for (int i = 0; i < NumberOfSteps; i++)
        {
            GameObject newObject = Instantiate(smallFishPrefab) as GameObject;
            Vector3 newObjectPos = Random.onUnitSphere * 5;
            newObject.transform.position = path.Get((float)i / NumberOfSteps) + newObjectPos;
            fishs.Add(new Fish(Fish.Type.SMALL, newObject));
        }
    }

    // Update player, returns the sea offset
    Vector3 UpdatePlayer()
    {
        if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
        {
            playerBoat.transform.Rotate(Vector3.down * boatRotationSpeed * Time.deltaTime);
        }
        if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
        {
            playerBoat.transform.Rotate(Vector3.up * boatRotationSpeed * Time.deltaTime);
        }

        if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
        {
            boatCurrentSpeed = Mathf.Lerp(boatMaxSpeed, boatCurrentSpeed, Mathf.Pow(2.0f, -boatAcceleration * Time.deltaTime));
        }
        else
        {
            boatCurrentSpeed = Mathf.Lerp(0.0f, boatCurrentSpeed, Mathf.Pow(2.0f, -boatDrag * Time.deltaTime));
        }

        return (playerBoat.transform.rotation * Vector3.forward) * boatCurrentSpeed;
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
        if (menu.GetState() != MenuManager.GameState.PLAYING)
            return;

        Vector3 playerOffset = UpdatePlayer();

        MoveEverythingWithPlayer(playerOffset);
        StickEverythingToSea();

		if(TimerSecondsLeft>0.0f)
		{
            // Catch fish
            List<Fish> fishesToDelete = new List<Fish>();
            foreach (Fish f in fishs)
            {
                if ((playerBoat.transform.position - f.go.transform.position).sqrMagnitude < FishCatchSqrRange)
                {
                    TimerSecondsLeft += ExtraTimeForSmallFish;
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
			if ((CurrentBuoy + 1) < NumberOfSteps)
			{
				TimerSecondsLeft -= Time.deltaTime;
			}
		}
		else
		{
			// Restart
			MenuManager.instance.RegisterScore(computeScore());
			TimerSecondsLeft = TimerAtTheStart;
			CurrentBuoy = -1;
			MenuManager.instance.EndGame();
		}

		// UI
		if (UIScoreValue!=null)
		{
			UIScoreValue.text = computeScore().ToString();
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
