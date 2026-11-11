using System;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;

    private void Start()
    {
        GameObject cam = GameObject.FindWithTag("MainCamera");
        if (cam == null)
            return;
        _cameraTransform = cam.transform;
    }

    void Update()
    {
        if (_cameraTransform == null) return;

        transform.rotation = _cameraTransform.rotation;
    }
}
