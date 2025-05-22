using System.Text;

namespace MapReconstruct
{
    internal class Program
    {
        public class MapFileDataIDs
        {
            public uint rootADT { get; set; }
            public uint obj0ADT { get; set; }
            public uint obj1ADT { get; set; }
            public uint tex0ADT { get; set; }
            public uint lodADT { get; set; }
            public uint mapTexture { get; set; }
            public uint mapTextureN { get; set; }
            public uint minimapTexture { get; set; }
        }

        public struct MPHD
        {
            public uint flags { get; set; }
            public uint lgtFileDataID { get; set; }
            public uint occFileDataID { get; set; }
            public uint fogsFileDataID { get; set; }
            public uint mpvFileDataID { get; set; }
            public uint texFileDataID { get; set; }
            public uint wdlFileDataID { get; set; }
            public uint pd4FileDataID { get; set; }
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguments. For mode updateWDT use: MapReconstruct.exe updateWDT <input WDT location> <mapID> <listfile with ADTs> <(true for minimaps/maptextures)>. For mode updateADT use: MapReconstruct.exe nameADT <directory with ADTs> <target mapID>. For mode dumpRefs <directory with ADTs>.");
                return;
            }

            var mode = args[0];
            if (mode != "updateWDT" && mode != "nameADT" && mode != "dumpRefs")
            {
                Console.WriteLine("Invalid mode, use updateWDT, nameADT or dumpRefs as first argument");
                return;
            }

