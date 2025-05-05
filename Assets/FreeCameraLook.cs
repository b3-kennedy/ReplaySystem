using UnityEngine;

public class FreeCameraLook : MonoBehaviour
{

    public float speed;
    public float sensitivity;

    float xRot;
    float yRot;

    bool isCursorLocked = true;

    // Start is called before the first frame update
    void Start()
    {

            
        Cursor.lockState = CursorLockMode.Locked;



    }

    // Update is called once per frame
    void Update()
    {
        if(isCursorLocked)
        {
            Rotation();
            Move();
        }


        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            isCursorLocked = false;
        }

        if(Input.GetKeyUp(KeyCode.LeftAlt))
        {
            isCursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal") * speed * Time.unscaledDeltaTime;
        float vertical = Input.GetAxisRaw("Vertical") * speed * Time.unscaledDeltaTime;
        transform.Translate(horizontal, 0,vertical);
    }

    void Rotation()
    {
            float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

            yRot += mouseX;
            xRot -= mouseY;

            xRot = Mathf.Clamp(xRot, - 90f, 90f);

            transform.rotation = Quaternion.Euler(xRot, yRot, 0);
    }

    void LateUpdate()
    {
        
     
    }
}
