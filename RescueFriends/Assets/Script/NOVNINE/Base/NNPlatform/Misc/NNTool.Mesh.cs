using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NOVNINE.Diagnostics;

namespace NOVNINE
{

	public enum LineOffset {
	    Centered,
	    Inner,
	    Outer
	}
	
	public static partial class NNTool
	{
		public static Mesh CreateLineMesh(Vector3[] pos, float lineWidth, float uvScale, bool flipY, bool closed, LineOffset Loffset = LineOffset.Centered)
	    {
	        int vcnt = pos.Length;
	
			//setup indices  SetTriangleStrip
	//		int[] indices = new int[vcnt*2];
	//		for (int i = 0; i < pos.Length; i++) {
	//			indices[i*2]      = i*2;
	//			indices[i*2+1]    = i*2+1;
	//		}
			
			//setup indices  SetTriangleStrip converted SetTriangles
			int[] indices = new int[(vcnt * 2 - 2) *3];
			int j = 0;
			for (int i = 0; i < pos.Length * 2 - 3; i+=2, j++)
			{
				indices[i*3] = j*2;
				indices[i*3+1] = j*2+1;            
				indices[i*3+2] = j*2+2;
				indices[i*3+3] = j*2+1;
				indices[i*3+4] = j*2+3;
				indices[i*3+5] = j*2+2;    
			}
	
	        //setup dist for uv
	        float[] dist = new float[vcnt];
			float totalDist = 0;
			for(int i=1; i<pos.Length; ++i) {
				totalDist += (pos[i-1] - pos[i]).magnitude;
				dist[i] = totalDist;
			}
			float scaleAdj = (int)(totalDist+0.5f) * uvScale;
			for(int i=0; i<pos.Length; ++i) 
				dist[i] = dist[i] * scaleAdj / totalDist;
	
	        //setup pos & uv
			Vector3[] vertices  = new Vector3[vcnt*2];
			Vector2[] uv = new Vector2[vcnt*2];
			Vector3 oldTangent  = Vector3.zero;
	
	        Vector3 faceNormal  = Vector3.back;
	        if(closed) {
				Vector3 dir = (pos[pos.Length-1] - pos[pos.Length-2]);
	            oldTangent = Vector3.Cross(dir, faceNormal).normalized;
	        }
	
	        j = 0;
			for (int i=0; i < pos.Length-1; i++)
			{
	            Vector3 cur = pos[i];
	            Vector3 next = pos[i+1];
				Vector3 dir         = (next - cur);
	
				Vector3 tangent     = Vector3.Cross(dir, faceNormal).normalized;
	            Vector3 offset      = Vector3.zero;
	            switch(Loffset) {
	                case LineOffset.Centered:
	                    offset      = (oldTangent+tangent).normalized * lineWidth*0.5f;
	                    vertices[j*2]       = cur - offset;
	                    vertices[j*2+1]     = cur + offset;
	                break;
	                case LineOffset.Inner:
	                    offset      = (oldTangent+tangent).normalized * lineWidth;
	                    vertices[j*2]       = cur - offset;
	                    vertices[j*2+1]     = cur;
	                break;
	                case LineOffset.Outer:
	                    offset      = (oldTangent+tangent).normalized * lineWidth;
	                    vertices[j*2]       = cur;
	                    vertices[j*2+1]     = cur + offset;
	                break;
	            }
	
	            if(!flipY) {
	                uv[j*2]				= new Vector2(dist[i], 0);
	                uv[j*2+1]			= new Vector2(dist[i], 1);
	            } else {
	                uv[j*2]				= new Vector2(dist[i], 1);
	                uv[j*2+1]			= new Vector2(dist[i], 0);
	            }
	
				if (i == pos.Length - 2)
				{
	                switch(Loffset) {
	                    case LineOffset.Centered:
	                        vertices[j*2+2] = next - tangent * lineWidth*0.5f;
	                        vertices[j*2+3] = next + tangent * lineWidth*0.5f;
	                    break;
	                    case LineOffset.Inner:
	                        vertices[j*2+2] = next - tangent * lineWidth;
	                        vertices[j*2+3] = next;
	                    break;
	                    case LineOffset.Outer:
	                        vertices[j*2+2] = next;
	                        vertices[j*2+3] = next + tangent * lineWidth;
	                    break;
	                }
	
	                if(!flipY) {
	                    uv[j*2+2]	= new Vector2(dist[i+1], 0);
	                    uv[j*2+3]	= new Vector2(dist[i+1], 1);
	                } else {
	                    uv[j*2+2]	= new Vector2(dist[i+1], 1);
	                    uv[j*2+3]	= new Vector2(dist[i+1], 0);
	                }
				}
				oldTangent = tangent;
	            j++;
			}
	
	        //build mesh
	        var mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.SetTriangles(indices, 0);
			//mesh.SetTriangleStrip(indices,0);
			mesh.RecalculateBounds(); 
	        ;
	        return mesh;
		}
	
