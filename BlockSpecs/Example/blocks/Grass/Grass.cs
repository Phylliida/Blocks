

public class Grass : Block
{
	public override void OnTick(BlockData block)
	{
		long x = block.x;
		long y = block.y;
		long z = block.z;
		long state1 = block.state1;
		long state2 = block.state2;
		long state3 = block.state3;
		
		foreach (Block neighbor in GetNeighbors(up: true, down: true, diag: true)
		{
			if (neighbor.block == DIRT && GetBlock(neighbor.x, neighbor.y+1, neighbor.z).block == AIR)
			{
				if (rand() < 0.01f)
				{
					neighbor.block = GRASS;
				}
				else
				{
					block.needsAnotherTick = true;
				}
			}
		}
	}
}