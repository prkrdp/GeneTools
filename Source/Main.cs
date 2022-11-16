using HarmonyLib;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GeneTools
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main() => new Harmony("GeneTools").PatchAll();
    }
}
