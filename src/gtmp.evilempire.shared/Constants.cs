﻿namespace gtmp.evilempire
{
    public static class Constants
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class Database
        {
            public static readonly string DatabasePath = "db/data.db"; // todo: by config
            public static readonly string DatabaseTemplatePath = "dbt";

            public static class Sequences
            {
                public static readonly string CharacterIdSequence = "characterId";
                public static readonly string ItemIdSequence = "itemId";
                public static readonly string VehicleIdSequence = "verhicleId";
            }
        }
        public static class MemoryMappedFiles
        {
            public static readonly string Status = "gtmpee_status";
        }
    }
}
