using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I'll just use this format for the convienence of minecraft modders
// https://minecraft.gamepedia.com/Model#Block_models
// As a result, the comments here are directly copied from that article. Hopefully that's ok? I'm not sure, let me know if I need to remove them.
namespace Blocks
{

    public class CubeMesh
    {
        public CubeMesh(CubeMesh from)
        {
            vertices = new Vector3[from.vertices.Length];
            uvs = new Vector2[from.uvs.Length];

            for (int i = 0; i < from.vertices.Length; i++)
            {
                vertices[i] = from.vertices[i];
            }

            for (int i = 0; i < from.uvs.Length; i++)
            {
                uvs[i] = from.uvs[i];
            }
        }

        public CubeMesh()
        {
            RenderTriangle[] cubeTris = World.mainWorld.defaultCubeTris;
            vertices = new Vector3[cubeTris.Length*3];
            uvs = new Vector2[cubeTris.Length*3];
            for (int i = 0; i < cubeTris.Length; i++)
            {
                vertices[i * 3] = new Vector3(cubeTris[i].vertex1.x, cubeTris[i].vertex1.y, cubeTris[i].vertex1.z);
                vertices[i * 3 + 1] = new Vector3(cubeTris[i].vertex2.x, cubeTris[i].vertex2.y, cubeTris[i].vertex2.z);
                vertices[i * 3 + 2] = new Vector3(cubeTris[i].vertex3.x, cubeTris[i].vertex3.y, cubeTris[i].vertex3.z);
                uvs[i * 3] = new Vector2(cubeTris[i].uv1.x, cubeTris[i].uv1.y);
                uvs[i * 3 + 1] = new Vector2(cubeTris[i].uv2.x, cubeTris[i].uv2.y);
                uvs[i * 3 + 2] = new Vector2(cubeTris[i].uv3.x, cubeTris[i].uv3.y);
            }
        }

        public Vector3[] vertices;
        public Vector2[] uvs;


        public delegate Vector3 TransformPoint(Vector3 point);
        public CubeMesh ApplyingTransformationToEachVertex(TransformPoint transformation)
        {
            CubeMesh res = new CubeMesh(this);
            for (int i = 0; i < vertices.Length; i++)
            {
                res.vertices[i] = transformation(vertices[i]);
            }
            return res;
        }


        public static CubeMesh operator+(Vector3 offset, CubeMesh cubeMesh)
        {
            CubeMesh res = new CubeMesh(cubeMesh);
            for (int i = 0; i < cubeMesh.vertices.Length; i++)
            {
                res.vertices[i] += offset;
            }
            return res;
        }

        public static CubeMesh operator +(CubeMesh cubeMesh, Vector3 offset)
        {
            return offset + cubeMesh;
        }


        public static CubeMesh operator *(Vector3 scale, CubeMesh cubeMesh)
        {
            CubeMesh res = new CubeMesh(cubeMesh);
            for (int i = 0; i < cubeMesh.vertices.Length; i++)
            {
                res.vertices[i] = new Vector3(res.vertices[i].x * scale.x, res.vertices[i].y * scale.y, res.vertices[i].z * scale.z);
            }
            return res;
        }

        public static CubeMesh operator *(CubeMesh cubeMesh, Vector3 scale)
        {
            return scale * cubeMesh;
        }


        public static CubeMesh operator -(CubeMesh cubeMesh, Vector3 offset)
        {
            return (-offset) + cubeMesh;
        }

        public static CubeMesh operator *(float val, CubeMesh cubeMesh)
        {
            CubeMesh res = new CubeMesh(cubeMesh);
            for (int i = 0; i < cubeMesh.vertices.Length; i++)
            {
                res.vertices[i] *= val;
            }
            return res;
        }


        public static CubeMesh operator *(CubeMesh cubeMesh, float val)
        {
            return val * cubeMesh;
        }


        public static CubeMesh operator /(CubeMesh cubeMesh, float val)
        {
            return (1.0f/val) * cubeMesh;
        }





        public static CubeMesh operator +(Vector2 offset, CubeMesh cubeMesh)
        {
            CubeMesh res = new CubeMesh(cubeMesh);
            for (int i = 0; i < cubeMesh.uvs.Length; i++)
            {
                res.uvs[i] += offset;
            }
            return res;
        }

        public static CubeMesh operator +(CubeMesh cubeMesh, Vector2 offset)
        {
            return offset + cubeMesh;
        }


