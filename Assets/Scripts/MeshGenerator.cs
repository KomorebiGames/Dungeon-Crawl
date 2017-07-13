using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mesh generator. Does not generate map grid values. See <see cref="MapGenerator"/>. 
/// </summary>
public class MeshGenerator : MonoBehaviour {

	/// <summary>
	/// The square grid.
	/// </summary>
	public SquareGrid squareGrid;
	/// <summary>
	/// The walls.
	/// </summary>
	public MeshFilter walls;
	/// <summary>
	/// The cave.
	/// </summary>
	public MeshFilter cave;

	/// <summary>
	/// The mesh vertices.
	/// </summary>
	List<Vector3> vertices;
	/// <summary>
	/// The mesh triangles.
	/// </summary>
	List<int> triangles;
	/// <summary>
	/// Keeps track of what triangles each vertex belongs to.
	/// </summary>
	Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
	/// <summary>
	/// The room outlines.
	/// </summary>
	List<List<int>> outlines = new List<List<int>>();
	/// <summary>
	/// The checked vertices.
	/// </summary>
	HashSet<int> checkedVertices = new HashSet<int>();

	/// <summary>
	/// Generates the map mesh.
	/// </summary>
	/// <param name="map">Map to generate mesh for.</param>
	/// <param name="squareSize">Square size.</param>
	public void GenerateMesh(int[,] map, float squareSize) {
		//
		// Clear previous map data
		//
		triangleDictionary.Clear();
		outlines.Clear();
		checkedVertices.Clear();

		squareGrid = new SquareGrid(map, squareSize);

		vertices = new List<Vector3>();
		triangles = new List<int>();

		//
		// Make mesh triangles for each square in the grid
		//
		for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
			for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
				TraingulateSquare(squareGrid.squares[x, y]);
			}
		}

		//
		// Make the cave mesh from the vertices and triangles
		//
		Mesh mesh = new Mesh();
		cave.mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		//
		// Add a collider to the cave mesh
		//
		MeshCollider caveCollider = cave.gameObject.AddComponent<MeshCollider>();
		caveCollider.sharedMesh = mesh;

		//
		// Apply texture to the cave mesh
		//
		int tileAmount = 150;
		Vector2[] uvs = new Vector2[vertices.Count];
		for (int i = 0; i < vertices.Count; i++) {
			//
			// Create the uvs based on the world position of the vertices of the squares
			//
			float percentX = Mathf.InverseLerp(-map.GetLength(0)/2*squareSize,map.GetLength(0)/2*squareSize,vertices[i].x) * tileAmount;
			float percentY = Mathf.InverseLerp(-map.GetLength(0)/2*squareSize,map.GetLength(0)/2*squareSize,vertices[i].z) * tileAmount;
			uvs[i] = new Vector2(percentX,percentY);
		}
		mesh.uv = uvs;

		//
		// Make the wall mesh and apply a collider and texture to it
		//
		CreateWallMesh();

		//
		// Add the NavMeshSourceTag to the cave mesh so that it is navigable by NavMeshAgents
		//
		cave.gameObject.AddComponent<NavMeshSourceTag>();
	}

	/// <summary>
	/// Creates the wall mesh and applies a collider and texture to it.
	/// </summary>
	void CreateWallMesh() {
		//
		// Get the room outlines for the wall mesh
		//
		CalculateMeshOutlines ();

		List<Vector3> wallVertices = new List<Vector3> ();
		List<int> wallTriangles = new List<int> ();
		Mesh wallMesh = new Mesh ();
		float wallHeight = 2;

		//
		// Follow each outline and make the triangles and vertices for the mesh
		//
		foreach (List<int> outline in outlines) {
			for (int i = 0; i < outline.Count - 1; i ++) {
				int startIndex = wallVertices.Count;
				wallVertices.Add(vertices[outline[i]] + Vector3.up * wallHeight); // left
				wallVertices.Add(vertices[outline[i+1]] + Vector3.up * wallHeight); // right
				wallVertices.Add(vertices[outline[i]]); // bottom left
				wallVertices.Add(vertices[outline[i+1]]); // bottom right

				//
				// Reverse order of the two index sets to change which way the mesh faces
				//
				wallTriangles.Add(startIndex + 3);
				wallTriangles.Add(startIndex + 2);
				wallTriangles.Add(startIndex + 0);

				wallTriangles.Add(startIndex + 0);
				wallTriangles.Add(startIndex + 1);
				wallTriangles.Add(startIndex + 3);
			}
		}

		//
		// Create mesh from triangles and vertices
		//
		wallMesh.vertices = wallVertices.ToArray ();
		wallMesh.triangles = wallTriangles.ToArray ();
		wallMesh.RecalculateNormals();
		walls.mesh = wallMesh;

		//
		// Apply texture to the wall mesh
		//
		Vector2[] uvs = new Vector2[wallMesh.vertices.Length];
		for (int i = 0; i < wallMesh.vertices.Length; i++)
		{
			float x = wallMesh.vertices[i].x;
			//
			// Fix bug texture
			//
			if( i + 1 < wallMesh.vertices.Length && x == wallMesh.vertices[i + 1].x && i > 1 && wallMesh.vertices[i - 1].x == x)
			{
				x = wallMesh.vertices[i].z;
			}
			uvs[i] = new Vector2(x, wallMesh.vertices[i].y * 0.5f);
		}
		wallMesh.uv = uvs;

		//
		// Add a mesh collider to the walls
		//
		MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
		wallCollider.sharedMesh = wallMesh;
	}

	/// <summary>
	/// Draws mesh triangles based on the state of the square.
	/// </summary>
	/// <param name="square">Square to draw triangles for.</param>
	void TraingulateSquare(Square square) {
		switch (square.configuration) {
		case 0:
			break;

			// 1 points:
		case 1:
			MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
			break;
		case 2:
			MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
			break;
		case 4:
			MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
			break;
		case 8:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
			break;

			// 2 points:
		case 3:
			MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
			break;
		case 6:
			MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
			break;
		case 9:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
			break;
		case 12:
			MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
			break;
		case 5:
			MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
			break;
		case 10:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
			break;

			// 3 point:
		case 7:
			MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
			break;
		case 11:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
			break;
		case 13:
			MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
			break;
		case 14:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
			break;

			// 4 point:
		case 15:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			checkedVertices.Add(square.topLeft.vertexIndex);
			checkedVertices.Add(square.topRight.vertexIndex);
			checkedVertices.Add(square.bottomRight.vertexIndex);
			checkedVertices.Add(square.bottomLeft.vertexIndex);
			break;
		}
	}

	/// <summary>
	/// Creates triangles and vertices using the given nodes.
	/// </summary>
	/// <param name="points">Nodes to make mesh with.</param>
	void MeshFromPoints(params Node[] points) {
		AssignVertices(points);

		if (points.Length >= 3) {
			CreateTriangle(points[0], points[1], points[2]);
		}
		if (points.Length >= 4) {
			CreateTriangle(points[0], points[2], points[3]);
		}
		if (points.Length >= 5) {
			CreateTriangle(points[0], points[3], points[4]);
		}
		if(points.Length >= 6) {
			CreateTriangle(points[0], points[4], points[5]);
		}
	}

	/// <summary>
	/// Assign vertices.
	/// </summary>
	/// <param name="points">Nodes to assign vertices from.</param>
	void AssignVertices(Node[] points) {
		for (int i = 0; i < points.Length; i++) {
			if (points[i].vertexIndex == -1) {
				points[i].vertexIndex = vertices.Count;
				vertices.Add(points[i].position);
			}
		}
	}

	/// <summary>
	/// Adds vertices to triangles list. Adds triangle to dictionary.
	/// </summary>
	/// <param name="a">Node a.</param>
	/// <param name="b">Node b.</param>
	/// <param name="c">Node c.</param>
	void CreateTriangle(Node a, Node b, Node c) {
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);

		Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTriangleToDictionary (triangle.vertexIndexA, triangle);
		AddTriangleToDictionary (triangle.vertexIndexB, triangle);
		AddTriangleToDictionary (triangle.vertexIndexC, triangle);
	}

	/// <summary>
	/// Adds the triangle to dictionary.
	/// </summary>
	/// <param name="vertexIndexKey">Index of the vertex to use as a key.</param>
	/// <param name="triangle">Triangle to add to the key's list.</param>
	void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
		//
		// If the key already exists, add to its list
		//
		if(triangleDictionary.ContainsKey(vertexIndexKey)) {
			triangleDictionary[vertexIndexKey].Add(triangle);
		}
		//
		// If key doesn't exist, create a new list
		//
		else {
			List<Triangle> triangleList = new List<Triangle>();
			triangleList.Add(triangle);
			triangleDictionary.Add(vertexIndexKey, triangleList);
		}
	}

	/// <summary>
	/// Calculates the mesh outlines.
	/// </summary>
	void CalculateMeshOutlines() {
		for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
			if (!checkedVertices.Contains(vertexIndex)) {
				//
				// Check if vertex is an outline. If it is, create a new outline
				// and follow it.
				//
				int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
				if (newOutlineVertex != -1) {
					checkedVertices.Add(vertexIndex);

					List<int> newOutline = new List<int>();
					newOutline.Add(vertexIndex);
					outlines.Add(newOutline);
					FollowOutline(newOutlineVertex, outlines.Count - 1);
					outlines[outlines.Count - 1].Add(vertexIndex);
				}
			}
		}
	}

	/// <summary>
	/// Recursively called to follow the outline vertices.
	/// </summary>
	/// <param name="vertexIndex">The vertex to check.</param>
	/// <param name="outlineIndex">The outline to add to.</param>
	void FollowOutline(int vertexIndex, int outlineIndex) {
		outlines [outlineIndex].Add (vertexIndex);
		checkedVertices.Add (vertexIndex);

		int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);
		if (nextVertexIndex != -1) {
			FollowOutline(nextVertexIndex, outlineIndex);
		}
	}

	/// <summary>
	/// Gets the the next vertex in the outline.
	/// </summary>
	/// <returns>The next vertex in the outline. <c>-1</c> if there is no next vertex.</returns>
	/// <param name="vertexIndex">The vertex to check.</param>
	int GetConnectedOutlineVertex(int vertexIndex) {
		List<Triangle> trianglesContainingVertex = triangleDictionary [vertexIndex];

		for (int i = 0; i < trianglesContainingVertex.Count; i++) {
			Triangle triangle = trianglesContainingVertex[i];

			for (int j = 0; j < 3; j++) {
				int vertexB = triangle[j];
				if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
					if (IsOutlineEdge(vertexIndex, vertexB)) {
						return vertexB;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Determines whether the given vertices form an outline edge.
	/// </summary>
	/// <returns><c>true</c> if the vertices form an outline edge; otherwise, <c>false</c>.</returns>
	/// <param name="vertexA">Vertex a.</param>
	/// <param name="vertexB">Vertex b.</param>
	bool IsOutlineEdge(int vertexA, int vertexB) {
		List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
		int sharedTriangleCount = 0;

		//
		// Check to see which triangles the edge shares. If it is only attached to one triangle,
		// then it is an outline edge.
		//
		for (int i = 0; i < trianglesContainingVertexA.Count; i ++) {
			if (trianglesContainingVertexA[i].Contains(vertexB)) {
				sharedTriangleCount ++;
				if (sharedTriangleCount > 1) {
					break;
				}
			}
		}
		return sharedTriangleCount == 1;
	}

	/// <summary>
	/// Triangle consisting of 3 vertices.
	/// </summary>
	struct Triangle {
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;
		int[] vertices;

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshGenerator+Triangle"/> struct.
		/// </summary>
		/// <param name="a">Vertex a.</param>
		/// <param name="b">Vertex b.</param>
		/// <param name="c">Vertex c.</param>
		public Triangle (int a, int b, int c) {
			vertexIndexA = a;
			vertexIndexB = b;
			vertexIndexC = c;

			vertices = new int[3];
			vertices[0] = a;
			vertices[1] = b;
			vertices[2] = c;
		}

		/// <summary>
		/// Gets the vertex with the specified i.
		/// </summary>
		/// <param name="i">The index.</param>
		public int this[int i] {
			get {
				return vertices[i];
			}
		}

		/// <summary>
		/// Checks if the triangle contains the specified vertex.
		/// </summary>
		/// <param name="vertexIndex">The vertex to check for.</param>
		public bool Contains(int vertexIndex) {
			return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
		}
	}

	/// <summary>
	/// A grid of squares to be triangulated for cave mesh generation.
	/// </summary>
	public class SquareGrid {
		/// <summary>
		/// The square grid.
		/// </summary>
		public Square[,] squares;

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshGenerator+SquareGrid"/> class.
		/// </summary>
		/// <param name="map">The map to repressent as a square grid.</param>
		/// <param name="squareSize">The square size.</param>
		public SquareGrid(int[,] map, float squareSize) {
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength(1);
			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;

			ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];

			//
			// Create grid of control nodes
			//
			for (int x = 0; x < nodeCountX; x++) {
				for (int y = 0; y < nodeCountY; y++) {
					Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
					controlNodes[x,y] = new ControlNode(pos, map[x,y] == 1, squareSize);
				}
			}

			//
			// Fill square grid using grid of control nodes
			//
			squares = new Square[nodeCountX - 1, nodeCountY - 1];
			for (int x = 0; x < nodeCountX - 1; x++) {
				for (int y = 0; y < nodeCountY - 1; y++) {
					squares[x, y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
				}
			}
		}
	}

	/// <summary>
	/// Square for triangulation. Has 16 possible states.
	/// </summary>
	public class Square {
		public ControlNode topLeft, topRight, bottomRight, bottomLeft;
		public Node centerTop, centerRight, centerBottom, centerLeft;
		public int configuration;

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshGenerator+Square"/> class.
		/// </summary>
		/// <param name="_topLeft">Top left control node.</param>
		/// <param name="_topRight">Top right control node.</param>
		/// <param name="_bottomRight">Bottom right control node.</param>
		/// <param name="_bottomLeft">Bottom left control node.</param>
		public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)  {
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;

			centerTop = topLeft.right;
			centerRight = bottomRight.above;
			centerBottom = bottomLeft.right;
			centerLeft = bottomLeft.above;

			//
			// Use bit masking to determine state
			//
			if (topLeft.active)
				configuration += 8;
			if (topRight.active)
				configuration += 4;
			if (bottomRight.active)
				configuration += 2;
			if (bottomLeft.active)
				configuration += 1;
		}
	}

	/// <summary>
	/// A single node on the map.
	/// </summary>
	public class Node {
		/// <summary>
		/// The node's world position.
		/// </summary>
		public Vector3 position;
		/// <summary>
		/// The index of the vertex.
		/// </summary>
		public int vertexIndex = -1;

		public Node(Vector3 _pos) {
			position = _pos;
		}
	}

	/// <summary>
	/// Determines the state of a square.
	/// </summary>
	public class ControlNode : Node {
		/// <summary>
		/// <c>true</c> if the node is active; otherwise, <c>false</c>.
		/// </summary>
		public bool active;
		public Node above, right;

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshGenerator+ControlNode"/> class.
		/// </summary>
		/// <param name="_pos">The node's world position.</param>
		/// <param name="_active">If set to <c>true</c> the control node is active.</param>
		/// <param name="squareSize">The square size.</param>
		public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
			active = _active;
			above = new Node(position + Vector3.forward * squareSize / 2f);
			right = new Node(position + Vector3.right * squareSize / 2f);
		}
	}

}
