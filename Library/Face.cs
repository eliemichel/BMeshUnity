using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMeshLib
{
    /**
     * A face is almost nothing more than a loop. Having a different structure
     * makes sense only 1. for clarity, because loops are a less intuitive
     * object and 2. to store face attributes.
     */
    public class Face
    {
        public int id; // [attribute]
        public Dictionary<string, AttributeValue> attributes; // [attribute] (extra attributes)
        public int vertcount; // stored for commodity, can be recomputed easily
        public Loop loop; // navigate list using next

        /**
         * Get the list of vertices used by the face, ordered.
         */
        public List<Vertex> NeighborVertices()
        {
            var verts = new List<Vertex>();
            if (this.loop != null)
            {
                Loop it = this.loop;
                do
                {
                    verts.Add(it.vert);
                    it = it.next;
                } while (it != this.loop);
            }
            return verts;
        }

        /**
         * Assuming the vertex is part of the face, return the loop such that
         * loop.vert = v. Return null otherwise.
         */
        public Loop Loop(Vertex v)
        {
            if (this.loop != null)
            {
                Loop it = this.loop;
                do
                {
                    Debug.Assert(it != null);
                    if (it.vert == v) return it;
                    it = it.next;
                } while (it != this.loop);
            }
            return null;
        }

        /**
         * Get the list of edges around the face.
         * It is garrantied to match the order of NeighborVertices(), so that
         * edge[0] = vert[0]-->vert[1], edge[1] = vert[1]-->vert[2], etc.
         */
        public List<Edge> NeighborEdges()
        {
            var edges = new List<Edge>();
            if (this.loop != null)
            {
                Loop it = this.loop;
                do
                {
                    edges.Add(it.edge);
                    it = it.next;
                } while (it != this.loop);
            }
            return edges;
        }

        /**
         * Compute the barycenter of the face vertices
         */
        public Vector3 Center()
        {
            Vector3 p = Vector3.zero;
            float sum = 0;
            foreach (var v in NeighborVertices())
            {
                p += v.point;
                sum += 1;
            }
            return p / sum;
        }
    }
}
