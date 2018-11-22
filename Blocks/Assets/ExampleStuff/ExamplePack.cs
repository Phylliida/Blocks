using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExamplePack : BlocksPack {

	// Use this for initialization
	void Awake () {
        AddCustomBlock(BlockValue.GRASS, new Grass());
        AddCustomBlock(BlockValue.CLAY, new Clay());
        AddCustomBlock(BlockValue.BARK, new Bark());
        SetCustomGeneration(new ExampleGeneration());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