        public static CubeMesh operator *(Vector2 scale, CubeMesh cubeMesh)
        {
            CubeMesh res = new CubeMesh(cubeMesh);
            for (int i = 0; i < cubeMesh.uvs.Length; i++)
            {
                res.uvs[i] = new Vector2(res.uvs[i].x * scale.x, res.uvs[i].y * scale.y);
            }
            return res;
        }

        public static CubeMesh operator *(CubeMesh cubeMesh, Vector2 scale)
        {
            return scale * cubeMesh;
        }


        public static CubeMesh operator -(CubeMesh cubeMesh, Vector2 offset)
        {
            return (-offset) + cubeMesh;
        }


        public Blocks.RenderTriangle[] ToRenderTriangles()
        {
            RenderTriangle[] res = new RenderTriangle[vertices.Length / 3];
            for (int i = 0; i < res.Length; i++)
            {
                //Debug.Log(vertices[i * 3].x + " " + vertices[i * 3].y + " " + vertices[i * 3].z);
                //Debug.Log(vertices[i * 3+1].x + " " + vertices[i * 3+1].y + " " + vertices[i * 3+1].z);
                //Debug.Log(vertices[i * 3+2].x + " " + vertices[i * 3+2].y + " " + vertices[i * 3+2].z);
                int pos = i * 3;
                RenderTriangle cur = new RenderTriangle();
                cur.vertex1 = Util.MakeFloat4(vertices[pos].x, vertices[pos].y, vertices[pos].z, 0);
                cur.vertex2 = Util.MakeFloat4(vertices[pos + 1].x, vertices[pos + 1].y, vertices[pos + 1].z, 0);
                cur.vertex3 = Util.MakeFloat4(vertices[pos + 2].x, vertices[pos + 2].y, vertices[pos + 2].z, 0);
                cur.uv1 = Util.MakeFloat2(uvs[pos].x, uvs[pos].y);
                cur.uv2 = Util.MakeFloat2(uvs[pos+1].x, uvs[pos+1].y);
                cur.uv3 = Util.MakeFloat2(uvs[pos+2].x, uvs[pos+2].y);
                res[i] = cur;
            }
            return res;
        }
    }


    public class BlockModelDisplay
    {

    }

    [System.Serializable]
    public class BlockModelFace
    {
        /// <summary>
        /// Defines the area of the texture to use according to the scheme [x1, y1, x2, y2]. If unset, it defaults to values equal to xyz position of the element. The texture behavior will be inconsistent if UV extends below 0 or above 16. If the numbers of x1 and x2 are swapped (e.g. from 0, 0, 16, 16 to 16, 0, 0, 16), the texture will be flipped. UV is optional, and if not supplied it will automatically generate based on the element's position.
        /// </summary>
        public float[] uv;

        /// <summary>
        ///  Specifies the texture in form of the texture variable prepended with a #.
        /// </summary>
        public string texture;

        /// <summary>
        /// Specifies whether a face does not need to be rendered when there is a block touching it in the specified position. The position can be: down, up, north, south, west, or east. It will also determine which side of the block to use the light level from for lighting the face, and if unset, defaults to the side.
        /// </summary>
        public bool cullface;

        /// <summary>
        ///  Rotates the texture by the specified number of degrees. Can be 0, 90, 180, or 270. Defaults to 0. Rotation does not affect which part of the texture is used. Instead, it amounts to permutation of the selected texture vertexes (selected implicitly, or explicitly though uv).
        /// </summary>
        public int rotation;

        /// <summary>
        /// Determines whether to tint the texture using a hardcoded tint index. The default is not using the tint, and any number causes it to use tint. Note that only certain blocks have a tint index, all others will be unaffected.
        /// </summary>
        public int tintindex;


        string __comment = "";
    }

    [System.Serializable]
    public class BlockModelRotation
    {
        /// <summary>
        /// Sets the center of the rotation according to the scheme [x, y, z].
        /// </summary>
        public float[] origin = new float[] { 0, 0, 0 };

        /// <summary>
        /// Specifies the direction of rotation, can be "x", "y" or "z".
        /// </summary>
        public string axis = "x";

        /// <summary>
        /// Specifies the angle of rotation. Can be 45 through -45 degrees in 22.5 degree increments.
        /// </summary>
        public float angle = 0;

        /// <summary>
        /// Specifies whether or not to scale the faces across the whole block. Can be true or false. Defaults to false.
        /// </summary>
        public bool rescale = false;

        // from http://answers.unity.com/answers/1306696/view.html
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            return rotation * (point - pivot) + pivot;
        }