		public static Mesh CreateBoundMeshFromRect(Rect rc, float lineWidth, float radius, float uvScale) {
			var vec = new Vector3[] {
				new Vector3(rc.xMin, rc.yMin, 0),
				new Vector3(rc.xMin, rc.yMax, 0),
				new Vector3(rc.xMax, rc.yMax, 0),
				new Vector3(rc.xMax, rc.yMin, 0),
				new Vector3(rc.xMin, rc.yMin, 0),
			};
			var nVec = MakeRound(new List<Vector3>(vec), radius);
			return CreateLineMesh(nVec.ToArray(), lineWidth, uvScale, false, true);
		}
	
	    public static Mesh CreateBoundMeshFromBlocks(Point[] blks, float blockSize, float border, float round, float lineWidth, float lineUVScale) 
	    {
			int xmax = blks.Max(b=>b.X)+1;
			int ymax = blks.Max(b=>b.Y)+1;
	
	        //Positive
			byte[,] grid = CreateBitMapFromBlocks(blks, 1, 0, xmax, ymax);
	        byte[,] labels = new byte[xmax, ymax];
	        int numGrp = ConnectedComponentLabeling(grid, labels, xmax, ymax, true);
	
	        List<Mesh> meshList = new List<Mesh>();
	        Vector3 offset = new Vector3(blockSize * 0.5f, blockSize * 0.5f, 0);
	
	        for(int i=1; i<=numGrp; ++i) {
	            var pts = CreateEdgePointsForLabel(labels, i);
	            if(pts == null) continue;
	            List<Vector3> vec = new List<Vector3>();
	            for(int p=0; p<pts.Length; ++p) {
	                //Debug.Log(pts[p]);
	                vec.Add(new Vector3(pts[p].X * blockSize, pts[p].Y * blockSize, 0) + offset);
	            }
	            vec = MakeRound(vec, round);
	            meshList.Add(CreateLineMesh(vec.ToArray(), lineWidth, lineUVScale, false, true));
	        }
	
	        //Negative
			byte[,] gridNeg = CreateBitMapFromBlocks(blks, 0, 1, xmax, ymax);
	        byte[,] labelsNeg = new byte[xmax, ymax];
	        int numGrpNeg = ConnectedComponentLabeling(gridNeg, labelsNeg, xmax, ymax, false);
	
	        //Find Hole for negative
	        var holes = new HashSet<int>();
	        for(int i=0; i<numGrpNeg; ++i)
	            holes.Add(i+1);
	        for(int y=0; y<ymax; ++y) {
	            if(holes.Contains(labelsNeg[0,y]))
	                holes.Remove(labelsNeg[0,y]);
	            if(holes.Contains(labelsNeg[xmax-1,y]))
	                holes.Remove(labelsNeg[xmax-1,y]);
	        }
	        for(int x=0; x<xmax; ++x) {
	            if(holes.Contains(labelsNeg[x,0]))
	                holes.Remove(labelsNeg[x,0]);
	            if(holes.Contains(labelsNeg[x,ymax-1]))
	                holes.Remove(labelsNeg[x,ymax-1]);
	        }
	
	        //build mesh for negative
	        foreach(var h in holes) {
	            var pts = CreateEdgePointsForLabel(labelsNeg, h);
	            if(pts == null) continue;
	            List<Vector3> vec = new List<Vector3>();
	            for(int p=0; p<pts.Length; ++p) {
	                vec.Add(new Vector3(pts[p].X * blockSize, pts[p].Y * blockSize, 0) + offset);
	            }
	            vec = MakeRound(vec, round);
	            meshList.Add(CreateLineMesh(vec.ToArray(), lineWidth, lineUVScale, true, true));
	        }
	
	        //build Final mesh
	        Mesh mesh = new Mesh();
	        CombineInstance[] combine = new CombineInstance[meshList.Count];
	        for(int i=0; i<meshList.Count; ++i) {
	            combine[i].mesh = meshList[i];
	            combine[i].subMeshIndex = 0;
	        }
	        mesh.CombineMeshes(combine, true, false);
			mesh.RecalculateBounds(); 
	        ;
	        return mesh;
	    }
	