            if (mode == "updateWDT")
            {
                if (args.Length < 4)
                {
                    Console.WriteLine("Invalid arguments. Use MapReconstruct.exe updateWDT <input WDT location> <mapID> <listfile with ADTs> (true to generate fake minimaps)");
                    return;
                }

                var inputWDT = args[1];
                if (!File.Exists(inputWDT))
                    throw new FileNotFoundException("Input WDT file not found", inputWDT);

                var inMap = args[2];

                var listfile = args[3];
                if (!File.Exists(listfile))
                    throw new FileNotFoundException("Input listfile not found", listfile);

                var generateMinimaps = false;
                if (args.Length > 4)
                {
                    if (!bool.TryParse(args[4], out generateMinimaps))
                    {
                        throw new Exception("Invalid boolean value as argument 5, need true or false");
                    }
                }

                var tiles = new List<(byte, byte)>();
                var newTiles = new List<(byte, byte)>();

                var tileFiles = new Dictionary<(byte, byte), MapFileDataIDs>();
                var newTileFiles = new Dictionary<(byte, byte), MapFileDataIDs>();

                for (byte x = 0; x < 64; x++)
                {
                    for (byte y = 0; y < 64; y++)
                    {
                        if (!newTileFiles.ContainsKey((x, y)))
                            newTileFiles.Add((x, y), new MapFileDataIDs());
                    }
                }

                var newMPHD = new MPHD();

                foreach (var line in File.ReadAllLines(listfile))
                {
                    var listfileEntry = line.Split(';');
                    if (listfileEntry.Length != 2)
                        continue;

                    if (
                        !listfileEntry[1].StartsWith("world/maps/" + inMap + "/") &&
                        !listfileEntry[1].StartsWith("world/minimaps/" + inMap + "/") &&
                        !listfileEntry[1].StartsWith("world/maptextures/" + inMap + "/"))
                        continue;

                    var fdid = uint.Parse(listfileEntry[0]);
                    var filename = listfileEntry[1];

                    var basename = Path.GetFileNameWithoutExtension(filename);
                    var splitname = basename.Split('_');

                    if (filename.EndsWith(".adt"))
                    {
                        Console.WriteLine("Adding ADT " + filename);
                        var x = byte.Parse(splitname[1]);
                        var y = byte.Parse(splitname[2]);

                        if (splitname.Length == 3)
                        {
                            newTiles.Add((x, y));
                            newTileFiles[(x, y)].rootADT = fdid;

                        }
                        else if (splitname.Length == 4)
                        {
                            switch (splitname[3])
                            {
                                case "obj0":
                                    newTileFiles[(x, y)].obj0ADT = fdid;
                                    break;
                                case "obj1":
                                    newTileFiles[(x, y)].obj1ADT = fdid;
                                    break;
                                case "tex0":
                                    newTileFiles[(x, y)].tex0ADT = fdid;
                                    break;
                                case "lod":
                                    newTileFiles[(x, y)].lodADT = fdid;
                                    break;
                            }
                        }
                    }
                    else if (generateMinimaps && filename.StartsWith("world/m") && filename.EndsWith(".blp"))
                    {
                        if (filename.StartsWith("world/minimaps"))
                        {
                            Console.WriteLine("Adding minimap " + filename);

                            var x = byte.Parse(splitname[0].Split("map")[1]);
                            var y = byte.Parse(splitname[1]);

                            newTileFiles[(x, y)].minimapTexture = fdid;
                        }
                        else if (filename.StartsWith("world/maptextures"))
                        {
                            Console.WriteLine("Adding maptexture " + filename);

                            var x = byte.Parse(splitname[1]);
                            var y = byte.Parse(splitname[2]);

                            if (filename.EndsWith("_n.blp"))
                            {
                                newTileFiles[(x, y)].mapTextureN = fdid;
                            }
                            else
                            {
                                newTileFiles[(x, y)].mapTexture = fdid;
                            }
                        }
                    }
                    else if (filename.EndsWith(".wdt"))
                    {
                        if (filename.EndsWith("_lgt.wdt"))
                        {
                            newMPHD.lgtFileDataID = fdid;
                        }
                        else if (filename.EndsWith("_occ.wdt"))
                        {
                            newMPHD.occFileDataID = fdid;
                        }
                        else if (filename.EndsWith("_fogs.wdt"))
                        {
                            newMPHD.fogsFileDataID = fdid;
                        }
                        else if (filename.EndsWith("_mpv.wdt"))
                        {
                            newMPHD.mpvFileDataID = fdid;
                        }
                    }
                    else if (filename.EndsWith(".tex"))
                    {
                        newMPHD.texFileDataID = fdid;
                    }
                    else if (filename.EndsWith(".wdl"))
                    {
                        newMPHD.wdlFileDataID = fdid;
                    }
                    else if (filename.EndsWith(".pd4"))
                    {
                        newMPHD.pd4FileDataID = fdid;
                    }
                }

                var outWDT = Path.Combine(Path.GetDirectoryName(inputWDT), Path.GetFileNameWithoutExtension(inputWDT) + "_new" + Path.GetExtension(inputWDT));

                using (var wdtInStream = File.OpenRead(inputWDT))
                using (var wdtOutStream = File.OpenWrite(outWDT))
                using (var bin = new BinaryReader(wdtInStream))
                using (var writer = new BinaryWriter(wdtOutStream))
                {
                    wdtInStream.CopyTo(wdtOutStream);
                    wdtInStream.Position = 0;

                    long position = 0;

                    var chunks = new List<string>();

                    var maidFound = false;
                    long flagOffset = 0;
                    int newFlags = 0;

                    while (position < wdtInStream.Length)
                    {
                        wdtInStream.Position = position;
                        var chunkNameBytes = bin.ReadBytes(4);
                        var chunkName = Encoding.ASCII.GetString(chunkNameBytes);
                        var chunkSize = bin.ReadUInt32();

                        position = wdtInStream.Position + chunkSize;

                        switch (chunkName)
                        {
                            case "REVM":
                                break;
                            case "DHPM":
                                var currentMPHD = bin.Read<MPHD>();
                                writer.BaseStream.Position = bin.BaseStream.Position - 32;

                                flagOffset = writer.BaseStream.Position;

                                newFlags = (int)currentMPHD.flags;

                                // Turn on adt_has_height_texturing flag
                                if ((newFlags & 0x80) == 0)
                                {
                                    newFlags |= 0x80;
                                }

                                // If 0x100 (lod) is set, turn it off
                                if ((newFlags & 0x100) != 0)
                                {
                                    newFlags &= ~0x100;
                                }

                                // If 0x8000 (also lod) is set, turn it off
                                if ((newFlags & 0x8000) != 0)
                                {
                                    newFlags &= ~0x8000;
                                }

                                writer.Write(newFlags);

                                if (newMPHD.lgtFileDataID != 0)
                                    writer.Write(newMPHD.lgtFileDataID);
                                else
                                    writer.Write((uint)1249658); // use empty _lgt file

                                if (newMPHD.occFileDataID != 0)
                                    writer.Write(newMPHD.occFileDataID);
                                else
                                    writer.Write((uint)1100613); // use empty _occ file

                                if (newMPHD.fogsFileDataID != 0)
                                    writer.Write(newMPHD.fogsFileDataID);
                                else
                                    writer.Write((uint)1668535); // use empty _fogs file

                                if (newMPHD.mpvFileDataID != 0)
                                    writer.Write(newMPHD.mpvFileDataID);
                                else
                                    writer.Write((uint)2495665); // use empty _mpv file

                                if (newMPHD.texFileDataID != 0)
                                    writer.Write(newMPHD.texFileDataID);
                                else
                                    writer.Write((uint)1249780); // use empty tex file

                                writer.Write(newMPHD.wdlFileDataID);

                                writer.Write(0); // PD4, just set as 0
                                break;
                            case "NIAM":
                                for (byte x = 0; x < 64; x++)
                                {
                                    for (byte y = 0; y < 64; y++)
                                    {
                                        var flags = bin.ReadUInt32();
                                        bin.ReadUInt32();
                                        if (flags == 1)
                                        {
                                            tiles.Add((y, x));
                                        }

                                        if (newTiles.Contains((y, x)))
                                        {
                                            writer.BaseStream.Position = bin.BaseStream.Position - 8;
                                            writer.Write((uint)1);
                                            writer.Write((uint)0);
                                        }
                                    }
                                }
                                break;
                            case "DIAM":
                                maidFound = true;
                                for (byte x = 0; x < 64; x++)
                                {
                                    for (byte y = 0; y < 64; y++)
                                    {
                                        var mfdids = new MapFileDataIDs();
                                        mfdids.rootADT = bin.ReadUInt32();
                                        mfdids.obj0ADT = bin.ReadUInt32();
                                        mfdids.obj1ADT = bin.ReadUInt32();
                                        mfdids.tex0ADT = bin.ReadUInt32();
                                        mfdids.lodADT = bin.ReadUInt32();
                                        mfdids.mapTexture = bin.ReadUInt32();
                                        mfdids.mapTextureN = bin.ReadUInt32();
                                        mfdids.minimapTexture = bin.ReadUInt32();
                                        tileFiles.Add((y, x), mfdids);

                                        if (newTileFiles.ContainsKey((y, x)) && tileFiles[(y, x)].rootADT == 0 && newTileFiles[(y, x)].rootADT != 0)
                                        {
                                            Console.WriteLine("Writing new tile " + y + "_" + x);
                                            writer.BaseStream.Position = bin.BaseStream.Position - 32;
                                            writer.Write(newTileFiles[(y, x)].rootADT);
                                            writer.Write(newTileFiles[(y, x)].obj0ADT);
                                            writer.Write(newTileFiles[(y, x)].obj1ADT);
                                            writer.Write(newTileFiles[(y, x)].tex0ADT);
                                            writer.Write(newTileFiles[(y, x)].lodADT);
                                            writer.Write(newTileFiles[(y, x)].mapTexture);
                                            writer.Write(newTileFiles[(y, x)].mapTextureN);
                                            writer.Write(newTileFiles[(y, x)].minimapTexture);
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    if (!maidFound)
                    {
                        // Set wdt_has_maid flag
                        if ((newFlags & 0x200) == 0)
                        {
                            writer.BaseStream.Position = flagOffset;
                            writer.Write(newFlags | 0x200);
                        }

                        // Move to end
                        writer.BaseStream.Position = wdtInStream.Length;

                        writer.Write("DIAM".ToCharArray());
                        writer.Write((uint)131072);
                        for (byte x = 0; x < 64; x++)
                        {
                            for (byte y = 0; y < 64; y++)
                            {
                                if (newTileFiles.ContainsKey((y, x)) && newTileFiles[(y, x)].rootADT != 0)
                                {
                                    Console.WriteLine("Writing new tile " + y + "_" + x + " to new MAID");
                                    writer.Write(newTileFiles[(y, x)].rootADT);
                                    writer.Write(newTileFiles[(y, x)].obj0ADT);
                                    writer.Write(newTileFiles[(y, x)].obj1ADT);
                                    writer.Write(newTileFiles[(y, x)].tex0ADT);
                                    writer.Write(newTileFiles[(y, x)].lodADT);
                                    writer.Write(newTileFiles[(y, x)].mapTexture);
                                    writer.Write(newTileFiles[(y, x)].mapTextureN);
                                    writer.Write(newTileFiles[(y, x)].minimapTexture);
                                }
                                else
                                {
                                    // Write empty tile
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                    writer.Write((uint)0);
                                }
                            }
                        }
                    }
                }
            }

            if (mode == "nameADT")
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Invalid arguments. Use MapReconstruct.exe nameADT <directory with ADTs> <target mapID>");
                    return;
                }

                var inDir = args[1];
                if (!Directory.Exists(inDir))
                {
                    Console.WriteLine("Input ADT directory does not exist");
                    return;
                }

                var targetMapID = args[2];
                var currentCoord = "";

                foreach (var file in Directory.GetFiles(inDir, "*.adt", SearchOption.AllDirectories))
                {
                    var basename = Path.GetFileNameWithoutExtension(file);
                    using (var stream = File.OpenRead(file))
                    {
                        var coord = GetCoordFromFile(stream);
                        stream.Position = 0;
                        var type = GetTypeFromFile(stream);
                        if (coord != "")
                        {
                            currentCoord = coord;
                            Console.WriteLine(Path.GetFileNameWithoutExtension(file) + ";" + "world/maps/" + targetMapID + "/" + targetMapID + "_" + currentCoord + ".adt");
                        }
                        else
                        {
                            Console.WriteLine(Path.GetFileNameWithoutExtension(file) + ";" + "world/maps/" + targetMapID + "/" + targetMapID + "_" + currentCoord + "_" + type + ".adt");
                        }

                        if (!basename.All(char.IsDigit))
                        {
                            if (type == "root")
                            {
                                if (basename != targetMapID + "_" + currentCoord)
                                    throw new Exception();
                            }
                            else
                            {
                                if (basename != targetMapID + "_" + currentCoord + "_" + type)
                                    throw new Exception();
                            }

                        }
                    }
                }
            }

            if (mode == "dumpRefs")
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Invalid arguments. Use MapReconstruct.exe dumpRefs <directory with ADTs>");
                    return;
                }

                var inDir = args[1];
                if (!Directory.Exists(inDir))
                {
                    Console.WriteLine("Input ADT directory does not exist");
                    return;
                }

                var listfile = new Dictionary<uint, string>();

                if (!File.Exists("listfile.csv"))
                {
                    Console.WriteLine("No listfile.csv found in application dir, will just dump FDIDs.");
                }
                else
                {
                    foreach (var line in File.ReadAllLines("listfile.csv"))
                    {
                        var listfileEntry = line.Split(';');
                        if (listfileEntry.Length != 2)
                            continue;

                        var fdid = uint.Parse(listfileEntry[0]);
                        var filename = listfileEntry[1];

                        if (filename.ToLowerInvariant().EndsWith(".m2") || filename.ToLowerInvariant().EndsWith(".wmo") || filename.ToLowerInvariant().EndsWith(".blp"))
                            listfile.Add(fdid, filename);
                    }
                }

                var allFiles = new List<uint>();

                foreach (var file in Directory.GetFiles(inDir, "*.adt", SearchOption.AllDirectories))
                {
                    var basename = Path.GetFileNameWithoutExtension(file);
                    using (var stream = File.OpenRead(file))
                    {
                        var type = GetTypeFromFile(stream);
                        if (type == "obj0")
                        {
                            var wmos = GetFilesFromADT(stream, "wmo");
                            //foreach (var wmo in wmos)
                            //{
                            //    if (listfile.TryGetValue(wmo, out string? filename))
                            //        Console.WriteLine(wmo + ";" + filename);
                            //    else
                            //        Console.WriteLine(wmo + ";");
                            //}

                            allFiles.AddRange(wmos);

                            var m2s = GetFilesFromADT(stream, "m2");
                            //foreach (var m2 in m2s)
                            //{
                            //    if (listfile.TryGetValue(m2, out string? filename))
                            //        Console.WriteLine(m2 + ";" + filename);
                            //    else
                            //        Console.WriteLine(m2 + ";");
                            //}

                            allFiles.AddRange(m2s);
                        }
                        else if (type == "tex0")
                        {
                            var blps = GetFilesFromADT(stream, "blp");
                            //foreach (var blp in blps)
                            //{
                            //    if (listfile.TryGetValue(blp, out string? filename))
                            //        Console.WriteLine(blp + ";" + filename);
                            //    else
                            //        Console.WriteLine(blp + ";");
                            //}

                            allFiles.AddRange(blps);
                        }
                    }
                }

                allFiles = allFiles.Distinct().ToList();
                allFiles.Sort();
                foreach (var file in allFiles)
                {
                    if (listfile.TryGetValue(file, out string? filename))
                        Console.WriteLine(file + ";" + filename);
                    else
                        Console.WriteLine(file + ";");
                }
            }
        }

