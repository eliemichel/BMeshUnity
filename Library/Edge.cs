using System.Collections.Generic;
using UnityEngine;

namespace BMeshLib
{
    /**
     * An edge links to vertices together, and may or may not be part of a face.
     * An edge can be shared by several faces.
     * 
     * Technical Note: The structure stores a reference to the two vertices.
     * Although the role of these two vertices is perfectly symmetrical, this
     * makes the iterations over linked list slightly trickier than expected.
     * 
     * The edge is a node of two (double) linked lists at the same time. Let's
     * recall that a (simply) linked list of Stuff is made of nodes of the form
     *     Node {
     *         Stuff value;
     *         Node next;
     *     }
     * Here we provide two "next", depending on whether the vertex that we are
     * interested in is vertex1 or vertex2. Note that a vertex stored in the
     * "vertex1" field for one edge might be stored in the "vertex2" of the
     * next one, so the function Next() is provided to return either next1 or
     * next2 depending on the vertex of interest.
     */
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

        /**
         * Tells whether a vertex is one of the extremities of this edge.
         */
        public bool ContainsVertex(Vertex v)
        {
            return v == vert1 || v == vert2;
        }

        /**
         * If one gives a vertex of the edge to this function, it returns the
         * other vertex of the edge. Otherwise, the behavior is undefined.
         */
        public Vertex OtherVertex(Vertex v)
        {
            Debug.Assert(ContainsVertex(v));
            return v == vert1 ? vert2 : vert1;
        }

        /**
         * If one gives a vertex of the edge to this function, it returns the
         * next edge in the linked list of edges that use this vertex.
         */
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

        /**
         * Similar to Next() but to go backward in the double-linked list
         */
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

        /**
         * Return all faces that use this edge as a side.
         */
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

        /**
         * Compute the barycenter of the edge's vertices
         */
        public Vector3 Center()
        {
            return (vert1.point + vert2.point) * 0.5f;
        }
    }
}
