using Project.Assets._Project._Scripts.Interactables;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public class BlockColorChanger : MonoBehaviour
    {
        [SerializeField] private Material _red, _blue, _yellow, _green, _orange, _purple;
        public void ChangeBlockColor(Block block, UnitColor color)
        {
            block.ChangeColor(color);
            switch(color)
            {
                case UnitColor.Red:
                    block.ChangeMaterial(_red);
                    
                    break;
                case UnitColor.Blue:
                    block.ChangeMaterial(_blue);
                    break;
                case UnitColor.Yellow:
                    block.ChangeMaterial(_yellow);
                    break;
                case UnitColor.Green:
                    block.ChangeMaterial(_green);
                    break;
                case UnitColor.Orange:
                    block.ChangeMaterial(_orange);
                    break;
                case UnitColor.Purple:
                    block.ChangeMaterial(_purple);
                    break;
                default: 
                    block.ChangeMaterial(_red);
                    break;
            }
        }
    }
}
