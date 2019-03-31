using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blocks
{
    [CustomEditor(typeof(BlocksWorld))]
    public class ProfileHelper : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BlocksWorld myScript = (BlocksWorld)target;
        }
    }
}