using UnityEngine;

using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NEP.DOOMLAB.WAD
{
    public class WADManager
    {
        public WADManager()
        {
            Instance = this;
        }

        public static WADManager Instance { get; private set; }

        public Dictionary<string, WADFile> LoadedWADs { get; private set; } = new Dictionary<string, WADFile>();

        public WADFile LoadedWAD { get; private set; }

        public string[] IWADS => GetWADsInFolder(WADFile.WADType.IWAD, true);
        public string[] PWADS => GetWADsInFolder(WADFile.WADType.PWAD, true);

        private readonly string[] iwads = new string[]
        {
            "DOOM",
            "DOOM2",
            "TNT",
            "PLUTONIA",
            "FREEDOOM1",
            "FREEDOOM2",
            "CHEX",
            "HERETIC",
            "HEXEN"
        };

        public void LoadWAD(string file)
        {
            string fileName = GetWADFileName(file);

            if(LoadedWADs.ContainsKey(fileName))
            {
                LoadedWAD = LoadedWADs[fileName];
                return;
            }

            var wadFile = new WADFile(file);

            wadFile.ReadHeader();
            wadFile.ReadIndexEntries();
            wadFile.ReadPalette();
            wadFile.ReadAllSounds();
            wadFile.ReadAllSprites();

            LoadedWAD = wadFile;
            LoadedWADs.Add(fileName, wadFile);
        }

        public string GetIWAD()
        {
            string[] files = Directory.GetFiles(Main.IWADDirectory);
            string result = "";

            for(int i = 0; i < iwads.Length; i++)
            {
                for(int j = 0; j < files.Length; j++)
                {
                    string iwadName = iwads[i] + ".WAD";
                    string file = files[j];
                    string fileName = Path.GetFileName(file);

                    if (fileName.ToLower() == iwadName.ToLower())
                    {
                        result = file;
                        break;
                    }
                }
            }

            return result;
        }
    
        public string[] GetWADsInFolder(WADFile.WADType wadType, bool fullPath = false)
        {
            string path = wadType == WADFile.WADType.IWAD ? Main.IWADDirectory : Main.PWADDirectory;
            string[] files = Directory.GetFiles(path);

            if(fullPath)
            {
                return files;
            }

            string[] wadFileNames = new string[files.Length];

            for(int i = 0; i < wadFileNames.Length; i++)
            {
                wadFileNames[i] = GetWADFileName(files[i]);
            }

            return wadFileNames;
        }

        public string GetWADFileName(string pathToWad)
        {
            string[] splitPath = pathToWad.Split((char)92);
            return splitPath[splitPath.Length - 1];
        }
    }
}
