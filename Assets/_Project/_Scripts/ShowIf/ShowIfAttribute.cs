using UnityEngine;

namespace Project.Assets._Project._Scripts.ShowIf
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionalSourceField;
        public object CompareValue;

        public ShowIfAttribute(string conditionalSourceField, object compareValue)
        {
            ConditionalSourceField = conditionalSourceField;
            CompareValue = compareValue;
        }
    }
}
