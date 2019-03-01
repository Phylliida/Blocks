using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Blocks
{
    [System.Serializable]
    public class SavedBlockValues
    {
        public string[] blockNames;
        public int[] blockIds;
        public string[] blockPacks;

        public SavedBlockValues()
        {

        }

        public SavedBlockValues(BlockValue[] allBlocks)
        {
            List<string> blockNames = new List<string>();
            List<int> blockIds = new List<int>();
            List<string> blockPacks = new List<string>();
            for (int i = 0; i < allBlocks.Length; i++)
            {
                if (allBlocks[i] != null)
                {
                    blockNames.Add(allBlocks[i].name);
                    blockIds.Add(allBlocks[i].id);
                    blockPacks.Add(allBlocks[i].packName);
                }
            }
            this.blockNames = blockNames.ToArray();
            this.blockIds = blockIds.ToArray();
            this.blockPacks = blockPacks.ToArray();
        }
    }

    public abstract class BlockCollection
    {
        public static BlockValue Air { get { return BlockValue.Air; } private set { } }
        public static BlockValue Wildcard { get { return BlockValue.Wildcard; } private set { } }
    }


    public class BlockValue
    {
        public static implicit operator BlockValue(int x)
        {
            return GetBlockValue(x);
        }

        public static implicit operator int(BlockValue x)
        {
            return x.id;
        }


        public static Dictionary<BlockValue, BlockModel> customModels = new Dictionary<BlockValue, BlockModel>();

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is BlockValue)
            {
                BlockValue item = (BlockValue)obj;
                return Equals(item);
            }
            else
            {
                return id.Equals(obj);
            }
        }

        public static int BlockNameToId(string blockName)
        {
            if (nameToBlockId.ContainsKey(blockName))
            {
                return nameToBlockId[blockName];
            }
            else
            {
                UnityEngine.Debug.LogError("Unknown block name " + blockName);
                throw new System.ArgumentOutOfRangeException("Unknown block name " + blockName);
            }
        }

        public static string IdToBlockName(int id)
        {
            int uid = System.Math.Abs(id);
            if (uid < allBlocks.Length)
            {
                if (allBlocks[uid] == null)
                {
                    return "Unknown block id " + id;
                }
                else
                {
                    return allBlocks[uid].name;
                }
            }
            else
            {
                return "Unknown block id " + id;
            }
        }

        public bool Equals(BlockValue other)
        {
            return this.id == other.id;
        }

        // see https://msdn.microsoft.com/en-us/library/ms173147.aspx
        public static bool operator ==(BlockValue a, BlockValue b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }


        public static bool operator !=(BlockValue a, BlockValue b)
        {
            return !(a == b);
        }

        public static bool operator ==(BlockValue a, int b)
        {
            if ((object)a == null)
            {
                return false;
            }
            return a.id == b;
        }


        public static bool operator !=(BlockValue a, int b)
        {
            return !(a == b);
        }



        public static bool operator ==(int a, BlockValue b)
        {
            if ((object)b == null)
            {
                return false;
            }
            return a == b.id;
        }


        public static bool operator !=(int a, BlockValue b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public string name;
        public string packName;
        int _id;
        public int id { get { return _id; } private set { } }


        static int largestIdMag;

        public BlockValue(bool transparent, string name = "", string packName = "")
        {
            this.name = name;
            if (allBlocks == null)
            {
                allBlocks = new BlockValue[initialSize];
                nameToBlockId = new Dictionary<string, int>();
                largestIdMag = 2;
            }
            largestIdMag += 1;
            if (transparent)
            {
                AddNewBlock(-largestIdMag, name, packName);
            }
            else
            {
                AddNewBlock(largestIdMag, name, packName);
            }
        }

        public string GetBlockTexturePath(string packName, string blockName)
        {
            string assetsPath = UnityEngine.Application.dataPath;
            // strip any trailing path seperators
            while (assetsPath[assetsPath.Length - 1] == '/' || assetsPath[assetsPath.Length - 1] == '\\')
            {
                assetsPath = assetsPath.Substring(0, assetsPath.Length - 1);
            }
            // append pack textures directory
            return assetsPath.Replace("\\", "/") + "/PackTextures/" + packName + "/" + blockName + ".png";
        }

        public string GetBlockModelPath(string packName, string blockName)
        {
            string assetsPath = UnityEngine.Application.dataPath;
            // strip any trailing path seperators
            while (assetsPath[assetsPath.Length - 1] == '/' || assetsPath[assetsPath.Length - 1] == '\\')
            {
                assetsPath = assetsPath.Substring(0, assetsPath.Length - 1);
            }
            // append pack textures directory
            return assetsPath.Replace("\\", "/") + "/PackTextures/" + packName + "/" + blockName + "/" + "model.json";
        }


        public BlockValue(int id, string name = "", string packName = "")
        {
            this.name = name;
            AddNewBlock(id, name, packName);
        }

        public const int animFrames = 64;
        void SetupTexture()
        {
            allBlocksTexture = new Texture2D(16 * 2* animFrames, 16 * 3 * allBlocks.Length, TextureFormat.ARGB32, false, true);
            allBlocksTexture.filterMode = FilterMode.Point;
            allBlocksTexture.wrapMode = TextureWrapMode.Clamp;
            allBlocksTexture.anisoLevel = 0;
            Color32[] allColors = new Color32[allBlocksTexture.width * allBlocksTexture.height];
            for (int i = 0; i < allBlocksTexture.height; i++)
            {
                for (int j = 0; j < allBlocksTexture.width; j++)
                {
                    int ind = i * allBlocksTexture.width + j;
                    if (i <= 1 || i >= allBlocksTexture.height-2)
                    {
                        allColors[ind] = new Color32(0, 0, 0, 255); // air and wildcard (first and last one due to offset by 1) get default black
                    }
                    else
                    {
                        allColors[ind] = new Color32(255, 0, 255, 255); // default gross pink/purple color if block not found
                    }
                }
            }
            allBlocksTexture.SetPixels32(allColors);
            allBlocksTexture.Apply();
        }

        public const int initialSize = 64;

        void AddNewBlock(int id, string name, string packName = "")
        {
            this.name = name;
            this.packName = packName;
            string texturePath = "";
            if (packName != "")
            {
                texturePath = GetBlockTexturePath(packName, name);
            }
            if (allBlocks == null)
            {
                allBlocks = new BlockValue[initialSize];
                nameToBlockId = new Dictionary<string, int>();
                largestIdMag = 2;
                SetupTexture();
            }
            nameToBlockId[name] = id;
            int uid = System.Math.Abs(id);
            // increase number of blocks until we have enough
            while (allBlocks.Length <= uid)
            {
                BlockValue[] newAllBlocks = new BlockValue[allBlocks.Length * 2];
                for (int i = 0; i < allBlocks.Length; i++)
                {
                    newAllBlocks[i] = allBlocks[i];
                }
                allBlocks = newAllBlocks;
                // if we are done doubling the size, create the texture
                if (allBlocks.Length > uid)
                {
                    SetupTexture();
                }
            }
            if (allBlocks[uid] != null)
            {
                UnityEngine.Debug.LogWarning("warning: multiple blocks have id" + uid + "(technically " + id + " with transparency flag)");
            }
            allBlocks[uid] = this;

            _id = id;

            largestIdMag = System.Math.Max(largestIdMag, uid);
            Debug.Log(name + " has id " + id);

            string modelPath = "";
            if (packName != "")
            {
                modelPath = GetBlockModelPath(packName, name);
            }

            if (modelPath != "" && File.Exists(modelPath))
            {
                BlockModel model = BlockModel.FromJSONFilePath(modelPath);
                Texture2D[] textures = model.GetTextures();
                for (int i = 0; i < textures.Length; i++)
                {
                    Color[] pixels = textures[i].GetPixels();
                    allBlocksTexture.SetPixels(i * 32, (uid - 1) * 16 * 3, 16 * 2, 16 * 3, pixels);
                }
                allBlocksTexture.Apply();
                File.WriteAllBytes("res.png", allBlocksTexture.EncodeToPNG());

                if (id > 0)
                {
                    _id = -uid;
                    Debug.LogWarning("warning: detected custom model for block " + name + " with updated id " + id + " but we were told it does not have a custom model, set it to custom model anyway");
                }

                customModels[_id] = model;
            }

            else if (texturePath != "")
            {
                if (File.Exists(texturePath))
                {
                    byte[] imageData = File.ReadAllBytes(texturePath);
                    Texture2D blockTexture = new Texture2D(2, 2);
                    blockTexture.LoadImage(imageData); // (will automatically resize as needed)
                    blockTexture.Apply();

                    // convert to argb32
                    Texture2D argbTexture = new Texture2D(blockTexture.width, blockTexture.height, TextureFormat.ARGB32, false, true);
                    argbTexture.SetPixels(blockTexture.GetPixels());
                    argbTexture.Apply();
                    Color32[] argbColors = argbTexture.GetPixels32();

                    // check for transparency
                    bool isTransparent = false;
                    for (int j = 0; j < argbColors.Length; j++)
                    {
                        if (argbColors[j].a < 240) // if it is only 240/255 or higher it isn't noticable enough to actually use the transparency
                        {
                            isTransparent = true;
                            break;
                        }
                    }


                    // sign of id should match transparency, fix if not
                    if (isTransparent)
                    {
                        if (id > 0)
                        {
                            _id = -uid;
                            Debug.LogWarning("warning: detected transparent texture for block " + name + " with updated id " + id + " and texture path " + texturePath + " but we were told it is not transparent, set it to transparent anyway");
                        }
                    }
                    else
                    {
                        if (id < 0)
                        {
                            _id = uid;
                            Debug.LogWarning("warning: detected not transparent texture for block " + name + " with updated id " + id + " and texture path " + texturePath + " but we were told it is transparent, set it to not transparent anyway");
                        }
                    }



                    if (argbTexture.width != 16 * 2 || argbTexture.height != 16 * 3)
                    {
                        Debug.Log("rescaling texture of block " + name + " with id " + id + " and texture path " + texturePath);
                        TextureScale.Bilinear(argbTexture, 16 * 2, 16 * 3);
                    }
                    argbTexture.Apply();
                    Color[] pixels = argbTexture.GetPixels();
                    for (int k = 0; k < animFrames; k++)
                    {
                        allBlocksTexture.SetPixels(k*32, (uid - 1) * 16 * 3, 16 * 2, 16 * 3, pixels);
                    }
                    allBlocksTexture.Apply();
                    File.WriteAllBytes("res.png", allBlocksTexture.EncodeToPNG());
                }
                else
                {
                    //bool animated = false;
                    // test if animated
                    for (int i = 0; i < 32; i++)
                    {
                        string frameITexture = texturePath.Substring(0, texturePath.Length - ".png".Length) + i + ".png";
                        if (File.Exists(frameITexture))
                        {
                            //animated = true;

                            byte[] imageData = File.ReadAllBytes(frameITexture);
                            Texture2D blockTexture = new Texture2D(2, 2);
                            blockTexture.LoadImage(imageData); // (will automatically resize as needed)
                            blockTexture.Apply();

                            // convert to argb32
                            Texture2D argbTexture = new Texture2D(blockTexture.width, blockTexture.height, TextureFormat.ARGB32, false, true);
                            argbTexture.SetPixels(blockTexture.GetPixels());
                            argbTexture.Apply();
                            Color32[] argbColors = argbTexture.GetPixels32();

                            // check for transparency
                            bool isTransparent = false;
                            for (int j = 0; j < argbColors.Length; j++)
                            {
                                if (argbColors[j].a < 240) // if it is only 240/255 or higher it isn't noticable enough to actually use the transparency
                                {
                                    isTransparent = true;
                                    break;
                                }
                            }


                            // sign of id should match transparency, fix if not
                            if (isTransparent)
                            {
                                if (id > 0)
                                {
                                    _id = -uid;
                                    Debug.LogWarning("warning: detected transparent texture for block " + name + " with updated id " + id + " and texture path " + texturePath + " but we were told it is not transparent, set it to transparent anyway");
                                }
                            }
                            else
                            {
                                if (id < 0)
                                {
                                    _id = uid;
                                    Debug.LogWarning("warning: detected not transparent texture for block " + name + " with updated id " + id + " and texture path " + texturePath + " but we were told it is transparent, set it to not transparent anyway");
                                }
                            }



                            if (argbTexture.width != 16 * 2 || argbTexture.height != 16 * 3)
                            {
                                Debug.Log("rescaling texture of block " + name + " with id " + id + " and texture path " + texturePath);
                                TextureScale.Bilinear(argbTexture, 16 * 2, 16 * 3);
                            }
                            argbTexture.Apply();
                            Color[] pixels = argbTexture.GetPixels();
                            // offset x by frame count
                            allBlocksTexture.SetPixels(i*16*2, (uid - 1) * 16 * 3, 16 * 2, 16 * 3, pixels);
                            allBlocksTexture.Apply();
                        }
                    }
                    Debug.LogWarning("warning: texture " + texturePath + " for block " + name + " with id " + id + " does not exist");
                }
            }
        }

        public static BlockValue GetBlockValue(int id)
        {
            int uid = System.Math.Abs(id);
            if (allBlocks[uid] == null)
            {
                throw new System.ArgumentOutOfRangeException("block value with id " + uid + " (technically " + id + " with transparency flag) " + " does not exist");
            }
            return allBlocks[uid];
        }


        public static string SaveIdConfigToJsonString()
        {
            string res = JsonConvert.SerializeObject(new SavedBlockValues(allBlocks));
            return res;
        }


        public static void LoadIdConfigFromJsonString(string idConfig)
        {
            // remove any previous blocks
            BlockValue[] prevBlocks = allBlocks;
            Dictionary<string, BlockValue> allPrevBlocks = new Dictionary<string, BlockValue>();
            for (int i = 0; i < prevBlocks.Length; i++)
            {
                if (prevBlocks[i] != null && prevBlocks[i].name != "")
                {
                    allPrevBlocks[prevBlocks[i].packName + ":" + prevBlocks[i].name] = prevBlocks[i];
                }
            }
            allBlocks = null;
            nameToBlockId = null;
            allBlocksTexture = null;
            largestIdMag = 0;

            Air = new BlockValue(0, "Air");
            Wildcard = new BlockValue(-1, "Wildcard");

            SavedBlockValues blockValues = JsonConvert.DeserializeObject<SavedBlockValues>(idConfig);

            for (int i = 0; i < blockValues.blockIds.Length; i++)
            {
                int blockId = blockValues.blockIds[i];
                string blockName = blockValues.blockNames[i];
                string blockPack = blockValues.blockPacks[i];

                BlockValue block;
                // we already took care of this above
                if (blockName == "Air")
                {
                    block = Air;
                }
                // we already took care of this above
                else if (blockName == "Wildcard")
                {
                    block = Wildcard;
                }
                // create the block and force the block id
                // note that if the texture doesn't exist anymore, this will force a cross pink rgb(255,0,255) texture to be used instead which should be fairly noticable
                else
                {
                    block = new BlockValue(blockId, blockName, blockPack);
                }

                string packBlockKey = block.packName + ":" + block.name;

                // update the id of the reference and no longer worry about it (since it is updated)
                if (allPrevBlocks.ContainsKey(packBlockKey))
                {
                    BlockValue prevBlock = allPrevBlocks[packBlockKey];
                    prevBlock._id = blockId;
                    allPrevBlocks.Remove(packBlockKey);
                }
            }

            // for all of the old things we haven't assigned to new ids yet, create them
            foreach (KeyValuePair<string, BlockValue> blockNotInSave in allPrevBlocks)
            {
                string blockName = blockNotInSave.Key;
                BlockValue block = blockNotInSave.Value;

                BlockValue newBlock = new BlockValue(block.id < 0, block.name, block.packName);
                block._id = newBlock.id;
            }
        }

        public static Dictionary<string, int> nameToBlockId;

        public static Texture2D allBlocksTexture;

        public static BlockValue[] allBlocks;

        public static BlockValue Air = new BlockValue(0, "Air");
        public static BlockValue Wildcard = new BlockValue(-1, "Wildcard");
    }




    public class BlockUtils
    {
        public static string BlockIdToString(int blockId)
        {
            return BlockValue.IdToBlockName(blockId);
        }

        public static int StringToBlockId(string blockName)
        {
            return BlockValue.BlockNameToId(blockName);
        }
    }
}