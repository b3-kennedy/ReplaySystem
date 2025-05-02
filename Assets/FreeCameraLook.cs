using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class FreeCameraLook : MonoBehaviour
{

    public float speed;
    public float sensitivity;

    float xRot;
    float yRot;

    // Start is called before the first frame update
    void Start()
    {

            
        Cursor.lockState = CursorLockMode.Locked;



    }

    // Update is called once per frame
    void Update()
    {
        Rotation();
        Move();
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;
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
