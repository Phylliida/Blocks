using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blocks;
using ExtensionMethods;
using Blocks.ExtensionMethods;

public class Light : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Light, positionOfBlock);
        destroyBlock = true;
    }

    public override int ConstantLightEmitted()
    {
        return 15;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Light;
    }


    public override void OnTick(BlockData block)
    {
        block.lightProduced = 15;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}
public class WaterSource : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Sand, positionOfBlock);
        destroyBlock = true;
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Sand;
    }

    public override void OnTick(BlockData block)
    {
        using (BlockData below = GetBlockDataNotRelative(block.x, block.y - 1, block.z))
        {
            if (below.block == Example.Air)
            {
                below.block = Example.Water;
                below.SetPressureIn(0);
                below.SetPressureOut(0);
                below.SetWaterAmount(1);
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}



namespace ExtensionMethods
{
    public static class WaterExampleExtensions
    {

        public static short GetWaterAmount(this BlockData block)
        {
            return block.animationState;
        }

        public static void SetWaterAmount(this BlockData block, short value)
        {
            block.animationState = value;
        }


        /// <summary>
        /// Warning: this converts to a short before assigning, this method is just provided so you don't have to litter the code with casts since c# will return an int when you subtract two shorts
        /// </summary>
        /// <param name="block"></param>
        /// <param name="value"></param>
        public static void SetWaterAmount(this BlockData block, int value)
        {
            block.animationState = (short)value;
        }

        public static short GetPressureIn(this BlockData block)
        {
            short pressureIn, pressureOut;
            PhysicsUtils.UnpackValuesFromInt(block.state, out pressureIn, out pressureOut);
            return pressureIn;
        }

        public static short GetPressureOut(this BlockData block)
        {
            short pressureIn, pressureOut;
            PhysicsUtils.UnpackValuesFromInt(block.state, out pressureIn, out pressureOut);
            return pressureOut;
        }

        public static void SetPressureIn(this BlockData block, short pressureIn)
        {
            short pressureOut = block.GetPressureOut();
            block.state = PhysicsUtils.PackTwoValuesIntoInt(pressureIn, pressureOut);
        }

        public static void SetPressureOut(this BlockData block, short pressureOut)
        {
            short pressureIn = block.GetPressureIn();
            block.state = PhysicsUtils.PackTwoValuesIntoInt(pressureIn, pressureOut);
        }
    }
}


public class WaterNewWithPressure : Block
{

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Water;
    }


    public short tickId = -1;
    public override void OnTickStart()
    {
        tickId += 1;
    }

    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }

    public static short MaxWater = 1;

    public float HowFull(int numWater)
    {
        return numWater / 4.0f;
    }

    public int NumCanAddTo(int myAmount, int theirAmount)
    {
        int maxCanAdd = MaxWater - theirAmount;
        return System.Math.Min(maxCanAdd, myAmount);
    }

    public void FlowMaxWaterPossibleInto(BlockData from, BlockData to)
    {
        FlowWaterInto(from, to, NumCanAddTo(from.animationState, to.animationState));
    }

    public void FlowCompletelyIntoAir(BlockData from, BlockData to)
    {
        SetAirToWater(to);
        FlowMaxWaterPossibleInto(from, to);
        int numWater = from.GetWaterAmount();
        if (numWater == 0)
        {
            SetWaterToAir(from);
            return;
        }
        else
        {
            Debug.LogWarning("warning: flowing to air did not completely empty us out? We still have " + numWater + " water left");
        }
    }

    public void FlowWaterInto(BlockData from, BlockData to, int amount)
    {
        from.SetWaterAmount(from.GetWaterAmount() - amount);
        to.SetWaterAmount(to.GetWaterAmount() + amount);
    }


    public int NumWaterNeighbors(BlockData block)
    {
        int numWaterNeighbors = 0;
        foreach (BlockData neighbor in GetNeighbors(block))
        {
            if (neighbor.block == Example.Water)
            {
                numWaterNeighbors += 1;
            }
        }
        return numWaterNeighbors;
    }

    public void RecomputePressureIn(BlockData block)
    {
        int totalPressureIn = 0;
        foreach (BlockData neighbor in GetNeighbors(block))
        {
            if (neighbor.block == Example.Water)
            {
                int neighborContribution = neighbor.GetPressureIn() / NumWaterNeighbors(neighbor);

                // blocks above us give us 12 pressure
                if (neighbor.y == block.y + 1)
                {
                    totalPressureIn += neighbor.GetWaterAmount() + neighborContribution;
                }
                // blocks below us can send us pressure but it uses some up
                else if(neighbor.y == block.y - 1)
                {
                    totalPressureIn += System.Math.Max(0, neighborContribution - 1);
                }
                else
                {
                    totalPressureIn += neighborContribution; 
                }
            }
        }
        block.SetPressureIn((short)totalPressureIn);
    }

    /// <summary>
    /// Note that this will set the block's value to Water and set pressure in, pressure out, and water amount to 0
    /// </summary>
    /// <param name="block"></param>
    public void SetAirToWater(BlockData block)
    {
        block.block = Example.Water;
        block.SetPressureIn(0);  // default initial values
        block.SetPressureOut(0);
        block.SetWaterAmount(0);
    }
    /// <summary>
    /// Note that this will set the block's pressure in, pressure out, and water amount to 0, then set block to air
    /// </summary>
    /// <param name="block"></param>
    public void SetWaterToAir(BlockData block)
    {
        block.SetPressureIn(0); // default initial values, just in case
        block.SetPressureOut(0);
        block.SetWaterAmount(0);
        block.block = Example.Air;
    }

    public override void OnTick(BlockData block)
    {
        // I know I'm things from ints to shorts haphazardly around here, but I don't really care that much since the values should never get that high (I think? I should check in the case of a very large tank)







        int numWater = block.GetWaterAmount();



        if (numWater == 0)
        {
            SetWaterToAir(block);
            block.needsAnotherTick = false;
            return;
        }

        // flow into below if possible
        using (BlockData below = GetBlockDataRelative(block, 0, -1, 0))
        {
            if (below.block == Example.Water && below.GetPressureIn() < 1)
            {
                FlowMaxWaterPossibleInto(block, below);
            }
            else if (below.block == Example.Air)
            {
                SetAirToWater(below);
                FlowMaxWaterPossibleInto(block, below);
            }
            // get updated water amount after flowing down
            numWater = block.GetWaterAmount();
        }


        if (numWater == 0)
        {
            SetWaterToAir(block);
            block.needsAnotherTick = false;
            return;
        }

        int oldPressureIn = block.GetPressureIn();
        RecomputePressureIn(block);

        int pressureIn = block.GetPressureIn();

        if (pressureIn != oldPressureIn)
        {
            world.AddBlockUpdateToNeighbors(block.x, block.y, block.z);
        }

        // if we have pressure, move to horizontal neighbor if we can (we already tried moving below us above, so this is the second best option we have)
        if (pressureIn >= 1)
        {
            foreach (BlockData horizontalNeighbor in GetHorizontalNeighbors(block))
            {
                // flow to neighbor if air
                if (horizontalNeighbor.block == Example.Air)
                {
                    FlowCompletelyIntoAir(block, horizontalNeighbor);
                    block.needsAnotherTick = false;
                    return;
                }
            }

            using (BlockData above = GetBlockDataRelative(block, 0, 1, 0))
            {
                if (above.block == BlockValue.Air || (above.block == Example.Water && above.GetWaterAmount() < MaxWater))
                {
                    if (above.block == BlockValue.Air)
                    {
                        SetAirToWater(above);
                    }
                    FlowWaterInto(block, above, 1);
                    int newWaterAmount = block.GetWaterAmount();
                    if (newWaterAmount == 0)
                    {
                        SetWaterToAir(block);
                        block.needsAnotherTick = false;
                        return;
                    }
                }
            }
        }
        block.needsAnotherTick = true;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.0f;
    }
}

public class Lava : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Lava;
    }

    public override int ConstantLightEmitted()
    {
        return 15;
    }

    public override void OnTick(BlockData block)
    {
        block.lightProduced = 15;
        /*
        using (BlockData below = GetBlockData(block.x, block.y - 1, block.z))
        {
            if (below.block == Example.Water || below.block == Example.WaterNoFlow)
            {
                below.block = Example.Water;
                block.state = 1 - block.state;
                below.state = block.state;
            }
            else if (below.block == Example.Air)
            {
                below.block = Example.Water;
            }
        }
        */
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.0f;
    }
}



public class BallTrack : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.BallTrack, positionOfBlock);
        destroyBlock = true;
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.BallTrack;
    }

    public override void OnTick(BlockData block)
    {
        int onEnterNextOne = 16;
        block.needsAnotherTick = true;
        int state3 = block.animationState;
        if (state3 == onEnterNextOne)
        {
            using (BlockData blockBelow = GetBlockDataRelative(block, 0, -1, 0))
            {
                if (blockBelow.block == Example.BallTrackEmpty)
                {
                    blockBelow.block = Example.BallTrack;
                    blockBelow.animationState = 1;
                }
                else
                {
                    block.needsAnotherTick = true;
                    return;
                }
            }
        }
        if (state3 >= 23)
        {
            block.needsAnotherTick = false;
            block.block = Example.BallTrackEmpty;
            block.animationState = 0;
        }
        else
        {
            block.animationState = (short)((block.animationState + 1) % 24);
        }

    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}


