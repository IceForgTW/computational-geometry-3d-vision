#region File Description
//-----------------------------------------------------------------------------
// CPK3Model.cs version 0.1b
//
// Component to render PK3Models
//
// Created by Spear (http://www.codernet.es), spear@codernet.es
// Coordinator of XNA Community (http://www.codeplex.com/XNACommunity)
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MD3Data;
#endregion

namespace PK3Model
{
    public struct SRenderInfoMesh
    {
        public VertexPositionNormalTexture[] vertex_info;
        public string texture_name;
    };

    public class CPK3Model
    {
        private enum TMD3Part
        {
            HEAD,
            UPPER,
            LOWER
        };

        private class MD3Object
        {
            public TMD3Part part;
            public List<MD3SubMeshes> meshes;
            public List<MD3tag> tags;
            public List<BoundingBox> bounding_boxes;
            public int num_frames;

            private bool finish_animation = true;
            private MD3animation current_animation = null;
            private int current_frame = 0;
            private int next_frame = 0;
            private float seconds = 0.0f;
            private float time_weight = 0.0f;

            public MD3Object(TMD3Part p)
            {
                part = p;
            }

            public void update(float e)
            {
                if (current_animation == null)
                    return;

                seconds += e;

                int start_frame = current_animation.startFrame;
                int num_frames = current_animation.numFrames;

                if (seconds * current_animation.framesPerSecond >= num_frames)
                {
                    if (current_animation.loopingFrames == 0)
                    {
                        finish_animation = true;
                        return;
                    }
                    seconds = 0.0f;
                    next_frame = start_frame;
                }

                float value = seconds * current_animation.framesPerSecond;
                value = value % num_frames;
                current_frame = start_frame + (int)(value);

                if (current_frame + 1 >= start_frame + num_frames)
                {
                    if (current_animation.loopingFrames == 0)
                        next_frame = current_frame;
                    else
                        next_frame = start_frame;
                }
                else
                {
                    next_frame = current_frame + 1;
                }

                time_weight = value - ((int)(value));
            }

            public void setAnimation(MD3animation current)
            {
                if (current != null)
                {
                    current_frame = current.startFrame;
                    finish_animation = false;
                }
                else
                {
                    current_frame = 0;
                }
                seconds = 0.0f;
                current_animation = current;
            }

            public bool IsFinishAnimation()
            {
                return finish_animation;
            }

            public Vector3 getPositionVertex(int index_mesh, int index_vertex)
            {
                Vector3 pos;
                if (current_animation != null)
                {
                    int num_vertices = meshes[index_mesh].vertices.Count / num_frames;
                    int current_offset_vertex = current_frame * num_vertices;
                    if (current_frame != next_frame)
                    {
                        int next_offset_vertex = next_frame * num_vertices;
                        Vector3 pos1 = meshes[index_mesh].vertices[current_offset_vertex + index_vertex];
                        Vector3 pos2 = meshes[index_mesh].vertices[next_offset_vertex + index_vertex];
                        pos = Vector3.Lerp(pos1, pos2, time_weight);
                    }
                    else
                    {
                        pos = meshes[index_mesh].vertices[current_offset_vertex + index_vertex];
                    }
                }
                else
                {
                    pos = meshes[index_mesh].vertices[index_vertex];
                }
                return pos;
            }

            public Vector3 getNormal(int index_mesh, int index_normal)
            {
                Vector3 normal;
                if (current_animation != null)
                {
                    int num_normals = meshes[index_mesh].normals.Count / num_frames;
                    int current_offset_normal = current_frame * num_normals;
                    if (current_frame != next_frame)
                    {
                        int next_offset_normal = next_frame * num_normals;
                        Vector3 normal1 = meshes[index_mesh].vertices[current_offset_normal + index_normal];
                        Vector3 normal2 = meshes[index_mesh].vertices[next_offset_normal + index_normal];
                        normal = Vector3.Lerp(normal1, normal2, time_weight);
                    }
                    else
                    {
                        normal = meshes[index_mesh].vertices[current_offset_normal + index_normal];
                    }
                }
                else
                {
                    normal = meshes[index_mesh].normals[index_normal];
                }
                return normal;
            }

