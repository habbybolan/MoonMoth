using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;


[RequireComponent(typeof(LightProbeGroup))]
[RequireComponent(typeof(LightProbeGroup))]
public class LightProbeVolume : MonoBehaviour
{
#if UNITY_EDITOR

    [Header("Tune These")]
    [SerializeField, Range(0.05f, 0.25f)] private float _minDensity = 0.15f;
    [SerializeField, Range(0.25f, 0.75f)] private float _maxDensity = 0.35f;
    [SerializeField, Range(0.05f, 2f)] private float _overlapCullDistance = 0.5f;
    [SerializeField] private LayerMask _collisionMask = 1;
    [SerializeField] private Bounds _bounds = new Bounds(Vector3.zero, new Vector3(20f, 20f, 20f));

    private float _occlusionTestDistance = 2f;
    private int _occlusionTestMinVisibility = 6;
    private float _surfaceOffset = 0.25f;

    public Bounds Bounds { get => _bounds; set => _bounds = value; }
    public LightProbeGroup LPG => GetComponent<LightProbeGroup>();
    public Vector3[] Positions { get => LPG.probePositions; set => LPG.probePositions = value; }

    private Vector3[] _directions =
    {
        Vector3.down,
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        Vector3.down,
        new Vector3(1f, 1f, 1f),
        new Vector3(1f, 1f, -1f),
        new Vector3(-1f, 1f, -1f),
        new Vector3(-1f, 1f, 1f),
        new Vector3(1f, -1f, 1f),
        new Vector3(1f, -1f, -1f),
        new Vector3(-1f, -1f, -1f),
        new Vector3(-1f, -1f, 1f)
    };

    public void Recalculate()
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        Debug.Log("Light Probe Volume recalculation start...");

        List<Vector3> positions = new List<Vector3>();

        Vector3 start = transform.position + Bounds.min + Vector3.one * (_minDensity * 0.5f + 0.05f);

        // first pass
        float density = _minDensity;
        for (int x = 0; x <= Bounds.size.x * density; x++)
        {
            for (int y = 0; y <= Bounds.size.y * density; y++)
            {
                for (int z = 0; z <= Bounds.size.z * density; z++)
                {
                    Vector3 position = start + new Vector3(x / density, y / density, z / density);
                    if (!TestVisible(position, _occlusionTestMinVisibility)) continue;
                    positions.Add(position);
                }
            }
        }

        // second pass
        density = _maxDensity;
        for (int x = 0; x <= Bounds.size.x * density; x++)
        {
            for (int y = 0; y <= Bounds.size.y * density; y++)
            {
                for (int z = 0; z <= Bounds.size.z * density; z++)
                {
                    Vector3 position = start + new Vector3(x / density, y / density, z / density);
                    if (!TestVisible(position, _occlusionTestMinVisibility)) continue;
                    if (Physics.OverlapSphere(position, 1f / density, _collisionMask).Length >= 1 && 
                        GetSurfaceOffsetPosition(position, 1f / density, _surfaceOffset, out Vector3 surfacePosition) &&
                        !TestOverlap(surfacePosition, positions, _overlapCullDistance))
                    {
                        positions.Add(surfacePosition);
                    }
                }
            }
        }

        // TODO: add surface optimizations

        Positions = positions.ToArray();

        timer.Stop();
        Debug.Log($"... completed in {timer.Elapsed.TotalMilliseconds}");
    }

    private bool TestVisible(Vector3 position, int minVisibility = 0)
    {
        int visibleCount = 0;
        foreach (Vector3 dir in _directions)
        {
            Vector3 start = position + dir * _occlusionTestDistance;
            if (!Physics.Linecast(start, position, _collisionMask)) visibleCount++;
        }

        return visibleCount > minVisibility;
    }

    private bool GetSurfaceOffsetPosition(Vector3 position, float distance, float offset, out Vector3 offsetPosition)
    {
        foreach (Vector3 dir in _directions)
        {
            if(Physics.Raycast(position, dir, out RaycastHit hit, distance, _collisionMask))
            {
                offsetPosition = hit.point + hit.normal * offset;
                return true;
            }
        }

        offsetPosition = position;
        return false;
    }

    private bool TestOverlap(Vector3 position, List<Vector3> list, float distance)
    {
        foreach (Vector3 pos in list)
        {
            if (Vector3.Distance(pos, position) < distance) return true;
        }

        return false;
    }

#endif
}