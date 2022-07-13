using System;
using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    public static IEnumerator WaitAndDo(this MonoBehaviour mono, Action toDo, float delay)
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(delay);
            toDo.Invoke();
        }
        IEnumerator cor = Coroutine();
        mono.StartCoroutine(cor);
        return cor;
    }

    public static IEnumerator WaitAndDoRealtime(this MonoBehaviour mono, Action toDo, float delay)
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSecondsRealtime(delay);
            toDo.Invoke();
        }
        IEnumerator cor = Coroutine();
        mono.StartCoroutine(cor);
        return cor;
    }

    public static void WaitForEndOfFrame(this MonoBehaviour mono, Action toDo)
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForEndOfFrame();
            toDo.Invoke();
        }
        mono.StartCoroutine(Coroutine());
    }
}