public class BallTrackHorizontalFull : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        //CreateBlockEntity(Example.BallTrackHorizontal, positionOfBlock);
        destroyBlock = true;
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.BallTrackZFull;
    }



    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        int ballWidth = 4;
        int trackSize = 16;

        // ball goes from [state, state+4]
        // if state == 12, ball it at edge (track is 16 wide)
        // if state == 0, ball is at other edge
        // thus, when going negative:

        //Debug.Log(block.animationState + " " + block.state + " " + (int)block.rotation);
        if (block.state > 0)
        {

            // when going positive
            //   [0                   < state <= trackSize-ballWidth]: moving like normal
            //   [trackSize-ballWidth < state <= trackSize]:  only move more if we can move into neighboring one, also animate us moving into there. Otherwise, reverse direction 
            //   [trackSize           < state]: we are no longer in this, move to neighbor


            block.animationState += 1;

            bool moveLikeNormal = 0 < block.animationState && block.animationState <= trackSize - ballWidth;
            bool movingIntoNext = trackSize - ballWidth < block.animationState && block.animationState <= trackSize;
            bool leftThingPos = trackSize < block.animationState;

            if (moveLikeNormal)
            {
            }
            else if (movingIntoNext || leftThingPos)
            {
                using (BlockData neighbor = GetBlockDataRelative(block, 0, 0, 1))
                {
                    bool needToInvertVelUponEntry;
                    bool canFlowIntoNeighbor = BallTrackUtils.CanFlowInto(block, neighbor, out needToInvertVelUponEntry);

                    // we can't flow into neighbor, flip directions
                    if (!canFlowIntoNeighbor)
                    {
                        block.animationState = (short)(trackSize - ballWidth - 1);
                        //Debug.Log("got to end 1, swapping");
                        block.state = -block.state;
                    }
                    // we can flow into neighbor, continue going
                    else
                    {
                        if (movingIntoNext)
                        {
                        }
                        else if (leftThingPos)
                        {
                            // flipped around
                            if (needToInvertVelUponEntry)
                            {
                                neighbor.animationState = (short)(trackSize - ballWidth);
                                neighbor.state = -block.state;
                            }
                            // not flipped around
                            else
                            {
                                neighbor.animationState = 0;
                                neighbor.state = block.state;
                            }
                            BallTrackUtils.FillBlock(neighbor);
                            BallTrackUtils.EmptyBlock(block);
                            block.animationState = 0;
                            block.needsAnotherTick = false;
                        }
                    }
                }
            }
            // none of the above, undo any changes
            else
            {
                block.animationState -= 1;
            }
        }
        else if (block.state < 0)
        {
            block.animationState -= 1;

            //   [0          <= state < trackSize-ballWidth]: moving like normal
            //   [-ballWidth <= state < 0]: only move more if we can move into neighboring one, also animate us moving into there. Otherwise, reverse direction
            //   [             state < -ballWidth]: we are no longer in this, move to neighbor
            bool moveLikeNormal = 0 <= block.animationState && block.animationState < trackSize - ballWidth;
            bool movingIntoNext = -ballWidth <= block.animationState && block.animationState < 0;
            bool leftThingPos = block.animationState < -ballWidth;

            //Debug.Log(moveLikeNormal + " " + movingIntoNext + " " + leftThingPos + " " + block.animationState);
            if (moveLikeNormal)
            {
            }
            else if (movingIntoNext || leftThingPos)
            {
                using (BlockData neighbor = GetBlockDataRelative(block, 0, 0, -1))
                {
                    bool needToInvertVelUponEntry;
                    bool canFlowIntoNeighbor = BallTrackUtils.CanFlowInto(block, neighbor, out needToInvertVelUponEntry);

                    // we can't flow into neighbor, flip directions
                    if (!canFlowIntoNeighbor)
                    {
                        block.animationState = 0;
                        block.state = -block.state;
                        //Debug.Log("got to end 2, swapping");
                    }
                    // we can flow into neighbor, continue going
                    else
                    {
                        if (movingIntoNext)
                        {
                        }
                        else if (leftThingPos)
                        {

                            // flipped around
                            if (needToInvertVelUponEntry)
                            {
                                neighbor.animationState = 0;
                                neighbor.state = -block.state;
                            }
                            // not flipped around
                            else
                            {
                                neighbor.animationState = (short)(trackSize - ballWidth);
                                neighbor.state = block.state;
                            }
                            BallTrackUtils.FillBlock(neighbor);
                            BallTrackUtils.EmptyBlock(block);
                            block.animationState = 0;
                            block.needsAnotherTick = false;
                        }
                    }
                }
            }
            // none of the above, undo any changes
            else
            {
                block.animationState += 1;
            }
        }
        else
        {
            block.state = 1;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}


public class AutoMiner : Block
{
    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        using (BlockData blockBelow = GetBlockDataNotRelative(block.pos.x, block.pos.y - 1, block.pos.z))
        {
            if (blockBelow.block == Example.IronOre)
            {
                block.state += 1;

                if (block.state > 30)
                {
                    using (BlockData blockGoingInto = GetBlockDataRelative(block, 0, 0, 1))
                    {
                        if (blockGoingInto.block == Example.ConveyerBelt)
                        {
                            if(ConveyerBelt.TryAddItem(blockGoingInto, Example.IronOre, block.pos.BlockCentertoUnityVector3()))
                            {
                                block.state = 0;
                            }
                        }
                    }
                }
            }
        }
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.AutoMiner;
    }
}

public class ConveyerBeltData
{
    public long whenLastUpdated = -1;
    public LVector3 pos;
    public Inventory inventory;
    public ConveyerBeltData(LVector3 pos)
    {
        if (World.mainWorld.BlockHasInventory(pos, out inventory))
        {

        }
        else
        {
            Debug.LogWarning("Warning: this conveyer belt at pos " + pos + " does not have an inventory, something went wrong");
        }
        this.pos = pos;
    }

    public void Tick()
    {

    }

    public BlockTileEntity[,] tileEntities = new BlockTileEntity[2,4];
}


public class ConveyerBelt : Block
{
    public override int InventorySpace()
    {
        return 8;
    }

    public static int subTick = 0;
    public static int numSubticks = 20;
    public static long currentTick = -1;
    public override void OnTickStart()
    {
        currentTick += 1;
        if (datas == null)
        {
            datas = new Dictionary<LVector3, ConveyerBeltData>();
        }
        subTick = (subTick + 1) % numSubticks;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        if (!datas.ContainsKey(pos))
        {
            datas[pos] = new ConveyerBeltData(pos);
        }
        return Example.ConveyerBelt;
    }


    BlockData GetBlockGoingInto(BlockData block)
    {
        return GetBlockDataRelative(block, 0, 0, 1);
    }

