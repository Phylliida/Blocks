using Blocks;

namespace Example_pack {
    public class Example : BlockCollection
    {
        public static BlockValue AutoMiner;
        public static BlockValue Axe;
        public static BlockValue BallTrack;
        public static BlockValue BallTrackEmpty;
        public static BlockValue BallTrackTurnEmpty;
        public static BlockValue BallTrackTurnFull;
        public static BlockValue BallTrackXEmpty;
        public static BlockValue BallTrackXFull;
        public static BlockValue BallTrackZEmpty;
        public static BlockValue BallTrackZFull;
        public static BlockValue Bark;
        public static BlockValue Barrel;
        public static BlockValue Bedrock;
        public static BlockValue Cheese;
        public static BlockValue Chest;
        public static BlockValue Clay;
        public static BlockValue Coal;
        public static BlockValue CoalOre;
        public static BlockValue ConveyerBelt;
        public static BlockValue CraftingTable;
        public static BlockValue Dirt;
        public static BlockValue Flower;
        public static BlockValue FlowerBlue;
        public static BlockValue FlowerWithNectar;
        public static BlockValue Furnace;
        public static BlockValue Grass;
        public static BlockValue Iron;
        public static BlockValue IronOre;
        public static BlockValue LargeRock;
        public static BlockValue LargeSharpRock;
        public static BlockValue Lava;
        public static BlockValue Leaf;
        public static BlockValue Light;
        public static BlockValue LooseRocks;
        public static BlockValue Observer;
        public static BlockValue Pickaxe;
        public static BlockValue Redstone;
        public static BlockValue RedstoneTorch;
        public static BlockValue RedstoneTorchOnSide;
        public static BlockValue Rock;
        public static BlockValue Sand;
        public static BlockValue Sapling;
        public static BlockValue SharpRock;
        public static BlockValue Shovel;
        public static BlockValue Stick;
        public static BlockValue Stone;
        public static BlockValue String;
        public static BlockValue Trunk;
        public static BlockValue Water;
        public static BlockValue WaterNoFlow;
        public static BlockValue WetBark;
        // This needed to be added to sync with Unity, since for some reason unity initializes
        // static fields on a seperate thread so sometimes we didn't finish initializing these
        // before Update was already being called for some classes
        public static void InitBlocks()
        {
            AutoMiner = new BlockValue(true, "AutoMiner", "Example");
            Axe = new BlockValue(true, "Axe", "Example");
            BallTrack = new BlockValue(true, "BallTrack", "Example");
            BallTrackEmpty = new BlockValue(false, "BallTrackEmpty", "Example");
            BallTrackTurnEmpty = new BlockValue(true, "BallTrackTurnEmpty", "Example");
            BallTrackTurnFull = new BlockValue(true, "BallTrackTurnFull", "Example");
            BallTrackXEmpty = new BlockValue(true, "BallTrackXEmpty", "Example");
            BallTrackXFull = new BlockValue(true, "BallTrackXFull", "Example");
            BallTrackZEmpty = new BlockValue(true, "BallTrackZEmpty", "Example");
            BallTrackZFull = new BlockValue(true, "BallTrackZFull", "Example");
            Bark = new BlockValue(false, "Bark", "Example");
            Barrel = new BlockValue(true, "Barrel", "Example");
            Bedrock = new BlockValue(false, "Bedrock", "Example");
            Cheese = new BlockValue(false, "Cheese", "Example");
            Chest = new BlockValue(false, "Chest", "Example");
            Clay = new BlockValue(false, "Clay", "Example");
            Coal = new BlockValue(true, "Coal", "Example");
            CoalOre = new BlockValue(false, "CoalOre", "Example");
            ConveyerBelt = new BlockValue(true, "ConveyerBelt", "Example");
            CraftingTable = new BlockValue(false, "CraftingTable", "Example");
            Dirt = new BlockValue(false, "Dirt", "Example");
            Flower = new BlockValue(true, "Flower", "Example");
            FlowerBlue = new BlockValue(false, "FlowerBlue", "Example");
            FlowerWithNectar = new BlockValue(false, "FlowerWithNectar", "Example");
            Furnace = new BlockValue(false, "Furnace", "Example");
            Grass = new BlockValue(false, "Grass", "Example");
            Iron = new BlockValue(true, "Iron", "Example");
            IronOre = new BlockValue(false, "IronOre", "Example");
            LargeRock = new BlockValue(true, "LargeRock", "Example");
            LargeSharpRock = new BlockValue(true, "LargeSharpRock", "Example");
            Lava = new BlockValue(false, "Lava", "Example");
            Leaf = new BlockValue(false, "Leaf", "Example");
            Light = new BlockValue(true, "Light", "Example");
            LooseRocks = new BlockValue(false, "LooseRocks", "Example");
            Observer = new BlockValue(false, "Observer", "Example");
            Pickaxe = new BlockValue(true, "Pickaxe", "Example");
            Redstone = new BlockValue(true, "Redstone", "Example");
            RedstoneTorch = new BlockValue(true, "RedstoneTorch", "Example");
            RedstoneTorchOnSide = new BlockValue(true, "RedstoneTorchOnSide", "Example");
            Rock = new BlockValue(true, "Rock", "Example");
            Sand = new BlockValue(false, "Sand", "Example");
            Sapling = new BlockValue(true, "Sapling", "Example");
            SharpRock = new BlockValue(true, "SharpRock", "Example");
            Shovel = new BlockValue(true, "Shovel", "Example");
            Stick = new BlockValue(true, "Stick", "Example");
            Stone = new BlockValue(false, "Stone", "Example");
            String = new BlockValue(true, "String", "Example");
            Trunk = new BlockValue(false, "Trunk", "Example");
            Water = new BlockValue(true, "Water", "Example");
            WaterNoFlow = new BlockValue(true, "WaterNoFlow", "Example");
            WetBark = new BlockValue(false, "WetBark", "Example");
        }

    }

}