using KBCore.Refs;
using Unity.Cinemachine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace Project.Assets._Project._Scripts.GridComponents
{
    [RequireComponent(typeof(Grid))]
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int _size;
        [SerializeField] private Vector2 _gap;
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private Vector3 _cameraOffset;
        [SerializeField] private float _orthographicSizeOffset = 2f;
        [SerializeField] private Transform _gridParent;
        [SerializeField] private int _gridHeight = 1;
        [SerializeField, Self] private Grid _grid;
        [SerializeField, Scene] private Camera _camera;
        [SerializeField, Scene] private CinemachineCamera _cinemachineCam;

        [SerializeField] private Transform _cameraPositionTarget;
        private float _cameraSizeTarget;
        private Bounds _bounds;
#if UNITY_EDITOR
        [ContextMenu("Generate Grid")]
        private void Generate()
        {
            ClearChildren();
            _grid.cellGap = new Vector3(_gap.x, 0, _gap.y);
            var bounds = new Bounds();
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    var coord = new Vector3Int(x, _gridHeight, y);
                    var position = _grid.GetCellCenterWorld(coord);
                    var spawned = Instantiate(_tilePrefab, position, _tilePrefab.transform.rotation, _gridParent);
                    //spawned.Init(coord);

                    var gridPos = new Vector2Int(x, y);
                    bounds.Encapsulate(position);
                }
            }
            SetCamera(bounds);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        [ContextMenu("Clear Children")]
        private void ClearChildren()
        {
            for (int i = _gridParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_gridParent.GetChild(i).gameObject);
            }
            EditorSceneManager.MarkSceneDirty(gameObject.scene);

        }

        [ContextMenu("Update Camera")]
#endif
        public void UpdateCamera()
        {
            var bounds = new Bounds();
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    var coord = new Vector3Int(x, y);
                    var position = _grid.GetCellCenterWorld(coord);
                    bounds.Encapsulate(position);
                }
            }
            SetCamera(bounds);
#if UNITY_EDITOR
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        private void SetCamera(Bounds bounds)
        {

            bounds.Expand(2);
            Vector3 gridCenter = transform.position + 0.5f * new Vector3(_size.x * (_grid.cellSize.x + _gap.x), _size.y * (_grid.cellSize.y + _gap.y), 0);
            var vertical = bounds.size.y;
            var horizontal = bounds.size.x * _camera.pixelHeight / _camera.pixelWidth;
            _cameraPositionTarget.position = gridCenter + _cameraOffset;
            _cameraSizeTarget = Mathf.Max(horizontal, vertical) * 0.5f;
            _cinemachineCam.Follow = _cameraPositionTarget;
            if (_camera.orthographic)
            {
                _cinemachineCam.Lens.OrthographicSize = _cameraSizeTarget + _orthographicSizeOffset;
            }
            else
            {
                float distance = 10f;

                _cinemachineCam.Lens.FieldOfView = 2f * Mathf.Atan(_cameraSizeTarget / distance) * Mathf.Rad2Deg;
            }
        }

    }
}
