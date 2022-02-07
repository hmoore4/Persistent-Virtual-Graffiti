using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*#region Simple Input Controller
interface IInput
{
    bool Start     { get; }
    bool Moving    { get; }
    bool End       { get; }
    int  PositionX { get; }
}
 
class CMouse : IInput
{
    bool IInput.Start    { get { return Input.GetMouseButtonDown(0); } }
    bool IInput.Moving   { get { return Input.GetMouseButton(0); } }
    bool IInput.End      { get { return Input.GetMouseButtonUp(0); } }
    int IInput.PositionX { get { return (int) Input.mousePosition.x; } }
}
 
class CTouch : IInput
{
    bool IInput.Start  { get { return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began; } }
    bool IInput.Moving { get { return Input.touchCount >  0 && Input.GetTouch(0).phase == TouchPhase.Moved; } }
    bool IInput.End    { get { return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended; } }
    int IInput.PositionX { get {
            return Input.touchCount > 0
                ? (int)Input.GetTouch(0).position.x
                : 0; } }
}
#endregion
 
 
public class Scene_Manager : MonoBehaviour
{
        private string nextScene = "PaintingScene";

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            CheckUserSwipe = ( inputType == eInputType.Touch ? (IInput)new CTouch() : new CMouse() );
            created = true;
        }
    }
 
    void Update()
    {
        bool changePage = false;
 
        switch (CheckSwipe())
        {
            case eSwipe.Left:
                if(m_currentPage > 0)
                {
                    --m_currentPage;
                    changePage = true;
                }
                break;
            case eSwipe.Right:
                if (m_currentPage < sceneNames.Length - 1)
                {
                    ++m_currentPage;
                    changePage = true;
                }
                break;
        }
 
        if (changePage)
            SceneManager.LoadScene(nextScene);
    }
 
    eSwipe CheckSwipe()
    {
        if (CheckUserSwipe.Start)
        {
            m_hasSwiped = false;
            m_touchPosX = CheckUserSwipe.PositionX;
        }
        else if (CheckUserSwipe.Moving && !m_hasSwiped)
        {
            var moved = CheckUserSwipe.PositionX - m_touchPosX;
            var retVal
                = (moved >  swipeDistance) ? eSwipe.Right
                : (moved < -swipeDistance) ? eSwipe.Left
                : eSwipe.None;
            m_hasSwiped = retVal != eSwipe.None;
            return retVal;
        }
 
        return eSwipe.None;
    }
 
    void LoadScene()
    {
        SceneManager.LoadScene(sceneNames[m_currentPage], LoadSceneMode.Single);
    }
 
#pragma warning disable 649
    [SerializeField] eInputType inputType;
    [SerializeField] [Range(10, 200)] int swipeDistance = 50;
    [SerializeField] string[] sceneNames;
#pragma warning restore 649
 
    enum eInputType { Mouse, Touch }
    enum eSwipe { None, Left, Right }
 
    static bool created = false;
    IInput CheckUserSwipe;
    int m_currentPage = 0;
    int m_touchPosX;
    bool m_hasSwiped = false;
}*/
public class Scene_Manager : MonoBehaviour
{
    private Vector2 fingerDown;
    private string paintingScene = "PaintingScene";
    private string mainScene = "NewUI";

    private Vector2 fingerUp;
    public bool detectSwipeOnlyAfterRelease = false;

    public float SWIPE_THRESHOLD = 20f;
        public static Scene_Manager instance;
        int numSceneManagers = 0;

    void Awake()
    {
     
      numSceneManagers = FindObjectsOfType<Scene_Manager>().Length;
Debug.Log("numSceneManagers " + numSceneManagers);
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Scene Manager Tag");
Debug.Log("OBJS LENGTH SCENE PRE " + objs.Length);
foreach(GameObject obj in objs){
    Debug.Log("THE GAME OBJECT IS (IN SCENE MANAGER): " + gameObject);

}
        if (numSceneManagers > 1)
        {
            Destroy(gameObject);

        }
else{
        DontDestroyOnLoad(gameObject);
}
                    Debug.Log("THE GAME OBJECT IS: " + gameObject);
                    Debug.Log("OBJS LENGTH SCENE POST " + objs.Length);
numSceneManagers = FindObjectsOfType<Scene_Manager>().Length;
Debug.Log("numSceneManagers " + numSceneManagers);

    }
    void Update()
    {

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                fingerUp = touch.position;
                fingerDown = touch.position;
            }

            //Detects Swipe while finger is still moving
            if (touch.phase == TouchPhase.Moved)
            {
                if (!detectSwipeOnlyAfterRelease)
                {
                    fingerDown = touch.position;
                    checkSwipe();
                }
            }

            //Detects swipe after finger is released
            if (touch.phase == TouchPhase.Ended)
            {
                fingerDown = touch.position;
                checkSwipe();
            }
        }
    }
void Start(){
    int numSceneManagers = FindObjectsOfType<Scene_Manager>().Length;
    Debug.Log("numSceneManagers in start" + numSceneManagers);

    
}
    void checkSwipe()
    {
        //Check if Vertical swipe
        if (verticalMove() > SWIPE_THRESHOLD && verticalMove() > horizontalValMove())
        {
            //Debug.Log("Vertical");
            if (fingerDown.y - fingerUp.y > 0)//up swipe
            {
                OnSwipeUp();
            }
            else if (fingerDown.y - fingerUp.y < 0)//Down swipe
            {
                OnSwipeDown();
            }
            fingerUp = fingerDown;
        }

        //Check if Horizontal swipe
        else if (horizontalValMove() > SWIPE_THRESHOLD && horizontalValMove() > verticalMove())
        {
            //Debug.Log("Horizontal");
            if (fingerDown.x - fingerUp.x > 0)//Right swipe
            {
                OnSwipeRight();
            }
            else if (fingerDown.x - fingerUp.x < 0)//Left swipe
            {
                OnSwipeLeft();
            }
            fingerUp = fingerDown;
        }

        //No Movement at-all
        else
        {
            //Debug.Log("No Swipe!");
        }
    }

    float verticalMove()
    {
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    float horizontalValMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    //////////////////////////////////CALLBACK FUNCTIONS/////////////////////////////
    void OnSwipeUp()
    {
        Debug.Log("Swipe UP");
    }

    void OnSwipeDown()
    {
        Debug.Log("Swipe Down");
    }

    void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
        SceneManager.LoadScene(paintingScene);

    }

    void OnSwipeRight()
    {
        Debug.Log("Swipe Right");
        SceneManager.LoadScene(mainScene);
    }
}