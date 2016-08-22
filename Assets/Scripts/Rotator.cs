using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public Vector3 rotation;
    


	void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);

        UnityHUD.Log("Rotator " + transform.rotation.ToString("F3") + "\n");
	}
}
