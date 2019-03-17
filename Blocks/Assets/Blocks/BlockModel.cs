using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            RenderTriangle[] cubeTris = World.defaultCubeTris;
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
        /// 
        [DefaultValue(new double[] { 0, 0, 0 })]
        public double[] origin;

        /// <summary>
        /// Specifies the direction of rotation, can be "x", "y" or "z".
        /// </summary>
        [DefaultValue("y")]
        public string axis;

        /// <summary>
        /// Specifies the angle of rotation. Can be 45 through -45 degrees in 22.5 degree increments.
        /// </summary>
        [DefaultValue(0)]
        public double angle;

        /// <summary>
        /// Specifies whether or not to scale the faces across the whole block. Can be true or false. Defaults to false.
        /// </summary>
        [DefaultValue(false)]
        public bool rescale;

        // from http://answers.unity.com/answers/1306696/view.html
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            return rotation * (point - pivot) + pivot;
        }

        public bool limitAngle = false;

        public Vector3 ApplyToPoint(Vector3 point)
        {
            if (angle == 0)
            {
                return point;
            }
            // -16 to 32 -> -1 to 2 (0 through 1 is inside the block)
            Vector3 pivot = new Vector3((float)origin[0], (float)origin[1], (float)origin[2])/16.0f;
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
            float resAngleInDegrees = (float)angle;
            if (limitAngle)
            {

                // -45 to 45 -> 0 to 1
                float angle01 = Mathf.Clamp01(((float)angle + 45) / 90.0f);
                // 0 to 1 -> 0 to 3
                float angle03 = angle01 * 3;
                // round to nearst int, will result in 0, 1, 2 or 3
                float actualAngle03 = Mathf.Round(angle03);
                // 0 to 3 -> 0 to 1
                float resAngle01 = actualAngle03 / 3;
                // 0 to 1 -> -45 to 45
                resAngleInDegrees = resAngle01 * 90 - 45;
            }

            return Quaternion.AngleAxis(resAngleInDegrees, vecAxis);
        }



        string __comment = "";
    }


    [System.Serializable]
    public class BlockModelElement
    {

        public bool DependsOnState()
        {
            return fromVars != null || toVars != null;
        }

        /// <summary>
        /// Start point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
        /// </summary>
        public double[] from;

        /// <summary>
        /// Stop point of a cube according to the scheme [x, y, z]. Values must be between -16 and 32.
        /// </summary>
        public double[] to;

        [DefaultValue(null)]
        public string[] fromVars { get; set; }

        [DefaultValue(null)]
        public string[] toVars { get; set; }


        public string texture;

        [DefaultValue(null)]
        public BlockModelRotation rotation;

        public bool shade;

        public BlockModel rootModel;

        string __comment = "";

        public string[] SplitAndKeepDelims(string str, char[] delims)
        {
            List<string> result = new List<string>();
            int startOfCur = 0;
            for (int i = 0; i < str.Length; i++)
            {
                for (int j = 0; j < delims.Length; j++)
                {
                    if (str[i] == delims[j])
                    {
                        int lenOfCur = i - startOfCur;
                        if (lenOfCur > 0)
                        {
                            result.Add(str.Substring(startOfCur, lenOfCur));
                        }
                        result.Add(str[i] + "");
                        startOfCur = i + 1;
                        break;
                    }
                }
            }
            if (startOfCur < str.Length)
            {
                result.Add(str.Substring(startOfCur));
            }
            return result.ToArray();
        }

        public class ArithmeticOp
        {
            public string op;
            public int precedence;

            public enum ArithmeticOpType
            {
                Unary,
                Binary
            }

            public ArithmeticOpType opType;

            public ArithmeticOp(string op, int precedence, ArithmeticOpType opType= ArithmeticOpType.Binary)
            {
                this.op = op;
                this.precedence = precedence;
                this.opType = opType;
            }

            public override string ToString()
            {
                return op;
            }
        }

        public class ArithmeticVarible
        {
            public string name;
            public float value;
            bool isFloat = false;

            public ArithmeticVarible(string name)
            {
                this.name = name;
                if (float.TryParse(name, out this.value))
                {
                    isFloat = true;
                }
            }

            public float GetValue(int state)
            {
                if (isFloat)
                {
                    return this.value;
                }
                else
                {
                    if (name == "state")
                    {
                        return state;
                    }
                    else
                    {
                        throw new ArithmeticException("unknown variable " + name);
                    }
                }
            }

            public override string ToString()
            {
                if (isFloat)
                {
                    return this.value + "";
                }
                else
                {
                    return name;
                }
            }
        }

        public class ArithmeticParen
        {
            public bool opening;
            public ArithmeticParen(bool opening)
            {
                this.opening = opening;
            }

            public override string ToString()
            {
                if (opening)
                {
                    return "(";
                }
                else
                {
                    return ")";
                }
            }
        }

        public class ArithmeticException : System.Exception
        {
            public ArithmeticException(string message) : base(message)
            {
            }
        }

        public class ArithmeticNode
        {
            public enum ArithmeticType
            {
                Operator,
                State,
                Paren,
                MergedUnaryOperator,
                MergedBinaryOperator,
                Scope
            }

            public string Join(string[] arr, string delim)
            {
                string res = "";
                for (int i = 0; i < arr.Length; i++)
                {
                    res += arr[i];
                    if (i != arr.Length-1)
                    {
                        res += delim;
                    }
                }
                return res;
            }

            public override string ToString()
            {
                if (arithemeticType == ArithmeticType.Operator)
                {
                    return op;
                }
                else if(arithemeticType == ArithmeticType.State)
                {
                    return variable.ToString();
                }
                else if(arithemeticType == ArithmeticType.Paren)
                {
                    return paren.ToString();
                }
                else if (arithemeticType == ArithmeticType.Scope)
                {
                    List<string> res = new List<string>();
                    for (int i = 0; i < children.Count; i++)
                    {
                        res.Add(children[i].ToString());
                    }
                    return Join(res.ToArray(), " ");
                }
                else if(arithemeticType == ArithmeticType.MergedBinaryOperator)
                {
                    return "[" + children[0] + " " + op + " " + children[1] + "]";
                }
                else if(arithemeticType == ArithmeticType.MergedUnaryOperator)
                {
                    return "[- " + children[0] + "]";
                }
                else 
                {
                    return "";
                }
            }


            public float GetValue(int state)
            {
                if (arithemeticType == ArithmeticType.MergedBinaryOperator)
                {
                    float val1 = children[0].GetValue(state);
                    float val2 = children[1].GetValue(state);
                    if (op == "+")
                    {
                        return val1 + val2;
                    }
                    else if(op == "-")
                    {
                        return val1 - val2;
                    }
                    else if(op == "*")
                    {
                        return val1 * val2;
                    }
                    else if (op == "/")
                    {
                        if (val2 == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            return val1 / val2;
                        }
                    }
                    else
                    {
                        throw new ArithmeticException("unknown binary operator " + op);
                    }
                }
                else if(arithemeticType == ArithmeticType.MergedUnaryOperator)
                {
                    float val = children[0].GetValue(state);
                    if (op == "-")
                    {
                        return -val;
                    }
                    else
                    {
                        throw new ArithmeticException("unknown unary operator " + op);
                    }
                }
                else if(arithemeticType == ArithmeticType.Scope)
                {
                    if (children.Count == 0)
                    {
                        throw new ArithmeticException("paren need at least one thing inside of them");
                    }
                    else if(children.Count > 1)
                    {
                        throw new ArithmeticException("scope has more than one term inside of it, did you remember to call MergeOperators?");
                    }
                    else
                    {
                        return children[0].GetValue(state);
                    }
                }
                else if(arithemeticType == ArithmeticType.State)
                {
                    return variable.GetValue(state);
                }
                else
                {
                    throw new ArithmeticException("cannot evaluate value of node of type " + arithemeticType);
                }
            }

            public bool merged = false;

            public ArithmeticNode parent = null;
            public List<ArithmeticNode> children = new List<ArithmeticNode>();
            public ArithmeticType arithemeticType;
            public List<ArithmeticOp> ops = new List<ArithmeticOp>();
            public string op;
            public ArithmeticVarible variable;
            public ArithmeticParen paren;

            public bool IsBinaryOperator()
            {
                // if - it might be unary so check to see if it merged (if it did, it is either unary or binary, check. if it hasn't merged, assume it is binary)
                return arithemeticType == ArithmeticType.Operator && (op != "-" || !merged || (merged && parent.arithemeticType == ArithmeticType.MergedBinaryOperator));
            }

            public ArithmeticNode(ArithmeticNode parent)
            {
                this.parent = parent;
                this.arithemeticType = ArithmeticType.Scope;
            }

            public ArithmeticNode(ArithmeticNode opHolder, ArithmeticOp unaryOp, ArithmeticNode applyTo)
            {
                this.op = unaryOp.op;
                this.ops.Add(unaryOp);
                this.children.Add(applyTo);
                applyTo.parent = this;
                applyTo.merged = true;
                opHolder.parent = this;
                opHolder.merged = true;
                this.arithemeticType = ArithmeticType.MergedUnaryOperator;
            }


            public ArithmeticNode(ArithmeticNode opHolder, ArithmeticOp binaryOp, ArithmeticNode applyToA, ArithmeticNode applyToB)
            {
                this.op = binaryOp.op;
                this.ops.Add(binaryOp);
                this.children.Add(applyToA);
                applyToA.parent = this;
                applyToA.merged = true;
                applyToB.parent = this;
                applyToB.merged = true;
                opHolder.parent = this;
                opHolder.merged = true;
                this.children.Add(applyToB);
                this.arithemeticType = ArithmeticType.MergedBinaryOperator;
            }


            public void MergeOperators()
            {
                // merge operators of children recursively
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].arithemeticType == ArithmeticType.Scope)
                    {
                        children[i].MergeOperators();
                    }
                }

                // now actually do the work. First, check if unary (precedence == 2), then merge * and / (precedence == 1), then merge + and - (precedence == 0) 
                for (int curPrecedence = 2; curPrecedence >= 0; curPrecedence--)
                {
                    bool mergedSomething = true;
                    while (mergedSomething)
                    {
                        mergedSomething = false;

                        //Debug.Log("cur loop");
                        bool prevWasNonOp = false;
                        List<ArithmeticNode> newChildren = new List<ArithmeticNode>();
                        for (int i = 0; i < children.Count; i++)
                        {
                            ArithmeticNode cur = children[i];
                            //Debug.Log(cur + " "  + cur.arithemeticType);
                            if (cur.arithemeticType == ArithmeticType.Operator)
                            {
                                prevWasNonOp = false;
                                if (!mergedSomething)
                                {
                                    bool foundSomethingThatFits = false;
                                    foreach (ArithmeticOp possibleOp in cur.ops)
                                    {
                                        if (possibleOp.precedence == curPrecedence)
                                        {
                                            // unary if we are first or prev is also an operator
                                            if ((i == 0 || children[i - 1].arithemeticType == ArithmeticType.Operator) && possibleOp.opType == ArithmeticOp.ArithmeticOpType.Unary)
                                            {
                                                if (i + 1 < children.Count && children[i + 1].arithemeticType != ArithmeticType.Operator)
                                                {
                                                    ArithmeticNode newChild = new ArithmeticNode(cur, possibleOp, children[i + 1]);
                                                    i += 1; // skip past next one that we just used
                                                    newChildren.Add(newChild);
                                                    mergedSomething = true;
                                                    foundSomethingThatFits = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    throw new ArithmeticException("- sign does not have term it can apply to, current scope is " + this);
                                                }
                                            }
                                            // otherwise we are a binary operator
                                            else if (possibleOp.opType == ArithmeticOp.ArithmeticOpType.Binary)
                                            {
                                                if (i == 0 || i == children.Count - 1 || children[i + 1].IsBinaryOperator() || children[i - 1].IsBinaryOperator())
                                                {
                                                    throw new ArithmeticException("operator " + possibleOp.op + " does not have a term on both sides it can apply to, current scope is " + this);
                                                }
                                                else
                                                {
                                                    ArithmeticNode beforeChild = children[i - 1];
                                                    ArithmeticNode afterChild = children[i + 1];
                                                    // go up if previous has already been used (say, for (-3 + 2) or (3 + 4 + 5)  (we are the rightmost plus in both of these examples))
                                                    while (beforeChild.merged)
                                                    {
                                                        beforeChild = beforeChild.parent;
                                                    }
                                                    while (afterChild.merged)
                                                    {
                                                        afterChild = afterChild.parent;
                                                    }
                                                    ArithmeticNode newChild = new ArithmeticNode(cur, possibleOp, beforeChild, afterChild);
                                                    i += 1; // skip past next one that we just used
                                                    newChildren.Add(newChild);
                                                    mergedSomething = true;
                                                    foundSomethingThatFits = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (!foundSomethingThatFits)
                                    {
                                        // in case we have the processing happen after us
                                        if (i != 0 && !children[i - 1].merged)
                                        {
                                            newChildren.Add(children[i - 1]);
                                        }
                                        newChildren.Add(cur);
                                    }
                                }
                                else
                                {
                                   newChildren.Add(cur);
                                }
                            }
                            else
                            {
                                if (prevWasNonOp)
                                {
                                    throw new ArithmeticException("multiple non-operators next to each other, this is invalid with us " + this.ToString());
                                }
                                prevWasNonOp = true;
                                if (mergedSomething)
                                {
                                    newChildren.Add(cur);
                                }
                            }
                        }

                        if (mergedSomething)
                        {
                            this.children = newChildren;
                        }

                    }
                }

            }

            public void AddChild(ArithmeticNode child)
            {
                if (this.arithemeticType == ArithmeticType.Operator)
                {
                    throw new ArithmeticException("Cannot add child to type " + this.arithemeticType);
                }
                else
                {
                    this.children.Add(child);
                }
            }

            public ArithmeticNode(string val)
            {
                val = val.Trim();
                bool foundMatchingOp = false;
                foreach (ArithmeticOp op in AllOps)
                {
                    if (op.op == val)
                    {
                        this.arithemeticType = ArithmeticType.Operator;
                        this.ops.Add(op);
                        this.op = val;
                        foundMatchingOp = true;
                        //Debug.Log("found dat match " + op.op);
                        // allow multiple matches (such as the unary and binary minus sign)
                    }
                }
                if (val == "(")
                {
                    this.arithemeticType = ArithmeticType.Paren;
                    this.paren = new ArithmeticParen(true);
                    foundMatchingOp = true;
                }
                else if(val == ")")
                {
                    this.arithemeticType = ArithmeticType.Paren;
                    this.paren = new ArithmeticParen(false);
                    foundMatchingOp = true;
                }
                if (!foundMatchingOp)
                {
                    arithemeticType = ArithmeticType.State;
                    variable = new ArithmeticVarible(val.ToLower());
                }
            }
        }

        static ArithmeticOp[] AllOps = new ArithmeticOp[] { new ArithmeticOp("+", 0), new ArithmeticOp("-", 0), new ArithmeticOp("-", 2, ArithmeticOp.ArithmeticOpType.Unary), new ArithmeticOp("*", 1), new ArithmeticOp("/", 1) };

        public ArithmeticNode ParseString(string var)
        {
            try
            {
                string[] pieces = SplitAndKeepDelims(var, new char[] { '+', '-', '*', '/', '(', ')' });

                ArithmeticNode curRoot = new ArithmeticNode(parent: null);
                for (int i = 0; i < pieces.Length; i++)
                {
                    if (pieces[i].Trim() != "")
                    {
                        ArithmeticNode curNode = new ArithmeticNode(pieces[i]);
                        if (curNode.arithemeticType == ArithmeticNode.ArithmeticType.Paren)
                        {
                            // go one stack down (opening paren)
                            if (curNode.paren.opening)
                            {
                                ArithmeticNode newRoot = new ArithmeticNode(curRoot);
                                curRoot.AddChild(newRoot);
                                curRoot = newRoot;
                            }
                            else
                            {
                                // go one stack up (closing paren)
                                if (curRoot.parent != null)
                                {
                                    curRoot = curRoot.parent;
                                }
                                else
                                {
                                    throw new ArithmeticException("Unmatched closing paren");
                                }
                            }
                        }
                        else
                        {
                            curRoot.AddChild(curNode);
                        }
                    }
                }

                // we should get back up to root with no parent if we have balanced parens
                if (curRoot.parent != null)
                {
                    throw new ArithmeticException("Unmatched opening paren");
                }

                curRoot.MergeOperators();

                //Debug.Log("parsed " + var + " and got " + curRoot + " which evaluates with state=0 to " + curRoot.GetValue(0) + " and state=1 to " + curRoot.GetValue(1) + " and state=-1 to" + curRoot.GetValue(-1));

                return curRoot;


            }
            catch (ArithmeticException arithException)
            {
                throw new ArithmeticException("Exception in processing string " + var + " of " + arithException);
            }
        }

        public Blocks.RenderTriangle[] ToRenderTriangles(BlockData.BlockRotation blockRotation=BlockData.BlockRotation.Degrees0, int state=0)
        {
            CubeMesh res = new CubeMesh();

            // -0.5 to 0.5 -> 0 to 1
            //res = res + new Vector3(0.5f, 0.5f, 0.5f);


            Vector3 fromPos;
            if (fromVars != null && fromVars.Length == 3)
            {
                float fromValX = ParseString(fromVars[0]).GetValue(state);
                float fromValY = ParseString(fromVars[1]).GetValue(state);
                float fromValZ = ParseString(fromVars[2]).GetValue(state);
                fromPos = new Vector3(fromValX, fromValY, fromValZ);
            }
            else
            {
                fromPos = new Vector3((float)from[0], (float)from[1], (float)from[2]);
            }


            Vector3 toPos;
            if (toVars != null && toVars.Length == 3)
            {
                float toValX = ParseString(toVars[0]).GetValue(state);
                float toValY = ParseString(toVars[1]).GetValue(state);
                float toValZ = ParseString(toVars[2]).GetValue(state);
                toPos = new Vector3(toValX, toValY, toValZ);
            }
            else
            {
                toPos = new Vector3((float)to[0], (float)to[1], (float)to[2]);
            }

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
            int rotationDegrees = 0;

            if (blockRotation == BlockData.BlockRotation.Degrees0)
            {

            }
            else if(blockRotation == BlockData.BlockRotation.Degrees180)
            {
                rotationDegrees = 180;
            }
            else if(blockRotation == BlockData.BlockRotation.Degrees270)
            {
                rotationDegrees = 270;
            }
            else if(blockRotation == BlockData.BlockRotation.Degrees90)
            {
                rotationDegrees = 90;
            }

            if (rotationDegrees != 0)
            {
                BlockModelRotation customRotation = new BlockModelRotation
                {
                    axis = "y",
                    angle = rotationDegrees,
                    origin = new double[] {8, 0, 8}
                };
                res = res.ApplyingTransformationToEachVertex(x => { return customRotation.ApplyToPoint(x); });

                //Debug.Log("got " + customRotation.ApplyToPoint(new Vector3(0, 0, 1)) + " is the res");
            }

            //Debug.Log("my texture value is " + texture);

            //Debug.Log("my to value is " + toPos[0] + " " + toPos[1] + " " + toPos[2]);
            //Debug.Log("my from value is " + fromPos[0] + " " + fromPos[1] + " " + fromPos[2]);
            // offset uvs x value to match correct texture
            if (texture != null)
            {
                int texIndex = rootModel.TexToIndex(texture);
                //Debug.Log("tex of " + texture + " maps to index " + texIndex);
                res += new Vector2(texIndex / 64.0f, 0.0f);
            }

            //res = res * new Vector2(1/64.0f, 1.0f/256.0f);

            //res = res * new Vector2(1.0f, 1.0f);

            

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
        public Dictionary<string, string> textures;

        /// <summary>
        /// internal variable created when loading and processing model json files, stores the mapping of texture name to (index, texture, texturePath)
        /// </summary>
        Dictionary<string, Tuple<int, Texture2D, string>> texToIndex;
        /// <summary>
        /// Contains all the elements of the model. they can only have cubic forms. If both "parent" and "elements" are set, the "elements" tag overrides the "elements" tag from the previous model.
        /// </summary>
        public BlockModelElement[] elements;

        [DefaultValue(null)]
        public Dictionary<string, BlockModelElement[]> variants;

        string __comment = "";


        public BlockModel()
        {

        }

        string rootPath = "";

        public static BlockModel FromJSONFilePath(string jsonPath)
        {
            BlockModel res = LitJson.JsonMapper.ToObject<BlockModel>(System.IO.File.ReadAllText(jsonPath));
            res.rootPath = System.IO.Path.GetDirectoryName(jsonPath);
            return res;
        }


        public int TexToIndex(string texName)
        {
            if (texToIndex == null)
            {
                GetTextures();
            }

            if (texToIndex.ContainsKey(texName))
            {
                return texToIndex[texName].a;
            }
            else
            {
                return 0;
            }
        }

        public string[] GetTexturePaths()
        {
            GetTextures();

            List<string> paths = new List<string>();
            foreach (KeyValuePair<string, Tuple<int, Texture2D, string>> texture in texToIndex)
            {
                paths.Add(texture.Value.c);
            }
            return paths.ToArray();
        }

        public Texture2D[] GetTextures()
        {
            texToIndex = new Dictionary<string, Tuple<int, Texture2D, string>>();
            List<Tuple<int, Texture2D, string>> res = new List<Tuple<int, Texture2D, string>>();
            int i = 0;
            foreach (KeyValuePair<string, string> texture in textures)
            {
                string texName = texture.Key;
                Debug.Log("has texture key with name " + texName);
                // already loaded
                if (texToIndex.ContainsKey(texName))
                {
                    res.Add(texToIndex[texName]);
                }
                // need to be loaded
                else
                {
                    string texFileName = texture.Value;
                    string texFilePath = System.IO.Path.Combine(rootPath, texFileName);
                    Debug.Log("trying to load flower texture with name " + texName + " and path " + texFilePath);
                    if (System.IO.File.Exists(texFilePath))
                    {
                        Texture2D curTex = new Texture2D(10, 10);
                        // will automatically resize and reformat as needed
                        curTex.LoadImage(System.IO.File.ReadAllBytes(texFilePath));
                        curTex.Apply();

                        // convert to argb32
                        Texture2D argbTexture = new Texture2D(curTex.width, curTex.height, TextureFormat.ARGB32, false, true);
                        argbTexture.SetPixels(curTex.GetPixels());
                        argbTexture.Apply();
                        Color32[] argbColors = argbTexture.GetPixels32();

                        // rescale if needed to correct size
                        if (argbTexture.width != 16 * 2 || argbTexture.height != 16 * 3)
                        {
                            TextureScale.Bilinear(argbTexture, 16 * 2, 16 * 3);
                        }

                        argbTexture.Apply();
                        texToIndex[texName] = new Tuple<int, Texture2D, string>(i, curTex, texFilePath);
                        i += 1;
                        res.Add(texToIndex[texName]);
                    }
                }
            }
            // sort by index, this will make lowest first so they'll be in the right order
            res.Sort((x, y) => { return x.a.CompareTo(y.a); });

            Texture2D[] result = new Texture2D[res.Count];
            for (int j = 0; j < res.Count; j++)
            {
                result[j] = res[j].b;
            }

            return result;
        }

        Dictionary<int, RenderTriangle[]>[] cachedStateAlternatives = new Dictionary<int, RenderTriangle[]>[] {
            new Dictionary<int, RenderTriangle[]>(),
            new Dictionary<int, RenderTriangle[]>(),
            new Dictionary<int, RenderTriangle[]>(),
            new Dictionary<int, RenderTriangle[]>()
        };


        BlockModel cachedParent = null;

        RotationUtils.RotationVariantCollection collection = null;

        public RenderTriangle[] ToRenderTriangles(BlockData.BlockRotation rotation, int state, int connectionFlags=0)
        {
            if (variants == null)
            {
                int rotationI = ((int)rotation) / 90;
                if (cachedStateAlternatives[rotationI].ContainsKey(state))
                {
                    return cachedStateAlternatives[rotationI][state];
                }
                else
                {
                    List<RenderTriangle> results = new List<RenderTriangle>();
                    if (parent != "")
                    {
                        if (cachedParent == null)
                        {
                            cachedParent = FromJSONFilePath(parent);
                        }
                        results.AddRange(cachedParent.ToRenderTriangles(rotation, state));
                    }
                    foreach (BlockModelElement element in elements)
                    {
                        element.rootModel = this;
                        results.AddRange(element.ToRenderTriangles(rotation, state));
                    }

                    RenderTriangle[] actualResults = results.ToArray();
                    cachedStateAlternatives[rotationI][state] = actualResults;
                    return actualResults;
                }
            }
            else
            {
                if (parent != "" && cachedParent == null)
                {
                    cachedParent = FromJSONFilePath(parent);
                }
                if (collection == null)
                {
                    collection = new RotationUtils.RotationVariantCollection(variants, this);
                }
                RotationUtils.RotationVariant renderThing = collection.GetRotationVariant(connectionFlags);
                if (renderThing != null)
                {
                    return renderThing.ToRenderTriangles(rotation, state, cachedParent);
                }
                else
                {
                    return collection.GetRotationVariant(0).ToRenderTriangles(rotation, state, cachedParent);
                }
            }
        }

        public RenderTriangle[] ToRenderTriangles(out bool dependsOnState, BlockData.BlockRotation rotation)
        {
            List<RenderTriangle> results = new List<RenderTriangle>();

            dependsOnState = false;
            if (parent != "")
            {
                if (cachedParent != null)
                {
                    cachedParent = FromJSONFilePath(parent);
                }
                RenderTriangle[] parentRes = cachedParent.ToRenderTriangles(out dependsOnState, rotation);
                if (parentRes != null)
                {
                    results.AddRange(parentRes);
                }
            }
            if (!dependsOnState)
            {
                foreach (BlockModelElement element in elements)
                {
                    if (element.DependsOnState())
                    {
                        dependsOnState = true;
                    }
                }

                if (variants != null)
                {
                    dependsOnState = true;
                }
            }
            if (dependsOnState)
            {
                return null;
            }
            else
            {
                foreach (BlockModelElement element in elements)
                {
                    element.rootModel = this;
                    results.AddRange(element.ToRenderTriangles(rotation));
                }
                return results.ToArray();
            }
        }
    }
}
