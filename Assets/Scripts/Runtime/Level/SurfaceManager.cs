using System;
using UnityEngine;

namespace SuperManual64.Level {
    [CreateAssetMenu]
    sealed class SurfaceManager : ScriptableObject {
        [SerializeField]
        Surface[] surfaces = Array.Empty<Surface>();
        Surface defaultSurface => surfaces[0];

        Surface FindSurface(Collider collider) {
            if (!collider) {
                return defaultSurface;
            }

            if (!collider.sharedMaterial) {
                return defaultSurface;
            }

            for (int i = 0; i < surfaces.Length; i++) {
                if (surfaces[i].material == collider.sharedMaterial) {
                    return surfaces[i];
                }
            }

            return defaultSurface;
        }

        RaycastHit[] results = new RaycastHit[16];
        [SerializeField]
        Vector3 findFloorOffset = new(0, 0.8f, 0);
        public bool TryFindFloor(Vector3 position, out SurfacePoint floor) {
            var ray = new Ray(position + findFloorOffset, Vector3.down);
            int count = Physics.RaycastNonAlloc(ray, results);
            if (count == 0) {
                floor = default;
                return false;
            }

            var result = results[0];
            for (int i = 1; i < count; i++) {
                if (result.point.y < results[i].point.y) {
                    result = results[i];
                }
            }

            floor = new(FindSurface(result.collider), result.point, result.normal);
            return true;
        }

        [SerializeField]
        Vector3 findCeilingOffset = new(0, 0.8f, 0);
        public bool TryFindCeiling(Vector3 position, out SurfacePoint ceil) {
            var ray = new Ray(position + findCeilingOffset, Vector3.up);
            int count = Physics.RaycastNonAlloc(ray, results);
            if (count == 0) {
                ceil = default;
                return false;
            }

            var result = results[0];
            for (int i = 1; i < count; i++) {
                if (result.point.y < results[i].point.y) {
                    result = results[i];
                }
            }

            ceil = new(FindSurface(result.collider), result.point, result.normal);
            return true;
        }
    }
}