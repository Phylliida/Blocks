using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antler : MonoBehaviour {

    public class AntlerNode
    {
        public AntlerNode[] children;
        public AntlerNode(AntlerNode parent, int depth, int numBranchesAllowed)
        {
            int numChildren = Random.Range(1, 4);
            if (depth > 3)
            {
                numChildren = Random.Range(0, 3);
            }
            if (depth > 7)
            {
                numChildren = 0;
            }

            if (depth == 0)
            {
                numChildren = 1;
            }
            numChildren = Mathf.Max(0, Mathf.Min(numChildren, numBranchesAllowed));
            if (parent != null && parent.children.Length > 1 && numChildren > 1)
            {
                numChildren = 1;
            }
            children = new AntlerNode[numChildren];
            for (int i = 0; i < numChildren; i++)
            {
                children[i] = new AntlerNode(this, depth+1, numBranchesAllowed-numChildren-1);
            }
        }


        public static Pose PoseWithUp(Vector3 pos, Vector3 up)
        {
            Vector3 forward = Vector3.forward;
            if (up.normalized == Vector3.forward || up.normalized == -Vector3.forward)
            {
                forward = Vector3.up;
            }
            return new Pose(pos, Quaternion.LookRotation(forward, up));
        }


        /// <summary>
        /// Returns endPos
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="prevUp"></param>
        /// <param name="endUp"></param>
        /// <param name="numSegments"></param>
        /// <param name="startWidth"></param>
        /// <param name="endWidth"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static Vector3 CreateTube(Vector3 startPos, Vector3 endPos, Vector3 prevUp, Vector3 endUp, int numSegments, float startWidth, float endWidth, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
        {
            for (int i = 0; i < numSegments; i++)
            {
                float pSegStart = 1.0f-i / (float)numSegments;
                float pSegEnd = 1.0f-(i+1) / (float)numSegments;

                Vector3 segStart = Vector3.Lerp(startPos, endPos, pSegStart);
                Vector3 segEnd = Vector3.Lerp(startPos, endPos, pSegEnd);
                Vector3 segUpStart = Vector3.Lerp(prevUp, endUp, pSegStart);
                Vector3 segUpEnd = Vector3.Lerp(prevUp, endUp, pSegEnd);

                float segStartWidth = Mathf.Lerp(startWidth, endWidth, pSegStart);
                float segEndWidth = Mathf.Lerp(startWidth, endWidth, pSegEnd);


                Pose startPose = PoseWithUp(segStart, segUpStart);
                Pose endPose = PoseWithUp(segEnd, segUpEnd);

                int numRingSegments = 8;
                int initialOffset = vertices.Count;
                // go around a circle, done by taking x=sin and y=cos of an angle that goes from 0 to 2pi
                for (int j = 0; j < numRingSegments; j++)
                {
                    float p = j / (float)Mathf.Max(1, numRingSegments - 1); // go from 0 to 1
                    p = p*2 * Mathf.PI; // go from 0 to 2*pi (because sin and cos takes angle in radians)

                    // gets us values from -1 to 1
                    float x = Mathf.Sin(p);
                    float y = Mathf.Cos(p);

                    // forward is up the tube so right and up are the two sides of the tube that we can use for drawing our vertices in a circle around it
                    // mathematically we shouldn't need to normalize these but I do anyway in case of floating point error
                    Vector3 curStartSegmentOffset = (startPose.right * x + startPose.forward * y).normalized;
                    Vector3 curEndSegmentOffset = (endPose.right * x + endPose.forward * y).normalized;

                    Vector3 curStartSegmentPos = segStart + curStartSegmentOffset * segStartWidth;
                    Vector3 curEndSegmentPos = segEnd + curEndSegmentOffset * segEndWidth;

                    vertices.Add(curStartSegmentPos);
                    normals.Add(curStartSegmentOffset);
                    vertices.Add(curEndSegmentPos);
                    normals.Add(curEndSegmentOffset);
                }

                // vertex indexing goes like this:

                // end segment
                //    7
                // 1     5
                //    3

                // start segment
                //    6
                // 0     4   
                //    2


                // we want to make triangles that go around

                int totalNumAdded = vertices.Count - initialOffset;
                Debug.Log("added " + totalNumAdded + " with initial offset " + initialOffset);

                for (int j = 0; j < numRingSegments; j++)
                {
                    int curStartI = j * 2;
                    int curEndI = j * 2 + 1;
                    int nextStartI = (j * 2+2) % totalNumAdded;
                    int nextEndI = (j * 2 + 3) % totalNumAdded;
                    int si = curStartI + initialOffset;
                    int ei = curEndI + initialOffset;
                    int nsi = nextStartI + initialOffset;
                    int nei = nextEndI + initialOffset;

                    // ei  nei
                    // si  nsi

                    triangles.Add(si);
                    triangles.Add(ei);
                    triangles.Add(nei);
                    triangles.Add(si);
                    triangles.Add(nei);
                    triangles.Add(ei);

                    triangles.Add(si);
                    triangles.Add(nsi);
                    triangles.Add(nei);
                    triangles.Add(si);
                    triangles.Add(nei);
                    triangles.Add(nsi);
                }
            }
            return endPos;
        }


        public Mesh CreateMesh(Vector3 rootPos, Vector3 rootUp, float width, float widthBranch, float lengthSingle, float lengthBranch)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            CreateMeshHelper(rootPos, rootUp, width, widthBranch, lengthSingle, lengthBranch, vertices, normals, triangles);
            Mesh res = new Mesh();
            res.SetVertices(vertices);
            res.SetNormals(normals);
            res.SetTriangles(triangles.ToArray(), 0);
            return res;
        }
        void CreateMeshHelper(Vector3 rootPos, Vector3 prevUp, float prevWidth, float widthBranch, float lengthSingle, float lengthBranch, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
        {
            // tip
            if(children.Length == 0)
            {
                CreateTube(rootPos, rootPos + prevUp * lengthSingle, prevUp, prevUp, 6, prevWidth, 0.0f, vertices, normals, triangles);
            }
            // long piece
            else if(children.Length == 1)
            {
                Debug.Log("log piece with " + children.Length + " children");
                Vector3 endUp = Vector3.up;
                Vector3 endPos = CreateTube(rootPos, rootPos + endUp * lengthSingle, prevUp, endUp, 6, prevWidth, prevWidth, vertices, normals, triangles);
                children[0].CreateMeshHelper(endPos, endUp, prevWidth, widthBranch, lengthSingle, lengthBranch, vertices, normals, triangles);
            }
            // branch out
            else
            {
                Debug.Log("branch with " + children.Length + " children");
                Pose startPose = PoseWithUp(rootPos, prevUp);
                float randPhaseOffset = Random.value * 2 * Mathf.PI;
                for (int j = 0; j < children.Length; j++)
                {
                    float p = j / (float)Mathf.Max(1, children.Length - 1); // go from 0 to 1
                    p = p * 2 * Mathf.PI; // go from 0 to 2*pi (because sin and cos takes angle in radians)
                    p += randPhaseOffset;

                    // gets us values from -1 to 1
                    float x = Mathf.Sin(p);
                    float y = Mathf.Cos(p);


                    Vector3 branchOffset = (startPose.right * x + startPose.up * y).normalized*widthBranch;

                    branchOffset += lengthBranch * prevUp;

                    Vector3 endUp = branchOffset.normalized;
                    Vector3 endPos = CreateTube(rootPos, branchOffset + rootPos, prevUp, endUp, 6, prevWidth, prevWidth, vertices, normals, triangles);
                    children[j].CreateMeshHelper(endPos, endUp, prevWidth, widthBranch, lengthSingle, lengthBranch, vertices, normals, triangles);
                }
            }
        }
    }

	// Use this for initialization
	void Start () {

    }

    public bool regen = false;
    public float width = 0.5f;
    public float lengthSingle = 1.0f;
    public float lengthBranch = 1.0f;
    public float widthBranch = 0.5f;
    public int numBranchesAllowed = 10;
	
	// Update is called once per frame
	void Update () {
	    if(regen)
        {
            regen = false;

            AntlerNode antlers = new AntlerNode(null, 0, numBranchesAllowed);
            Mesh antlerMesh = antlers.CreateMesh(new Vector3(0, 0, 0), Vector3.up, width, widthBranch, lengthSingle, lengthBranch);
            GetComponent<MeshFilter>().mesh = antlerMesh;
        }
	}
}