        public Vector3 ApplyToPoint(Vector3 point)
        {
            if (angle == 0)
            {
                return point;
            }
            Vector3 pivot = new Vector3(origin[0], origin[1], origin[2]);
            return RotatePointAroundPivot(point, pivot, ToQuat());
        }

        Quaternion ToQuat()
        {
            Vector3 vecAxis = Vector3.up;
            if (axis.ToLower() == "x")
            {
                vecAxis = new Vector3(1, 0, 0);
            }
            else if(axis.ToLower() == "y")
            {
                vecAxis = new Vector3(0, 1, 0);
            }
            else if(axis.ToLower() == "z")
            {
                vecAxis = new Vector3(0, 0, 1);
            }
            // -45 to 45 -> 0 to 1
            float angle01 = Mathf.Clamp01((angle + 45) / 90.0f);
            // 0 to 1 -> 0 to 3
            float angle03 = angle01 * 3;
            // round to nearst int, will result in 0, 1, 2 or 3
            float actualAngle03 = Mathf.Round(angle03);
            // 0 to 3 -> 0 to 1
            float resAngle01 = actualAngle03 / 3;
            // 0 to 1 -> -45 to 45
            float resAngleInDegrees = resAngle01 * 90 - 45;

            return Quaternion.AngleAxis(resAngleInDegrees, vecAxis);
        }



        string __comment = "";
    }


    [System.Serializable]
    public class BlockModelElement
    {
        /// <summary>
        /// Start point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
        /// </summary>
        public float[] from;

        /// <summary>
        /// Stop point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
        /// </summary>
        public float[] to;

        public BlockModelRotation rotation;



        string __comment = "";


        public Blocks.RenderTriangle[] ToRenderTriangles()
        {
            CubeMesh res = new CubeMesh();

            // -0.5 to 0.5 -> 0 to 1
            //res = res + new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 fromPos = new Vector3(from[0], from[1], from[2]);
            Vector3 toPos = new Vector3(to[0], to[1], to[2]);

            // -16 to 32 -> -1 to 2
            //fromPos = fromPos + new Vector3(16, 16, 16);
            fromPos /= 16;
            //toPos = toPos + new Vector3(16, 16, 16);
            toPos /= 16;
            // (0,0,0) to (16,16,16) is a regular cube (0 to 1, 0 to 1, 0 to 1) in minecraft


            ////// (0,0,0) to (1,1,1) -> fromPos to toPos /////
            Vector3 diff = toPos - fromPos;
            res = res * diff + fromPos;

            if (rotation != null)
            {
                res = res.ApplyingTransformationToEachVertex(x => { return rotation.ApplyToPoint(x); });
            }

            //res = res * new Vector2(1/64.0f, 1.0f/256.0f);

            res = res * new Vector2(1.0f, 1.0f);

            

            // 0 to 1 -> -0.5 to 0.5
            //res = res - new Vector3(0.5f, 0.5f, 0.5f);

            return res.ToRenderTriangles();


        }
    }



    [System.Serializable]
    public class BlockModel
    {
        /// <summary>
        /// Loads a different model from the given path, starting in assets/<namespace>/models. If both "parent" and "elements" are set, the "elements" tag overrides the "elements" tag from the previous model.
        /// Can be set to "builtin/generated" to use a model that is created out of the specified icon.Note that only the first layer is supported, and rotation can only be achieved using block states files.
        /// </summary>
        public string parent = "";

        /// <summary>
        /// Whether to use ambient occlusion (true - default), or not (false).
        /// </summary>
        public bool ambientocclusion = true;

        // display todo

        /// <summary>
        ///  Holds the textures of the model. Each texture starts in assets/<namespace>/textures or can be another texture variable.
        /// </summary>
        public Dictionary<string, string> textures = new Dictionary<string, string>();

        /// <summary>
        /// Contains all the elements of the model. they can only have cubic forms. If both "parent" and "elements" are set, the "elements" tag overrides the "elements" tag from the previous model.
        /// </summary>
        public BlockModelElement[] elements;

        string __comment = "";


        public BlockModel()
        {

        }

        public static BlockModel FromJSONFilePath(string jsonPath)
        {
            return JsonUtility.FromJson<BlockModel>(System.IO.File.ReadAllText(jsonPath));
        }


        public RenderTriangle[] ToRenderTriangles()
        {
            List<RenderTriangle> results = new List<RenderTriangle>();

            if (parent != "")
            {
                results.AddRange(FromJSONFilePath(parent).ToRenderTriangles());
            }
            foreach (BlockModelElement element in elements)
            {
                results.AddRange(element.ToRenderTriangles());
            }
            return results.ToArray();
        }


    }

}
