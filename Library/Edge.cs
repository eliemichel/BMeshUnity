using System.Collections.Generic;
using UnityEngine;

namespace BMeshLib
{
    
     // An edge links to vertices together, and may or may not be part of a face.
     // An edge can be shared by several faces.
     //
     // Technical Note: The structure stores a reference to the two vertices.
     // Although the role of these two vertices is perfectly symmetrical, this
     // makes the iterations over linked list slightly trickier than expected.
     //
     // The edge is a node of two (double) linked lists at the same time. Let's
     // recall that a (simply) linked list of Stuff is made of nodes of the form
     //     Node {
     //         Stuff value;
     //         Node next;
     //     }
     // Here we provide two "next", depending on whether the vertex that we are
     // interested in is vertex1 or vertex2. Note that a vertex stored in the
     // "vertex1" field for one edge might be stored in the "vertex2" of the
     // next one, so the function Next() is provided to return either next1 or
     // next2 depending on the vertex of interest.

     /// <summary>
    /// Links two <see cref="Vertex"/>s together, and may or may not be part of a <see cref="Face"/>.
    /// </summary>
    /// <remarks>Multiple <see cref="Face"/>s can share the same <see cref="Edge"/>.</remarks>
    public class Edge
    {
        public int id; // [attribute]
        public Dictionary<string, AttributeValue> attributes; // [attribute] (extra attributes)
        public Vertex vert1;
        public Vertex vert2;
        public Edge next1; // next edge around vert1. If you don't know whether your vertex is vert1 or vert2, use Next(v)
        public Edge next2; // next edge around vert1
        public Edge prev1;
        public Edge prev2;
        public Loop loop; // first node of the list of faces that use this edge. Navigate list using radial_next

        /// <summary>
        /// Whether the specified <see cref="Vertex"/> is one of the vertices that comprise the <see cref="Edge"/>.
        /// </summary>
        /// <param name="v">The <see cref="Vertex"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="v"/> is used by the <see cref="Edge"/>; otherwise, <c>false</c>.</returns>
        public bool ContainsVertex(Vertex v)
        {
            return v == vert1 || v == vert2;
        }

        /// <summary>
        /// Returns the other <see cref="Vertex"/> that comprises the <see cref="Edge"/>.
        /// </summary>
        /// <remarks>
        /// Assumes that the specified <see cref="Vertex"/> is one of the two vertices 
        /// that comprise the <see cref="Edge"/>; otherwise, behavior is undefined.
        /// </remarks>
        /// <param name="v">The <see cref="Vertex"/> to get the other of.</param>
        /// <returns>The other <see cref="Vertex"/> that makes up the edge.</returns>
        public Vertex OtherVertex(Vertex v)
        {
            Debug.Assert(ContainsVertex(v));
            return v == vert1 ? vert2 : vert1;
        }

        /// <summary>
        /// Returns the next <see cref="Edge"/> in the linked list of edges that use the specified <see cref="Vertex"/>. (Opposite of <see cref="Prev(Vertex)"/>).
        /// </summary>
        /// <remarks>
        /// Assumes that the specified <see cref="Vertex"/> is one of the vertices that 
        /// comprise the <see cref="Edge"/>; otherwise, behavior is undefined.
        /// It is ensured calling <c>Next</c> on each resulting <see cref="Edge"/> will iterate through
        /// all edges that the specified <see cref="Vertex"/> comprises.
        /// </remarks>
        /// <example>
        /// For instance the following iterates through all edges:
        /// <code>
        /// Edge firstEdge = edge;
        /// do {
        ///     // do something with `edge`
        ///     edge = edge.Next(v);
        /// } while(edge != firstEdge);
        /// </code>
        /// </example>
        /// <seealso cref="Vertex.NeighborEdges"/>
        /// <param name="v">The <see cref="Vertex"/> to get the next <see cref="Edge"/> of.</param>
        /// <returns>
        /// The next <see cref="Edge"/> in the linked list of edges that uses <paramref name="v"/>, 
        /// both edges having <paramref name="v"/> in common.
        /// </returns>
        public Edge Next(Vertex v)
        {
            Debug.Assert(ContainsVertex(v));
            return v == vert1 ? next1 : next2;
        }

        /**
         * This is used when inserting a new Edge in the lists.
         */
        public void SetNext(Vertex v, Edge other)
        {
            Debug.Assert(ContainsVertex(v));
            if (v == vert1) next1 = other;
            else next2 = other;
        }

        /// <summary>
        /// Returns the previous <see cref="Edge"/> in the linked list of edges that use the specified <see cref="Vertex"/>. (Opposite of <see cref="Next(Vertex)"/>).
        /// </summary>
        /// <remarks>
        /// Assumes that the specified <see cref="Vertex"/> is one of the vertices that 
        /// comprise the <see cref="Edge"/>; otherwise, behavior is undefined.
        /// It is ensured calling <c>Prev</c> on each resulting <see cref="Edge"/> will iterate through
        /// all edges that the specified <see cref="Vertex"/> comprises.
        /// </remarks>
        /// <example>
        /// For instance the following iterates through all edges:
        /// <code>
        /// Edge firstEdge = edge;
        /// do {
        ///     // do something with `edge`
        ///     edge = edge.Prev(v);
        /// } while(edge != firstEdge);
        /// </code>
        /// </example>
        /// <seealso cref="Vertex.NeighborEdges"/>
        /// <param name="v">The <see cref="Vertex"/> to get the previous <see cref="Edge"/> of.</param>
        /// <returns>
        /// The previous <see cref="Edge"/> in the linked list of edges that uses <paramref name="v"/>, 
        /// bothing edges having <paramref name="v"/> in common.
        /// </returns>
        public Edge Prev(Vertex v)
        {
            Debug.Assert(ContainsVertex(v));
            return v == vert1 ? prev1 : prev2;
        }

        /**
         * Similar to SetNext()
         */
        public void SetPrev(Vertex v, Edge other)
        {
            Debug.Assert(ContainsVertex(v));
            if (v == vert1) prev1 = other;
            else prev2 = other;
        }

        /// <summary>
        /// Returns all <see cref="Face"/>s that use the <see cref="Edge"/> as a side.
        /// </summary>
        /// <returns>All <see cref="Face"/>s that use the <see cref="Edge"/> as one of it's sides.</returns>
        public List<Face> NeighborFaces()
        {
            var faces = new List<Face>();
            if (this.loop != null)
            {
                var it = this.loop;
                do
                {
                    faces.Add(it.face);
                    it = it.radial_next;
                } while (it != this.loop);
            }
            return faces;
        }

        /// <summary>
        /// The center of the <see cref="Edge"/>'s vertices.
        /// </summary>
        /// <returns>The center between <see cref="vert1"/> and <see cref="vert2"/>.</returns>
        public Vector3 Center()
        {
            return (vert1.point + vert2.point) * 0.5f;
        }
    }
}
