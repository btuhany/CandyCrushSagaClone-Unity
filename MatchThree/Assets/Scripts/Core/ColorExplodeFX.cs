using System.Collections;
using UnityEngine;

public class ColorExplodeFX : Movable
{
    public void PlayFX(Vector3 pos)
    {
        StartCoroutine(MoveAndReturnPool(pos));
    }
    private IEnumerator MoveAndReturnPool(Vector3 pos)
    {
        yield return StartCoroutine(MoveToPosition(pos));
        ColorExplodeFXPool.Instance.ReturnObject(this);
    }
}
