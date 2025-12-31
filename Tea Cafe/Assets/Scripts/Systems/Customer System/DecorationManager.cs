using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DecorationManager : MonoBehaviour
{
    public static DecorationManager Instance { get; private set; }

    [SerializeField] private List<TransformTarget> decorations;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject[] tag = GameObject.FindGameObjectsWithTag("Decoration");
        var list = new List<TransformTarget>(tag.Length);

        foreach (var dec in tag)
        {
            if (dec.TryGetComponent(out TransformTarget decoration))
            {
                list.Add(decoration);
            }
        }

        decorations = list;
    }

    public void AddDecoration(Transform decorationTransform)
    {
        if (decorationTransform == null) return;
        var tt = decorationTransform.GetComponent<TransformTarget>();
        if (tt == null) tt = decorationTransform.gameObject.AddComponent<TransformTarget>();
        if (!decorations.Contains(tt))
            decorations.Add(tt);
        Debug.Log("DecorationManager: Added decoration " + decorationTransform.name);
    }

    public void RemoveDecoration(Transform decorationTransform)
    {
        if (decorationTransform == null) return;
        var tt = decorationTransform.GetComponent<TransformTarget>();
        if (tt == null) return;
        decorations.Remove(tt);
        Debug.Log("DecorationManager: Removed decoration " + decorationTransform.name);
    }

    public List<TransformTarget> GetListOfDecorations => decorations;
}
