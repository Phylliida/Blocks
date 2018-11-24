using UnityEngine;

public class ExampleChunkData : ChunkData
{
	public override OnGenerateChunk()
	{
		float minVal = 0.0f;
		float maxVal = 30.0f;
		AddChunkProperty("elevation", minVal, maxVal);
	}
}
public class ExampleGeneration : GenerationClass
{
	
	public override OnGenerateBlock(long x, long y, long z, Block outBlock)
	{
		float elevation = GetChunkProperty(x,y,z,"elevation");
		if (y <= 0)
		{
			outBlock.block = STONE;
		}
		else if(y >= elevation)
		{
			outBlock.block = AIR;
		}
		else
		{
			long elevationL = (long)Mathf.Round(elevation);
			long distFromSurface = elevation - y;
			if (distFromSurface == 1)
			{
				outBlock.block = GRASS;
			}
			else
			{
				outBlock.block = DIRT;
			}
		}
	}
}