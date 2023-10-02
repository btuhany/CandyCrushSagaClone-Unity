using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private Vector2Int _dimensions;
    [SerializeField] private TextMeshProUGUI _gridOutput;
    private void Start()
    {
        MatchableGrid.Instance.InitializeGrid(_dimensions);
        StartCoroutine(Demo());
    }

    private IEnumerator Demo()
    {
        _gridOutput.text = MatchableGrid.Instance.ToString();
        yield return new WaitForSeconds(2);

        Matchable m1 = MatchablePool.Instance.GetObject();
        m1.gameObject.name = "a";

        Matchable m3 = MatchablePool.Instance.GetObject();
        m3.gameObject.name = "c";

        Matchable m2 = MatchablePool.Instance.GetObject();
        m2.gameObject.name = "y";

        MatchableGrid.Instance.PutItemAt(m1, 0, 0);
        MatchableGrid.Instance.PutItemAt(m2, 1, 1);
        MatchableGrid.Instance.PutItemAt(m3, 3, 3);

        _gridOutput.text = MatchableGrid.Instance.ToString();
        yield return new WaitForSeconds(3);

        MatchableGrid.Instance.PutItemAt(m3, 2, 2);

        _gridOutput.text = MatchableGrid.Instance.ToString();
    }
    //private IEnumerator GoNewPos(Movable movable)
    //{
    //    Color newColor = Random.ColorHSV();
    //    for (int i = 0; i < 2; i++)
    //    {
    //        Vector2 random = Random.insideUnitSphere;
    //        random *= Random.Range(1f, 25f);
    //        yield return StartCoroutine(movable.MoveToPosition(random));
    //        movable.GetComponent<SpriteRenderer>().color = newColor;
    //    }
    //    Movable newMovable = MovablePool.Instance.GetObject();
    //    newMovable.transform.position = movable.transform.position;
    //    newMovable.GetComponent<SpriteRenderer>().color = newColor;
    //    StartCoroutine(GoNewPos(newMovable));
    //}
}
