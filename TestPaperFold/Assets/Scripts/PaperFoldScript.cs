using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Figure
{
    public Transform FigureObj;
    public Image FigureImage;
    public Transform[] Neighbors;
    public Transform backgroundStorage;
}
public class PaperFoldScript : MonoBehaviour
{
    public Sprite shayaTex;

    public Figure[] figures;

    public int _screenWidth;
    public int _screenHeight;

    public List<int> _ochered = new List<int>();
    public Transform storage;

    public GameObject WinImage;

    public Color color_close;
    public Color color_open;

    public float speed;

    int[] WinArray = new int[4] { 0, 2, 1, 3 };

    bool isWork = false;

    void Start()
    {
        _screenWidth = Screen.width / 2;
        _screenHeight = Screen.height / 2;

        for(int i = 0; i < figures.Length; i++)
        {
            ChangeColor(false, figures[i].FigureObj);
            ChangeTexture(false, figures[i].FigureImage);
        }
    }
    void LateUpdate()
    {   
        ReadingInputs();
    }
    void ReadingInputs()
    {
        if (Input.GetMouseButtonDown(0) && !isWork)
        {
            ClickFigure(Input.mousePosition);
        }
    }
    void ClickFigure(Vector2 touchPos)
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

        if (index_figure >= 0)
        {
            Behavior(index_figure);
        }
    }
    void Behavior(int index_figure)
    {
        if (figures[index_figure].FigureImage.sprite == null)
        {
            StartAnimOpen(index_figure);
        }
        else 
        {
            StartCoroutine(FigureAnimClose(index_figure));
        }
    }
    void StartAnimOpen(int index_figure)
    {
        Transform tr = figures[index_figure].FigureObj;

        Ochered(true, index_figure);
        ParentFigure(tr, storage, _ochered.Count - 1);
        ParentNeighbor(true, tr.GetSiblingIndex(), index_figure);

        StartCoroutine(FigureAnimOpen(index_figure));
    }
    IEnumerator FigureAnimOpen(int index_figure)
    {
        isWork = true;
        float target = -1;
        Transform tr = figures[index_figure].FigureObj;
        float y_change = tr.localScale.y;

        while (tr.localScale.y != target)
        {
            y_change = Mathf.MoveTowards(y_change, target, speed * Time.deltaTime);
            tr.localScale = new Vector3(1, y_change, 1);

            if (y_change < 0)
            {
                ChangeColor(true, tr);
                ChangeTexture(true, figures[index_figure].FigureImage);
            }            
            yield return null;
        }

        tr.localScale = new Vector3(1, target, 1);

        CheckWin();
        isWork = false;
    }
    IEnumerator FigureAnimClose(int index_figure)
    {
        isWork = true;
        float target = 1;

        int count = _ochered.Count - 1;
        int index = figures[index_figure].FigureObj.GetSiblingIndex();

        for (int i = count; i >= index; i--)
        {
            int new_index_Figure = _ochered[i];
            Transform newTr = figures[new_index_Figure].FigureObj;
            float y_change = newTr.localScale.y;

            while (newTr.localScale.y != target)
            {
                y_change = Mathf.MoveTowards(y_change, target, speed * Time.deltaTime);
                newTr.localScale = new Vector3(1, y_change, 1);

                yield return null;
            }
            newTr.localScale = new Vector3(1, target, 1);

            EndAnimClose(newTr, new_index_Figure);
        }

        CheckWin();
        isWork = false;
    }
    void EndAnimClose(Transform tr, int index_figure)
    {
        ParentNeighbor(false, tr.GetSiblingIndex(), index_figure);
        ChangeColor(false, tr);
        ChangeTexture(false, figures[index_figure].FigureImage);
        ParentFigure(tr, storage.parent, 2);
        Ochered(false, index_figure);
    }
    void Ochered(bool add, int index_figure)
    {
        if (add)
        {
            _ochered.Add(index_figure);
        }
        else
        {
            _ochered.Remove(index_figure);
        }
    }
    void ParentNeighbor(bool open, int sibling_index, int index_figure)
    {
        if (open)
        {
            foreach (Transform neighbor in figures[index_figure].Neighbors)
            {
                neighbor.SetParent(figures[index_figure].backgroundStorage);
            }
        }
        else
        {
            if (storage.childCount > 1)
            {
                foreach (Transform neighbor in figures[index_figure].Neighbors)
                {
                    for (int j = 1; j < storage.childCount; j++)
                    {
                        if (sibling_index > 0)
                        {
                            int current_index = _ochered[sibling_index - j];
                            for (int i = 0; i < figures[current_index].Neighbors.Length; i++)
                            {
                                if (neighbor == figures[current_index].Neighbors[i])
                                {
                                    neighbor.SetParent(figures[current_index].backgroundStorage);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    void ChangeTexture(bool open, Image im)
    {
        if (open)
        {
            im.sprite = shayaTex;
            im.color = Color.white;
        }
        else
        {
            im.sprite = null;
        }
    }
    void ChangeColor(bool open, Transform tr)
    {
        Image[] im = tr.GetComponentsInChildren<Image>();
        foreach (Image cur_image in im)
        {
            if (open)
            {
                cur_image.color = color_open;
            }
            else
            {
                cur_image.color = color_close;
            }
        }
    }
    void ParentFigure(Transform tr, Transform parent, int sibling)
    {
        tr.SetParent(parent);
        tr.SetSiblingIndex(sibling);
    }
    void CheckWin()
    {
        if (_ochered.Count == 4)
        {
            int cur_index = 0;
            while(cur_index < 4)
            {
                if (WinArray[cur_index] == _ochered[cur_index])
                {
                    cur_index++;
                    if(cur_index == 4)
                    {
                        WinImage.SetActive(true);
                    }
                }
                else
                {
                    //lose
                    return;
                }
            }
        }
        else
        {
            WinImage.SetActive(false);
        }
    }
}
