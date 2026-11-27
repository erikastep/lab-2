using UnityEditor;

namespace Lab2.EditorTools
{
    public static class Lab2Export
    {
        [MenuItem("Lab2/Export Gabriele unitypackage")]
        public static void Export()
        {
            AssetDatabase.ExportPackage(new[] { "Assets/_Project/Police", "Assets/_Project/Shared" },
                "../Lab2_Gabriele.unitypackage", ExportPackageOptions.Recurse);
        }

        public static void ExportBatch() { Export(); EditorApplication.Exit(0); }
    }
}
