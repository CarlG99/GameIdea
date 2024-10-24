using UnityEngine;

public class ControlManager : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;

    public Vector2 GetMovementInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public bool GetJumpInput()
    {
        return Input.GetButton("Jump");
    }

    public bool GetRunInput()
    {
        return Input.GetButton("Run");
    }

    public Vector2 GetMouseInput()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }
}
