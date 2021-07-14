using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BMeshLib
{
    /**
     * NB: For all topology types, except for members marked as "attribute",
     * all classes only store references to objects allocated by BMesh itself.
     * Variables marked as attributes have no impact on the topological
     * algorithm applied to the mesh (e.g. finding the neighbors), they are
     * only set by calling code. In particuler, the 'id' field has no meaning
     * for this class, it is put here purely for commodity, but don't expect it
     * to get actually filled unless you explicitely did do.
     */

    /**
     * A vertex corresponds roughly to a position in space. Many primitives
     * (edges, faces) can share a given vertex. Several vertices can be located
     * at the very same position.
     * It references a chained list of the edges that use it, embeded inside the Edge
     * structure (see below, and see implementation of NeighborEdges).
     * The vertex position does not affect topological algorithms, but is used by
     * commodity functions that help finding the center of an edge or a face.
     */
    /// <summary>
    /// Corresponds to a point in space, and is used by <see cref="Edge"/> and <see cref="Face"/> to construct a <see cref="BMesh"/>. 
    /// </summary>
    /// <remarks>
    ///  Multiple <see cref="Edge"/>s and <see cref="Face"/>s can use the same <see cref="Vertex"/>, and multiple vertices can be located at the exact same position.
    /// </remarks>
    public class Vertex
    {
        public int id; // [attribute]
        public Vector3 point; // [attribute]
        public Dictionary<string, AttributeValue> attributes; // [attribute] (extra attributes)
        public Edge edge; // one of the edges using this vertex as origin, navigates other using edge.next1/next2

        public Vertex(Vector3 _point)
        {
            point = _point;
        }

        /// <summary>
        /// Returns all <see cref="Edge"/>s that reach the <see cref="Vertex"/>.
        /// </summary>
        /// <returns>All <see cref="Edge"/>s that reach the <see cref="Vertex"/>. Uses <see cref="Edge.Next(Vertex)"/> from <see cref="edge"/> until it reaches <see cref="edge"/> again.</returns>
        public List<Edge> NeighborEdges()
        {
            var edges = new List<Edge>();
            if (this.edge != null)
            {
                Edge it = this.edge;
                do
                {
                    edges.Add(it);
                    it = it.Next(this);
                } while (it != edge);
            }
            return edges;
        }

        /// <summary>
        /// Returns all <see cref="Face"/>s that use the <see cref="Vertex"/> as a corner.
        /// </summary>
        /// <returns>All <see cref="Face"/>s that use the <see cref="Vertex"/> as one of it's corners.</returns>
        public List<Face> NeighborFaces()
        {
            var faces = new HashSet<Face>();
            if (edge != null)
            {
                Edge it = edge;
                do
                {
                    foreach (var f in it.NeighborFaces())
                    {
                        faces.Add(f);
                    }
                    it = it.Next(this);
                } while (it != edge);
            }
            return faces.ToList();
        }
    }
}
