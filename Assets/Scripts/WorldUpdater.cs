using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUpdater : MonoBehaviour {

    public GameObject playerBoat;
    public GameObject buoy;
    public MenuManager menu;
    public Water sea;

    public float boatRotationSpeed = 150.0f;

	// Use this for initialization
	void Start () {
		
	}

    // Update player, returns the sea offset
    Vector2 UpdatePlayer()
    {
        if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
        {
            playerBoat.transform.Rotate(Vector3.down * boatRotationSpeed * Time.deltaTime);
        }
        if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
        {
            playerBoat.transform.Rotate(Vector3.up * boatRotationSpeed * Time.deltaTime);
        }

        return Vector2.zero;
    }

    void StickEverythingToSea()
    {
        Vector3 pos;
        Vector3 normal;

        // Player Boat
        sea.GetSurfacePosAndNormalForWPos(playerBoat.transform.position, out pos, out normal);
        playerBoat.transform.position = pos;

        // Buoy
        sea.GetSurfacePosAndNormalForWPos(buoy.transform.position, out pos, out normal);
        buoy.transform.position = pos;
    }

    // Update is called once per frame
    void Update () {
        if (menu.GetState() != MenuManager.GameState.PLAYING)
            return;

        UpdatePlayer();

        StickEverythingToSea();
	}
}