        private static List<uint> GetFilesFromADT(Stream adtStream, string type)
        {
            var bin = new BinaryReader(adtStream);
            long position = 0;

            var files = new List<uint>();

            while (position < adtStream.Length)
            {
                adtStream.Position = position;
                var chunkNameBytes = bin.ReadBytes(4);
                var chunkName = Encoding.ASCII.GetString(chunkNameBytes);
                var chunkSize = bin.ReadUInt32();

                if (type == "m2" && chunkName == "FDDM")
                {
                    for (int i = 0; i < chunkSize / 36; i++)
                    {
                        var fdid = bin.ReadUInt32();
                        if (!files.Contains(fdid))
                            files.Add(fdid);

                        bin.BaseStream.Position += 32;
                    }
                }
                else if (type == "wmo" && chunkName == "FDOM")
                {
                    for (int i = 0; i < chunkSize / 64; i++)
                    {
                        var fdid = bin.ReadUInt32();
                        if (!files.Contains(fdid))
                            files.Add(fdid);

                        bin.BaseStream.Position += 60;
                    }
                }
                else if (type == "blp" && (chunkName == "DIDM" || chunkName == "DIHM"))
                {
                    for (int i = 0; i < chunkSize / 4; i++)
                    {
                        var fdid = bin.ReadUInt32();
                        if (!files.Contains(fdid))
                            files.Add(fdid);
                    }
                }
                
                position = adtStream.Position + chunkSize;
            }

            return files;
        }

