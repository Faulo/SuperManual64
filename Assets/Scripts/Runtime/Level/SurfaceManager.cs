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

        RaycastHit[] raycastResults = new RaycastHit[16];
        [SerializeField]
        Vector3 findFloorOffset = new(0, 0.8f, 0);
        public bool TryFindFloor(Vector3 position, out SurfacePoint floor) {
            var ray = new Ray(position + findFloorOffset, Vector3.down);
            int count = Physics.RaycastNonAlloc(ray, raycastResults);
            if (count == 0) {
                floor = default;
                return false;
            }

            var result = raycastResults[0];
            for (int i = 1; i < count; i++) {
                if (result.point.y < raycastResults[i].point.y) {
                    result = raycastResults[i];
                }
            }

            floor = new(FindSurface(result.collider), result.point, result.normal);
            return true;
        }

        [SerializeField]
        Vector3 findCeilingOffset = new(0, 0.8f, 0);
        public bool TryFindCeiling(Vector3 position, out SurfacePoint ceil) {
            var ray = new Ray(position + findCeilingOffset, Vector3.up);
            int count = Physics.RaycastNonAlloc(ray, raycastResults);
            if (count == 0) {
                ceil = default;
                return false;
            }

            var result = raycastResults[0];
            for (int i = 1; i < count; i++) {
                if (result.point.y < raycastResults[i].point.y) {
                    result = raycastResults[i];
                }
            }

            ceil = new(FindSurface(result.collider), result.point, result.normal);
            return true;
        }

        bool isSetUp => testCollider && testCollider.gameObject && testCollider.gameObject.scene.isLoaded;
        SphereCollider testCollider;

        void SetUpCollider(float radius) {
            if (!isSetUp) {
                testCollider = new GameObject(nameof(testCollider)).AddComponent<SphereCollider>();
            }

            testCollider.radius = radius;
            Physics.SyncTransforms();
        }

        public SurfacePoint ResolveWallCollisions(ref Vector3 position, float offsetY, float radius) {
            var collisionData = new WallCollisionData {
                position = position,
                offsetY = offsetY,
                radius = radius
            };

            ResolveWallCollisionsInternal(collisionData);

            position = collisionData.position;

            return collisionData.wall;
        }

        [SerializeField]
        float findWallNormalY = 0.5f;

        Collider[] colliderResults = new Collider[16];
        void ResolveWallCollisionsInternal(WallCollisionData data) {
            SetUpCollider(data.radius);

            var position = data.position;
            position.y += data.offsetY;
            int count = Physics.OverlapSphereNonAlloc(position, 2 * data.radius, colliderResults);

            for (int i = 0; i < count; i++) {
                var collider = colliderResults[i];
                if (Physics.ComputePenetration(
                    testCollider, position, Quaternion.identity,
                    collider, collider.transform.position, collider.transform.rotation,
                    out var direction, out float distance)) {
                    if (direction.y < findWallNormalY) {
                        direction.y = 0;
                        var offset = direction * distance;
                        position += offset;
                        data.wall = new(FindSurface(collider), position, direction.normalized);
                    }
                }
            }

            position.y -= data.offsetY;
            data.position = position;
        }
    }
}