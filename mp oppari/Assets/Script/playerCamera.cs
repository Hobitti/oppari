using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCamera : MonoBehaviour
{

    public float sensX;
    public float sensY;

    private Transform orientation;
    private Transform camposition;

    float yRotation;
    float xRotation;
    // Start is called before the first frame update
 

    // Update is called once per frame
    void Update()
    {
        if (orientation != null)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            transform.position = camposition.position;
        }
    }
    public void respawn(GameObject player)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        orientation = player.transform.GetChild(0).transform;
        camposition = player.transform.GetChild(1).transform;
    }
}
