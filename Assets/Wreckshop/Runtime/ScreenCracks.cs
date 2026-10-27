using System.Collections.Generic;
using UnityEngine;

namespace PocketWreckshop
{
    // Persistent pixel geometry over the original glass. No replacement image or texture.
    public sealed class ScreenCracks : MonoBehaviour
    {
        Mesh mesh;
        MeshRenderer view;
        int previousLevel=-1;
        Rect previousArea;
        Sprite previousSprite;
        readonly HashSet<Vector2Int> pixels=new HashSet<Vector2Int>();
        public void Refresh(SpriteRenderer owner,Rect area,int level,Material material)
        {
            if(!mesh)
            {
                mesh=new Mesh{name="Persistent glass crack pixels"};
                gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                view=gameObject.AddComponent<MeshRenderer>();
            }
            view.sharedMaterial=material;view.sortingLayerID=owner.sortingLayerID;view.sortingOrder=owner.sortingOrder+1;
            if(previousLevel==level&&previousArea==area&&previousSprite==owner.sprite)return;
            previousLevel=level;previousArea=area;previousSprite=owner.sprite;pixels.Clear();
            // Later paths extend the existing fractures; every earlier pixel remains.
            Trace(area,.24f,.69f,.31f,.64f,.34f,.64f,.38f,.58f);
            Trace(area,.31f,.64f,.29f,.60f,.29f,.57f);
            if(level>=2)
            {
                Trace(area,.24f,.69f,.20f,.77f,.16f,.81f);
                Trace(area,.38f,.58f,.41f,.52f,.46f,.49f);
                Trace(area,.34f,.64f,.43f,.67f,.48f,.66f);
            }
            if(level>=3)
            {
                Trace(area,.70f,.30f,.63f,.34f,.60f,.40f,.52f,.42f);
                Trace(area,.70f,.30f,.76f,.22f,.79f,.19f);
                Trace(area,.70f,.30f,.78f,.34f,.84f,.34f);
                Trace(area,.70f,.30f,.68f,.23f);
            }
            if(level>=4)
            {
                Trace(area,.48f,.66f,.59f,.71f,.70f,.69f,.78f,.75f,.96f,.78f);
                Trace(area,.16f,.81f,.12f,.90f,.07f,.95f);
                Trace(area,.46f,.49f,.52f,.42f,.53f,.34f);
                Trace(area,.78f,.34f,.82f,.44f,.91f,.49f);
            }
            if(level>=5)
            {
                Trace(area,.29f,.57f,.21f,.51f,.20f,.43f,.10f,.38f,.02f,.37f);
                Trace(area,.63f,.34f,.56f,.25f,.57f,.16f,.50f,.04f);
                Trace(area,.70f,.30f,.71f,.16f,.76f,.04f);
                Trace(area,.84f,.34f,.93f,.29f,.98f,.24f);
                Trace(area,.59f,.71f,.58f,.82f,.65f,.90f,.64f,.97f);
            }
            var vertices=new List<Vector3>();var colors=new List<Color32>();var triangles=new List<int>();
            foreach(var pixel in pixels)Quad(pixel,new Color32(25,36,44,255));
            var highlights=new HashSet<Vector2Int>();
            foreach(var pixel in pixels)
            {
                var edge=pixel+Vector2Int.up;
                if(!pixels.Contains(edge)&&area.Contains(edge))highlights.Add(edge);
            }
            foreach(var pixel in highlights)Quad(pixel,new Color32(211,229,232,255));
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
            void Quad(Vector2Int pixel,Color32 color)
            {
                int start=vertices.Count;var pivot=owner.sprite.pivot;float ppu=owner.sprite.pixelsPerUnit;
                float x=(pixel.x-pivot.x)/ppu,y=(pixel.y-pivot.y)/ppu;
                vertices.Add(new Vector3(x,y,0));vertices.Add(new Vector3(x+1/ppu,y,0));
                vertices.Add(new Vector3(x+1/ppu,y+1/ppu,0));vertices.Add(new Vector3(x,y+1/ppu,0));
                for(int i=0;i<4;i++)colors.Add(color);
                triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
            }
        }
        void Trace(Rect area,params float[] points)
        {
            for(int i=0;i<points.Length-2;i+=2)
            {
                int x=Mathf.RoundToInt(area.x+area.width*points[i]),y=Mathf.RoundToInt(area.y+area.height*points[i+1]);
                int endX=Mathf.RoundToInt(area.x+area.width*points[i+2]),endY=Mathf.RoundToInt(area.y+area.height*points[i+3]);
                int dx=Mathf.Abs(endX-x),dy=-Mathf.Abs(endY-y),sx=x<endX?1:-1,sy=y<endY?1:-1,error=dx+dy;
                while(true)
                {
                    var pixel=new Vector2Int(x,y);if(area.Contains(pixel))pixels.Add(pixel);
                    if(x==endX&&y==endY)break;
                    int twice=2*error;if(twice>=dy){error+=dy;x+=sx;}if(twice<=dx){error+=dx;y+=sy;}
                }
            }
        }
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
}
