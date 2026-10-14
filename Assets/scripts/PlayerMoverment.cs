using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PlayerMoverment : Movement
{
    [SerializeField] InputActionReference reset;
    [SerializeField] InputActionReference reverse;
    [SerializeField] InputActionReference input;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        input.action.Enable();
        reset.action.Enable();
        reverse.action.Enable();
        reset.action.performed += ResetFunc;
        reverse.action.performed += Reverse;
    }
    void OnDisable()
    {
        input.action.Disable();
        reset.action.performed -= ResetFunc;
        reverse.action.performed -= Reverse;
        reverse.action.Disable();
        reset.action.Disable();
    }

    // Update is called once per frame
    protected override void Update()
    {
        Vector2 movementOutput = input.action.ReadValue<Vector2>();
        steeringInput = movementOutput.x;
        forwardInput = movementOutput.y;

        // deadszones
        if (forwardInput < -0.9)
        {
            forwardInput = -1;
        }

        if (forwardInput > -0.1 && forwardInput < 0.1)
        {
            forwardInput = 0;
        }
        base.Update();
    }
    
    void ResetFunc(InputAction.CallbackContext _)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    void Reverse(InputAction.CallbackContext _)
    {
        if (currentGear == 0)
        {
            currentGear = 1;
        }
        else
        {
            currentGear = 0;
        }
    }
}
