using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public Vector3 rotation;
    


    void Start()
    {
        UnityHUD.Help("Rotator has no hotkeys.");

        rotation = UnityHUD.GetConfigVector3("rotator_rotation");
    }

	void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);

        UnityHUD.Debug("Rotator " + transform.rotation.ToString("F3") + "\n");
	}
}
