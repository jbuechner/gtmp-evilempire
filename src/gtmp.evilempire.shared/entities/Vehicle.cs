using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class Vehicle
    {
        public static readonly long ZeroId = long.MinValue;

        public struct Neon
        {
            public int Index { get; set; }
            public bool IsTurnedOn { get; set; }
        }

        public long Id { get; set; } = ZeroId;

        public string TemplateName { get; set; }

        public int Hash { get; set; }
        public Vector3f Position { get; set; }
        public Vector3f Rotation { get; set; }

        public int Color1 { get; set; }
        public int Color2 { get; set; }

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

        public Vehicle()
        {
        }

        public Vehicle(Vehicle other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            TemplateName = other.TemplateName;
            Hash = other.Hash;
            Position = other.Position;
            Rotation = other.Rotation;
            Color1 = other.Color1;
            Color2 = other.Color2;
            IsInvincible = other.IsInvincible;
            IsLocked = other.IsLocked;
            IsCollisionless = other.IsCollisionless;
            IsEngineRunning = other.IsEngineRunning;
            HasBulletproofTyres = other.HasBulletproofTyres;
            IsPositionFrozen = other.IsPositionFrozen;
            NumberPlate = other.NumberPlate;
            NumberPlateStyle = other.NumberPlateStyle;
            IsSpecialLightEnabled = other.IsSpecialLightEnabled;
            TrimColor = other.TrimColor;

            BrokenWindows = other.BrokenWindows == null ? null : new List<int>(other.BrokenWindows).ToArray();
            BrokenDoors = other.BrokenDoors == null ? null : new List<int>(other.BrokenDoors).ToArray();
            PoppedTyres = other.PoppedTyres == null ? null : new List<int>(other.PoppedTyres).ToArray();
            Neons = other.Neons == null ? null : new List<Neon>(other.Neons).ToArray();

            EnginePowerMultiplier = other.EnginePowerMultiplier;
            EngineTorqueMultiplier = other.EngineTorqueMultiplier;

            CustomPrimaryColor = other.CustomPrimaryColor;
            CustomSecondaryColor = other.CustomSecondaryColor;

            ModColor1 = other.ModColor1;
            ModColor2 = other.ModColor2;

            NeonColor = other.NeonColor;
            TyreSmokeColor = other.TyreSmokeColor;

            WheelColor = other.WheelColor;
            WheelType = other.WheelType;
            WindowTint = other.WindowTint;

            DashboardColor = other.DashboardColor;
            Health = other.Health;

            Livery = other.Livery;
            PearlescentColor = other.PearlescentColor;
        }
    }
}