    public override void OnTick(BlockData block)
    {
        if (!datas.ContainsKey(block.pos))
        {
            datas[block.pos] = new ConveyerBeltData(block.pos);
        }

        block.needsAnotherTick = true;
        if (datas.ContainsKey(block.pos))
        {
            ConveyerBeltData myData = datas[block.pos];

            // only update if we haven't been updated yet (because we will automatically call tick on the things we depend on so they execute before us)
            if (myData.whenLastUpdated < currentTick)
            {
                //Debug.Log("calling OnTick on conveyer belt " + block.pos + " with currentTick " + currentTick);
                // set this before callilng tick on our dependencies, this ensures that if they call us we won't go into an infinite loop
                myData.whenLastUpdated = currentTick;

                // update the conveyer belts we are going into first
                using (BlockData blockGoingInto = GetBlockGoingInto(block))
                {
                    if(blockGoingInto.block == Example.ConveyerBelt)
                    {
                        //Debug.Log("calling OnTick on dependent conveyer belt " + blockGoingInto.pos);
                        OnTick(blockGoingInto);
                    }
                }


                // move along conveyer belt if we are at the last subtick before the cycle
                if (subTick == 0)
                {
                    //Debug.Log("doing move along");
                    MoveUp(block);
                }

                // update positions of tile entities (move them along the conveyer)
                for (int x = 0; x < 2; x++)
                {
                    for (int z = 0; z < 4; z++)
                    {
                        if (myData.tileEntities[x,z] != null)
                        {
                            Vector3 posToMoveTo = TileEntityPosToWorldPos(block, x, z);
                            subTick += 1;
                            Vector3 nextPosToMoveTo = TileEntityPosToWorldPos(block, x, z);
                            subTick -= 1;
                            // don't move it backwards along the conveyer belt, only forwards
                            if (Vector3.Distance(myData.tileEntities[x, z].transform.position, posToMoveTo) < Vector3.Distance(myData.tileEntities[x, z].transform.position, nextPosToMoveTo))
                            {
                                myData.tileEntities[x, z].transform.position = posToMoveTo;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Assumes that you already called Tick (which then calls MoveUp) on any conveyer belts that this will flow into (otherwise you'll get gaps since it'll have to wait)
    /// </summary>
    /// <param name="block"></param>
    public void MoveUp(BlockData block)
    {
        ConveyerBeltData myData = GetConveyerBeltData(block);
        using (BlockData blockMoveInto = GetBlockGoingInto(block))
        {
            ConveyerBeltData blockMoveIntoData = null;
            bool goingIntoConveyerBelt = false;
            if (blockMoveInto.block == Example.ConveyerBelt)
            {
                goingIntoConveyerBelt = true;
                blockMoveIntoData = GetConveyerBeltData(blockMoveInto);
            }
            for (int z = 3; z >= 0; z--)
            {
                for (int x = 0; x < 2; x++)
                {
                    if (myData.tileEntities[x, z] != null)
                    {
                        ConveyerBeltData dataMoveInto;
                        BlockData blockDataMoveInto;
                        int moveToX = x;
                        int moveToZ = z + 1;
                        // move to next conveyer (if available)
                        if (z == 3)
                        {
                            if (goingIntoConveyerBelt)
                            {
                                BlockData.BlockRotation relativeRotation = block.GetRelativeRotationOf(blockMoveInto);

                                // same rotation as us, just go into it
                                if (relativeRotation == BlockData.BlockRotation.Degrees0)
                                {
                                    moveToX = x;
                                    moveToZ = 0;
                                }
                                // 90 degrees angle, we go into x = 1
                                else if(relativeRotation == BlockData.BlockRotation.Degrees90)
                                {
                                    moveToX = 1;
                                    if (x == 0)
                                    {
                                        moveToZ = 1;
                                    }
                                    else
                                    {
                                        moveToZ = 2;
                                    }
                                }
                                // going in opposite direction as us, we can't move
                                else if(relativeRotation == BlockData.BlockRotation.Degrees180)
                                {
                                    continue;
                                }
                                // 270 degrees angle, we go into x = 0
                                else if (relativeRotation == BlockData.BlockRotation.Degrees270)
                                {
                                    moveToX = 0;
                                    if (x == 0)
                                    {
                                        moveToZ = 2;
                                    }
                                    else
                                    {
                                        moveToZ = 1;
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("unknown rotation of " + relativeRotation);
                                }
                            }
                            else
                            {
                                continue;
                            }

                            dataMoveInto = blockMoveIntoData;
                            blockDataMoveInto = blockMoveInto;
                        }
                        // move along conveyer (if available)
                        else
                        {
                            dataMoveInto = myData;
                            blockDataMoveInto = block;
                        }


                        int moveToInventoryI = TileEntityPosToInventoryPos(moveToX, moveToZ);

                        int inventoryI = TileEntityPosToInventoryPos(x, z);
                        BlockValue blockValue = myData.inventory.blocks[inventoryI].Block;
                        // if spot we want to move to is empty
                        // dataMoveInto != null will be true if z == 3 and the block we are moving into is not a conveyer belt
                        if (dataMoveInto.tileEntities[moveToX, moveToZ] == null)
                        {
                            // move tile entity and block stack to there 
                            dataMoveInto.tileEntities[moveToX, moveToZ] = myData.tileEntities[x, z];
                            dataMoveInto.inventory.blocks[moveToInventoryI] = myData.inventory.blocks[inventoryI];
                            // update tile entity position
                            dataMoveInto.tileEntities[moveToX, moveToZ].transform.position = TileEntityPosToWorldPos(blockDataMoveInto, moveToX, moveToZ);

                            // clear tile entity and block stack in current spot (since they have been moved to the moveTo spot) 
                            myData.inventory.blocks[inventoryI] = null;
                            myData.tileEntities[x, z] = null;
                        }
                    }
                }
            }
        }  
    }

    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        // clean up tile entities
        if (datas.ContainsKey(block.pos))
        {
            ConveyerBeltData myData = datas[block.pos];
            for (int z = 3; z >= 0; z--)
            {
                for (int x = 0; x < 2; x++)
                {
                    if (myData.tileEntities[x, z] != null)
                    {
                        GameObject.Destroy(myData.tileEntities[x, z].gameObject);
                        myData.tileEntities[x, z] = null;
                    }
                }
            }
        }

        // turn stuff in the conveyer belt into block entities (empty out the inventory)
        Inventory myInventory;
        if (world.BlockHasInventory(block.pos, out myInventory))
        {
            myInventory.ThrowAllBlocks(block.pos.BlockCentertoUnityVector3());
        }

        // create the conveyer belt entity we just mined
        CreateBlockEntity(Example.ConveyerBelt, positionOfBlock);

        destroyBlock = true;
    }


    static Dictionary<LVector3, ConveyerBeltData> datas;


    public static Vector3 TileEntityPosToWorldPos(BlockData block, int tileEntityPosX, int tileEntityPosZ)
    {
        int xOffset = tileEntityPosX;
        int zOffset = tileEntityPosZ;

        float minX = 0.25f;
        float maxX = 0.75f;


        float xOff = (xOffset) * (maxX - minX) + minX;


        Quaternion rotation = Quaternion.Euler(0, -PhysicsUtils.RotationToDegrees(block.rotation), 0);

        Vector3 localPos = new Vector3(xOff, 0.12f, zOffset / 4.0f + subTick / (4.0f * numSubticks));

        localPos = (rotation * (localPos - Vector3.one * 0.5f)) + Vector3.one * 0.5f;
        return block.pos.LocalToWorldPos(localPos);
    }


    public static int TileEntityPosToInventoryPos(int x, int z)
    {
        return x * 4 + z;
    }

    public static void InventoryPosToTileEntityPos(int i, out int x, out int z)
    {
        x = i / 4;
        z = i % 4;
    }


    public static ConveyerBeltData GetConveyerBeltData(BlockData block)
    {
        if (!datas.ContainsKey(block.pos))
        {
            datas[block.pos] = new ConveyerBeltData(block.pos);
        }
        return datas[block.pos];
    }

    static bool TryAddItem(BlockData block, BlockValue itemAdding, int posX, int posZ)
    {
        ConveyerBeltData myData = GetConveyerBeltData(block);

        if (myData.tileEntities[posX, posZ] == null)
        {
            int inventoryPos = TileEntityPosToInventoryPos(posX, posZ);
            BlockStack myBlockStack = new BlockStack(itemAdding, 1);
            BlockTileEntity tileEntity = World.mainWorld.CreateTileEntity(myBlockStack, TileEntityPosToWorldPos(block, posX, posZ));
            myData.tileEntities[posX, posZ] = tileEntity;
            myData.inventory.blocks[inventoryPos] = myBlockStack;
            return true;
        }
        return false;
    }

    public static bool TryAddItem(BlockData block, BlockValue itemAdding, Vector3 itemPos)
    {
        int closestX = -1;
        int closestZ = -1;
        float closestDist = float.MaxValue;
        ConveyerBeltData myData = GetConveyerBeltData(block);
        for (int i = 0; i < myData.inventory.blocks.Length; i++)
        {
            int x, z;
            InventoryPosToTileEntityPos(i, out x, out z);

            if (myData.tileEntities[x, z] == null)
            {
                Vector3 tileEntityPos = TileEntityPosToWorldPos(block, x, z);


                float dist = Vector3.Distance(itemPos, tileEntityPos);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestX = x;
                    closestZ = z;
                }
            }
        }
        // found the closest pos to stick it, (try to, it'll success but this just prevents us from having to duplicate code to minimize bugs) stick it there
        if (closestDist != float.MaxValue)
        {
            return TryAddItem(block, itemAdding, closestX, closestZ);
        }
        return false;
    }

    public override void BlockStackAbove(BlockData block, BlockStack blockStack, Vector3 blockStackPos, out BlockStack consumedBlockStack)
    {
        consumedBlockStack = blockStack.Copy();
        if (blockStack != null && blockStack.count > 0 && blockStack.block != Example.Air)
        {
            //Debug.Log("got block stack above with " + blockStack.count + " " + BlockValue.IdToBlockName(blockStack.block));
            bool inserted = true;
            while (inserted)
            {
                inserted = false;
                if (consumedBlockStack.count > 0)
                {
                    if(TryAddItem(block, blockStack.Block, blockStackPos))
                    {
                        //Debug.Log("nomming some block stack above with " + blockStack.count + " " + BlockValue.IdToBlockName(blockStack.block));
                        consumedBlockStack.count -= 1;
                        inserted = true;
                    }
                }
            }
        }
    }
}

public class RedstoneTorch : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        //CreateBlockEntity(Example.RedstoneTorch, positionOfBlock);
        destroyBlock = true;
    }

    public BlockData GetBlockAttachedTo(BlockData block)
    {
        if (block.block == Example.RedstoneTorchOnSide)
        {
            return GetBlockDataRelative(block, -1, 0, 0);
        }
        else
        {
            return GetBlockDataRelative(block, 0, -1, 0);
        }
    }

    public override void OnTick(BlockData block)
    {
        bool activated = true;
        using (BlockData attachedTo = GetBlockAttachedTo(block))
        {
            if (RedstoneUtil.GetMaxPowerIntoBlock(attachedTo) > 0)
            {
                activated = false;
            }
        }

        RedstonePower myPower;
        if (activated)
        {
            myPower = new RedstonePower(15);
            myPower.SetAllPower(15);
            using (BlockData attachedTo = GetBlockAttachedTo(block))
            {
                long offsetX = attachedTo.x - block.x;
                long offsetY = attachedTo.y - block.y;
                long offsetZ = attachedTo.z - block.z;

                // don't power the block we are attached to
                myPower.SetPower(offsetX, offsetY, offsetZ, 0);
            }
        }
        else
        {
            myPower = new RedstonePower(0);
            myPower.SetAllPower(0);
        }

        int myResPower = (int)myPower.RawValue;

        if (block.state != myResPower)
        {
            block.state = myResPower;
            foreach (BlockData neighbor in GetConnectedToMe(block))
            {
                World.mainWorld.AddBlockUpdate(neighbor.x, neighbor.y, neighbor.z);
            }
        }
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        if (facePlacedOn == AxisDir.YMinus || facePlacedOn == AxisDir.YPlus || facePlacedOn == AxisDir.None)
        {
            return Example.RedstoneTorch;
        }
        else
        {
            return Example.RedstoneTorchOnSide;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}




public class Observer : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        //CreateBlockEntity(Example.Observer, positionOfBlock);
    }

    public override void OnTick(BlockData block)
    {
        RedstonePower myPower = new RedstonePower(0);
        long offX, offY, offZ;
        block.LocalOffsetToWorldOffset(0, 0, -1, out offX, out offY, out offZ);
        if (myPower.GetPower(0, 0, 0) == 0)
        {
            block.needsAnotherTick = true;
            myPower.SetPower(0, 0, 0, 15);
            myPower.SetPower(offX, offY, offZ, 15);
        }
        else
        {
            block.needsAnotherTick = false;
            myPower.SetAllPower(0);
        }
        int resState = (int)myPower.RawValue;

        if (resState != block.state)
        {
            block.state = resState;
        }
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return BlockValue.Air;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}


public struct RedstonePower
{
    SplitInt val;

    public uint RawValue
    {
        get
        {
            return val.rawVal;
        }
        set
        {
            val.rawVal = value;
        }
    }
    public uint GetPower(long offsetX, long offsetY, long offsetZ)
    {
        if (offsetX < 0) return val[0];
        else if (offsetX > 0) return val[1];
        else if (offsetZ < 0) return val[2];
        else if (offsetZ > 0) return val[3];
        else if (offsetY < 0) return val[4];
        else if (offsetY > 0) return val[5];
        else return val[6]; // default (when all offsets are 0) is val[6]
    }

    public void SetPower(long offsetX, long offsetY, long offsetZ, uint value)
    {
        if (offsetX < 0) val[0] = value;
        else if (offsetX > 0) val[1] = value;
        else if (offsetZ < 0) val[2] = value;
        else if (offsetZ > 0) val[3] = value;
        else if (offsetY < 0) val[4] = value;
        else if (offsetY > 0) val[5] = value;
        else val[6] = value; // default (when all offsets are 0) is val[6]
    }

    public void SetAllPower(uint power)
    {
        val[0] = power;
        val[1] = power;
        val[2] = power;
        val[3] = power;
        val[4] = power;
        val[5] = power;
        val[6] = power;
    }

    public RedstonePower(uint val)
    {
        this.val = new SplitInt(val, 7);
    }
}


public static class RedstoneUtil
{

    public static BlockValue[] mightBeProducingRedstonePower = new BlockValue[]
    {
        Example.RedstoneTorch,
        Example.RedstoneTorchOnSide,
        Example.Redstone
    };

    public static uint GetMaxPowerIntoBlock(BlockData block)
    {
        uint maxPowerIntoMe = 0;
        for (int i = 0; i < RotationUtils.NUM_CONNECTIVITY_OFFSETS; i++)
        {
            int offsetX = RotationUtils.CONNECTIVITY_OFFSETS[i * 3];
            int offsetY = RotationUtils.CONNECTIVITY_OFFSETS[i * 3+1];
            int offsetZ = RotationUtils.CONNECTIVITY_OFFSETS[i * 3+2];

            int connectFlag = (1 << i);
            // use negative offset since we these are actually our neighbor's offsets
            using (BlockData neighbor = World.mainWorld.GetBlockData(block.x - offsetX, block.y - offsetY, block.z - offsetZ))
            {
                // if it is a redstone component and is connected to us
                if (mightBeProducingRedstonePower.Contains(neighbor.block) && ((neighbor.connectivityFlags & connectFlag) != 0))
                {
                    RedstonePower neighborPower = new RedstonePower((uint)neighbor.state);
                    uint powerToMe = neighborPower.GetPower(offsetX, offsetY, offsetZ);
                    if (powerToMe > 0)
                    {
                        maxPowerIntoMe = System.Math.Max(powerToMe - 1, maxPowerIntoMe);
                    }
                }
            }
        }
        return maxPowerIntoMe;
    }
}

public class Redstone : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        // temporairly set us to air so our neighbors think we are gone for connectivity reasons
        block.block = BlockValue.Air;
        foreach (BlockData connectedTo in GetConnectedTo(block))
        {
            if (connectedTo.block == Example.Redstone)
            {
                OnTick(connectedTo);
            }
        }
        // set us back to what we were
        block.block = Example.Redstone;



        CreateBlockEntity(Example.Redstone, positionOfBlock);
        destroyBlock = true;
    }

    public static BlockValue[] connectables =
    {
        Example.Redstone,
        Example.RedstoneTorch,
        Example.RedstoneTorchOnSide
    };

    public override bool CanConnect()
    {
        return true;
    }

    BlockValue[] cantPlaceOn = new BlockValue[]
    {
        Example.Redstone,
        Example.RedstoneTorch,
        Example.RedstoneTorchOnSide
    };


    BlockValue[] producesPower = new BlockValue[]
    {
        Example.RedstoneTorch,
        Example.RedstoneTorchOnSide
    };



    public override bool CanBePlaced(AxisDir facePlacedOn, LVector3 pos)
    {
        BlockValue belowBlock = GetBlockNotRelative(pos.x, pos.y - 1, pos.z);
        if (cantPlaceOn.Contains(belowBlock))
        {

        }
        return true;
    }

    public override bool CanConnect(BlockData block, BlockData other, bool onSameYPlane, int numConnectedSoFar)
    {
        if (!onSameYPlane)
        {
            long offsetX = other.x - block.x;
            long offsetY = other.y - block.y;
            long offsetZ = other.z - block.z;
            
            // check if we are going down, is there a block in the way?
            if (offsetY < 0 && GetBlockNotRelative(block.x+offsetX, block.y, block.z+offsetZ) != BlockValue.Air)
            {
                return false;
            }
            // check if we are going up, is there a block in the way?
            else if(offsetY > 0 && GetBlockNotRelative(block.x, other.y, block.z) != BlockValue.Air)
            {
                return false;
            }
        }
        if (connectables.Contains(other.block))
        {
            return true;
        }
        return false;
    }

    static uint producedPower = 15;
    public override void OnTick(BlockData block)
    {
        UpdateConnections(block);

        int numConnected = block.numConnected;

        // redstone doesn't do a half line, it extends to a single line if only connected to one
        if (numConnected == 1)
        {
            foreach (BlockData connectedTo in GetConnectedTo(block))
            {
                long offsetX = connectedTo.x - block.x;
                long offsetY = connectedTo.y - block.y;
                long offsetZ = connectedTo.z - block.z;
                // force connected to other side
                ForceAddConnectedTo(block, block.x - offsetX, block.y, block.z - offsetZ);
            }
        }

        block.rotation = BlockData.BlockRotation.Degrees0;
        uint maxPower = 0;
        int conFlags = block.connectivityFlags;
        int prevState = block.state;
        if (producesPower.Contains(block.block))
        {
            maxPower = producedPower;
        }
        foreach (BlockData connectedTo in GetConnectedTo(block))
        {
            if (RedstoneUtil.mightBeProducingRedstonePower.Contains(connectedTo.block))
            {
                long offsetX = connectedTo.x - block.x;
                long offsetY = connectedTo.y - block.y;
                long offsetZ = connectedTo.z - block.z;

                RedstonePower neighborPower = new RedstonePower((uint)connectedTo.state);
                uint neighborPowerToMe = neighborPower.GetPower(-offsetX,-offsetY,-offsetZ); // negative because we are going from them to us instead of us to them

                if (neighborPowerToMe > 0)
                {
                    maxPower = System.Math.Max(neighborPowerToMe - 1, maxPower);
                }
            }
        }

        // set out power values along connected directions
        RedstonePower myPower = new RedstonePower(0);
        uint powerValue = maxPower;
        myPower.SetPower(0, 0, 0, powerValue); // set default value to it
        foreach (BlockData connectedTo in GetConnectedTo(block))
        {
            long offsetX = connectedTo.x - block.x;
            long offsetY = connectedTo.y - block.y;
            long offsetZ = connectedTo.z - block.z;

            myPower.SetPower(offsetX, offsetY, offsetZ, powerValue);
        }

        bool modified = false;
        int myResState = (int)myPower.RawValue;
        if (myResState != block.state)
        {
            modified = true;
            block.state = myResState;
        }

        block.animationState = (short)powerValue;
        //Debug.LogWarning("updating " + block.x + " " + block.y + " " + block.z + " with connections " + conFlags + " cur power " + powerValue + " and prev state " + prevState + " and res state " + myResState);

        foreach (BlockData connectedTo in GetConnectedTo(block))
        {
            //Debug.LogWarning("i have neighbor " + connectedTo.x + " " + connectedTo.y + " " + connectedTo.z + " " + block.x + " " + block.y + " " + block.z);

            if ((modified) && connectedTo.block == Example.Redstone)
            {
                //Debug.LogWarning("recursing " + connectedTo.x + " " + connectedTo.y + " " + connectedTo.z + " " + block.x + " " + block.y + " " + block.z);
                OnTick(connectedTo);
                //world.AddBlockUpdate(connectedTo.x, connectedTo.y, connectedTo.z);
            }
            else if(modified)
            {
                World.mainWorld.AddBlockUpdate(connectedTo.x, connectedTo.y, connectedTo.z);
                foreach (BlockData neighbor in GetNeighbors(connectedTo, includingDown: false))
                {
                    if (neighbor.block != Example.Redstone && RedstoneUtil.mightBeProducingRedstonePower.Contains(neighbor.block))
                    {
                        World.mainWorld.AddBlockUpdate(neighbor.x, neighbor.y, neighbor.z);
                    }
                }
            }
        }
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Redstone;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}



public static class BallTrackUtils
{
   

    public static bool CanGoInto(BlockData a, AxisDir aRelativeOutAxis, BlockData b, AxisDir bRelativeInAxis)
    {
        if (!DoesNotHaveBall(b.block))
        {
            return false;
        }
        long relativePosX, relativePosY, relativePosZ;

        a.GetRelativePosOf(b, out relativePosX, out relativePosY, out relativePosZ);
        bool goesOutA = OffsetMatchesAxis(relativePosX, relativePosY, relativePosZ, aRelativeOutAxis);

        b.GetRelativePosOf(a, out relativePosX, out relativePosY, out relativePosZ);
        bool goesInB = OffsetMatchesAxis(relativePosX, relativePosY, relativePosZ, bRelativeInAxis);



        return goesOutA && goesInB;

    }

    public static bool DoesNotHaveBall(BlockValue block)
    {
        return block == Example.BallTrackTurnEmpty || block == Example.BallTrackZEmpty;
    }

    public static bool OffsetMatchesAxis(long offX, long offY, long offZ, AxisDir axis)
    {
        if (axis == AxisDir.XMinus) return offX == -1;
        if (axis == AxisDir.XPlus) return offX == 1;
        if (axis == AxisDir.YMinus) return offY == -1;
        if (axis == AxisDir.YPlus) return offY == 1;
        if (axis == AxisDir.ZMinus) return offZ == -1;
        if (axis == AxisDir.ZPlus) return offZ == 1;
        return false;
    }


    public static void GetInAndOutAxes(BlockData a, out AxisDir outAxis, out AxisDir inAxis, out bool inverted, bool allowInvert =false,bool forceInvert = false)
    {
        if (a.block == Example.BallTrackZFull || a.block == Example.BallTrackZEmpty)
        {
            outAxis = AxisDir.ZMinus;
            inAxis = AxisDir.ZPlus;
        }
        else if(a.block == Example.BallTrackTurnFull || a.block == Example.BallTrackTurnEmpty)
        {
            outAxis = AxisDir.XPlus;
            inAxis = AxisDir.ZPlus;
        }
        else
        {
            //Debug.LogWarning("getting axis for unknown block " + a.block);
            outAxis = AxisDir.None;
            inAxis = AxisDir.None;
        }

        inverted = false;
        // swap them since we are going backwards
        if (forceInvert || (a.state < 0 && allowInvert))
        {
            inverted = true;
            AxisDir tmp = outAxis;
            outAxis = inAxis;
            inAxis = tmp;
        }
    }

    public static void FillBlock(BlockData a)
    {
        if (a.block == Example.BallTrackZEmpty)
        {
            a.block = Example.BallTrackZFull;
        }
        else if (a.block == Example.BallTrackTurnEmpty)
        {
            a.block = Example.BallTrackTurnFull;
        }
    }


    public static void EmptyBlock(BlockData a)
    {
        if (a.block == Example.BallTrackZFull)
        {
            a.block = Example.BallTrackZEmpty;
        }
        else if (a.block == Example.BallTrackTurnFull)
        {
            a.block = Example.BallTrackTurnEmpty;
        }
    }

    public static bool CanFlowInto(BlockData a, BlockData b, out bool needToInvertVelUponEntry)
    {
        needToInvertVelUponEntry = false;

        BlockData.BlockRotation relativeRot = a.GetRelativeRotationOf(b);

        AxisDir inA, outA, inB, outB;
        bool aInverted, bInverted;
        GetInAndOutAxes(a, out inA, out outA, out aInverted, allowInvert: true);
        GetInAndOutAxes(b, out inB, out outB, out bInverted, allowInvert: false, forceInvert: aInverted);

        if (CanGoInto(a, outA, b, inB))
        {
            return true;
        }
        else if(CanGoInto(a, outA, b, outB))
        {
            needToInvertVelUponEntry = true;
            return true;
        }
        else
        {
            return false;
        }
    }
}


public class BallTrackTurnFull : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        //CreateBlockEntity(Example.BallTrackHorizontal, positionOfBlock);
        destroyBlock = true;
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.BallTrackTurnFull;
    }

    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        int ballWidth = 4;
        int trackSize = 16;

        // ball goes from [state, state+4]
        // if state == 12, ball it at edge (track is 16 wide)
        // if state == 0, ball is at other edge
        // thus, when going negative:

        //Debug.Log(block.animationState + " " + block.state + " " + (int)block.rotation);
        if (block.state > 0)
        {

            // when going positive
            //   [0                   < state <= trackSize-ballWidth]: moving like normal
            //   [trackSize-ballWidth < state <= trackSize]:  only move more if we can move into neighboring one, also animate us moving into there. Otherwise, reverse direction 
            //   [trackSize           < state]: we are no longer in this, move to neighbor


            block.animationState += 1;

            bool moveLikeNormal = 0 < block.animationState && block.animationState <= trackSize - ballWidth;
            bool movingIntoNext = trackSize - ballWidth < block.animationState && block.animationState <= trackSize;
            bool leftThingPos = trackSize < block.animationState;

            if (moveLikeNormal)
            {
            }
            else if (movingIntoNext || leftThingPos)
            {
                using (BlockData neighbor = GetBlockDataRelative(block, 0, 0, 1))
                {
                    bool needToInvertVelUponEntry;
                    bool canFlowIntoNeighbor = BallTrackUtils.CanFlowInto(block, neighbor, out needToInvertVelUponEntry);

                    // we can't flow into neighbor, flip directions
                    if (!canFlowIntoNeighbor)
                    {
                        block.animationState = (short)(trackSize - ballWidth - 1);
                        //Debug.Log("got to end 1, swapping");
                        block.state = -block.state;
                    }
                    // we can flow into neighbor, continue going
                    else
                    {
                        if (movingIntoNext)
                        {
                        }
                        else if (leftThingPos)
                        {
                            // flipped around
                            if (needToInvertVelUponEntry)
                            {
                                neighbor.animationState = (short)(trackSize - ballWidth);
                                neighbor.state = -block.state;
                            }
                            // not flipped around
                            else
                            {
                                neighbor.animationState = 0;
                                neighbor.state = block.state;
                            }
                            BallTrackUtils.FillBlock(neighbor);
                            BallTrackUtils.EmptyBlock(block);
                            block.animationState = 0;
                            block.needsAnotherTick = false;
                        }
                    }
                }
            }
            // none of the above, undo any changes
            else
            {
                block.animationState -= 1;
            }
        }
        else if (block.state < 0)
        {
            block.animationState -= 1;

            //   [0          <= state < trackSize-ballWidth]: moving like normal
            //   [-ballWidth <= state < 0]: only move more if we can move into neighboring one, also animate us moving into there. Otherwise, reverse direction
            //   [             state < -ballWidth]: we are no longer in this, move to neighbor
            bool moveLikeNormal = 0 <= block.animationState && block.animationState < trackSize - ballWidth;
            bool movingIntoNext = -ballWidth <= block.animationState && block.animationState < 0;
            bool leftThingPos = block.animationState < -ballWidth;

            //Debug.Log(moveLikeNormal + " " + movingIntoNext + " " + leftThingPos + " " + block.animationState);
            if (moveLikeNormal)
            {
            }
            else if (movingIntoNext || leftThingPos)
            {
                using (BlockData neighbor = GetBlockDataRelative(block, 1, 0, 0))
                {
                    bool needToInvertVelUponEntry;
                    bool canFlowIntoNeighbor = BallTrackUtils.CanFlowInto(block, neighbor, out needToInvertVelUponEntry);

                    // we can't flow into neighbor, flip directions
                    if (!canFlowIntoNeighbor)
                    {
                        block.animationState = 0;
                        block.state = -block.state;
                        //Debug.Log("got to end 2, swapping");
                    }
                    // we can flow into neighbor, continue going
                    else
                    {
                        if (movingIntoNext)
                        {
                        }
                        else if (leftThingPos)
                        {

                            // flipped around
                            if (needToInvertVelUponEntry)
                            {
                                neighbor.animationState = 0;
                                neighbor.state = -block.state;
                            }
                            // not flipped around
                            else
                            {
                                neighbor.animationState = (short)(trackSize - ballWidth);
                                neighbor.state = block.state;
                            }
                            BallTrackUtils.FillBlock(neighbor);
                            BallTrackUtils.EmptyBlock(block);
                            block.animationState = 0;
                            block.needsAnotherTick = false;
                        }
                    }
                }
            }
            // none of the above, undo any changes
            else
            {
                block.animationState += 1;
            }
        }
        else
        {
            block.state = 1;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}



public class SimpleWater : Block
{
    static int maxUpdates = 300;
    static int numUpdatesThisTick = 0;
    public override void OnTickStart()
    {
        numUpdatesThisTick = 0;
    }
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Water;
    }

    bool IsWater(long x, long y, long z)
    {
        BlockValue block = GetBlockNotRelative(x, y, z);
        return block == Example.Water || block == Example.WaterNoFlow;
    }


    public override void OnTick(BlockData block)
    {
        numUpdatesThisTick += 1;

        if (maxUpdates < numUpdatesThisTick)
        {
            block.needsAnotherTick = true;
            return;
        }

        // water: state 2 = time I got here
        block.state = 0;

            // if air below, set below = water and us = air
        if (GetBlockNotRelative(block.x, block.y - 1, block.z) == Example.Air)
        {
            SetBlockNotRelative(block.x, block.y - 1, block.z, Example.Water);
            block.block = Example.Air;
            return;
        }
        else if(false)
        {
            // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
            foreach (BlockData neighbor in GetNeighbors(block, includingUp: false, includingDown: false))
            {
                if (neighbor.block == Example.Air && GetBlockNotRelative(neighbor.x, neighbor.y - 1, neighbor.z) == Example.Air)
                {
                    SetBlockNotRelative(neighbor.x, neighbor.y - 1, neighbor.z, Example.Water);
                    block.block = Example.Air;
                    return;
                }
            }
        }
        block.needsAnotherTick = false;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 10000.0f;
    }
}


public class Water : Block
{
    static int maxUpdates = 100;
    static int numUpdatesThisTick = 0;
    public override void OnTickStart()
    {
        numUpdatesThisTick = 0;
    }
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }



    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Water;
    }

