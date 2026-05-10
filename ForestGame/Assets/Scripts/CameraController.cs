using UnityEngine;

public class CameraController : MonoBehaviour
{

    public int boundary = 50;
    public int speed = 5;

    private int screenWidth;
    private int screenHeight;

    private bool moveCam = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) moveCam = !moveCam;


        if (moveCam)
        {
            float x = transform.position.x;
            float y = transform.position.y;

            if (Input.mousePosition.x > screenWidth - boundary)
            {
                x += speed * Time.deltaTime;
            }

            if (Input.mousePosition.x < 0 + boundary)
            {
                x -= speed * Time.deltaTime;
            }

            if (Input.mousePosition.y > screenHeight - boundary)
            {
                y += speed * Time.deltaTime;
            }

            if (Input.mousePosition.y < 0 + boundary)
            {
                y -= speed * Time.deltaTime;
            }

            transform.position = new Vector3(x, y, -10);
        }
    }
}