            public Matrix getMatrixTag(int index)
            {
                Matrix m;
                int num_tags = tags.Count / num_frames;
                int current_offset_tag = current_frame * num_tags;
                if (current_frame != next_frame)
                {
                    int next_offset_tag = next_frame * num_tags;
                    Vector3 tag1_pos = tags[current_offset_tag + index].vPosition;
                    Vector3 tag2_pos = tags[next_offset_tag + index].vPosition;
                    Vector3 pos = Vector3.Lerp(tag1_pos, tag2_pos, time_weight);
                    Matrix translation = Matrix.CreateTranslation(pos);
                    Matrix rotation1 = tags[current_offset_tag + index].rotation;
                    Matrix rotation2 = tags[next_offset_tag + index].rotation;
                    Matrix rotation = Matrix.Lerp(rotation1, rotation2, time_weight);
                    m = rotation * translation;
                }
                else
                {
                    Vector3 pos = tags[current_offset_tag + index].vPosition;
                    Matrix translation = Matrix.CreateTranslation(pos);
                    Matrix rotation = tags[current_offset_tag + index].rotation;
                    m = rotation * translation;
                }
                return m;
            }

            public BoundingBox getBoundingBox()
            {
                return bounding_boxes[current_frame];
            }

        };

        //loaded data
        private List<MD3Object> head_md3 = new List<MD3Object>();
        private List<MD3Object> upper_md3 = new List<MD3Object>();
        private List<MD3Object> lower_md3 = new List<MD3Object>();
        private Dictionary<string, MD3animation> animations = new Dictionary<string, MD3animation>();

        //Matrix
        Matrix push_matrix = Matrix.Identity;

        //render

        private List<SRenderInfoMesh> head_vertices;
        private List<SRenderInfoMesh> upper_vertices;
        private List<SRenderInfoMesh> lower_vertices;

        public SRenderInfoMesh[] HeadVertices
        {
            get { return head_vertices.ToArray(); }
        }

        public SRenderInfoMesh[] UpperVertices
        {
            get { return upper_vertices.ToArray(); }
        }

        public SRenderInfoMesh[] LowerVertices
        {
            get { return lower_vertices.ToArray(); }
        }

        private int current_head = 0;
        private int current_upper = 0;
        private int current_lower = 0;

        //render declarations
        private VertexDeclaration quad_vertex_decl;
        private BasicEffect model_effect;
        private Dictionary<string, Texture> list_textures;
        private bool use_lighting = true;

        private float scale_time = 1.0f;

        private void loadTextures(ContentReader input, int textures_count)
        {
            list_textures = new Dictionary<string, Texture>();
            for (int i = 0; i < textures_count; ++i)
            {
                string key = input.ReadString();
                Texture texture = input.ReadExternalReference<Texture>();
                list_textures.Add(key, texture);
            }
        }

        public Texture2D getTexture(string name)
        {
            Texture2D tex = null;
            if (list_textures.ContainsKey(name))
            {
                tex = (Texture2D)list_textures[name];
            }
            return tex;
        }

        public void load(ContentReader input)
        {
            int textures_count = input.ReadInt32();
            loadTextures(input, textures_count);
            int md3_count = input.ReadInt32();
            //read md3 data
            for (int i = 0; i < md3_count; ++i)
            {
                MD3Object o = new MD3Object(TMD3Part.HEAD);
                loadMD3Data(input, o);
                if (o.part == TMD3Part.HEAD)
                    head_md3.Add(o);
                else if (o.part == TMD3Part.UPPER)
                    upper_md3.Add(o);
                else
                    lower_md3.Add(o);
            }
            Debug.Assert(head_md3.Count >= 1, "PK3 invalid format, not found head md3 file!");
            Debug.Assert(upper_md3.Count >= 1, "PK3 invalid format, not found upper md3 file!");
            Debug.Assert(lower_md3.Count >= 1, "PK3 invalid format, not found lower md3 file!");

            int anim_count = input.ReadInt32();
            for (int i = 0; i < anim_count; ++i)
            {
                MD3animation anim = new MD3animation();
                anim.strName = input.ReadString();
                anim.startFrame = input.ReadInt32();
                anim.numFrames = input.ReadInt32();
                anim.loopingFrames = input.ReadInt32();
                anim.framesPerSecond = input.ReadInt32();
                animations.Add(anim.strName, anim);
            }
        }