    bool IsWater(long x, long y, long z)
    {
        BlockValue block = GetBlockNotRelative(x, y, z);
        return block == Example.Water || block == Example.WaterNoFlow;
    }

    public int GetNumAirNeighbors(long wx, long wy, long wz)
    {
        return
            (GetBlockNotRelative(wx + 1, wy, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlockNotRelative(wx - 1, wy, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlockNotRelative(wx, wy + 1, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlockNotRelative(wx, wy - 1, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlockNotRelative(wx, wy, wz + 1) == (int)Example.Air ? 1 : 0) +
            (GetBlockNotRelative(wx, wy, wz - 1) == (int)Example.Air ? 1 : 0);

    }

    public override void OnTick(BlockData block)
    {
        numUpdatesThisTick += 1;
        if (maxUpdates < numUpdatesThisTick)
        {
            block.needsAnotherTick = true;
            return;
        }

        // water: state 2 = time I got here


        block.needsAnotherTick = false;


        // if we are WATER without water above and with water below, pathfind to look for open space
        //if (block == (int)Example.Water && IsWater(this[wx, wy - 1, wz]) && !IsWater(this[wx, wy + 1, wz]))
        if (block.block == Example.Water && IsWater(block.x, block.y-1, block.z) && !IsWater(block.x, block.y+1, block.z))
        {
            // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
            //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
            //numWaterUpdatesThisTick += 1;
            if (PhysicsUtils.SearchOutwards(new LVector3(block.x, block.y, block.z), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
            {
                return by < block.y && (b == (int)Example.Water || b == (int)Example.WaterNoFlow);
            },
                isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                {
                    if (b == (int)Example.Air && by < block.y)
                    {
                        SetBlockNotRelative(bx, by, bz, Example.Water);
                        SetStateNotRelative(bx, by, bz, GetNumAirNeighbors(bx, by, bz), BlockState.State);
                        return true;
                    }
                    return false;
                }
            ))
            {
                block.state = 0;
                block.block = Example.Air;
                return;
            }
            else
            {
                block.needsAnotherTick = true;
                block.block = Example.WaterNoFlow;
                return;
            }
        }
        else
        {

            // if air below, set below = water and us = air
            if (GetBlockNotRelative(block.x, block.y-1, block.z) == Example.Air)
            {
                SetBlockNotRelative(block.x, block.y-1, block.z, Example.Water);
                block.state = 0;
                SetStateNotRelative(block.x, block.y - 1, block.z, GetNumAirNeighbors(block.x, block.y - 1, block.z), BlockState.State); // +1 because we are now air instead of water
                block.block = Example.Air;
                world.AddBlockUpdateToNeighbors(block.x, block.y + 1, block.z);
                world.AddBlockUpdateToNeighbors(block.x, block.y, block.z);
                world.AddBlockUpdateToNeighbors(block.x, block.y - 1, block.z);
                return;
            }
            else
            {
                // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
                foreach (BlockData neighbor in GetNeighbors(block, includingUp: false, includingDown: false))
                {
                    LVector3 pos2 = 2 * (neighbor.pos - block.pos) + block.pos;
                    if (neighbor.block == Example.Air)
                    {
                        if (GetBlockNotRelative(neighbor.x, neighbor.y-1, neighbor.z) == Example.Air)
                        {
                            SetBlockNotRelative(neighbor.x, neighbor.y - 1, neighbor.z, Example.Water);
                            SetStateNotRelative(neighbor.x, neighbor.y - 1, neighbor.z, GetNumAirNeighbors(neighbor.x, neighbor.y-1, neighbor.z), BlockState.State);
                            block.state = 0;
                            block.block = Example.Air;
                            return;
                        }
                        else if (pos2.BlockV == Example.Air && GetBlockNotRelative(pos2.x, pos2.y - 1, pos2.z) == Example.Air)
                        {
                            SetBlockNotRelative(pos2.x, pos2.y - 1, pos2.z, Example.Water);
                            SetStateNotRelative(pos2.x, pos2.y - 1, pos2.z, GetNumAirNeighbors(pos2.x, pos2.y - 1, pos2.z), BlockState.State);
                            block.state = 0;
                            block.block = Example.Air;
                            return;
                        }
                    }
                }
            }
            int prevNumAirNeighbors = block.state;
            int curNumAirNeighbors = GetNumAirNeighbors(block.x, block.y, block.z);
            // if we have a more air neighbors, flood fill back and set valid blocks that have WATER_NOFLOW back to WATER so they can try again
            if (curNumAirNeighbors != prevNumAirNeighbors)
            {
                block.state = curNumAirNeighbors;
                if (curNumAirNeighbors > 0)
                {
                    LVector3 airNeighbor = new LVector3(block.x, block.y, block.z);
                    foreach (BlockData neighbor in GetNeighbors(block))
                    {
                        if (neighbor.block == Example.Air)
                        {
                            airNeighbor = neighbor.pos;
                            break;
                        }
                    }
                    //numBlockUpdatesThisTick += 1;

                    // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
                    //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    if (PhysicsUtils.SearchOutwards(block.pos, maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    {
                        return (b == (int)Example.Water || b == (int)Example.WaterNoFlow);
                    },
                        isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                        {
                            if (b == (int)Example.WaterNoFlow && IsWater(bx, by - 1, bz) && airNeighbor.y < by)
                            {
                                SetBlockNotRelative(bx, by, bz, Example.Air);
                                SetStateNotRelative(bx, by, bz, 0, BlockState.State);
                                return true;
                            }
                            return false;
                        }
                    ))
                    {

                        SetBlockNotRelative(airNeighbor.x, airNeighbor.y, airNeighbor.z, Example.Water);
                        SetStateNotRelative(airNeighbor.x, airNeighbor.y, airNeighbor.z, GetNumAirNeighbors(airNeighbor.x, airNeighbor.y, airNeighbor.z), BlockState.State);
                        block.state = curNumAirNeighbors - 1; // we just replaced an air neighbor with water
                        block.needsAnotherTick = true;
                        return;
                    }
                    else
                    {
                        block.needsAnotherTick = true;
                        block.block = Example.WaterNoFlow;
                        return;
                    }
                }
            }
        }

    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 10000.0f;
    }
}

public class Grass : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Dirt, positionOfBlock);
        destroyBlock = true;
    }



    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Grass;
    }

    int numGrassTicks;
    public override void OnTickStart()
    {
        if (numGrassTicks > 0)
        {

            Debug.Log("num grass ticks = " + numGrassTicks);
            numGrassTicks = 0;
        }
    }

    public override void OnTick(BlockData block)
    {
        return;
        long x = block.x;
        long y = block.y;
        long z = block.z;
        long state1 = block.state;
        long state2 = block.lightingState;
        long state3 = block.animationState;
        block.needsAnotherTick = false;
        if (GetBlockNotRelative(block.x, block.y+1, block.z) != Example.Air)
        {
            block.block = Example.Dirt;
            return;
        }
        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            // if neighbor is dirt and it has air above it, try growing into it
            if (neighbor.block == Example.Dirt && GetBlockNotRelative(neighbor.x, neighbor.y + 1, neighbor.z) == Example.Air)
            {
                if (rand() < 0.5f)
                {
                    // grow
                    neighbor.block = Example.Grass;
                }
                else
                {
                    // we failed to grow but still need to, try again next tick
                    block.needsAnotherTick = true;
                }
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Shovel)
        {
            return 1.0f;
        }
        else
        {
            return 2.0f;
        }
    }
}


