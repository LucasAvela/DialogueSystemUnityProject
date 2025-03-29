using UnityEngine;
using UnityEngine.InputSystem;

public class Steer : MonoBehaviour
{
    [SerializeField] float steerSpeed = 135f;
    public void SteerCar(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        float steer = input.x * -steerSpeed;
        Vector3 rotation = new Vector3(0, 0, steer);
        transform.localRotation = Quaternion.Euler(rotation);
    }
}
