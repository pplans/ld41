using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUpdater : MonoBehaviour {

    public GameObject playerBoat;
    public MenuManager menu;
    public Water sea;

    public Object buoyPrefab;
    private List<GameObject> instanciatedBuoys;

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

    // Use this for initialization
    void Start () {
        boatCurrentSpeed = 0.0f;
        instanciatedBuoys = new List<GameObject>();

        // Generate some buoys
        for (int i=0; i < 100; i++)
        {
            GameObject newObject = Instantiate(buoyPrefab) as GameObject;
            Vector3 newObjectPos = Random.onUnitSphere * 100;
            newObjectPos.y = 0.0f;
            newObject.transform.position = newObjectPos;
            instanciatedBuoys.Add(newObject);
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

        foreach (var go in instanciatedBuoys)
            objects.Add(go);

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
        
        foreach (var go in instanciatedBuoys)
            go.transform.position -= deltaPlayerPos;
    }

    bool IsInsideSea(Vector3 pos)
    {
        Rect seaRect = new Rect(Vector2.one * -seaWidth / 2, Vector2.one * seaWidth);
        return seaRect.Contains(new Vector2(pos.x, pos.z));
    }

    void ClipEverythingOutsideSea()
    {
        foreach (var go in instanciatedBuoys)
            foreach (var comp in go.GetComponentsInChildren<Renderer>())
                comp.enabled = IsInsideSea(go.transform.position);
    }

    // Update is called once per frame
    void Update () {
        if (menu.GetState() != MenuManager.GameState.PLAYING)
            return;

        Vector3 playerOffset = UpdatePlayer();

        MoveEverythingWithPlayer(playerOffset);
        StickEverythingToSea();

        ClipEverythingOutsideSea();
	}
}
