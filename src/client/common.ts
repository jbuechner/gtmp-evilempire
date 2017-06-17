const enum EntityType {
    Vehicle = 1,
    Prop = 2,
    Blip = 3,
    Marker = 4,
    Pickup = 5,
    Player = 6,
    TextLabel = 7,
    Ped = 8,
    Particle = 9,
    World = 255
}

const SynchronizationProperties = {
    EntityType : "ENTITY_TYPE",
    EntityId : "ENTITY_ID",
    EntityDisplayName : "ENTITY_DISPLAYNAME",
    EntityDialogueName : "ENTITY_DIALOGUENAME",
    FaceShapeFirst : "FACE::SHAPEFIRST",
    FaceShapeSecond : "FACE::SHAPESECOND",
    FaceSkinFirst : "FACE::SKINFIRST",
    FaceSkinSecond : "FACE::SKINSECOND",
    FaceSkinMix : "FACE::SKINMIX",
    FaceShapeMix : "FACE::SHAPEMIX",
    FaceHairStyle : "HAIR::STYLE",
    FaceHairColor : "HAIR::COLOR"
};