public class Leaf : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Stick, positionOfBlock);
        if (rand() < 0.2f)
        {
            CreateBlockEntity(Example.Stick, positionOfBlock);
        }
        if (rand() < 0.05f)
        {
            CreateBlockEntity(Example.Sapling, positionOfBlock);
        }
        destroyBlock = true;
    }



    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Leaf;
    }


    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.7f;
    }
}


public class Sapling : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Sapling, positionOfBlock);
        destroyBlock = true;
    }



    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Sapling;
    }


    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.7f;
    }

    public override void OnRandomTick(BlockData block)
    {
        if (rand() < 0.02f)
        {
            if(TryMakeTree(block))
            {

            }
        }
        block.needsAnotherTick = true;
    }



    bool TreeOverridable(BlockValue value)
    {
        return value == BlockValue.Air || value == Example.Sapling || value == Example.Trunk;
    }

    public bool TryMakeTree(BlockData basePos)
    {
        long x = basePos.x;
        long y = basePos.y;
        long z = basePos.z;
        int treeHeight = (int)System.Math.Round(Simplex.Noise.rand(x, y + 2, z) * 20 + 6);
        bool checkingButNotMaking = true;
        for (int w = 0; w < 2; w++)
        {
            if (w == 0)
            {
                checkingButNotMaking = true;
            }
            else
            {
                checkingButNotMaking = false;
            }
            for (int i = 0; i < treeHeight; i++)
            {
                if (i == 0)
                {
                    if (checkingButNotMaking)
                    {
                        if (!TreeOverridable(GetBlockRelative(basePos, 0, i, 0)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        SetBlockRelative(basePos, 0, i, 0, Example.Trunk);
                        using (BlockData trunkData = GetBlockDataRelative(basePos, 0, i, 0))
                        {
                            trunkData.state = 4;
                            trunkData.lightingState = 0;
                            trunkData.animationState = 0;
                        }
                    }
                    continue;
                }
                float pAlong = (i + 1) / (float)(treeHeight);

                float maxWidth = 3.0f;
                float topWidth = 0.5f;
                float bottomWidth = 0.2f;
                float decreasePoint = 0.3f;

                float p;
                float minVal;
                float maxVal;
                if (pAlong < decreasePoint)
                {
                    minVal = bottomWidth;
                    maxVal = maxWidth;
                    p = (decreasePoint - pAlong) / decreasePoint;
                }
                else
                {
                    minVal = maxWidth;
                    maxVal = topWidth;
                    p = 1 - (pAlong - decreasePoint) / (1.0f - decreasePoint);
                }

                float width = minVal * p + maxVal * (1 - p);
                int widthI = (int)System.Math.Floor(width);


                bool addedLeaf = false;
                for (int j = -widthI; j <= widthI; j++)
                {
                    for (int k = -widthI; k <= widthI; k++)
                    {
                        if (System.Math.Sqrt(j * j + k * k) <= width)
                        {
                            if (j != 0 || k != 0)
                            {
                                addedLeaf = true;
                            }
                            if (checkingButNotMaking)
                            {
                                if (!TreeOverridable(GetBlockRelative(basePos, j, i, k)))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                SetBlockRelative(basePos, j, i, k, Example.Leaf);
                            }
                        }
                    }
                }
                if (addedLeaf)
                {
                    if (checkingButNotMaking)
                    {
                        if (!TreeOverridable(GetBlockRelative(basePos, 0, i, 0)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        SetBlockRelative(basePos, 0, i, 0, Example.Trunk);
                        using (BlockData trunkData = GetBlockDataRelative(basePos, 0, i, 0))
                        {
                            trunkData.state = 4;
                            trunkData.lightingState = 0;
                            trunkData.animationState = 0;
                        }
                    }
                }

            }

        }

        return true;
    }
}


public class Rock : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock ||
            thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock ||
            thingBreakingWith.Block == Example.Pickaxe)
        {
            CreateBlockEntity(Example.SharpRock, positionOfBlock);
        }
        else
        {
            CreateBlockEntity(Example.Rock, positionOfBlock);
        }
    }



    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Rock;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock)
        {
            return 3.0f;
        }
        else if(thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock)
        {
            return 2.0f;
        }
        else if(thingBreakingWith.Block == Example.Pickaxe)
        {
            return 1.5f;
        }
        else
        {
            return 0.1f;
        }
    }
}

