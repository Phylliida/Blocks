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
        Grass=9,
        LargeRock=-10,
        LargeSharpRock=-11,
        Leaf=12,
        LooseRocks=13,
        Pickaxe=-14,
        Rock=-15,
        Sand=16,
        SharpRock=-17,
        Shovel=-18,
        Stick=-19,
        Stone=20,
        String=-21,
        Trunk=22,
        Water=-23,
        WaterNoFlow=-24,
        WetBark=25
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
            if(blockId == 9){ return "Grass";}
            if(blockId == 10){ return "LargeRock";}
            if(blockId == 11){ return "LargeSharpRock";}
            if(blockId == 12){ return "Leaf";}
            if(blockId == 13){ return "LooseRocks";}
            if(blockId == 14){ return "Pickaxe";}
            if(blockId == 15){ return "Rock";}
            if(blockId == 16){ return "Sand";}
            if(blockId == 17){ return "SharpRock";}
            if(blockId == 18){ return "Shovel";}
            if(blockId == 19){ return "Stick";}
            if(blockId == 20){ return "Stone";}
            if(blockId == 21){ return "String";}
            if(blockId == 22){ return "Trunk";}
            if(blockId == 23){ return "Water";}
            if(blockId == 24){ return "WaterNoFlow";}
            if(blockId == 25){ return "WetBark";}

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
            if(blockName == "Grass"){ return 9;}
            if(blockName == "LargeRock"){ return -10;}
            if(blockName == "LargeSharpRock"){ return -11;}
            if(blockName == "Leaf"){ return 12;}
            if(blockName == "LooseRocks"){ return 13;}
            if(blockName == "Pickaxe"){ return -14;}
            if(blockName == "Rock"){ return -15;}
            if(blockName == "Sand"){ return 16;}
            if(blockName == "SharpRock"){ return -17;}
            if(blockName == "Shovel"){ return -18;}
            if(blockName == "Stick"){ return -19;}
            if(blockName == "Stone"){ return 20;}
            if(blockName == "String"){ return -21;}
            if(blockName == "Trunk"){ return 22;}
            if(blockName == "Water"){ return -23;}
            if(blockName == "WaterNoFlow"){ return -24;}
            if(blockName == "WetBark"){ return 25;}

            UnityEngine.Debug.LogError("Unknown block name " + blockName);
            throw new System.ArgumentOutOfRangeException("Unknown block name " + blockName);
        }
    }
        
}