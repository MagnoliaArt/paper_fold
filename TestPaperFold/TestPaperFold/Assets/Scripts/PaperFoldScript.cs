using UnityEngine;

[RequireComponent(typeof(FigureWork))]
public class PaperFoldScript : MonoBehaviour
{
    FigureWork figureWork;

    int _screenWidth;
    int _screenHeight;
    private void Start()
    {
        figureWork = GetComponent<FigureWork>();
        ScreenInfo();
    }
    private void ScreenInfo()
    {
        _screenWidth = Screen.width / 2;
        _screenHeight = Screen.height / 2;
    }
    private void LateUpdate()
    {   
        ReadingInputs();
    }
    private void ReadingInputs()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            figureWork.ClickFigure(IndexFigure(Input.mousePosition));
        }
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                figureWork.ClickFigure(IndexFigure(touch.position));
            }         
        }         
#endif      
    }
    private int IndexFigure(Vector2 touchPos)
    {
        int index_figure = -1;

        if (touchPos.x < _screenWidth / 1.4f)
        {
            index_figure = 0;
        }
        else if (touchPos.x > _screenWidth * 1.4f)
        {
            index_figure = 1;
        }
        else if (touchPos.y > _screenHeight * 1.2f)
        {
            index_figure = 2;
        }
        else if (touchPos.y < _screenHeight / 1.2f)
        {
            index_figure = 3;
        }
        else
        {
            //miss
        }
        return index_figure;
    }
}