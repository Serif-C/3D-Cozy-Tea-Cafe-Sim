using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DecorationManager : MonoBehaviour
{
    /*
     * There are a bunch of decorations that can be placed around the cafe to improve atmosphere.
     * 
     * Each decoration is a prefab with a child containing a TransformTarget component 
     * where the customer can go to, to admire the decoration.
     * 
     * 
     */
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

    internal void AddDecoration(Transform decorationTransform)
    {
        if (decorationTransform == null) return;
        var tt = decorationTransform.GetComponent<TransformTarget>();
        if (tt == null) tt = decorationTransform.gameObject.AddComponent<TransformTarget>();
    }

    internal void RemoveDecoration(Transform decorationTransform)
    {
        if (decorationTransform == null) return;
        var tt = decorationTransform.GetComponent<TransformTarget>();
        if (tt == null) return;
    }
}
