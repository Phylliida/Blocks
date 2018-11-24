using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExamplePack : BlocksPack {

	// Use this for initialization
	void Awake () {
        AddCustomBlock(BlockValue.Grass, new Grass());
        AddCustomBlock(BlockValue.Clay, new Clay());
        AddCustomBlock(BlockValue.Bark, new Bark());
        SetCustomGeneration(new ExampleGeneration());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
