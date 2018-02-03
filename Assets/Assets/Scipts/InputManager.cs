using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapping class for doing necessary changes to pathfinding in correct order on Input.
/// </summary>
public class InputManager : MonoBehaviour {

    public MapGenerator mapGenerator;
    public GridManager gridManager;
    public RepositionObject[] repositionObjects;
    public Unit[] units;

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0))
        {
            mapGenerator.GenerateMap();
            gridManager.FillGrid();
            gridManager.BlurPenaltyMap(3);
            foreach (RepositionObject repositionObject in repositionObjects)
            {
                repositionObject.Reposition();
            }
            foreach (Unit unit in units)
            {
                unit.RecalculatePath();
            }
        }
	}
}
