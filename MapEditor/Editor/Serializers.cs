﻿using Editor.FileManager;
using Editor.GameEntities;
using System;
using System.Collections.Generic;
using System.IO;
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
                var id = billboards.IndexOf(billboard);
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
                    (float)track.Keypoints[i].X / 10.0f,
                    (float)track.Keypoints[i].Y / 10.0f
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
                Buffer.BlockCopy(new int[] { gameObjects.IndexOf(track.GameObjects[i]) }, 0, data, curr_offset, 4);
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
                        entity_id = billboards.IndexOf(billboard);
                        break;
                    case GameObject game_object:
                        file_type = 2;
                        entity_id = gameObjects.IndexOf(game_object);
                        break;
                    case Track track: 
                        file_type = 3;
                        entity_id = tracks.IndexOf(track);
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
                foreach (var root in folder.Contents)
                    FillFilesCollection(root);
            }
            else
                files.Add(hierarchy_root as FileManager.File);
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

            foreach(var file in files)
            {
                var file_data = SerializeFile(file);
                ms.Write(file_data, 0, file_data.Length);
            }

            return ms;
        }
    }
}