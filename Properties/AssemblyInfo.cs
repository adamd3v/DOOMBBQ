using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(NEP.DOOMBBQ.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(NEP.DOOMBBQ.BuildInfo.Company)]
[assembly: AssemblyProduct(NEP.DOOMBBQ.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + NEP.DOOMBBQ.BuildInfo.Author)]
[assembly: AssemblyTrademark(NEP.DOOMBBQ.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(NEP.DOOMBBQ.BuildInfo.Version)]
[assembly: AssemblyFileVersion(NEP.DOOMBBQ.BuildInfo.Version)]
[assembly: MelonInfo(typeof(NEP.DOOMBBQ.Main), NEP.DOOMBBQ.BuildInfo.Name, NEP.DOOMBBQ.BuildInfo.Version, NEP.DOOMBBQ.BuildInfo.Author, NEP.DOOMBBQ.BuildInfo.DownloadLink)]

// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]