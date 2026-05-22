using KBCore.Refs;
using UnityEngine;

namespace Project.Assets._Project._Scripts.GridComponents
{
    public enum ExitDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public class ExitPoint : ValidatedMonoBehaviour
    {
        [SerializeField] private ExitDirection _direction;
        [SerializeField] private bool _useColorRestriction = false;
        [SerializeField] private UnitColor _requiredColor;
        [SerializeField, Self] private BoxCollider _collider;

        public ExitDirection Direction => _direction;
        public bool UseColorRestriction => _useColorRestriction;
        public UnitColor RequiredColor => _requiredColor;

        public bool CanExit(Bounds blockBounds, UnitColor blockColor)
        {
            if (_useColorRestriction && blockColor != _requiredColor)
            {
                //print("Wrong Color");
                return false;
            }

            Bounds exitBounds = _collider.bounds;

            switch (_direction)
            {
                case ExitDirection.Up:
                    return blockBounds.min.x >= exitBounds.min.x - 0.1f &&
                           blockBounds.max.x <= exitBounds.max.x + 0.1f;
                case ExitDirection.Down:
                    return blockBounds.min.x >= exitBounds.min.x - 0.1f &&
                           blockBounds.max.x <= exitBounds.max.x + 0.1f;
                case ExitDirection.Left:
                    return blockBounds.min.z >= exitBounds.min.z - 0.1f &&
                           blockBounds.max.z <= exitBounds.max.z + 0.1f;
                case ExitDirection.Right:
                    return blockBounds.min.z >= exitBounds.min.z - 0.1f &&
                           blockBounds.max.z <= exitBounds.max.z + 0.1f;
                default:
                    break;
            }
            print("Wrong Bounds");
            return false;
        }

        public Vector3 GetExitVector()
        {
            return _direction switch
            {
                ExitDirection.Up => Vector3.forward,
                ExitDirection.Down => Vector3.back,
                ExitDirection.Left => Vector3.left,
                ExitDirection.Right => Vector3.right,
                _ => Vector3.zero
            };
        }
    }
}
