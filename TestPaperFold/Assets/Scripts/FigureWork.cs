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
    [SerializeField]
    private Figure[] figures;

    private List<int> _ochered = new List<int>();

    [SerializeField]
    private TextureSettings _textureSettings;
    [SerializeField]
    private ColorSettings _colorSettings;
    [SerializeField]
    private WinClass winCheck;
    [SerializeField]
    private MoveFigure _moveFigure;

    [SerializeField]
    private Transform storage;

    private bool isWork = false;
    private void Start()
    {
        ResetFiguresColor();
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
            StartAnimAtOpen(figures[index_figure].FigureObj, index_figure);
        }
        StartCoroutine(FigureAnimation(index_figure));
    }
    private void StartAnimAtOpen(Transform current_figure, int index_figure)
    {
        AddToOchered(index_figure);
        FigureToParent(current_figure, storage, _ochered.Count - 1);
        FiguresNeighborToParent(true, current_figure.GetSiblingIndex(), index_figure);
    }
    private void AddToOchered(int index_figure)
    {
        _ochered.Add(index_figure);
    }
    private IEnumerator FigureAnimation(int index_figure)
    {
        bool open = figures[index_figure].open;

        float target = !open ? -1 : 1;

        int count = !open ? 1 : _ochered.Count - 1;
        int index = !open ? 0 : figures[index_figure].FigureObj.GetSiblingIndex();

        for (int i = count; i >= index; i--)
        {
            int new_index_figure = !open ? index_figure : _ochered[i];
            Transform current_figure = figures[new_index_figure].FigureObj;

            while (!_moveFigure.FigureIsMoved(current_figure, target))
            {
                if (!open && current_figure.localScale.y < 0)
                {
                    ChangeColorAndTexture(true, new_index_figure);
                }
                yield return null;
            }

            if (open)
            {      
                EndAnimAtClose(current_figure, new_index_figure);
            }
            figures[new_index_figure].open = !open;
        }
        isWork = false;
        winCheck.CheckWin(_ochered);
    }
    private void EndAnimAtClose(Transform current_figure, int index_figure)
    {
        FiguresNeighborToParent(false, current_figure.GetSiblingIndex(), index_figure);
        ChangeColorAndTexture(false, index_figure);
        FigureToParent(current_figure, storage.parent, 2);
        RemoveToOchered(index_figure);
    }
    private void RemoveToOchered(int index_figure)
    {
        _ochered.Remove(index_figure);
    }
    private void FigureToParent(Transform current_figure, Transform parent, int sibling)
    {
        current_figure.SetParent(parent);
        current_figure.SetSiblingIndex(sibling);
    }
    private void FiguresNeighborToParent(bool open, int sibling_index, int index_figure)
    {
        foreach (Transform neighbor in figures[index_figure].Neighbors)
        {
            if (open)
            {
                FiguresNeighborToParentAtOpen(neighbor, index_figure);
            }
            else
            {
                FiguresNeighborToParentAtClose(neighbor, sibling_index);
            }
        }
    }
    private void FiguresNeighborToParentAtOpen(Transform neighbor, int index_figure)
    {
        neighbor.SetParent(figures[index_figure].backgroundStorage);
    }
    private void FiguresNeighborToParentAtClose(Transform neighbor, int sibling_index)
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
    private void ChangeColorAndTexture(bool open, int index_figure)
    {
        _colorSettings.ChangeColor(open, figures[index_figure].FigureObj);
        _textureSettings.ChangeTexture(open, figures[index_figure].FigureImage);
    }
    private void ResetFiguresColor()
    {
        for (int i = 0; i < figures.Length; i++)
        {
            ChangeColorAndTexture(false, i);
        }
    }
}
[System.Serializable]
public class MoveFigure
{
    public float speed;
    public bool FigureIsMoved(Transform current_figure, float target)
    {
        float y_change = current_figure.localScale.y;
        y_change = Mathf.MoveTowards(y_change, target, speed * Time.deltaTime);
        current_figure.localScale = new Vector3(1, y_change, 1);
        if (current_figure.localScale.y == target)
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
    public void ChangeColor(bool open, Transform current_figure)
    {
        Image[] im = current_figure.GetComponentsInChildren<Image>();
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
