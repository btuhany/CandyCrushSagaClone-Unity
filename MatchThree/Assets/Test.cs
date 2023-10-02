using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Test : Test2
{
    [SerializeField] public float godDamn = 5f;
    private int counter;
    private void Awake()
    {
        Debug.Log("a");
        StartCoroutine(ggg());
        Debug.Log("b");
        Debug.Log("c");
        Debug.Log("d");
        Debug.Log("e");
    }
    private new void Start()
    {
        Debug.Log("start");
    }
    private void Update()
    {
        counter++;
        if (counter <= 2)
        {
            Debug.Log("ups");
        }
    }
    IEnumerator ggg()
    {
        Debug.Log("1");
        yield return null;
        Debug.Log("2");
        yield return null;
        Debug.Log("3");
    }
}
public class Test2 : MonoBehaviour
{
    private float godddDa = 4f;
    protected virtual void Start()
    {
        Debug.Log(godddDa);
    }
}