        private void loadMD3Data(ContentReader input, MD3Object m)
        {
            //load meshes
            m.meshes = new List<MD3SubMeshes>();
            int part = input.ReadInt32();
            if (part == 0)
                m.part = TMD3Part.HEAD;
            else if (part == 1)
                m.part = TMD3Part.LOWER;
            else
                m.part = TMD3Part.UPPER;

            m.num_frames = input.ReadInt32();
            int sub_meshes_count = input.ReadInt32();
            for (int i = 0; i < sub_meshes_count; ++i)
            {
                MD3SubMeshes sub_mesh = new MD3SubMeshes();
                sub_mesh.indices = new List<int>();
                sub_mesh.vertices = new List<Vector3>();
                sub_mesh.normals = new List<Vector3>();
                sub_mesh.text_coord = new List<Vector2>();
                sub_mesh.skins = new List<string>();
                sub_mesh.meshinfo.strName = input.ReadString();
                input.ReadObject<List<int>>(sub_mesh.indices);
                input.ReadObject<List<Vector3>>(sub_mesh.vertices);
                input.ReadObject<List<Vector3>>(sub_mesh.normals);
                input.ReadObject<List<Vector2>>(sub_mesh.text_coord);
                m.meshes.Add(sub_mesh);
            }

            //load tags
            m.tags = new List<MD3tag>();
            int tags_count = input.ReadInt32();
            for (int i = 0; i < tags_count; ++i)
            {
                MD3tag tag = new MD3tag();
                tag.strName = input.ReadString();
                tag.vPosition = input.ReadVector3();
                tag.rotation = input.ReadMatrix();
                m.tags.Add(tag);
            }
            //load bounding_boxes
            m.bounding_boxes = new List<BoundingBox>();
            int bb_count = input.ReadInt32();
            for (int i = 0; i < bb_count; ++i)
            {
                BoundingBox bb = new BoundingBox();
                bb.Min = input.ReadVector3();
                bb.Max = input.ReadVector3();
                m.bounding_boxes.Add(bb);
            }
        }

        private void MD3DataToRenderData(MD3Object source)
        {
            int count = source.meshes.Count;
            for (int i = 0; i < count; ++i)
            {
                List<SRenderInfoMesh> v_list = null;
                if (source.part == TMD3Part.HEAD)
                    v_list = head_vertices;
                else if (source.part == TMD3Part.UPPER)
                    v_list = upper_vertices;
                else
                    v_list = lower_vertices;

                if (v_list != null)
                {
                    int vcount = v_list[i].vertex_info.Length;
                    for (int a = 0; a < vcount; ++a)
                    {
                        int id = source.meshes[i].indices[a];
                        v_list[i].vertex_info[a].Position = source.getPositionVertex(i, id);
                        v_list[i].vertex_info[a].Normal = source.getNormal(i, id);
                    }
                }
            }
        }

        private void initializeVertices(MD3Object m)
        {
            if (m.part == TMD3Part.HEAD)
                head_vertices = new List<SRenderInfoMesh>();
            else if (m.part == TMD3Part.UPPER)
                upper_vertices = new List<SRenderInfoMesh>();
            else
                lower_vertices = new List<SRenderInfoMesh>();

            int count = m.meshes.Count;
            for (int i = 0; i < count; ++i)
            {
                int vcount = m.meshes[i].indices.Count;
                SRenderInfoMesh render_info = new SRenderInfoMesh();
                render_info.texture_name = m.meshes[i].meshinfo.strName;
                render_info.vertex_info = new VertexPositionNormalTexture[vcount];
                for (int a = 0; a < vcount; ++a)
                {
                    int id = m.meshes[i].indices[a];
                    render_info.vertex_info[a].TextureCoordinate.X = m.meshes[i].text_coord[id].X;
                    render_info.vertex_info[a].TextureCoordinate.Y = m.meshes[i].text_coord[id].Y;
                }
                if (m.part == TMD3Part.HEAD)
                    head_vertices.Add(render_info);
                else if (m.part == TMD3Part.UPPER)
                    upper_vertices.Add(render_info);
                else
                    lower_vertices.Add(render_info);
            }
        }

        public void initialize(GraphicsDevice device)
        {
            model_effect = new BasicEffect(device, null);
            model_effect.TextureEnabled = true;
            quad_vertex_decl = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);

