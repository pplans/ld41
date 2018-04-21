using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUpdater : MonoBehaviour {

    public GameObject playerBoat;
    public GameObject buoy;
    public MenuManager menu;
    public Water sea;

    public float boatRotationSpeed = 200.0f;
    public float boatAcceleration = 2.0f;
    public float boatMaxSpeed = 0.5f;
    public float boatDrag = 1.0f;
    private float boatCurrentSpeed = 0.0f;

    public float stickyUpRate = 30.0f;
    public float stickyDownRate = 9.8f;
    public float stickyTiltRate = 9.8f;
    public float stickyOffset = -0.05f;

    // Use this for initialization
    void Start () {
        boatCurrentSpeed = 0.0f;
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
        objects.Add(buoy);

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
        buoy.transform.position -= deltaPlayerPos;
    }
    
    // Update is called once per frame
    void Update () {
        if (menu.GetState() != MenuManager.GameState.PLAYING)
            return;

        Vector3 playerOffset = UpdatePlayer();

        MoveEverythingWithPlayer(playerOffset);
        StickEverythingToSea();
	}
}
