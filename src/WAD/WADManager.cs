using System.IO;
using System.Collections.Generic;

namespace NEP.DOOMBBQ.WAD
{
    public class WADManager
    {
        public WADManager()
        {
            Instance = this;
        }

        public static WADManager Instance { get; private set; }

        public Dictionary<string, WADFile> LoadedWADs { get; private set; } = new Dictionary<string, WADFile>();

        public Dictionary<string, WADIndexEntry> LumpMap { get; private set; } = new Dictionary<string, WADIndexEntry>();

        public WADFile IWAD;
        public WADFile LoadedWAD { get; private set; }

        public string[] IWADS => GetWADsInFolder(WADFile.WADType.IWAD, true);
        public string[] PWADS => GetWADsInFolder(WADFile.WADType.PWAD, true);

        private readonly string[] iwads = new string[]
        {
            "D", // for some reason, this is the actual wad file name that's bundled with an official release of doom.
            "DOOM",
            "DOOMU",
            "BFGDOOM",
            "DOOMBFG",
            "DOOM2",
            "BFGDOOM2",
            "DOOM2BFG",
            "TNT",
            "PLUTONIA",
            "FREEDOOM1",
            "FREEDOOM2",
            "CHEX",
            "HERETIC",
            "HEXEN"
        };

        public void LoadIWAD(string file)
        {
            string fileName = GetWADFileName(file);

            var wadFile = new WADFile(file);

            wadFile.Preload();
            wadFile.ReadHeader();
            wadFile.ReadIndexEntries();
            wadFile.ReadPalette();
            wadFile.ReadAllSounds();
            wadFile.ReadAllSprites();

            IWAD = wadFile;
        }

        public void LoadPWAD(string file)
        {
            string fileName = GetWADFileName(file);

            var wadFile = new WADFile(file);

            wadFile.Preload();

            wadFile.ReadHeader();
            wadFile.ReadIndexEntries();

            LumpMap = IWAD.entryTable;

            foreach(var entry in wadFile.entries)
            {
                if(LumpMap.ContainsKey(entry.name))
                {
                    LumpMap[entry.name] = entry;
                }
            }

            wadFile.colorPal = IWAD.colorPal;

            wadFile.ReadIndexEntries();
            wadFile.ReadAllSounds();
            wadFile.ReadAllSprites();
        }

        public void LoadWAD(string file)
        {
            string fileName = GetWADFileName(file);

            if(LoadedWADs.ContainsKey(fileName))
            {
                LoadedWAD = LoadedWADs[fileName];
                return;
            }

            var wadFile = new WADFile(file);

            wadFile.Preload();

            wadFile.ReadHeader();
            wadFile.ReadIndexEntries();
            wadFile.ReadPalette();
            wadFile.ReadAllSounds();
            wadFile.ReadAllSprites();

            LoadedWAD = wadFile;
            LoadedWADs.Add(fileName, wadFile);

            if(file == GetIWAD())
            {
                IWAD = wadFile;
            }
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

        public string GetWADFileName(string pathToWad, bool quest = false)
        {
            string[] splitPath = !quest ? pathToWad.Split((char)92) : pathToWad.Split('/');
            return splitPath[splitPath.Length - 1];
        }
    }
}