public class LargeRock : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock ||
            thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock ||
            thingBreakingWith.Block == Example.Pickaxe)
        {
            CreateBlockEntity(Example.LargeSharpRock, positionOfBlock);
        }
        else
        {
            CreateBlockEntity(Example.LargeRock, positionOfBlock);
        }
    }


    public override Recipe[] GetRecipes()
    {
        List<Recipe> recipes = new List<Recipe>();
        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.LargeRock, Example.LargeRock, Example.LargeRock },
            { Example.LargeRock, Example.Air, Example.LargeRock },
            { Example.LargeRock, Example.LargeRock, Example.LargeRock }
        }, new BlockStack(Example.Furnace, 1)));

        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Coal},
            { Example.Stick }
        }, new BlockStack(Example.Light, 4)));

        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Trunk, Example.Trunk,  Example.Trunk},
            { Example.Trunk, Example.Air, Example.Trunk },
            { Example.Trunk, Example.Trunk, Example.Trunk }
        }, new BlockStack(Example.Chest, 1)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Trunk, Example.Air,  Example.Trunk},
            { Example.Trunk, Example.Air, Example.Trunk },
            { Example.Trunk, Example.Trunk, Example.Trunk }
        }, new BlockStack(Example.Barrel, 1)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Air, Example.LargeRock,  Example.Air},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Shovel, 1, 64, 64)));

        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Air, Example.LargeSharpRock,  Example.Air},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Axe, 1, 64, 64)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.LargeSharpRock, Example.LargeRock,  Example.LargeSharpRock},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Pickaxe, 1, 64, 64)));


        return recipes.ToArray();
    }

    public override int InventorySpace()
    {
        return 9;
    }

    public override bool ReturnsItemsWhenDeselected()
    {
        return true;
    }

    public override int NumInventoryRows()
    {
        return 3;
    }

    public override int NumCraftingOutputs()
    {
        return 1;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.LargeRock;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock)
        {
            return 5.0f;
        }
        else if (thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock)
        {
            return 4.0f;
        }
        else if (thingBreakingWith.Block == Example.Pickaxe)
        {
            return 2.0f;
        }
        else
        {
            return 0.1f;
        }
    }
}


