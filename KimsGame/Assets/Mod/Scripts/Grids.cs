using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Grids : MonoBehaviour {

	//public Vector3[] Coordinates;

	const float MINX = -0.0679f;
	const float MAXX = 0.01529f;
	const float MINZ = -0.05345f;
	const float MAXZ = 0.0543f;
	const float Y = 0.0208f;
	const float MINDISTANCE = 0.000288f;

	public bool configure;

	public IconGrid[] IconGrids;

	float[,] _coordinates;

	void Start() {
		if (configure) {
			CalculateSpritePositions();
		}
	}

	/// <summary>
	/// For use in the Unity Editor, generates random grids to be used in gameplay.
	/// </summary>
	void CalculateSpritePositions() {

		_coordinates = new float[20, 2];

		for (int k = 0; k < IconGrids.Length; k++) {

			for (int i = 0; i < 20; i++) {
			Restart:
				float x = UnityEngine.Random.Range(MINX, MAXX);
				float z = UnityEngine.Random.Range(MINZ, MAXZ);

				for (int j = 0; j < i; j++) {
					float diffX = Mathf.Abs(_coordinates[j, 0] - x);
					float diffZ = Mathf.Abs(_coordinates[j, 1] - z);
					float pythagoras = diffX * diffX + diffZ * diffZ;
					if (pythagoras < MINDISTANCE) {
						// our sprite collides with another sprite. Reposition.
						Debug.LogFormat("{0} {1} {2}", k, i, j);
						goto Restart;
					}
				}
				_coordinates[i, 0] = x;
				_coordinates[i, 1] = z;
			}

			IconGrids[k].Coordinates = new Vector3[20];
			for (int l = 0; l < 20; l++) {
				//Coordinates[k + l] = new Vector3(_coordinates[l, 0], Y, _coordinates[l, 1]);
				IconGrids[k].Coordinates[l] = new Vector3(_coordinates[l, 0], Y, _coordinates[l, 1]);
			}
		}
	}
}
