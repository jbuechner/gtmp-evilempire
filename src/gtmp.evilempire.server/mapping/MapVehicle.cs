using gtmp.evilempire.entities;
using System.Collections.Generic;

namespace gtmp.evilempire.server.mapping
{
    public class MapVehicle
    {
        public class Neon
        {
            public int Index { get; set; }
            public bool IsTurnedOn { get; set; }
        }

        public string TemplateName { get; set; }
        public int Hash { get; }
        public Vector3f Position { get; }
        public Vector3f Rotation { get; }

        public int Color1 { get; }
        public int Color2 { get; }

        public bool IsInvincible { get; set; }
        public bool IsLocked { get; set; }
        public bool IsCollisionless { get; set; }
        public bool IsEngineRunning { get; set; }
        public bool HasBulletproofTyres { get; set; }
        public bool IsPositionFrozen { get; set; }
        public string NumberPlate { get; set; }
        public int? NumberPlateStyle { get; set; }
        public bool IsSpecialLightEnabled { get; set; }
        public int? TrimColor { get; set; }

        public IEnumerable<int> BrokenWindows { get; set; }
        public IEnumerable<int> OpenedDoors { get; set; }
        public IEnumerable<int> BrokenDoors { get; set; }
        public IEnumerable<int> PoppedTyres { get; set; }
        public IEnumerable<Neon> Neons { get; set; }

        public float? EnginePowerMultiplier { get; set; }
        public float? EngineTorqueMultiplier { get; set; }

        public Color? CustomPrimaryColor { get; set; }
        public Color? CustomSecondaryColor { get; set; }

        public Color? ModColor1 { get; set; }
        public Color? ModColor2 { get; set; }

        public Color? NeonColor { get; set; }
        public Color? TyreSmokeColor { get; set; }

        public int? WheelColor { get; set; }
        public int? WheelType { get; set; }
        public int? WindowTint { get; set; }
        public int? DashboardColor { get; set; }
        public float? Health { get; set; }
        public int? Livery { get; set; }
        public int? PearlescentColor { get; set; }

        public bool IsTemplate
        {
            get
            {
                return !string.IsNullOrEmpty(TemplateName);
            }
        }

        public MapVehicle(string templateName, int hash, Vector3f position, Vector3f rotation, int color1, int color2)
        {
            TemplateName = templateName;
            Hash = hash;
            Position = position;
            Rotation = rotation;
            Color1 = color1;
            Color2 = color2;
        }
    }
}
