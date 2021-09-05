using System.Collections.Generic;
using UnityEngine;

namespace BMeshLib
{
    /**
     * Since a face is basically a list of edges, and the Loop object is a node
     * of this list, called so because the list must loop.
     * A loop is associated to one and only one face.
     * 
     * A loop can be seen as a list of edges, it also stores a reference to a
     * vertex for commodity but technically it could be found through the edge.
     * It may also be interpreted as a "face corner", and is hence where one
     * typically stores UVs, because one vertex may have different UV
     * coordinates depending on the face.
     * 
     * On top of this, the loop is also used as a node of another linked list,
     * namely the radial list, that enables iterating over all the faces using
     * the same edge.
     */
    /// <summary>
    /// A combination of a <see cref="Vertex"/>, an <see cref="Edge"/> and a <see cref="Face"/>.
    /// Meant to provide fast access to neighboring edges, by traversing around the <see cref="Face"/> with <see cref="Edge.Next(Vertex)"/>
    /// and <see cref="Edge.Prev(Vertex)"/>. Or by traversing around the <see cref="Vertex"/> with ReadialNext and RadialPrev.
    /// </summary>
    public class Loop
    {
        public Dictionary<string, AttributeValue> attributes; // [attribute] (extra attributes)

        public Vertex vert;
        public Edge edge;
        public Face face; // there is exactly one face using a loop

        public Loop radial_prev; // around edge
        public Loop radial_next;
        public Loop prev; // around face
        public Loop next;

        public Loop(Vertex v, Edge e, Face f)
        {
            vert = v;
            SetEdge(e);
            SetFace(f);
        }

        /// <summary>
        /// Insert the <see cref="Loop"/> in to the linked list of the specified <see cref="Face"/>.
        /// </summary>
        /// <param name="f">The <see cref="Face"/> to insert the <see cref="Loop"/> in to.</param>
        public void SetFace(Face f)
        {
            Debug.Assert(this.face == null);
            if (f.loop == null)
            {
                f.loop = this;
                this.next = this.prev = this;
            }
            else
            {
                this.prev = f.loop;
                this.next = f.loop.next;

                f.loop.next.prev = this;
                f.loop.next = this;

                f.loop = this;
            }
            this.face = f;
        }

        /// <summary>
        /// Insert the <see cref="Loop"/> in to the radial linked list.
        /// </summary>
        /// <param name="e">The <see cref="Edge"/> to insert the <see cref="Loop"/> in to.</param>
        public void SetEdge(Edge e)
        {
            Debug.Assert(this.edge == null);
            if (e.loop == null)
            {
                e.loop = this;
                this.radial_next = this.radial_prev = this;
            }
            else
            {
                this.radial_prev = e.loop;
                this.radial_next = e.loop.radial_next;

                e.loop.radial_next.radial_prev = this;
                e.loop.radial_next = this;

                e.loop = this;
            }
            this.edge = e;
        }
    }
}
