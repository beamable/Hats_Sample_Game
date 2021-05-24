using UnityEngine;

namespace VIS.StickyNotes.Examples
{
    //[CreateAssetMenu(fileName = nameof(ExampleScriptableObject), menuName = "test/test", order = 0)]
    public class ExampleScriptableObject : ScriptableObject
    {
#pragma warning disable
        [SerializeField]
        private int SomeInt = 14;
        [SerializeField]
        private string SomeString = "Lorem ipsum....";
        [SerializeField]
        private bool SomeBool = true;
        [SerializeField, Range(0f, 100f)]
        private float SomeFloat = 56f;
#pragma warning enable

    }
}
