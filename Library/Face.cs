using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMeshLib
{
    /// <summary>
    /// Represents a face of a mesh in a <see cref="BMesh"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="Face"/> is little more than a <see cref="BMeshLib.Loop"/>.
    /// However it is used since it is more intuitive than a <see cref="BMeshLib.Loop"/>,
    /// and stores face <see cref="AttributeValue"/>s.
    /// </remarks>
    public class Face
    {
        public int id; // [attribute]
        public Dictionary<string, AttributeValue> attributes; // [attribute] (extra attributes)
        public int vertcount; // stored for commodity, can be recomputed easily
        public Loop loop; // navigate list using next

        /// <summary>
        /// Returns the ordered <see cref="Vertex"/>s that comprise the corners of the <see cref="Face"/>.
        /// </summary>
        /// <returns>
        /// The vertices that comprise corners of the <see cref="Face"/>.
        /// </returns>
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

        /// <summary>
        /// Returns the <see cref="BMeshLib.Loop"/> in the <see cref="Face"/>
        /// whose <see cref="Loop.vert"/> matches the specified <see cref="Vertex"/>.
        /// </summary>
        /// <param name="v">The <see cref="Vertex"/> to get the <see cref="BMeshLib.Loop"/> of.</param>
        /// <returns>
        /// The <see cref="BMeshLib.Loop"/> of the <see cref="Face"/>
        /// whose <see cref="Loop.vert"/> matches <paramref name="v"/>
        /// if it is part of the <see cref="Face"/>; otherwise, <c>null</c>.
        /// </returns>
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

        /// <summary>
        /// Returns the ordered <see cref="Edge"/>s around the <see cref="Face"/>.
        /// </summary>
        /// <returns>The <see cref="Edge"/>s that make up the <see cref="Face"/>.</returns>
        /// /// <remarks>
        /// Guarantied to match the order of <see cref="NeighborVertices"/>.
        /// So that <c>edge[0] = vert[0]-->vert[1], edge[1] = vert[1]-->vert[2], etc.</c>
        /// </remarks>
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

        /// <summary>
        /// The center of the vertices that are used by the <see cref="Face"/>.
        /// </summary>
        /// <returns>The center of <see cref="Face"/>.</returns>
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