	#region CCL
	//FYI (https://en.wikipedia.org/wiki/Connected-component_labeling)
	    static HashSet<byte> Get5Neighbors(byte[,] grid, Point cur)
	    {
	        var N = new HashSet<byte>();
	
	        //left
	        cur.X -= 1;
	        if(Grid(grid, cur) != 0) N.Add(grid[cur.X, cur.Y]);
	
	        //up-left
	        cur.Y += 1;
	        if(Grid(grid, cur) != 0) N.Add(grid[cur.X, cur.Y]);
	
	        //up
	        cur.X += 1;
	        if(Grid(grid, cur) != 0) N.Add(grid[cur.X, cur.Y]);
	
	        //up-right
	        cur.X += 1;
	        if(Grid(grid, cur) != 0) N.Add(grid[cur.X, cur.Y]);
	
	        return N;
	    }
	
	    static HashSet<byte> Get3Neighbors(byte[,] grid, Point cur)
	    {
	        var N = new HashSet<byte>();
	
	        //left
	        cur.X -= 1;
	        if(Grid(grid, cur) != 0) N.Add(grid[cur.X, cur.Y]);
	
	        //up
	        cur.Y += 1;
	        cur.X += 1;
	        if(Grid(grid, cur) != 0) N.Add(grid[cur.X, cur.Y]);
	
	        return N;
	    }
	
	    //CCL 구현함수, 
	    //data : 입력 bitmap
	    //labels : 출력 bitmap
	    //xmax, ymax : bitmap의 범위
	    //use8Way : true 인 경우 8방향 CCL, false인 경우 4방향 CCL
	    static int ConnectedComponentLabeling(byte[,] data, byte[,] labels, int xmax, int ymax, bool use8Way)
	    {
	        Dictionary<byte, HashSet<byte>> links = new Dictionary<byte, HashSet<byte>>();
	        byte nextLabel = 1;
	
	        //Pass1
	        for(int y=ymax-1; y>=0; --y) {
	            for(int x=0; x<xmax; ++x) {
	                if(data[x,y] == 0) continue;
	
	                byte currentLabel;
	                HashSet<byte> neighbors = null;
	                if(use8Way)
	                    neighbors = Get5Neighbors(labels, new Point(x,y));
	                else
	                    neighbors = Get3Neighbors(labels, new Point(x,y));
	                if(neighbors.Count == 0) {
	                    currentLabel = nextLabel;
	                    links.Add(currentLabel, new HashSet<byte>(){ currentLabel });
	                    nextLabel++;
	                } else {
	                    currentLabel = neighbors.Min();
	
						foreach(var n in neighbors) {
	                        links[n].UnionWith(neighbors);
	                    }
	                }
	                labels[x,y] = currentLabel;
	            }
	        }
	
	        //spread label links
	        foreach(var k in links.Keys) {
	            foreach(var v in links[k])
	                if(k != v)
	                    links[v].UnionWith(links[k]);
	        }
	
	        //Pass2
	        int maxV = 0;
	        for(int y=ymax-1; y>=0; --y) {
	            for(int x=0; x<xmax; ++x) {
	                byte v = labels[x,y];
	                if(v == 0) continue;
	                labels[x,y] = links[v].Min();
	                if(labels[x,y] > maxV)
	                    maxV = labels[x,y];
	            }
	        }
	
	        return maxV;
	    }
	
	#endregion //CCL
	
	#region Common functions
	    static void Print(byte[,] board) 
	    {
	        var sb = new System.Text.StringBuilder();
			var height = board.GetLength(1);
			var width = board.GetLength(0);
	        for(int y=height-1; y>=0; --y) {
	            for(int x=0; x<width; ++x)
	                sb.Append(board[x,y]);
	            sb.AppendLine();
	        }
	        Debug.Log(sb.ToString());
	    }
	
	    static byte[,] CreateBitMapFromBlocks(Point[] blks, byte posVal, byte negVal, int xmax, int ymax) 
	    {
			byte[,] grid = new byte[xmax, ymax];
	        for(int y=0; y<ymax; ++y)
	            for(int x=0; x<xmax; ++x)
	                grid[x,y] = negVal;
			foreach(var b in blks) {
				grid[b.X, b.Y] = posVal;
	        }
	        return grid;
	    }
	
