namespace gtmp.evilempire.entities
{
    public class Character
    {
        public int Id { get; set; }
        public string AssociatedLogin { get; set; }
        public Vector3f? Position { get; set; }
        public Vector3f? Rotation { get; set; }
        public Gender Gender { get; set; }
        public bool HasBeenThroughInitialCustomization { get; set; }
    }
}
