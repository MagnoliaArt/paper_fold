using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Figure
{
    public Transform FigureObj;
    public Image FigureImage;
    public Transform[] Neighbors;
    public Transform backgroundStorage;
    internal bool open = false;
}
[RequireComponent(typeof(PaperFoldScript))]
public class FigureWork : MonoBehaviour
{    
    public Figure[] figures;

    private List<int> _ochered = new List<int>();

    public TextureSettings _textureSettings;
    public ColorSettings _colorSettings;
    public WinClass winCheck;

    public MoveFigure _moveFigure;

    public Transform storage;

    private bool isWork = false;
    private void Start()
    {
        ResetFiguresColor();
    }
    private void ResetFiguresColor()
    {
        for (int i = 0; i < figures.Length; i++)
        {
            ChangeColorAndTexture(false, i);
        }
    }
    public void ClickFigure(int index_figure)
    {
        if (isWork || index_figure < 0)
        {
            return;
        }
        isWork = true;

        if (!figures[index_figure].open)
        {
            StartAnimOpen(figures[index_figure].FigureObj, index_figure);
        }
        StartCoroutine(FigureAnim(index_figure));
    }
    private IEnumerator FigureAnim(int index_figure)
    {
        bool open = figures[index_figure].open;

        float target = !open ? -1 : 1;

        int count = !open ? 1 : _ochered.Count - 1;
        int index = !open ? 0 : figures[index_figure].FigureObj.GetSiblingIndex();

        for (int i = count; i >= index; i--)
        {
            int new_index_figure = !open ? index_figure : _ochered[i];
            Transform tr = figures[new_index_figure].FigureObj;

            while (!_moveFigure.Moving(tr, target))
            {
                if (!open && tr.localScale.y < 0)
                {
                    ChangeColorAndTexture(true, new_index_figure);
                }
                yield return null;
            }

            if (open)
            {      
                EndAnimClose(tr, new_index_figure);
            }
            figures[new_index_figure].open = !open;
        }
        isWork = false;

        winCheck.CheckWin(_ochered);
    }

    private void StartAnimOpen(Transform tr, int index_figure)
    {
        Ochered(true, index_figure);
        ParentFigure(tr, storage, _ochered.Count - 1);
        ParentNeighbor(true, tr.GetSiblingIndex(), index_figure);
    }
    private void EndAnimClose(Transform tr, int index_figure)
    {
        ParentNeighbor(false, tr.GetSiblingIndex(), index_figure);
        ChangeColorAndTexture(false, index_figure);
        ParentFigure(tr, storage.parent, 2);
        Ochered(false, index_figure);
    }

    private void ParentNeighbor(bool open, int sibling_index, int index_figure)
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
    private void ParentFigure(Transform tr, Transform parent, int sibling)
    {
        tr.SetParent(parent);
        tr.SetSiblingIndex(sibling);
    }    
    private void Ochered(bool open, int index_figure)
    {
        if (open)
        {
            _ochered.Add(index_figure);
        }
        else
        {
            _ochered.Remove(index_figure);
        }
    }
    private void ChangeColorAndTexture(bool open, int index_figure)
    {
        _colorSettings.ChangeColor(open, figures[index_figure].FigureObj);
        _textureSettings.ChangeTexture(open, figures[index_figure].FigureImage);
    }
}
[System.Serializable]
public class MoveFigure
{
    public float speed;

    public bool Moving(Transform tr, float target)
    {
        float y_change = tr.localScale.y;
        y_change = Mathf.MoveTowards(y_change, target, speed * Time.deltaTime);
        tr.localScale = new Vector3(1, y_change, 1);
        if (tr.localScale.y == target)
        {
            return true;
        }
        return false;
    }
}
[System.Serializable]
public class WinClass
{
    public GameObject WinObject;
    private int[] WinArray = new int[4] { 0, 2, 1, 3 };

    public void CheckWin(List<int> ochered)
    {
        if (ochered.Count == 4)
        {
            int cur_index = 0;
            while (cur_index < 4)
            {
                if (WinArray[cur_index] == ochered[cur_index])
                {
                    cur_index++;
                    if (cur_index == 4)
                    {
                        WinObject.SetActive(true);
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
            WinObject.SetActive(false);
        }
    }
}
[System.Serializable]
public class TextureSettings
{
    public Sprite shayaTex;

    public void ChangeTexture(bool open, Image im)
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
}
[System.Serializable]
public class ColorSettings
{
    public Color color_close;
    public Color color_open;

    public void ChangeColor(bool open, Transform tr)
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
}
