using Editor.FileManager;
using Editor.TabbedEditors;

namespace Editor
{
    public static class MainModel
    {
        static FileManagerModel fileManagerModel = new FileManagerModel();
        public static FileManagerModel FileManagerModel { get => fileManagerModel; }

        static TabbedEditorsModel tabbedEditorsModel = new TabbedEditorsModel();
        public static TabbedEditorsModel TabbedEditorsModel { get => tabbedEditorsModel; }

        //===================================================================================
        // .rmap file is a binary file that contains:
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
        //  + their positions relative to GameObject[int + float3]
        //
        // Track:
        //  Number of HeelKeypoints[int]
        //  List of HeelKeypoints[float2]
        //  Number of Curvatures[int]
        //  List of Curvatures(start[float], length[float], value[float])
        //  Number of GameObjects[int]
        //  List of GameObjects(each game object is represented by it's index in list)
        //  + their positions in the track coordinate system[int + float3]
        //===================================================================================
        // .rproj file is a binary file that contains:
        //  {Everything that .rmap file contains}
        //  Number of files[int]
        //  List of all files[File]
        //
        // File(folders are represented as files with FileType = 0 and no data):
        //  Header:
        //    Header length in bytes[int]
        //    Data length in bytes[int]
        //    Parent directory id in this list[int] (if file is in root then -1)
        //    File type[byte]  (*)
        //    File name[ASCII, to the end of header]
        //  Data[nothing/Billboard/GameObject/Track]
        //
        // (*)
        //   0 - Folder
        //   1 - Billboard
        //   2 - GameObject
        //   3 - Track
        //===================================================================================

        static byte[] SerializeBillboard()
        {
            return new byte[0];
        }

        static byte[] SerializeGameObject()
        {
            return new byte[0];
        }

        static byte[] SerializeTrack()
        {
            return new byte[0];
        }

        public static void SaveProjectToFile(string path)
        {
            
        }

        public static void LoadProjectFromFile(string path)
        {

        }
    }
}
