using Editor.FileManager;
using Editor.GameEntities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Editor
{
    //===================================================================================
    // .rmap file is a binary file that contains:
    //  Endianness(0x00 is Little-endian, 0xFF is Big-endian)[byte]
    //  Number of billboards[int]
    //  List of all billboards[Billboard]
    //  Number of gameobjects[int]
    //  List of all gameobjects[GameObject]
    //  Number of tracks[int]
    //  List of all tracks[Track]
    // 
    // Billboard:
    //  Number of LODs[int]
    //  List of LOD image sizes in bytes[int]
    //  List of LODs (PNG-encoded images)
    //
    // Collider:
    //  Position(relative to parent GameObject) [float3]
    //  Size [float3]
    //
    // GameObject:
    //  Number of Colliders[int]
    //  List of colliders[Collider = 24 bytes]
    //  Number of Billboards[int]
    //  List of all Billboards(each billboard is represented by it's index in list)
    //  + their positions relative to GameObject
    //  + ther widths [int + float3 + float]
    //
    // Track:
    //  Number of HeelKeypoints[int]
    //  List of HeelKeypoints(ZY)[float2]
    //  Number of Curvatures[int]
    //  List of Curvatures(start[float], length[float], value[float])
    //  Number of GameObjects[int]
    //  List of GameObjects(each game object is represented by it's index in list)
    //  + their positions(XZ) in the track coordinate system[int + float2]
    //===================================================================================
    // .rproj file is a binary file that contains:
    //  Endianness(0x00 is Little-endian, 0xFF is Big-endian)[byte] |
    //  Number of billboards[int]                                   |
    //  List of all billboards[Billboard]                           |
    //  Number of gameobjects[int]                                  |
    //  List of all gameobjects[GameObject]                         |
    //  Number of tracks[int]                                       |
    //  List of all tracks[Track]                                   | <= like in .rmap file
    //  Number of files[int]
    //  List of all files[File]
    //
    // File(folders are represented as files with FileType = 0 and entity id = -1):
    //    Parent directory id in this list[int] (if file is in root then -1)
    //    File type[byte]  (*)
    //    Entity id(if file is folder -1)[int]
    //    File name length in bytes[int]
    //    File name[ASCII, to the end of data]
    //
    // (*)
    //   0 - Folder
    //   1 - Billboard
    //   2 - GameObject
    //   3 - Track
    //===================================================================================

    public class Serializers
    {
        List<Billboard> billboards = new List<Billboard>();
        List<GameObject> gameObjects = new List<GameObject>();
        List<Track> tracks = new List<Track>();
        List<IContent> files = new List<IContent>();

        #region Get__Id

        byte[] ShaHash(Bitmap image)
        {
            var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));
            return new SHA256Managed().ComputeHash(bytes);
        }

        bool CompareBillboards(Billboard x, Billboard y)
        {
            if (x.LODs.Count != y.LODs.Count)
                return false;

            for (int i = 0; i < x.LODs.Count; i++)
            {
                var x_hash = ShaHash(x.LODs[i].Image);
                var y_hash = ShaHash(y.LODs[i].Image);
                for (int j = 0; j < x_hash.Length; j++)
                    if (x_hash[j] != y_hash[j])
                        return false;
            }

            return true;
        }

        bool CompareGameObjects(GameObject x, GameObject y)
        {
            if (x.Billboards.Count != y.Billboards.Count)
                return false;

            if (x.Colliders.Count != y.Colliders.Count)
                return false;

            for (int i = 0; i < x.Billboards.Count; i++)
                if (!CompareBillboards(x.Billboards[i], y.Billboards[i]))
                    return false;

            for (int i = 0; i < x.Colliders.Count; i++)
            {
                if (x.Colliders[i].Size.GetHashCode() != y.Colliders[i].Size.GetHashCode())
                    return false;

                if (x.Colliders[i].Position.GetHashCode() != y.Colliders[i].Position.GetHashCode())
                    return false;
            }

            return true;
        }

        int getGameObjectId(GameObject game_object)
        {
            for (int i = 0; i < gameObjects.Count; i++)
                if (CompareGameObjects(game_object, gameObjects[i]))
                    return i;

            return -1;
        }

        int getBillboardId(Billboard billboard)
        {
            for (int i = 0; i < billboards.Count; i++)
                if (CompareBillboards(billboard, billboards[i]))
                    return i;
                    
            return -1;
        }

        int getTrackId(Track track)
        {
            for (int i = 0; i < tracks.Count; i++)
                if (tracks[i].GetHashCode() == track.GetHashCode())
                    return i;

            return -1;
        }

        #endregion

        #region Game entities serializers

        byte[] SerializeBillboard(Billboard billboard)
        {
            int[] meta = new int[1 + billboard.LODs.Count];
            meta[0] = billboard.LODs.Count;
            List<byte> all_data = new List<byte>();
            // Reserve space for meta.
            for (int i = 0; i < meta.Length * 4; i++)
                all_data.Add(0);

            for (int i = 0; i < billboard.LODs.Count; i++)
            {
                MemoryStream ms = new MemoryStream();
                billboard.LODs[0].Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var lod_data = ms.ToArray();

                meta[i + 1] = lod_data.Length;
                all_data.AddRange(lod_data);
            }

            var all_data_arr = all_data.ToArray();
            Buffer.BlockCopy(meta, 0, all_data_arr, 0, meta.Length * 4);
            return all_data_arr;
        }

        byte[] SerializeCollider(Collider collider)
        {
            float[] float_data = new float[] {
                (float)collider.Position.X,
                (float)collider.Position.Y,
                (float)collider.Position.Z,
                (float)collider.Size.X,
                (float)collider.Size.Y,
                (float)collider.Size.Z,
            };

            byte[] data = new byte[24];
            Buffer.BlockCopy(float_data, 0, data, 0, 24);
            return data;
        }

        byte[] SerializeGameObject(GameObject game_object)
        {
            var colliders_data_len = 4 + game_object.Colliders.Count * 24;
            var billboards_data_len = 4 + game_object.Billboards.Count * 20;
            byte[] data = new byte[colliders_data_len + billboards_data_len];

            // Collider data.
            Buffer.BlockCopy(new int[] { game_object.Colliders.Count }, 0, data, 0, 4);
            for (int i = 0; i < game_object.Colliders.Count; i++)
            {
                var collider_data = SerializeCollider(game_object.Colliders[i]);
                Buffer.BlockCopy(collider_data, 0, data, 4 + i * 24, 24);
            }
            // Billboard data.
            Buffer.BlockCopy(new int[] { game_object.Billboards.Count }, 0, data, colliders_data_len, 4);
            for (int i = 0; i < game_object.Billboards.Count; i++)
            {
                var billboard = game_object.Billboards[i];

                int id = getBillboardId(billboard);

                int write_offset = 4 + colliders_data_len + 20 * i;
                Buffer.BlockCopy(new int[] { id }, 0, data, write_offset, 4);
                float[] pos = new float[] {
                    (float)billboard.Position.X,
                    (float)billboard.Position.Y,
                    (float)billboard.Position.Z
                };
                Buffer.BlockCopy(pos, 0, data, write_offset + 4, 12);
                Buffer.BlockCopy(new float[] { (float)billboard.Width }, 0, data, write_offset + 16, 4);
            }

            return data;
        }

        byte[] SerializeTrack(Track track)
        {
            int heel_keypoints_data_len = 4 + 8 * track.Keypoints.Count;
            int curvatures_data_len = 4 + 12 * track.Curvatures.Count;
            int game_objects_data_len = 4 + 16 * track.GameObjects.Count;

            byte[] data = new byte[heel_keypoints_data_len + curvatures_data_len + game_objects_data_len];
            int curr_offset = 0;

            Buffer.BlockCopy(new int[] { track.Keypoints.Count }, 0, data, curr_offset, 4);
            curr_offset += 4;
            for (int i = 0; i < track.Keypoints.Count; i++)
            {
                var arr = new float[]
                {
                    (float)track.Keypoints[i].X,
                    (float)track.Keypoints[i].Y
                };
                Buffer.BlockCopy(arr, 0, data, curr_offset, 8);
                curr_offset += 8;
            }

            Buffer.BlockCopy(new int[] { track.Curvatures.Count }, 0, data, curr_offset, 4);
            curr_offset += 4;
            for (int i = 0; i < track.Curvatures.Count; i++)
            {
                var arr = new float[]
                {
                    (float)track.Curvatures[i].Start,
                    (float)track.Curvatures[i].Length,
                    (float)track.Curvatures[i].Value
                };
                Buffer.BlockCopy(arr, 0, data, curr_offset, 12);
                curr_offset += 12;
            }

            Buffer.BlockCopy(new int[] { track.GameObjects.Count }, 0, data, curr_offset, 4);
            curr_offset += 4;
            for (int i = 0; i < track.GameObjects.Count; i++)
            {
                int id = getGameObjectId(track.GameObjects[i]);

                Buffer.BlockCopy(new int[] { id }, 0, data, curr_offset, 4);
                curr_offset += 4;

                var pos = new float[]
                {
                    (float)track.GameObjects[i].Offset,
                    (float)track.GameObjects[i].RoadDistance
                };
                Buffer.BlockCopy(pos, 0, data, curr_offset, 8);
                curr_offset += 8;
            }

            return data;
        }

        // Files must be serialized after their parents.
        byte[] SerializeFile(IContent content)
        {
            int parent = -1;
            if (files.Contains(content.Parent))
                parent = files.IndexOf(content.Parent);

            byte file_type = 0;
            int entity_id = -1;
            if (content is FileManager.File file)
            {
                switch (file.Content)
                {
                    case Billboard billboard: 
                        file_type = 1;
                        entity_id = getBillboardId(billboard);;
                        break;
                    case GameObject game_object:
                        file_type = 2;
                        entity_id = getGameObjectId(game_object);
                        break;
                    case Track track: 
                        file_type = 3;
                        entity_id = getTrackId(track);
                        break;
                }
            }

            byte[] file_name = Encoding.ASCII.GetBytes(content.Name);

            byte[] data = new byte[13 + file_name.Length];

            // Parent.
            Buffer.BlockCopy(new int[] { parent }, 0, data, 0, 4);
            // File type.
            data[4] = file_type;
            // Entity ID.
            Buffer.BlockCopy(new int[] { entity_id }, 0, data, 5, 4);
            // File name length.
            Buffer.BlockCopy(new int[] { file_name.Length }, 0, data, 9, 4);
            // File name.
            Buffer.BlockCopy(file_name, 0, data, 13, file_name.Length);

            return data;
        }

        #endregion

        void FillEntityCollections(IContent hierarchy_root)
        {
            if(hierarchy_root is FileManager.File file)
            {
                switch(file.Content)
                {
                    case Billboard billboard: billboards.Add(billboard); break;
                    case GameObject game_object: gameObjects.Add(game_object); break;
                    case Track track: tracks.Add(track); break;
                }
            }
            else
            {
                foreach (var root in (hierarchy_root as FileManager.Folder).Contents)
                    FillEntityCollections(root);
            }    
        }

        public MemoryStream SerializeRmap(List<IContent> file_hierarchy)
        {
            billboards.Clear();
            gameObjects.Clear();
            tracks.Clear();

            foreach(var file in file_hierarchy)
                FillEntityCollections(file);

            var ms = new MemoryStream();

            byte endianness = (byte)(BitConverter.IsLittleEndian ? 0x00 : 0xFF);
            ms.Write(new byte[] { endianness }, 0, 1);

            byte[] billboard_count = new byte[4];
            Buffer.BlockCopy(new int[] { billboards.Count }, 0, billboard_count, 0, 4);
            ms.Write(billboard_count, 0, 4);

            foreach(var billboard in billboards)
            {
                var data = SerializeBillboard(billboard);
                ms.Write(data, 0, data.Length);
            }

            byte[] game_object_count = new byte[4];
            Buffer.BlockCopy(new int[] { gameObjects.Count }, 0, game_object_count, 0, 4);
            ms.Write(game_object_count, 0, 4);

            foreach (var game_object in gameObjects)
            {
                var data = SerializeGameObject(game_object);
                ms.Write(data, 0, data.Length);
            }

            byte[] track_count = new byte[4];
            Buffer.BlockCopy(new int[] { tracks.Count }, 0, track_count, 0, 4);
            ms.Write(track_count, 0, 4);

            foreach (var track in tracks)
            {
                var data = SerializeTrack(track);
                ms.Write(data, 0, data.Length);
            }

            return ms;
        }

        void FillFilesCollection(IContent hierarchy_root)
        {
            if (hierarchy_root is FileManager.Folder folder)
            {
                files.Add(hierarchy_root);
                foreach (var root in folder.Contents)
                    FillFilesCollection(root);
            }
            else
                files.Add(hierarchy_root);
        }

        public MemoryStream SerializeRproj(List<IContent> file_hierarchy)
        {
            files.Clear();

            foreach (var file in file_hierarchy)
                FillFilesCollection(file);

            MemoryStream ms = SerializeRmap(file_hierarchy);

            byte[] file_count = new byte[4];
            Buffer.BlockCopy(new int[] { files.Count }, 0, file_count, 0, 4);
            ms.Write(file_count, 0, 4);

            foreach (var file in files)
            {
                var file_data = SerializeFile(file);
                ms.Write(file_data, 0, file_data.Length);
            }

            return ms;
        }

        #region Read__ from memory stream

        int ReadInt(MemoryStream ms, bool is_little_endian)
        {
            byte[] output = new byte[4];
            ms.Read(output, 0, 4);
            if (BitConverter.IsLittleEndian ^ is_little_endian)
                Array.Reverse(output);
            return BitConverter.ToInt32(output, 0);
        }

        float ReadFloat(MemoryStream ms, bool is_little_endian)
        {
            byte[] output = new byte[4];
            ms.Read(output, 0, 4);
            if (BitConverter.IsLittleEndian ^ is_little_endian)
                Array.Reverse(output);
            return BitConverter.ToSingle(output, 0);
        }

        Vector3 ReadVector3(MemoryStream ms, bool is_little_endian)
        {
            float x = ReadFloat(ms, is_little_endian);
            float y = ReadFloat(ms, is_little_endian);
            float z = ReadFloat(ms, is_little_endian);

            return new Vector3(x, y, z);
        }

        #endregion

        public List<IContent> DeserializeRproj(MemoryStream ms)
        { 
            billboards.Clear();
            gameObjects.Clear();
            tracks.Clear();
            files.Clear();

            bool is_little_endian = (byte)ms.ReadByte() == 0x00;

            // Billboards.
            int num_billboards = ReadInt(ms, is_little_endian);
            for(int i = 0; i < num_billboards; i++)
            {
                var billboard = new Billboard();
                int num_lods = ReadInt(ms, is_little_endian);

                List<int> image_sizes = new List<int>(num_lods);
                for(int j = 0; j < num_lods; j++)
                    image_sizes.Add(ReadInt(ms, is_little_endian));

                for(int j = 0; j < num_lods; j++)
                {
                    var img_stream = new MemoryStream();
                    for (int k = 0; k < image_sizes[j]; k++)
                        img_stream.WriteByte((byte)ms.ReadByte());
                    var img = Image.FromStream(img_stream);
                    var lod = new LOD((Bitmap)img);
                    billboard.AddLOD(lod);
                }

                billboards.Add(billboard);
            }
            // GameObjects
            int num_go = ReadInt(ms, is_little_endian);
            for(int i = 0; i < num_go; i++)
            {
                var go = new GameObject();
                // Colliders.
                int num_colliders = ReadInt(ms, is_little_endian);
                for(int j = 0; j < num_colliders; j++)
                {
                    var pos = ReadVector3(ms, is_little_endian);
                    var size = ReadVector3(ms, is_little_endian);
                    var collider = new Collider(pos, size);
                    go.Colliders.Add(collider);
                }
                // Billboards.
                int num_bb = ReadInt(ms, is_little_endian);
                for(int j = 0; j < num_bb; j++)
                {
                    int bb_id = ReadInt(ms, is_little_endian);
                    var bb = new Billboard(billboards[bb_id]);
                    bb.Position = ReadVector3(ms, is_little_endian);
                    bb.Width = ReadFloat(ms, is_little_endian);
                    go.Billboards.Add(bb);
                }

                gameObjects.Add(go);
            }
            // Tracks.
            int num_tracks = ReadInt(ms, is_little_endian);
            for(int i = 0; i < num_tracks; i++)
            {
                var track = new Track();
                // Heel keypoints
                int num_keypoints = ReadInt(ms, is_little_endian);
                for (int j = 0; j < num_keypoints; j++)
                {
                    float z = ReadFloat(ms, is_little_endian);
                    float y = ReadFloat(ms, is_little_endian);
                    track.Keypoints.Add(new HeelKeypoint(z, y));
                }
                // Curvatures
                int num_curvatures = ReadInt(ms, is_little_endian);
                for (int j = 0; j < num_curvatures; j++)
                {
                    // X : start, y : length, z : value.
                    Vector3 curv_data = ReadVector3(ms, is_little_endian);
                    Curvature curvature = new Curvature(curv_data.X, curv_data.Y, curv_data.Z);
                    track.Curvatures.Add(curvature);
                }
                // GameObjects
                int num_gameobjects = ReadInt(ms, is_little_endian);
                for(int j = 0; j < num_gameobjects; j++)
                {
                    int go_id = ReadInt(ms, is_little_endian);
                    float pos_x = ReadFloat(ms, is_little_endian);
                    float pos_z = ReadFloat(ms, is_little_endian);
                    var go = new GameObject(gameObjects[go_id]);
                    go.Offset = pos_x;
                    go.RoadDistance = pos_z;
                    track.GameObjects.Add(go);
                }

                tracks.Add(track);
            }
            // Files.
            // As files are serialized after their parents there is always exist parent in the list of already deserialized files.
            int num_files = ReadInt(ms, is_little_endian);
            List<IContent> hierarchy = new List<IContent>();
            for (int i = 0; i < num_files; i++)
            {
                int parent = ReadInt(ms, is_little_endian);
                byte file_type = (byte)ms.ReadByte();
                int entity_id = ReadInt(ms, is_little_endian);
                int name_length = ReadInt(ms, is_little_endian);
                byte[] name_data = new byte[name_length];
                ms.Read(name_data, 0, name_length);
                string name = ASCIIEncoding.ASCII.GetString(name_data);

                IContent parent_content = null;
                if(parent != -1)
                    parent_content = files[parent];

                IContent current_content = null;
                switch(file_type)
                {
                    case 0:
                        current_content = new Folder(name, parent_content);
                        break;
                    case 1:
                        current_content = new FileManager.File(name, parent_content, billboards[entity_id]);
                        break;
                    case 2:
                        current_content = new FileManager.File(name, parent_content, gameObjects[entity_id]);
                        break;
                    case 3:
                        current_content = new FileManager.File(name, parent_content, tracks[entity_id]);
                        break;
                }

                if(parent == -1)
                    hierarchy.Add(current_content);
                else
                    (parent_content as Folder).AddContent(current_content);
            }
            
            return hierarchy;
        }
    }
}