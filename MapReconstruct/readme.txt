WDT updating:
	Command line: MapReconstruct.exe updateWDT <input WDT location> <mapID> <listfile with ADTs> (true, optional to generate fake minimaps)
	Sample usage: MapReconstruct.exe updateWDT 2454.wdt 2454 community-listfile.csv
	This will output a 2454_new.wdt in the same folder as the original WDT for map 2454 with any ADTs for that map that are in the community-listfile.csv.
	Minimaps are random Azeroth BLPs, so they will be wrong.

ADT naming:
	Command line: nameADT inputDirectory
	Sample usage: nameADT "D:\Downloads\unknown" 2454
	Input directory with 5 unknown ADT files (preferably just named <FileDataID>.adt) will generate output like this:
		4818260;world/maps/2454/2454_37_24.adt
		4818261;world/maps/2454/2454_37_24_obj0.adt
		4818262;world/maps/2454/2454_37_24_obj1.adt
		4818263;world/maps/2454/2454_37_24_tex0.adt
		4818264;world/maps/2454/2454_37_24_lod.adt
	Where first part is the original filename without extension and the latter part is the reconstructed filename.

Reference dumping:
	Command line: dumpRefs inputDirectory
	Sample usage: dumpRefs "D:\Downloads\unknown"
	Input directory with ADTs (can be unnamed) will generate a list of used WMOs/M2s/BLPs from the MDDF, MODF, MHID and MDID chunks. If listfile.csv is present, it will also output filenames:
		108785;World/WMO/Dungeon/BoreanMagnataurMicro/BoreanMagnataurMicro.wmo
		189528;World/Azeroth/Duskwood/PassiveDoodads/tombs/DirtMound01.M2
		5214448;tileset/expansion10/11nr_road01_512_s.blp