            initializeVertices(head_md3[current_head]);
            initializeVertices(upper_md3[current_upper]);
            initializeVertices(lower_md3[current_lower]);

            MD3DataToRenderData(head_md3[current_head]);
            MD3DataToRenderData(upper_md3[current_upper]);
            MD3DataToRenderData(lower_md3[current_lower]);
        }

        public bool useLighting
        {
            get
            {
                return use_lighting;
            }
            set
            {
                use_lighting = value;
            }
        }

        public int currentHeadID
        {
            get
            {
                return current_head;
            }
            set
            {
                if (value < head_md3.Count && current_head != value)
                {
                    current_head = value;
                    initializeVertices(head_md3[current_head]);
                    MD3DataToRenderData(head_md3[current_head]);

                }
            }
        }

        public int currentLegsID
        {
            get
            {
                return current_lower;
            }
            set
            {
                if (value < lower_md3.Count && current_lower != value)
                {
                    current_lower = value;
                    initializeVertices(lower_md3[current_lower]);
                    MD3DataToRenderData(lower_md3[current_lower]);
                }
            }
        }

        public int currentUpperID
        {
            get
            {
                return current_upper;
            }
            set
            {
                if (value < upper_md3.Count && current_upper != value)
                {
                    current_upper = value;
                    initializeVertices(upper_md3[current_upper]);
                    MD3DataToRenderData(upper_md3[current_upper]);
                }

            }
        }

        public float scaleTime
        {
            get
            {
                return scale_time;
            }
            set
            {
                scale_time = value;
            }
        }

        public void update(GameTime gameTime)
        {
            float elapsed = gameTime.ElapsedGameTime.Seconds + (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            elapsed *= scale_time;
            lower_md3[current_lower].update(elapsed);
            MD3DataToRenderData(lower_md3[current_lower]); //only if current_frame change
            upper_md3[current_upper].update(elapsed);
            MD3DataToRenderData(upper_md3[current_upper]); //only if current_frame change
            head_md3[current_head].update(elapsed);
            MD3DataToRenderData(head_md3[current_head]); //only if current_frame change
        }

        public void render(GraphicsDevice device, Matrix world, Matrix view, Matrix proj)
        {
            device.VertexDeclaration = quad_vertex_decl;

            model_effect.View = view;
            model_effect.Projection = proj;

            if (use_lighting)
                model_effect.EnableDefaultLighting();
            else
                model_effect.LightingEnabled = false;

            push_matrix = world;
            renderPart(device, lower_vertices, push_matrix);
            pushMatrix(lower_md3[current_lower], "tag_torso");
            renderPart(device, upper_vertices, push_matrix);
            pushMatrix(upper_md3[current_upper], "tag_head");
            renderPart(device, head_vertices, push_matrix);
        }

        //method shadow effect render
        public void renderShadowEffect(GraphicsDevice device, Matrix world, Effect shadow_effect)
        {
            device.VertexDeclaration = quad_vertex_decl;

            push_matrix = world;
            renderPart(device, lower_vertices, push_matrix, shadow_effect);
            pushMatrix(lower_md3[current_lower], "tag_torso");
            renderPart(device, upper_vertices, push_matrix, shadow_effect);
            pushMatrix(upper_md3[current_upper], "tag_head");
            renderPart(device, head_vertices, push_matrix, shadow_effect);
        }

        private void pushMatrix(MD3Object md3, string tag_id)
        {
            for (int i = 0; i < md3.tags.Count; ++i)
            {
                if (md3.tags[i].strName == tag_id)
                {
                    //cache md3.tags[i].cache_matrix
                    Matrix m = md3.getMatrixTag(i);
                    push_matrix = m * push_matrix;
                    break;
                }
            }
        }

        public BoundingBox getBoundingBoxHead()
        {
            Matrix m = Matrix.Identity;
            MD3Object md3 = lower_md3[current_lower];
            m = getTagMatrix(md3, "tag_torso");
            md3 = upper_md3[current_lower];
            m = getTagMatrix(md3, "tag_head") * m;
            md3 = head_md3[current_head];
            BoundingBox bounding_box = md3.getBoundingBox();
            Matrix mtx = m * Matrix.CreateTranslation(bounding_box.Min);
            bounding_box.Min = mtx.Translation;
            mtx = m * Matrix.CreateTranslation(bounding_box.Max);
            bounding_box.Max = mtx.Translation;
            return bounding_box;
        }

        public BoundingBox getBoundingBoxUpper()
        {
            Matrix m = Matrix.Identity;
            MD3Object md3 = lower_md3[current_lower];
            m = getTagMatrix(md3, "tag_torso");
            md3 = upper_md3[current_upper];
            BoundingBox bounding_box = md3.getBoundingBox();
            Matrix mtx = m * Matrix.CreateTranslation(bounding_box.Min);
            bounding_box.Min = mtx.Translation;
            mtx = m * Matrix.CreateTranslation(bounding_box.Max);
            bounding_box.Max = mtx.Translation;
            return bounding_box;
        }

        public BoundingBox getBoundingBoxLegs()
        {
            MD3Object md3 = lower_md3[current_lower];
            return md3.getBoundingBox();
        }

        public BoundingBox getBoundingBox()
        {
            BoundingBox lower_bounding_box = getBoundingBoxLegs();
            BoundingBox head_bounding_box = getBoundingBoxHead();
            BoundingBox upper_bounding_box = getBoundingBoxUpper();
            BoundingBox bb = new BoundingBox();
            bb = BoundingBox.CreateMerged(lower_bounding_box, head_bounding_box);
            bb = BoundingBox.CreateMerged(bb, upper_bounding_box);
            return bb;
        }

        private Matrix getTagMatrix(MD3Object md3, string name)
        {
            Matrix m = Matrix.Identity;
            for (int i = 0; i < md3.tags.Count; ++i)
            {
                if (md3.tags[i].strName == name)
                {
                    m = md3.getMatrixTag(i);
                    break;
                }
            }
            return m;
        }

        public Matrix getWeaponMatrix()
        {
            Matrix m_torso = Matrix.Identity;
            MD3Object md3 = lower_md3[current_lower];
            m_torso = getTagMatrix(md3, "tag_torso");
            md3 = upper_md3[current_upper];
            return getTagMatrix(md3, "tag_weapon") * m_torso;
        }

        //method shadow effect render part
        private void renderPart(GraphicsDevice device, List<SRenderInfoMesh> v_list, Matrix world, Effect shadow_effect)
        {
            shadow_effect.Begin();
            shadow_effect.Parameters["xWorld"].SetValue(world);
            for (int i = 0; i < v_list.Count; ++i)
            {
                VertexPositionNormalTexture[] vertices = v_list[i].vertex_info;
                //model_effect.Texture = getTexture(v_list[i].texture_name);
                foreach (EffectPass pass in shadow_effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    int count_primitives = vertices.Length / 3;
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, count_primitives);
                    pass.End();
                }
            }
            shadow_effect.End();
        }

