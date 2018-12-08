namespace Example_pack {
    public enum BlockValue
    {
        Air = 0,
        Wildcard = -1,
        Axe=-3,
        Bark=4,
        Bedrock=5,
        Clay=6,
        CraftingTable=7,
        Dirt=8,
        Flower=9,
        FlowerWithNectar=10,
        Grass=11,
        LargeRock=-12,
        LargeSharpRock=-13,
        Leaf=14,
        LooseRocks=15,
        Pickaxe=-16,
        Rock=-17,
        Sand=18,
        SharpRock=-19,
        Shovel=-20,
        Stick=-21,
        Stone=22,
        String=-23,
        Trunk=24,
        Water=-25,
        WaterNoFlow=-26,
        WetBark=27
    }

    public class BlockUtils {
        public static string BlockIdToString(int blockId){
            if (blockId < 0){ blockId = -blockId; }
            if (blockId == 0){ return "Air"; }
            if (blockId == 1){ return "Wildcard"; }
            if (blockId == 2){ return "?? 2 is invalid"; }
            if(blockId == 3){ return "Axe";}
            if(blockId == 4){ return "Bark";}
            if(blockId == 5){ return "Bedrock";}
            if(blockId == 6){ return "Clay";}
            if(blockId == 7){ return "CraftingTable";}
            if(blockId == 8){ return "Dirt";}
            if(blockId == 9){ return "Flower";}
            if(blockId == 10){ return "FlowerWithNectar";}
            if(blockId == 11){ return "Grass";}
            if(blockId == 12){ return "LargeRock";}
            if(blockId == 13){ return "LargeSharpRock";}
            if(blockId == 14){ return "Leaf";}
            if(blockId == 15){ return "LooseRocks";}
            if(blockId == 16){ return "Pickaxe";}
            if(blockId == 17){ return "Rock";}
            if(blockId == 18){ return "Sand";}
            if(blockId == 19){ return "SharpRock";}
            if(blockId == 20){ return "Shovel";}
            if(blockId == 21){ return "Stick";}
            if(blockId == 22){ return "Stone";}
            if(blockId == 23){ return "String";}
            if(blockId == 24){ return "Trunk";}
            if(blockId == 25){ return "Water";}
            if(blockId == 26){ return "WaterNoFlow";}
            if(blockId == 27){ return "WetBark";}

            return "Unknown block id " + blockId;
        }
        
        public static int StringToBlockId(string blockName){
            if (blockName == "Air"){ return 0; }
            if (blockName == "Wildcard"){ return -1; }
            if(blockName == "Axe"){ return -3;}
            if(blockName == "Bark"){ return 4;}
            if(blockName == "Bedrock"){ return 5;}
            if(blockName == "Clay"){ return 6;}
            if(blockName == "CraftingTable"){ return 7;}
            if(blockName == "Dirt"){ return 8;}
            if(blockName == "Flower"){ return 9;}
            if(blockName == "FlowerWithNectar"){ return 10;}
            if(blockName == "Grass"){ return 11;}
            if(blockName == "LargeRock"){ return -12;}
            if(blockName == "LargeSharpRock"){ return -13;}
            if(blockName == "Leaf"){ return 14;}
            if(blockName == "LooseRocks"){ return 15;}
            if(blockName == "Pickaxe"){ return -16;}
            if(blockName == "Rock"){ return -17;}
            if(blockName == "Sand"){ return 18;}
            if(blockName == "SharpRock"){ return -19;}
            if(blockName == "Shovel"){ return -20;}
            if(blockName == "Stick"){ return -21;}
            if(blockName == "Stone"){ return 22;}
            if(blockName == "String"){ return -23;}
            if(blockName == "Trunk"){ return 24;}
            if(blockName == "Water"){ return -25;}
            if(blockName == "WaterNoFlow"){ return -26;}
            if(blockName == "WetBark"){ return 27;}

            UnityEngine.Debug.LogError("Unknown block name " + blockName);
            throw new System.ArgumentOutOfRangeException("Unknown block name " + blockName);
        }
    }
        
}