        private static string GetTypeFromFile(Stream adtStream)
        {
            var bin = new BinaryReader(adtStream);

            long position = 0;

            var chunks = new List<string>();

            while (position < adtStream.Length)
            {
                adtStream.Position = position;
                var chunkNameBytes = bin.ReadBytes(4);
                var chunkName = Encoding.ASCII.GetString(chunkNameBytes);
                var chunkSize = bin.ReadUInt32();

                position = adtStream.Position + chunkSize;
                chunks.Add(chunkName);
            }

            if (chunks.Contains("DMLM") || chunks.Contains("DDLM"))
                return "obj1";

            if (chunks.Contains("FDDM") || chunks.Contains("FDOM"))
                return "obj0";

            if (chunks.Contains("PMAM") || chunks.Contains("XETM") || chunks.Contains("DIDM") || chunks.Contains("DIHM"))
                return "tex0";

            if (chunks.Contains("VLLM") || chunks.Contains("ILLM") || chunks.Contains("DNLM"))
                return "lod";

            return "root";
        }

        private static string GetCoordFromFile(Stream adtStream)
        {
            var bin = new BinaryReader(adtStream);
            long position = 0;
            var chunks = new List<string>();
            while (position < adtStream.Length)
            {
                adtStream.Position = position;
                var chunkNameBytes = bin.ReadBytes(4);
                var chunkName = Encoding.ASCII.GetString(chunkNameBytes);
                var chunkSize = bin.ReadUInt32();

                position = adtStream.Position + chunkSize;
                chunks.Add(chunkName);
            }

            if (chunks.Contains("DMLM") || chunks.Contains("DDLM"))
                return "";

            if (chunks.Contains("FDDM") || chunks.Contains("FDOM"))
                return "";

            if (chunks.Contains("PMAM") || chunks.Contains("XETM") || chunks.Contains("DIDM") || chunks.Contains("DIHM"))
                return "";

            if (chunks.Contains("VLLM") || chunks.Contains("ILLM") || chunks.Contains("DNLM"))
                return "";

            adtStream.Position = 0;
            position = 0;
            while (position < adtStream.Length)
            {
                adtStream.Position = position;
                var chunkNameBytes = bin.ReadBytes(4);
                var chunkName = Encoding.ASCII.GetString(chunkNameBytes);
                var chunkSize = bin.ReadUInt32();

                position = adtStream.Position + chunkSize;

                switch (chunkName)
                {
                    case "KNCM":
                        if (chunkSize > 0)
                        {
                            bin.ReadBytes(104);
                            var x = bin.ReadSingle();
                            var y = bin.ReadSingle();
                            var z = bin.ReadSingle();

                            var firstCoord = 32 - Math.Floor((y - 533 / 16) / 533.33333) - 1;
                            var secondCoord = 32 - Math.Floor((x - 533 / 16) / 533.33333) - 1;

                            return firstCoord + "_" + secondCoord;
                        }
                        break;
                    default:
                        break;
                }
            }
            return "";
        }
    }
}