        private void renderPart(GraphicsDevice device, List<SRenderInfoMesh> v_list, Matrix world)
        {
            model_effect.Begin();
            model_effect.World = world;
            for (int i = 0; i < v_list.Count; ++i)
            {
                VertexPositionNormalTexture[] vertices = v_list[i].vertex_info;
                model_effect.Texture = getTexture(v_list[i].texture_name);
                foreach (EffectPass pass in model_effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    int count_primitives = vertices.Length / 3;
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, count_primitives);
                    pass.End();
                }
            }
            model_effect.End();
        }

        public void setAnimation(string name)
        {
            if (animations.ContainsKey(name))
            {
                MD3animation current = animations[name];
                bool legs, torso;
                legs = torso = false;
                if (name.IndexOf("BOTH") >= 0)
                {
                    legs = torso = true;
                }
                else if (name.IndexOf("LEGS") >= 0)
                {
                    legs = true;
                }
                else if (name.IndexOf("TORSO") >= 0)
                {
                    torso = true;
                }
                if (legs)
                {
                    for (int i = 0; i < lower_md3.Count; ++i)
                    {
                        lower_md3[i].setAnimation(current);
                    }
                }
                if (torso)
                {
                    for (int i = 0; i < upper_md3.Count; ++i)
                    {
                        upper_md3[i].setAnimation(current);
                    }
                }
            }
        }

        public bool isFinishAnimationLegs()
        {
            return lower_md3[current_lower].IsFinishAnimation();
        }
        public bool isFinishAnimationTorso()
        {
            return upper_md3[current_upper].IsFinishAnimation();
        }
    }
}
