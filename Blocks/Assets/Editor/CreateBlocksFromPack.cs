using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;




[CustomEditor(typeof(BlocksWorld))]
public class GenerateBlocks : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BlocksWorld myScript = (BlocksWorld)target;
        string text = GUILayout.TextField("../../BlockSpecs");
        if (GUILayout.Button("Build Object"))
        {
            List<Pack> packs = new List<Pack>();
            string actualPath = Application.dataPath;
            DirectoryInfo dinfo = new DirectoryInfo(actualPath);
            while (text.Substring(0,3) == "../" && dinfo.Exists && dinfo.Parent.Exists)
            {
                text = text.Substring(3);
                dinfo = dinfo.Parent;
            }

            dinfo = new DirectoryInfo(dinfo.FullName.Replace("\\", "/") + "/" + text);
            if (dinfo.Exists)
            {
                actualPath = dinfo.FullName;
            }

            Debug.Log("res = " + actualPath);
            string[] directories = Directory.GetDirectories(actualPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string directory in directories)
            {
                DirectoryInfo info = new DirectoryInfo(directory);
                if (info.Exists)
                {
                    Debug.Log("---got pack root " + info.FullName + " with name " + info.Name);
                    Pack pack = new Pack(info.FullName);
                    if (pack.packBlocks.Count > 0)
                    {
                        Debug.Log("pack " + pack.packName + " at directory " + pack.packRootDir + " has " + pack.packBlocks.Count + " blocks");
                        packs.Add(pack);
                    }
                }
            }

            Debug.Log("got a total of " + packs.Count + " packs");

            myScript.packs = packs.ToArray();
        }
    }
}