public class WetBark : Block
{
    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.WetBark;
    }
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        if (rand() < 0.8)
        {
            CreateBlockEntity(Example.String, positionOfBlock + Random.insideUnitSphere*0.1f);
        }
        CreateBlockEntity(Example.String, positionOfBlock + Random.insideUnitSphere * 0.1f);
        CreateBlockEntity(Example.String, positionOfBlock + Random.insideUnitSphere * 0.1f);
    }
    
    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.2f;
    }
}


public class CoalOre : Block
{
    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.CoalOre;
    }
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        if (thingBreakingWith.Block == Example.Pickaxe)
        {
            int numCoal = (int)(rand() * 5) + 1;

            for (int i = 0; i < numCoal; i++)
            {
                CreateBlockEntity(Example.Coal, positionOfBlock + Random.insideUnitSphere * 0.1f);
            }

        }
        else
        {

        }

        destroyBlock = true;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Pickaxe)
        {
            return 2.0f;
        }
        else
        {
            return 10.0f;
        }
    }
}

public class LooseRocks : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        if (block.state > 0)
        {
            block.state -= 1;
            CreateBlockEntity(Example.Rock, posOfOpening);
            destroyBlock = false;
        }
        else
        {
            CreateBlockEntity(Example.LargeRock, positionOfBlock);
            destroyBlock = true;
        }
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.LooseRocks;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Pickaxe)
        {
            return 0.8f;
        }
        else
        {
            return 1.3f;
        }
    }
}