		static List<Vector3> MakeRound(List<Vector3> vec, float radius) {
			Vector3 v1;
			Vector3 v2;
			List<Vector3> nVec = new List<Vector3>();
			for(int i=0; i<vec.Count; ++i) {
				int i1, i2;
				if(i == 0) {
					i1 = vec.Count-2;
					i2 = 1;
	            }
	            else if(i == vec.Count-1) {
					i1 = i-1;
					i2 = 1;
				} else {
					i1 = i-1;
					i2 = i+1;
				}
				v1 = (vec[i1] - vec[i]).normalized;
				v2 = (vec[i2] - vec[i]).normalized;
	
				float curvature = Vector3.Dot(v1, v2);
				if(Mathf.Abs(curvature) < 0.1f) {
					nVec.Add(vec[i] + v1 * radius);
					nVec.Add(vec[i] + (v1+v2) * radius * 0.3f);
					nVec.Add(vec[i] + v2 * radius);
				} else {
					nVec.Add(vec[i]);
				}
			}
			return nVec;
		}
	
		static byte Grid(byte[,] grid, Point p) 
	    {
			var x = p.X;
			var y = p.Y;
			var height = grid.GetLength(1);
			var width = grid.GetLength(0);
			return (0 <= x && x < width && 0 <= y && y < height) ? grid[x, y] : (byte)0;
		}
	
		static byte Grid(byte[,] grid, int x, int y) 
	    {
			int height = grid.GetLength(1);
			int width = grid.GetLength(0);
			return (0 <= x && x < width && 0 <= y && y < height) ? grid[x, y] : (byte)0;
		}
	#endregion //Common functions
	
	#region MarchingSquare
	//FYI (http://www.sakri.net/blog/2009/05/28/detecting-edge-pixels-with-marching-squares-algorithm/)
	    static int MarchingSquareVal(byte[,] data, int x, int y, byte v) 
	    {
	        int val = 0;
	        if(Grid(data,x,y) == v) val |= 1;
	        if(Grid(data,x+1,y) == v) val |= 2;
	        if(Grid(data,x+1,y+1) == v) val |= 4;
	        if(Grid(data,x,y+1) == v) val |= 8;
	        return val;
	    }
	
	    static Point FindFirstOccurance(byte[,] grid, byte val) 
	    {
			int ymax = grid.GetLength(1);
			int xmax = grid.GetLength(0);
	        for(int y=0; y<ymax; ++y) {
	            for(int x=0; x<xmax; ++x) {
	                if(grid[x,y] == val) {
	                    return new Point(x,y);
	                }
	            }
	        }
	        return new Point(-1,-1);
	    }
	
	    static Point[] CreateEdgePointsForLabel(byte[,] labels, int labelIndex) {
	        Point pt = FindFirstOccurance(labels, (byte)labelIndex);
	        if(pt.X < 0) {
	            return null;
	        }
	
	        pt.X-=1;
	        pt.Y-=1;
	
	        int ix = pt.X;
	        int iy = pt.Y;
	
	        int initialVal = MarchingSquareVal(labels, pt.X, pt.Y, (byte)labelIndex);
	        if(initialVal == 0 || initialVal == 15) {
	            Debug.LogError("CreateBoundMeshFromBlocks initialVal("+initialVal+") is out of range "+pt);
	            return null;
	        }
	
	        List<Point> pts = new List<Point>();
	        pts.Add(pt);
	        int prevDir = -1;
	        do{
	            int d = -1;
	            int v = MarchingSquareVal(labels, pt.X, pt.Y, (byte)labelIndex);
	            switch(v) {
	                case 8:
	                case 9:
	                case 11:
	                    d = 0; break;
	
	                case 4:
	                case 12:
	                case 13:
	                    d = 1; break;
	
	                case 2: 
	                case 6: 
	                case 14: 
	                    d = 2; break;
	
	                case 1:
	                case 3:
	                case 7:
	                    d = 3; break;
	
	                case 5:
	                    d = (prevDir == 0)?1:3;
	                    break;
	
	                case 10:
	                    d = (prevDir == 3)?0:2;
	                    break;
	
	                default: 
	                    Debug.LogError("CreateBoundMeshFromBlocks MarchingSquareVal for "+pt+" is "+v);
	                    return null;
	            }
	            switch(d) {
	                case 0: pt.Y += 1; break;
	                case 1: pt.X += 1; break;
	                case 2: pt.Y -= 1; break;
	                case 3: pt.X -= 1; break;
	            }
	            pts.Add(pt);
	            prevDir = d;
	        }
	        while(ix != pt.X || iy != pt.Y);
	        return pts.ToArray();
	    }
	#endregion //MarchingSquare
	}
}

