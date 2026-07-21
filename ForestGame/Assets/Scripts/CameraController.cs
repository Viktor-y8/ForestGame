using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public int boundary = 50;
    public float speed = 5f;

    private int screenWidth;
    private int screenHeight;
    private bool moveCam = true;

    private float minX, maxX, minY, maxY;
    private bool hasBounds = false;

    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite camOnSprite;
    [SerializeField] private Sprite camOffSprite;

    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    public void SetBounds(Bounds bounds)
    {
        float camHalfH = Camera.main.orthographicSize;
        float camHalfW = camHalfH * Camera.main.aspect;

        minX = bounds.min.x + camHalfW;
        maxX = bounds.max.x - camHalfW;
        minY = bounds.min.y + camHalfH;
        maxY = bounds.max.y - camHalfH;

        if (minX > maxX) minX = maxX = bounds.center.x;
        if (minY > maxY) minY = maxY = bounds.center.y;

        hasBounds = true;
    }

    public void setMoveCam()
    {
        moveCam = !moveCam;
        buttonImage.sprite = moveCam ? camOnSprite : camOffSprite;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            setMoveCam();
            SoundManager.Instance.PlaySFX("buttonSFX");
        }

        if (!moveCam) return;

        if (TutorialManager.IsTutorialActive) return;

        float x = transform.position.x;
        float y = transform.position.y;

        if (Input.mousePosition.x > screenWidth - boundary) x += speed * Time.deltaTime;
        if (Input.mousePosition.x < boundary) x -= speed * Time.deltaTime;
        if (Input.mousePosition.y > screenHeight - boundary) y += speed * Time.deltaTime;
        if (Input.mousePosition.y < boundary) y -= speed * Time.deltaTime;

        if (hasBounds)
        {
            x = Mathf.Clamp(x, minX, maxX);
            y = Mathf.Clamp(y, minY, maxY);
        }

        transform.position = new Vector3(x, y, -10);
    }
}