public class Stone : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        for (int i = 0; i < block.state / 6; i++)
        {
            CreateBlockEntity(Example.LargeRock, positionOfBlock);
        }
        for (int i = 0; i < block.state % 6; i++)
        {
            CreateBlockEntity(Example.Rock, positionOfBlock);
        }
        destroyBlock = true;
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Stone;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Pickaxe)
        {
            return 4.0f;
        }
        else
        {
            return 100.0f;
        }
    }
}


public class Trunk : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        if (block.state > 0)
        {
            CreateBlockEntity(Example.Bark, posOfOpening);
            block.state -= 1;
            destroyBlock = false;
        }
        else
        {
            CreateBlockEntity(Example.Trunk, positionOfBlock);
            destroyBlock = true;
        }
    }


    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Trunk;
    }


    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        // breaking bark off off tree
        if (block.state > 0)
        {
            if (thingBreakingWith.Block == Example.Axe)
            {
                return 0.5f;
            }
            else
            {
                return 1.0f;
            }
        }
        // breaking the log itself, not bark
        else
        {
            if (thingBreakingWith.Block == Example.Axe)
            {
                return 3.0f;
            }
            else
            {
                return 10.0f;
            }
        }
    }
}


public class Chest : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Chest, positionOfBlock);
        destroyBlock = true;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Chest;
    }

    public override int InventorySpace()
    {
        return 16*3;
    }

    public override int NumInventoryRows()
    {
        return 3;
    }

    public override int NumCraftingOutputs()
    {
        return 0;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 1.0f;
    }
}

public class Barrel : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Barrel, positionOfBlock);
        destroyBlock = true;
    }

    public override int InventorySpace()
    {
        return 8*3;
    }

    public override int NumInventoryRows()
    {
        return 3;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Barrel;
    }


    public override int NumCraftingOutputs()
    {
        return 0;
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 1.0f;
    }
}


public class Furnace : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Furnace, positionOfBlock);
        destroyBlock = true;
    }

    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Furnace;
    }

    public override Recipe[] GetRecipes()
    {
        List<Recipe> recipes = new List<Recipe>();
        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.IronOre, Example.Coal },
        }, new BlockStack(Example.Iron, 1)));

        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Trunk, Example.Coal },
        }, new BlockStack(Example.Coal, 16)));

        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Trunk, Example.Trunk },
        }, new BlockStack(Example.Coal, 16)));


        return recipes.ToArray();
    }

    public override int InventorySpace()
    {
        return 2;
    }

    public override bool ReturnsItemsWhenDeselected()
    {
        return true;
    }

    public override int NumInventoryRows()
    {
        return 1;
    }

    public override int NumCraftingOutputs()
    {
        return 1;
    }


    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 1.0f;
    }
}




public class Bark : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        CreateBlockEntity(Example.Bark, positionOfBlock);
    }



    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Bark;
    }

    System.Random randomGen = new System.Random();
    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            if (neighbor.block == Example.Water || neighbor.block == Example.WaterNoFlow)
            {
                if (randomGen.NextDouble() < 0.01)
                {
                    block.block = Example.WetBark;
                    block.needsAnotherTick = false;
                    break;
                }
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.0f;
    }
}

public class SimpleItem : Item
{

}


public class Cheese : Block
{
    public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
    {
        return Example.Cheese;
    }


    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Cheese, positionOfBlock);
        destroyBlock = true;
    }

    public override int ConstantLightEmitted()
    {
        return 6;
    }

    public override void OnTick(BlockData block)
    {
        block.lightProduced = 6;

        block.needsAnotherTick = false;
        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            if (neighbor.block == Example.Grass)
            {
                if (rand() < 0.003f)
                {
                    neighbor.block = Example.Cheese;
                }
                else
                {
                    block.needsAnotherTick = true;
                }
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}

public class ExamplePack : BlocksPack {

    // Use this for initialization
    // Awake because we want this to happen before World calls its Start()
    void Awake () {
        AddCustomBlock(Example.Grass, new Grass(), 64);
        AddCustomBlock(Example.Bark, new Bark(), 64);
        AddCustomBlock(Example.Trunk, new Trunk(), 64);
        AddCustomBlock(Example.Dirt, new SimpleBlock(Example.Dirt, 2.0f, new Tuple<BlockValue, float>(Example.Shovel, 1.0f)), 64);
        AddCustomBlock(Example.Clay, new SimpleBlock(Example.Clay, 1.0f, new Tuple<BlockValue, float>(Example.Shovel, 0.6f)), 64);
        AddCustomBlock(Example.Stone, new Stone(), 64);
        AddCustomBlock(Example.Leaf, new Leaf(), 64);
        AddCustomBlock(Example.Furnace, new Furnace(), 64);
        AddCustomBlock(Example.Sapling, new Sapling(), 64);
        //AddCustomBlock(Example.CraftingTable, new CraftingTable(), 64);
        AddCustomBlock(Example.LooseRocks, new LooseRocks(), 64);
        AddCustomBlock(Example.Rock, new Rock(), 64);
        AddCustomBlock(Example.LargeRock, new LargeRock(), 64);
        AddCustomBlock(Example.LooseRocks, new LooseRocks(), 64);
        AddCustomBlock(Example.IronOre, new SimpleBlock(Example.IronOre, 100.0f, new Tuple<BlockValue, float>(Example.Pickaxe, 5.0f)), 64);
        AddCustomBlock(Example.Stick, new SimpleItem(), 64);
        AddCustomBlock(Example.Coal, new SimpleItem(), 64);
        AddCustomBlock(Example.Iron, new SimpleItem(), 64);
        AddCustomBlock(Example.Pickaxe, new SimpleItem(), 64);
        AddCustomBlock(Example.SharpRock, new SimpleItem(), 64);
        AddCustomBlock(Example.LargeSharpRock, new SimpleItem(), 64);
        AddCustomBlock(Example.Shovel, new SimpleItem(), 1);
        AddCustomBlock(Example.Axe, new SimpleItem(), 1);
        AddCustomBlock(Example.WetBark, new WetBark(), 64);
        AddCustomBlock(Example.Chest, new Chest(), 64);
        AddCustomBlock(Example.Barrel, new Barrel(), 64);
        AddCustomBlock(Example.Sand, new SimpleBlock(Example.Sand, 1.0f, new Tuple<BlockValue, float>(Example.Shovel, 0.5f)), 64);
        AddCustomBlock(Example.CoalOre, new CoalOre(), 64);
        //AddCustomBlock(Example.Sand, new WaterSource(), 64);
        AddCustomBlock(Example.Light, new Light(), 64);
        AddCustomBlock(Example.String, new SimpleItem(), 64);
        //AddCustomBlock(Example.Water, new Water(), 64);
        //AddCustomBlock(Example.WaterNoFlow, new Water(), 64);
        AddCustomBlock(Example.Water, new WaterNewWithPressure(), 64);
        AddCustomBlock(Example.WaterNoFlow, new SimpleBlock(Example.WaterNoFlow, 0.2f), 64);
        AddCustomBlock(Example.Lava, new Lava(), 64);
        AddCustomBlock(Example.BallTrackZFull, new BallTrackHorizontalFull(), 64);
        AddCustomBlock(Example.BallTrackTurnFull, new BallTrackTurnFull(), 64);
        AddCustomBlock(Example.BallTrackZEmpty, new SimpleBlock(Example.BallTrackZEmpty, 0.2f), 64);
        AddCustomBlock(Example.BallTrackTurnEmpty, new SimpleBlock(Example.BallTrackTurnEmpty, 0.2f), 64);
        AddCustomBlock(Example.Cheese, new Cheese(), 64);

        AddCustomBlock(Example.AutoMiner, new AutoMiner(), 64);
        AddCustomBlock(Example.ConveyerBelt, new ConveyerBelt(), 64);

        AddCustomBlock(Example.RedstoneTorch, new RedstoneTorch(), 64);
        AddCustomBlock(Example.RedstoneTorchOnSide, new RedstoneTorch(), 64);
        AddCustomBlock(Example.Redstone, new Redstone(), 64);
        //AddCustomBlock(Example.BallTrackEmpty, new SimpleBlock(0.2f, new Tuple<BlockValue, float>(Example.Shovel, 0.2f)), 64);




        SetCustomGeneration(new ExampleGeneration());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
