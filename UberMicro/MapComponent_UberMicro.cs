using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace UberMicro
{
    class MapComponent_UberMicro : MapComponent
    {
        public bool enabled;
        private Dictionary<Pawn, string> prioritizedJobs;

        public MapComponent_UberMicro(Map map) : base(map)
        {
            this.enabled = true;
            this.prioritizedJobs = new Dictionary<Pawn, string>();
        }

        public override void MapComponentTick()
        {
            if (enabled)
            {
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {
                    if (p.CurJob.playerForced)
                    {
                        prioritizedJobs[p] = p.CurJob.ToString();
                    }
                    else
                    {
                        if (prioritizedJobs[p] != null)
                        {
                            Find.LetterStack.ReceiveLetter(
                                    "Job Done",
                                    p.NameStringShort + " has completed their prioritized job to: " + prioritizedJobs[p],
                                    LetterType.BadUrgent,
                                    new GlobalTargetInfo(p)
                                );
                            prioritizedJobs[p] = null;
                        }
                    }
                }
            }
        }
    }
}
