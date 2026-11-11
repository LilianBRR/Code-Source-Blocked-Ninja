using System.Collections.Generic;
using UnityEngine;

public class CameraObstacleFader : MonoBehaviour
{
    [Header("Dependances")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private Material _transparent;

    [Header("BoxCast")]
    [SerializeField] private Vector3 _boxSize = new Vector3(1.5f, 2.5f, 1.5f); 
    private Dictionary<Renderer, Material> _activeFadedWalls = new Dictionary<Renderer, Material>();
    private HashSet<Renderer> _currentHitRenderers = new HashSet<Renderer>();

    private void Start()
    {
        if(_playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            _playerTransform = player.transform;
        }
       
    }

    void Update()
    {
        if (_playerTransform == null) return;

        Vector3 direction = _playerTransform.position - transform.position;
        float distance = direction.magnitude;
        Quaternion boxRotation = Quaternion.LookRotation(direction);

        _currentHitRenderers.Clear();

        RaycastHit[] hits = Physics.BoxCastAll(
            transform.position,
            _boxSize / 2f,
            direction.normalized,
            boxRotation,
            distance,
            _wallLayer
        );

        foreach (RaycastHit hit in hits)
        {
            Renderer wallRenderer = hit.collider.GetComponent<Renderer>();
            if (wallRenderer == null) continue;

            _currentHitRenderers.Add(wallRenderer);
            if (!_activeFadedWalls.ContainsKey(wallRenderer))
            {
                _activeFadedWalls.Add(wallRenderer, wallRenderer.material);
                wallRenderer.material = _transparent;
            }
        }

        List<Renderer> wallsToReset = new List<Renderer>();

        foreach (var pair in _activeFadedWalls)
        {
            Renderer rend = pair.Key;

            if (!_currentHitRenderers.Contains(rend))
            {
                if (rend != null)
                {
                    rend.material = pair.Value; 
                }
                wallsToReset.Add(rend);
            }
        }
        foreach (Renderer rend in wallsToReset)
        {
            _activeFadedWalls.Remove(rend);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_playerTransform == null) return;

        Gizmos.color = Color.red;
        Vector3 direction = _playerTransform.position - transform.position;
        Gizmos.DrawWireCube(transform.position + direction / 2f, _boxSize);
    }
}